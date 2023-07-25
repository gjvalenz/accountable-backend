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
    public class NotificationsController : ControllerBase
    {
        private readonly DBContext _context;

        public NotificationsController(DBContext context)
        {
            _context = context;
        }

        private byte[] GetContent(int id, string kind)
        {
            if (kind == "Message")
            {
                // fetch from message db
                return _context.Messages.Find(id)!.Content!;
            }
            else if (kind == "Like")
            {
                return Encoding.UTF8.GetBytes("liked your post.");
            }
            else if (kind == "FriendAccepted")
            {
                return Encoding.UTF8.GetBytes("accepted your friend request.");
            }
            else if (kind == "FriendRequest")
            {
                return Encoding.UTF8.GetBytes("sent you a friend request.");
            }
            else if (kind == "CommentPost") // comment on post
            {
                // fetch from comment db
                return _context.Comments.Find(id)!.Content!;
            }
            else
            {
                return _context.Comments.Find(id)!.Content!;
            }

        }

        [Authorize]
        [HttpGet("all")]
        public ActionResult<IEnumerable<NotificationInformation>> GetAll()
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            if (!_context.IsAuthenticated(ids))
                return Problem("Could not retreived user data.");
            User user = _context.Users.Find(ids.UserID)!;
            var notifications = from notifs in _context.Notifications where notifs.To == user.Id select notifs;
            var notifts = notifications.ToList();
            HashSet<string> msgs = new HashSet<string>();
            notifts.Sort((a, b) => DateTime.Compare(b.TimeSent, a.TimeSent));
            notifts.RemoveAll(n =>
            {
                if (n.Kind != "Message")
                    return false;
                if (msgs.Contains(n.To.ToString() + "+" + n.From.ToString()))
                    return true;
                msgs.Add(n.To.ToString() + "+" + n.From.ToString());
                return false;
            });
            return Ok(notifts.Select(n => new NotificationInformation
            {
                Id = n.Id,
                Kind = n.Kind,
                Content = GetContent(n.KeyTo, n.Kind!),
                KeyTo = n.KeyTo,
                FromUserId = n.From,
                FromUsername = _context.Users.Find(n.From)!.Name,
                FromProfilePicture = _context.Users.Find(n.From)!.ProfilePicture,
                Read = n.Read,
                TimeSent = n.TimeSent
            }));
        }

        // get notifications sent after {notificationId}
        [Authorize]
        [HttpGet("subscribeAfter/{notificationId}")]
        public ActionResult<IEnumerable<Object>> GetAfter(int notificationId)
        {
            if (notificationId <= 0)
                return Problem("Invalid Notification ID.");
            var ids = JWTHelper.FromUserClaims(User.Claims);
            if (!_context.IsAuthenticated(ids))
                return Problem("Could not retreived user data.");
            Notification latest = _context.Notifications.Find(notificationId)!;
            HashSet<string> msgs = new HashSet<string>();
            var notifs = _context.Notifications.Where(n => (DateTime.Compare(n.TimeSent, latest.TimeSent) > 0)  && n.To == ids.UserID);
            var ordered = notifs.OrderBy(n => n.TimeSent).ToList();
            ordered.RemoveAll(n =>
            {
                if (n.Kind != "Message")
                    return false;
                if (msgs.Contains(n.To.ToString() + "+" + n.From.ToString()))
                    return true;
                msgs.Add(n.To.ToString() + "+" + n.From.ToString());
                msgs.Add(n.From.ToString() + "+" + n.To.ToString());
                return false;
            });
            return Ok(ordered.Select(n => new NotificationInformation
            {
                Id = n.Id,
                Kind = n.Kind,
                Content = GetContent(n.KeyTo, n.Kind!),
                KeyTo = n.KeyTo,
                FromUserId = n.From,
                FromUsername = _context.Users.Find(n.From)!.Name,
                FromProfilePicture = _context.Users.Find(n.From)!.ProfilePicture,
                Read = n.Read,
                TimeSent = n.TimeSent
            }));
        }

        [Authorize]
        [HttpGet("readAll")]
        public ActionResult<Object> ReadAll()
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            if (!_context.IsAuthenticated(ids))
                return Problem("Could not retreived user data.");
            User user = _context.Users.Find(ids.UserID)!;
            var notifications = from notifs in _context.Notifications where notifs.To == user.Id select notifs;
            notifications.ToList().ForEach(notif => { notif.Read = true; });
            _context.SaveChanges();
            return Ok(new { Read = true });
        }
    }
}
