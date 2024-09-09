using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserServer.DTOs;
using UserServer.Models;
using UserServer.Services;

namespace UserServer.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IMapper _mapper;

        public UserController(UserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get a paginated and sorted list of users.
        /// </summary>
        /// <param name="_page">Page number for pagination, defaults to 1.</param>
        /// <param name="_per_page">Number of items per page, defaults to 10.</param>
        /// <param name="_sort">Sort order, defaults to "name".</param>        
        /// <returns>
        /// Returns a paginated list of users.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(PagedResult<UserDto>))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ActionResult<PagedResult<UserDto>>> GetPagedUsers(
            [FromQuery] int _page = 1,
            [FromQuery] int _per_page = 10,
            [FromQuery] string _sort = "name")
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var pagedUsers = await _userService.GetPagedUsers(_page, _per_page, _sort);

                return Ok(pagedUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get a user by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(UserDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        public async Task<ActionResult<UserDto>> GetUserById(
            [FromRoute] string id)
        {
            if (String.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid guid))
                return BadRequest("Invalid or missing UserID");

            var user = await _userService.GetUserById(guid);

            return user == null ? NotFound("Not Found") : Ok(user);
        }

        /// <summary>
        /// Get a user by email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("by-email")]
        [ProducesResponseType(200, Type = typeof(UserDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ActionResult<UserDto>> GetUserByEmail(
            [FromQuery] string email)
        {
            try
            {
                if (String.IsNullOrEmpty(email))
                    return BadRequest("Missing email address");

                var user = await _userService.GetUserByEmail(email);

                return user == null ? NotFound("User not found") : Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(UserDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ActionResult<UserDto>> CreateUser(
            [FromBody] CreateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _userService.CreateUser(request);
                var userDto = _mapper.Map<UserDto>(user);

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Update an existing user.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(200, Type = typeof(UserDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ActionResult<UserDto>> UpdateUser(
            [FromRoute] string id,
            [FromBody] UpdateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid || String.IsNullOrEmpty(id) || !Guid.TryParse(id, out _))
                    return BadRequest(ModelState);

                var user = await _userService.UpdateUser(Guid.Parse(id), request);

                return user == null ? NotFound("User not found") : Ok(_mapper.Map<UserDto>(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Delete a user by ID.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<IActionResult> DeleteUser(
            [FromRoute] string id)
        {
            try
            {
                if (String.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid guid))
                    return BadRequest("Invalid or missing UserId");

                bool isDeleted = await _userService.DeleteUser(guid);

                return !isDeleted ? NotFound("User not found") : NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error: Failed to delete user");
            }
        }

        /// <summary>
        /// Export users to Excel
        /// </summary>
        [HttpGet("export")]
        [ProducesResponseType(200, Type = typeof(FileContentResult))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<IActionResult> ExportUsersToExcel(
            [FromQuery] int _page = 1,
            [FromQuery] int _per_page = Int32.MaxValue,
            [FromQuery] string _sort = "name")
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                byte[] excelData = await _userService.ExportUsersToExcel(_page, _per_page, _sort);
                string fileName = _userService.exportUsersFileName;
                string fileType = _userService.exportUsersFileType;

                return File(excelData, fileType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
