using Microsoft.AspNetCore.Mvc;
using Accountable.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Accountable.DataStructures.ResponseRequestData;
using System.Text;
using Microsoft.VisualBasic;

namespace Accountable.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : Controller
    {
        private readonly DBContext _context;

        public MessagesController(DBContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("{messageId}/read")]
        public ActionResult<Object> ReadMessage(int messageId)
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            if (!_context.IsAuthenticated(ids))
                return Problem("Could not retreived user data.");
            var message = _context.Messages.Find(messageId);
            if (message == null)
                return Problem("No message with such an ID exists");
            if (message.ToUserId != ids.UserID)
                return Problem("Cannot modify this message");
            message.Read = true;
            //var ourId = ids.UserID;
            //var theirId = message.ToUserId == ourId ? message.UserId : message.ToUserId;
            // doing this breaks the notification system!!!
            //_context.Notifications.RemoveRange(_context.Notifications.Where(n => (n.Kind == "Message" && ((n.To == ourId && n.From == theirId)||(n.To == theirId && n.From == ourId)))));
            _context.SaveChanges();
            return Ok(new { Read = true });
        }

        // get {limit = 150} message interactions with {toUserId} sent before {messageId}
        [Authorize]
        [HttpGet("{toUserId}/{messageId}/{limit?}")]
        public ActionResult<IEnumerable<Object>> GetLimitedFromAndTo(int toUserId, int messageId, int limit = 150)
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            if (!_context.IsAuthenticated(ids))
                return Problem("Could not retreived user data.");
            Message latest = _context.Messages.Find(messageId)!;
            var messages = _context.Messages.Where(m =>
            ((m.UserId == toUserId && m.ToUserId == ids.UserID) || (m.UserId == ids.UserID && m.ToUserId == toUserId)) && m.SentAt < latest.SentAt
            ).ToList();
            messages.Sort((a, b) => DateTime.Compare(a.SentAt, b.SentAt));
            return Ok(messages.TakeLast(limit));
        }

        // get message interactions with {toUserId/UserId} sent after {messageId}
        [Authorize]
        [HttpGet("subscribeAfter/{messageId}")]
        public ActionResult<IEnumerable<Object>> GetAfter(int messageId)
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            if (!_context.IsAuthenticated(ids))
                return Problem("Could not retreived user data.");
            Message latest = _context.Messages.Find(messageId)!;
            var otherId = latest.UserId == ids.UserID ? latest.ToUserId : latest.UserId;
            var messages = _context.Messages.Where(m =>
            ((m.UserId == otherId && m.ToUserId == ids.UserID) || (m.UserId == ids.UserID && m.ToUserId == otherId)) && m.SentAt > latest.SentAt
            ).ToList();
            messages.Sort((a, b) => DateTime.Compare(a.SentAt, b.SentAt));
            return Ok(messages);
        }

        // get {limit = 20} most recent messages FROM and TO each
        [Authorize]
        [HttpGet("all/{limit?}")]
        public ActionResult<IEnumerable<Object>> GetTopLevel(int limit = 20)
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            if (!_context.IsAuthenticated(ids))
                return Problem("Could not retreived user data.");
            var messages = from message in _context.Messages where message.UserId == ids.UserID || message.ToUserId == ids.UserID select (message.UserId == ids.UserID ? message.ToUserId : message.UserId);
            var trueIds = messages.ToHashSet().ToList();
            Func<int, Object> lambda = id =>
            {
                var msgs = _context.Messages.Where(m => m.UserId == id || m.ToUserId == id).ToList();
                msgs.Sort((a, b) => DateTime.Compare(a.SentAt, b.SentAt));
                var user = _context.Users.Find(id)!;
                return new
                {
                    Messages = msgs.TakeLast(limit),
                    UserId = id,
                    Username = user.Name,
                    UserPhoto = user.ProfilePicture
                };
            };
            return Ok(trueIds.Select(lambda));
        }

        [Authorize]
        [HttpPost("post")]
        public ActionResult<Object> PostMessage([FromForm] CreateMessage payload)
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            if (!_context.IsAuthenticated(ids))
                return Problem("Could not retreived user data.");
            var newMessage = new Message
            {
                Id = 0,
                UserId = ids.UserID,
                ToUserId = payload.ToUserId,
                Content = Encoding.UTF8.GetBytes(payload.Content!),
                SentAt = DateTime.Now,
                Read = false
            };
            _context.Messages.Add(newMessage);
            _context.SaveChanges();
            _context.Notifications.Add(new Notification
            {
                Id = 0,
                Kind = Notification.Kinds[0],
                TimeSent = DateTime.Now,
                KeyTo = newMessage.Id,
                To = payload.ToUserId,
                From = ids.UserID,
                Read = false
            });
            _context.SaveChanges();
            return Ok(newMessage);
        }
    }
}
