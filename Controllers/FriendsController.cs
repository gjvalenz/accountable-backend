using Accountable.DataStructures.ResponseRequestData;
using Accountable.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Accountable.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendsController : ControllerBase
    {
        private readonly DBContext _context;

        public FriendsController(DBContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("all")]
        public ActionResult<IEnumerable<UserView>> GetAllFriends()
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            var userId = ids.UserID;
            var friends = _context.Friends.Where(f => (f.UserId1 == userId || f.UserId2 == userId)).ToList();
            return friends.Select(f =>
            {
                var correctId = (f.UserId2 == userId) ? f.UserId1 : f.UserId2;
                var user = _context.Users.Find(correctId)!;
                var uv = UserView.FromUser(user);
                uv.FriendRequestSent = f.FriendsSince;
                return uv;
            }).ToList();
        }

        [Authorize]
        [HttpGet("remove/{otherId}")]
        public ActionResult<FriendRequestResult> RemoveFriend(int otherId)
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            var userId = ids.UserID;
            var friend = _context.Friends.Where(f =>
                ((f.UserId2 == otherId && f.UserId1 == userId) || (f.UserId2 == userId && f.UserId1 == otherId))).FirstOrDefault();
            if (friend != null)
            {
                _context.Friends.Remove(friend);
                _context.Users.Find(userId)!.NumFriends -= 1;
                _context.Users.Find(otherId)!.NumFriends -= 1;
                _context.SaveChanges();
            }
            return Ok(new FriendRequestResult { Friended = false });
        }

        [Authorize]
        [HttpGet("removeRequest/{otherId}")]
        public ActionResult<FriendRequestResult> RemoveRequest(int otherId)
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            var userId = ids.UserID;
            var frSent = _context.FriendRequests.Where(r => r.ToUserId == otherId && r.FromUserId == userId).FirstOrDefault();
            var frReceived = _context.FriendRequests.Where(r => r.ToUserId == userId && r.FromUserId == otherId).FirstOrDefault();
            if (frSent != null)
            {
                _context.FriendRequests.Remove(frSent);
            }
            if (frReceived != null)
            {
                _context.FriendRequests.Remove(frReceived);
            }
            _context.SaveChanges();
            return Ok(new FriendRequestResult { Friended = false });
        }

        [Authorize]
        [HttpGet("requestsSent")]
        public ActionResult<IEnumerable<UserView>> RequestsSent()
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            var userId = ids.UserID;
            var frs = from f in _context.FriendRequests where f.FromUserId == userId select f;
            var uvs = frs.AsEnumerable().Select(f =>
            {
                var uv = UserView.FromUser(_context.Users.Find(f.ToUserId)!);
                uv.FriendRequestSent = f.DateSent;
                return uv;
            });
            return Ok(uvs);
        }

        [Authorize]
        [HttpGet("requestsReceived")]
        public ActionResult<IEnumerable<UserView>> RequestsReceived()
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            var userId = ids.UserID;
            var frs = from f in _context.FriendRequests where f.ToUserId == userId select f;
            var uvs = frs.AsEnumerable().Select(f =>
            {
                var uv = UserView.FromUser(_context.Users.Find(f.FromUserId)!);
                uv.FriendRequestSent = f.DateSent;
                return uv;
            });
            return Ok(uvs);
        }

        [Authorize]
        [HttpGet("request/{otherId}")]
        public ActionResult<FriendRequestResult> RequestF(int otherId)
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            var ourId = ids.UserID;
            var frReceived = _context.FriendRequests.Where(r => r.ToUserId == ourId && r.FromUserId == otherId).FirstOrDefault();
            var frSent = _context.FriendRequests.Where(r => r.ToUserId == otherId && r.FromUserId == ourId).FirstOrDefault();
            var friend = _context.Friends.Where(f => ((f.UserId2 == otherId && f.UserId1 == ourId) || (f.UserId2 == ourId && f.UserId1 == otherId))).FirstOrDefault();
            if (friend != null)
            {
                return Ok(new FriendRequestResult { Friended = true }); // prevent fr if already friend
            }
            if (frReceived != null)
            {
                _context.FriendRequests.Remove(frReceived);
                Friend fri = new Friend
                {
                    Id = 0,
                    UserId1 = ourId,
                    UserId2 = otherId,
                    FriendsSince = DateTime.Now
                };
                _context.Friends.Add(fri);
                _context.SaveChanges();
                Notification notif = new Notification
                {
                    Id = 0,
                    Kind = Notification.Kinds[2],
                    TimeSent = DateTime.Now,
                    KeyTo = fri.Id,
                    To = otherId,
                    From = ourId,
                    Read = false
                };
                _context.Notifications.Add(notif);
                _context.SaveChanges();
                _context.Users.Find(ourId)!.NumFriends += 1;
                _context.Users.Find(otherId)!.NumFriends += 1;
                _context.SaveChanges();
                return Ok(new FriendRequestResult { Friended = true });
            }
            else
            {
                if (frSent == null) // make new one
                {
                    FriendRequest fr = new FriendRequest
                    {
                        Id = 0,
                        FromUserId = ourId,
                        ToUserId = otherId,
                        DateSent = DateTime.Now
                    };
                    _context.FriendRequests.Add(fr);
                    _context.SaveChanges();
                    Notification notif = new Notification
                    {
                        Id = 0,
                        Kind = Notification.Kinds[3],
                        TimeSent = DateTime.Now,
                        KeyTo = fr.Id,
                        To = otherId,
                        From = ourId,
                        Read = false
                    };
                    _context.Notifications.Add(notif);
                    _context.SaveChanges();
                }
                //_context.SaveChanges();
                return Ok(new FriendRequestResult { Friended = false });
            }
        }

    }
}
