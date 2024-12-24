using BookServiceServer.Dtos;
using BookServiceServer.Data;
using BookServiceServer.Models;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace BookServiceServer.Services
{
    public interface IUserService
    {
        Task<(bool Success, string Message)> RegisterUserAsync(SignUpDto signupdto);
        Task<string> AuthenticateUserAsync(LoginDto logindto);
    }

    public class UsersService : IUserService
    {
        private readonly UsersDbContext _context;
        private readonly IConfiguration _configuration; // JWT conf

        public UsersService(UsersDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Signup func
        public async Task<(bool Success, string Message)> RegisterUserAsync(SignUpDto signupdto)
        {
            // Check if username already exists
            if (_context.Users.Any(u => u.Username == signupdto.Username))
            {
                return (false, "Username already exists");
            }

            // Hash password
            var passHash = BCrypt.Net.BCrypt.HashPassword(signupdto.Password);
            
            // Setup user model
            var user = new User
            {
                Username = signupdto.Username,
                PasswordHash = passHash
            };

            // Save changes to db and return success
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return (true, "User registered :)");
        }

        // Login func
        public async Task<string> AuthenticateUserAsync(LoginDto logindto)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Username == logindto.Username);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(logindto.Password, user.PasswordHash))
            {
                return null; // Invalid creds
            }

            // Generate JWT token
            var tokenhandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { 
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)}),
                Expires = DateTime.UtcNow.AddHours(4),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenhandler.CreateToken(tokenDescriptor);
            return tokenhandler.WriteToken(token);
        }
    }
}
