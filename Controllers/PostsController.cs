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
