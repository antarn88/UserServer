using Microsoft.AspNetCore.Mvc;
using UserServer.DTOs;
using UserServer.Services;

namespace UserServer.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController(AuthService authService) : ControllerBase
    {
        private readonly AuthService _authService = authService;

        [HttpPost("login")]
        public ActionResult<AuthResponse> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || !ModelState.IsValid) return BadRequest("Invalid request.");

            try
            {
                AuthResponse? response = _authService.Login(loginRequest.Email, loginRequest.Password);

                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid email or password.");
            }
        }
    }
}
