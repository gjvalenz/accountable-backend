using System.Text;
using Accountable.DataStructures.ResponseRequestData;
using Accountable.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;

namespace Accountable.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class PostsController : ControllerBase
    {

        private readonly DBContext _context;
        private readonly IWebHostEnvironment _env;

        public PostsController(IWebHostEnvironment env, DBContext context)
        {
            _context = context;
            _env = env;
        }


        [Authorize]
        [HttpGet("{postId}/like")]
        public ActionResult<PostLiked> LikePost(int postId)
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            var userId = ids.UserID;
            var first = _context.PostLikes.Where(p => p.UserId == userId && p.PostId == postId).FirstOrDefault();
            var post = _context.Posts.Find(postId);
            if (post == null)
                return Problem("Post does not exist!");
            if (first == null)
            { // like post, create like
                var like = new PostLike
                {
                    Id = 0,
                    UserId = userId,
                    PostId = postId,
                };
                _context.PostLikes.Add(like);
                _context.SaveChanges();
                Notification notif = new Notification
                {
                    Id = 0,
                    Kind = Notification.Kinds[1],
                    TimeSent = DateTime.Now,
                    KeyTo = like.Id,
                    To = post.UserId,
                    From = userId,
                    Read = false
                };
                _context.Notifications.Add(notif);
                post.Likes += 1;
                _context.SaveChanges();
                return Ok(new PostLiked { Liked = true });
            }
            else // unlike!
            {
                var notif = _context.Notifications.Where(n => n.KeyTo == first.Id).First();
                _context.PostLikes.Remove(first);
                _context.Notifications.Remove(notif);
                post.Likes -= 1;
                _context.SaveChanges();
                return Ok(new PostLiked { Liked = false });
            }
        }

        private void LoadPhotos(CreatePost form, Post newPost)
        {
            IFormFile[] files = { form.Photo1, form.Photo2, form.Photo3 };
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i] != null)
                    continue;
                for (int j = i + 1; j < files.Length; j++)
                {
                    if (files[j] != null)
                    {
                        files[i] = files[j];
                        files[j] = null;
                        break;
                    }
                }
            }
            int toUpload = files.Where(f => f != null).Count();
            for (int i = 0; i < toUpload; i++)
            {
                var image = files[i];
                var extension = Path.GetExtension(image.FileName);
                var guid = Guid.NewGuid().ToString();
                var path = Path.Combine(_env.ContentRootPath, "images", guid + extension);
                var webpath = "https://localhost:7126/api/Photos/images/" + guid + extension;
                ImagesHelper.SaveFormFileImageTo(image, path);
                if (i == 0)
                    newPost.PostPhoto1 = webpath;
                else if (i == 1)
                    newPost.PostPhoto2 = webpath;
                else
                    newPost.PostPhoto3 = webpath;
            }
            _context.SaveChanges();
        }

        [Authorize]
        [HttpPost("post")]
        public ActionResult<PostInfo> Post([FromForm] CreatePost post)
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            var created = DateTime.Now;
            var newPost = new Post
            {
                Id = 0,
                CreatedAt = created,
                Content = Encoding.UTF8.GetBytes(post.Content!),
                UserId = ids.UserID,
                Likes = 0,
            };
            _context.Posts.Add(newPost);
            _context.SaveChanges();
            LoadPhotos(post, newPost);
            var postInfo = new PostInfo
            {
                CreatedAt = created,
                Content = newPost.Content,
                Id = newPost.Id,
                UserId = ids.UserID,
                PostPhoto1 = newPost.PostPhoto1,
                PostPhoto2 = newPost.PostPhoto2,
                PostPhoto3 = newPost.PostPhoto3,
                Likes = 0,
                Liked = false
            };
            return Ok(postInfo);
        }

        [Authorize]
        [HttpGet("{date}")]
        public ActionResult<IEnumerable<PostInfo>> GetAll(string date)
        {
            DateTime before = DateTime.Parse(date);
            var ids = JWTHelper.FromUserClaims(User.Claims);
            var posts = (from post in _context.Posts where post.CreatedAt < before select post).Take(25);
            var allPostIds = from post in posts select post.Id;
            var userLikedPostsIds = from like in _context.PostLikes where allPostIds.Contains(like.PostId) && like.UserId == ids.UserID select like.PostId;
            var postList = posts.ToArray().Select(post => new PostInfo
            {
                Id = post.Id,
                UserId = post.UserId,
                Username = _context.Users.Find(post.UserId)!.Name,
                UserPhoto = _context.Users.Find(post.UserId)!.ProfilePicture,
                PostPhoto1 = post.PostPhoto1,
                PostPhoto2 = post.PostPhoto2,
                PostPhoto3 = post.PostPhoto3,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                Likes = post.Likes,
                Liked = userLikedPostsIds.Contains(post.Id)
            }).ToList();
            postList.Sort((a, b) => DateTime.Compare(a.CreatedAt, b.CreatedAt));
            postList.Reverse();
            return postList;
        }
    }
}
