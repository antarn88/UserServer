using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserServer.DTOs;
using UserServer.Models;
using UserServer.Services;

namespace UserServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;

        public UsersController(UsersService usersService)
        {
            _usersService = usersService;
        }

        /// <summary>
        /// Get paginated and sorted list of users or get a user by email.
        /// </summary>
        [HttpGet]
        public ActionResult<PagedResult<UserDto>> GetUsers(
            [FromQuery] int _page = 1,
            [FromQuery] int _per_page = 10,
            [FromQuery] string _sort = "name",
            [FromQuery] string? email = null)
        {
            if (_per_page <= 0)
            {
                _per_page = 10;
            }

            if (_page <= 0)
            {
                _page = 1;
            }

            if (!string.IsNullOrEmpty(email))
            {
                UserDto? user = _usersService.GetUserByEmail(email);

                if (user == null)
                {
                    return NotFound("Not Found");
                }

                return Ok(user);
            }

            var users = _usersService.GetUsers(_page, _per_page, _sort);

            return Ok(users);
        }

        /// <summary>
        /// Get a user by ID.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<UserDto> GetUserById(string id)
        {
            UserDto? user = _usersService.GetUserById(id);

            if (user == null)
            {
                return NotFound("Not Found");
            }

            return Ok(user);
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        [HttpPost]
        public ActionResult<UserDto> CreateUser([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                User? user = _usersService.CreateUser(request);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Age = user.Age
                };

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, userDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing user.
        /// </summary>
        [HttpPut("{id}")]
        public ActionResult<UserDto> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                User? user = _usersService.UpdateUser(id, request);

                if (user == null)
                {
                    return NotFound("Not Found");
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Age = user.Age
                };

                return Ok(userDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a user by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(string id)
        {
            bool success = _usersService.DeleteUser(id);

            if (!success)
            {
                return NotFound("Not Found");
            }

            return NoContent();
        }
    }
}
