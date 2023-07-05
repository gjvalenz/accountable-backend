using Microsoft.AspNetCore.Mvc;
using Accountable.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Accountable.DataStructures.ResponseRequestData;
using System.Text;


namespace Accountable.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : Controller
    {
        private readonly DBContext _context;

        public CommentsController(DBContext context)
        {
            _context = context;
        }

        [HttpGet("{postId}/topLevel")]
        public ActionResult<IEnumerable<Object>> GetTopLevel(int postId)
        {
            var allCms = from comment in _context.Comments where comment.ReplyToKey == postId && comment.ReplyType == 1 select comment;
            var cms = allCms.ToList();
            cms.Sort((a, b) => DateTime.Compare(a.CreatedAt, b.CreatedAt));
            var totalComments = cms.Count();
            var remainingComments = Math.Max(totalComments - 2, 0);
            var remCms = cms.Skip(Math.Max(0, cms.Count() - 2));
            return Ok(new
            {
                Comments = remCms.Select(c => new
                {
                    c.Id,
                    c.UserId,
                    c.Content,
                    c.CreatedAt,
                    c.ReplyToKey,
                    c.ReplyType,
                    Username = _context.Users.Find(c.UserId)!.Name,
                    UserPhoto = _context.Users.Find(c.UserId)!.ProfilePicture,
                    SubRepliesCount = _context.Comments.Where(com => com.ReplyToKey == c.Id && com.ReplyType == 2).Count()
                }),
                Remaining = remainingComments
            });
        }

        [HttpGet("{postId}/topLevelAll")]
        public ActionResult<IEnumerable<Object>> GetTopLevelAll(int postId)
        {
            var allCms = from comment in _context.Comments where comment.ReplyToKey == postId && comment.ReplyType == 1 select comment;
            var cms = allCms.ToList();
            cms.Sort((a, b) => DateTime.Compare(a.CreatedAt, b.CreatedAt));
            return Ok(cms.Select(c => new
            {
                c.Id,
                c.UserId,
                c.Content,
                c.CreatedAt,
                c.ReplyToKey,
                c.ReplyType,
                Username = _context.Users.Find(c.UserId)!.Name,
                UserPhoto = _context.Users.Find(c.UserId)!.ProfilePicture,
                SubRepliesCount = _context.Comments.Where(com => com.ReplyToKey == c.Id && com.ReplyType == 2).Count()
            }));
        }

        [HttpGet("{commentId}/subReplies")]
        public ActionResult<IEnumerable<Object>> GetSubReplies(int commentId)
        {
            var allCms = from comment in _context.Comments where comment.ReplyToKey == commentId && comment.ReplyType == 2 select comment;
            var cms = allCms.ToList();
            cms.Sort((a, b) => DateTime.Compare(a.CreatedAt, b.CreatedAt));
            return Ok(cms.Select(c => new
            {
                c.Id,
                c.UserId,
                c.Content,
                c.CreatedAt,
                c.ReplyToKey,
                c.ReplyType,
                Username = _context.Users.Find(c.UserId)!.Name,
                UserPhoto = _context.Users.Find(c.UserId)!.ProfilePicture
            }));
        }

        [Authorize]
        [HttpPost("post")]
        public ActionResult<Object> PostComment([FromForm] CreateComment comment)
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            if (!_context.IsAuthenticated(ids))
                return Problem("Could not retreived user data.");
            var newComment = new Comment
            {
                Id = 0,
                UserId = ids.UserID,
                Content = Encoding.UTF8.GetBytes(comment.Content!),
                CreatedAt = DateTime.Now,
                ReplyType = comment.ReplyKind,
                ReplyToKey = comment.PostOrCommentId
            };
            _context.Comments.Add(newComment);
            _context.SaveChanges();
            if (comment.ReplyKind == 1)
            {
                var to = _context.Posts.Find(comment.PostOrCommentId)!.UserId;
                if (to != ids.UserID)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Id = 0,
                        Kind = Notification.Kinds[4],
                        TimeSent = DateTime.Now,
                        KeyTo = newComment.Id,
                        To = to,
                        From = ids.UserID,
                        Read = false
                    });
                    _context.SaveChanges();
                }
            }
            else
            {
                var toOrig = _context.Comments.Find(comment.PostOrCommentId)!.UserId;
                var otherToIds = _context.Comments.Where(c => c.ReplyToKey == comment.PostOrCommentId && c.ReplyType == 2).Select(c => c.UserId).ToList();
                otherToIds.Add(toOrig);
                otherToIds = otherToIds.Where(id => id != ids.UserID).ToList();
                foreach (var id in otherToIds)
                {
                    _context.Notifications.Add(new Notification
                    {
                        Id = 0,
                        Kind = Notification.Kinds[5],
                        TimeSent = DateTime.Now,
                        KeyTo = newComment.Id,
                        To = id,
                        From = ids.UserID,
                        Read = false
                    });
                }
                _context.SaveChanges();
            }
            return Ok(comment);
        }
    }
}
