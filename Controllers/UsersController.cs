using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VistaTiBooks.Api.Data;
using VistaTiBooks.Api.DTOs;
using VistaTiBooks.Api.Models;

namespace VistaTiBooks.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(CreateUserDto dto)
        {
            var user = new User
            {
                UserName = dto.UserName
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }
    }
}
