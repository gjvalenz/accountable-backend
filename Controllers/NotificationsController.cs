﻿using Microsoft.AspNetCore.Mvc;
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
                return Encoding.UTF8.GetBytes("sent you a message.");
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
            else if(kind == "CommentPost") // comment on post
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
            notifts.Sort((a, b) => DateTime.Compare(b.TimeSent, a.TimeSent));
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