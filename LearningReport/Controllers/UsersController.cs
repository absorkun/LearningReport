using FluentValidation;
using FluentValidation.Results;
using LearningReport.Data;
using LearningReport.Models;
using LearningReport.Users.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace LearningReport.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(AppDbContext db,
        IValidator<CreateUserDto> createUserValidator,
        IValidator<UpdateUserDto> updateUserValidator,
        IValidator<LoginUserDto> loginUserValidator,
        IConfiguration configuration
        ) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetUserDto>>> GetUsers()
        {
            var users = await db.Users
                .Select(u => new GetUserDto(u.Id, u.Email, u.Role))
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetUserDto>> GetUserById(int id)
        {
            var user = await db.Users.FindAsync(id);
            return user is null
                ? NotFound()
                : Ok(new GetUserDto(user.Id, user.Email, user.Role));
        }

        [HttpPost]
        public async Task<ActionResult<GetUserDto>> AddUser([FromBody] CreateUserDto create)
        {
            var result = await createUserValidator.ValidateAsync(create);
            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }

            var user = new User
            {
                Email = create.Email,
                Role = create.Role,
                Password = BCrypt.Net.BCrypt.HashPassword(create.Password),
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById),
                new { id = user.Id },
                new GetUserDto(user.Id, user.Email, user.Role));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto update, int id)
        {
            var result = await updateUserValidator.ValidateAsync(update);
            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }

            var user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(update.Email))
                user.Email = update.Email;
            if (!string.IsNullOrEmpty(update.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(update.Password);
            if (!string.IsNullOrEmpty(update.Role))
                user.Role = update.Role;

            await db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await db.Users.FindAsync(id);
            if (user == null)
                return NotFound();
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto login)
        {
            var result = await loginUserValidator.ValidateAsync(login);
            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
            if (user == null)
            {
                return Unauthorized("Invalid email");
            }

            if (!BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
            {
                return BadRequest("Invalid password");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var secretKkey = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);
            var securityKey = new SymmetricSecurityKey(secretKkey);

            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = creds,
                Expires = DateTime.UtcNow.AddMinutes(60),
            };

            var tokenHandler = new JsonWebTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new { token });
        }

        [HttpGet("Protected")]
        [Authorize]
        public IActionResult Protected()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role);
            return Ok(new {email, role});
        }
    }
}
