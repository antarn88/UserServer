using Microsoft.AspNetCore.Mvc;
using UserServer.DTOs;
using UserServer.Services;

namespace UserServer.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [ProducesResponseType(200, Type = typeof(AuthResponse))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ActionResult<AuthResponse>> Login(
           [FromBody] LoginRequest loginRequest)
        {
            try
            {
                if (loginRequest == null || !ModelState.IsValid)
                    return BadRequest("Invalid request.");

                var response = await _authService.Login(loginRequest.Email, loginRequest.Password);

                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid email or password.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
