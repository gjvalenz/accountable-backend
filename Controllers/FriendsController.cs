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
                _context.Friends.Add(new Friend
                {
                    Id = 0,
                    UserId1 = ourId,
                    UserId2 = otherId,
                    FriendsSince = DateTime.Now
                });
                return Ok(new FriendRequestResult { Friended = true });
            }
            else
            {
                if (frSent == null) // make new one
                {
                    _context.FriendRequests.Add(new FriendRequest
                    {
                        Id = 0,
                        FromUserId = ourId,
                        ToUserId = otherId,
                        DateSent = DateTime.Now
                    });
                }
                return Ok(new FriendRequestResult { Friended = false });
            }
        }

    }
}
