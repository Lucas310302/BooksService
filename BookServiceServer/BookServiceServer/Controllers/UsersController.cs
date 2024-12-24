using Microsoft.AspNetCore.Mvc;
using BookServiceServer.Dtos;
using BookServiceServer.Services;

namespace BookServiceServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto signupDto)
        {
            var results = await _userService.RegisterUserAsync(signupDto);
            if (!results.Success)
            {
                return BadRequest(results.Message);
            }

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto logindto)
        {
            var token = await _userService.AuthenticateUserAsync(logindto);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Invalid username or password");
            }

            return Ok(token);
        }
    }
}
