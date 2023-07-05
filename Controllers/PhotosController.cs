using Accountable.DataStructures.ResponseRequestData;
using Accountable.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;

namespace Accountable.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {

        private readonly DBContext _context;


        private readonly IWebHostEnvironment _env;

        public PhotosController(IWebHostEnvironment env, DBContext context)
        {
            _context = context;
            _env = env;
        }

        [Authorize]
        [HttpPost("profilePhoto")]
        public ActionResult<PhotoInformation> UploadProfilePhoto([FromForm] ImageUpload img)
        {
            var ids = JWTHelper.FromUserClaims(User.Claims);
            if (!_context.IsAuthenticated(ids))
                return Problem("Could not retreived user data.");
            var image = img.Image;
            if (image == null)
                return Problem("No image uploaded.");
            if (image!.Length > 0)
            {
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
                User u = _context.Users.Find(ids.UserID)!;
                u.ProfilePicture = webpath;
                _context.SaveChanges();
                return Ok(new PhotoInformation
                {
                    Url = webpath
                });
            }
            else
                return Problem("Bad Upload Detected.");
        }

        [HttpGet("images/{imageName}")]
        public IActionResult RetreiveImage(string imageName)
        {
            var path = Path.Combine(_env.ContentRootPath, "images/", imageName);
            if (!System.IO.File.Exists(path))
                return Problem("File does not exist.");
            return PhysicalFile(path, "image/jpeg");
            //byte[] data = System.IO.File.ReadAllBytes(path);
            //return Ok(File(data, "image/jpeg"));
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
