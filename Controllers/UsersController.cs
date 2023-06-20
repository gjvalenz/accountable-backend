using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Accountable.Models;

namespace Accountable.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly DBContext _context;

        public UsersController(DBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            //var u = from user in _context.Users where user.Gender == 'f' select user;
            return await _context.Users.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var res = await _context.Users.FindAsync(id);
            if (res != null)
                return res;
            else
                return Problem("User with that id not found.");
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateNewUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetUsers", new { id = user.Id }, user);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUserById(int id)
        {
            var res = await _context.Users.FindAsync(id);
            if (res == null)
                return Problem("No such user exists!");
            _ = _context.Users.Remove(res);
            await _context.SaveChangesAsync();
            return res;
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
