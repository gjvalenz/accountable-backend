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
    public class AccountController : Controller
    {

        private readonly DBContext _context;
        private readonly IConfiguration _config;
        private readonly string _secret;
        private readonly IWebHostEnvironment _env;

        public AccountController(IWebHostEnvironment env, DBContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _secret = _config.GetSection("AppSettings:Secret").Value!;
            _env = env;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<UserAccount>>> GetAll()
        {
            return await _context.UserAccounts.ToListAsync();
        }

        [Authorize]
        [HttpGet]
        public ActionResult<Object> GetSelf()
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            if (!_context.IsAuthenticated(ids))
                return Problem("Could not retreived user data.");
            User user = _context.Users.Find(ids.UserID)!;
            UserAccount userAccount = _context.UserAccounts.Find(ids.UserAccountID)!;
            return Ok(new
            {
                Name = user.Name,
                ProfilePicture = user.ProfilePicture!,
                Registered = user.Registered,
                Email = userAccount.Email!,
                Id = user.Id,
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenInfo>> Login(LoginInfo login)
        {
            var result = _context.UserAccounts.FirstOrDefault(ua => ua.Email!.Equals(login.Email) && ua.HashPass!.Equals(Hasher.Hash(login.Password!)));
            if (result == null)
                return Problem("No account with that email and password.");
            var user = _context.Users.Find(result.UserId);
            if (user == null)
                return Problem("No user associated with this account.");
            var token = JWTHelper.GenerateJWT(user, result, _secret);
            result.AuthorizationToken = token;
            await _context.SaveChangesAsync();
            return Ok(new TokenInfo
            {
                AuthenticationToken = token
            });
        }

        [Authorize]
        [HttpGet("logout")]
        public ActionResult<String> Logout()
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            var user = _context.Users.Find(ids.UserID);
            var userAccount = _context.UserAccounts.Find(ids.UserAccountID);
            if (userAccount == null || user == null)
                return Problem("Could not retreived user data.");
            userAccount.AuthorizationToken = null;
            _context.SaveChanges();
            // have to delete token on front-end as well
            return Ok(new
            {
                Success = true
            });
        }

        [HttpPost("register")]
        public ActionResult<TokenInfo> Register([FromForm] RegisterInfo Register)
        {
            var result = _context.UserAccounts.FirstOrDefault(ua => ua.Email!.Equals(Register.Email));
            if (result != null)
                return Problem("User with email already exists.");
            var timeAdded = DateTime.Now;
            var user = new User
            {
                Id = 0,
                Name = Register.Name,
                ProfilePicture = "",
                Registered = timeAdded,
                Weight = Register.Weight,
                Height = Register.Height,
                About = Register.About != null ? Encoding.UTF8.GetBytes(Register.About) : null,
                NumFriends = 0,
                Gender = Register.Gender,
                PrivateProgress = false
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            var confirmationCode = System.Guid.NewGuid().ToString();
            var userAccount = new UserAccount
            {
                Id = 0,
                Email = Register.Email,
                HashPass = Hasher.Hash(Register.Password!),
                ConfirmationCode = confirmationCode,
                Confirmed = false,
                UserId = user.Id,
                AuthorizationToken = ""
            };
            _context.UserAccounts.Add(userAccount);
            _context.SaveChanges();
            if (Register.Photo != null)
            {
                var image = Register.Photo;
                var extension = Path.GetExtension(image.FileName);
                var guid = Guid.NewGuid().ToString();
                var path = Path.Combine(_env.ContentRootPath, "images", guid + extension);
                var webpath = "https://localhost:7126/api/Photos/images/" + guid + extension;
                ImagesHelper.SaveFormFileImageTo(image, path);
                if (OperatingSystem.IsWindows())
                {
                    string pathTo = Path.Combine(_env.ContentRootPath, "images", guid + "-64" + extension);
                    ImagesHelper.ResizeImageFromTo(path, pathTo, 64, 64);
                }
                user.ProfilePicture = webpath;
                _context.SaveChanges();
            }
            else
            {
                user.ProfilePicture = "https://i.ibb.co/VHWWb2s/cropped.jpg";
                _context.SaveChanges();
            }
            var token = JWTHelper.GenerateJWT(user, userAccount, _secret);
            userAccount.AuthorizationToken = token;
            _context.SaveChanges();
            return Ok(new TokenInfo
            {
                AuthenticationToken = token
            });
        }
    }
}
