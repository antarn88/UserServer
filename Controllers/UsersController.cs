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
    public class UsersController(UsersService usersService) : ControllerBase
    {
        private readonly UsersService _usersService = usersService;

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
            _per_page = _per_page <= 0 ? 10 : _per_page;
            _page = _page <= 0 ? 1 : _page;

            if (!string.IsNullOrEmpty(email))
            {
                UserDto? user = _usersService.GetUserByEmail(email);

                return user == null ? NotFound("Not Found") : Ok(user);
            }

            Models.PagedResult<UserDto>? users = _usersService.GetUsers(_page, _per_page, _sort);

            return Ok(users);
        }

        /// <summary>
        /// Get a user by ID.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<UserDto> GetUserById(string id)
        {
            UserDto? user = _usersService.GetUserById(id);

            return user == null ? NotFound("Not Found") : Ok(user);
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

                UserDto userDto = new()
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

                UserDto userDto = new()
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

            return success ? NoContent() : NotFound("Not Found");
        }
    }
}
