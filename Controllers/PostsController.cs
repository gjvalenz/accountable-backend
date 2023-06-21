using Accountable.DataStructures.ResponseRequestData;
using Accountable.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Accountable.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class PostsController : ControllerBase
    {

        private readonly DBContext _context;

        public PostsController(DBContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("post")]
        public ActionResult<PostInfo> Post(CreatePost post)
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            var created = DateTime.Now;
            var len = post.PhotoUrls!.Count();
            var newPost = new Post
            {
                Id = 0,
                CreatedAt = created,
                Content = post.Content,
                UserId = ids.UserID,
                Likes = 0,
                PostPhoto1 = len > 0 ? post.PhotoUrls[0] : null,
                PostPhoto2 = len > 1 ? post.PhotoUrls[1] : null,
                PostPhoto3 = len > 2 ? post.PhotoUrls[2] : null,
            };
            _context.Posts.Add(newPost);
            var postInfo = new PostInfo
            {
                CreatedAt = created,
                Content = post.Content,
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
