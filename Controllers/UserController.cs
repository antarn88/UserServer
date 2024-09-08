using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Globalization;
using UserServer.DTOs;
using UserServer.Models;
using UserServer.Repositories;

namespace UserServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserController(UserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
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
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var pagedUsers = await _userRepository.GetPagedUsers(_page, _per_page, _sort);

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
        public async Task<ActionResult<UserDto>> GetUserById([FromRoute] string id)
        {
            if (String.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid guid))
                return BadRequest("Invalid or missing UserID");

            var user = await _userRepository.GetUserById(guid);

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
        public async Task<ActionResult<UserDto>> GetUserByEmail([FromQuery] string email)
        {
            if (String.IsNullOrEmpty(email))
                return BadRequest("Missing email address");

            try
            {
                var user = await _userRepository.GetUserByEmail(email);

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
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _userRepository.CreateUser(request);
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
            if (!ModelState.IsValid || String.IsNullOrEmpty(id) || !Guid.TryParse(id, out _))
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userRepository.UpdateUser(Guid.Parse(id), request);

                if (user == null)
                    return NotFound("User not found");

                var userDto = _mapper.Map<UserDto>(user);

                return Ok(userDto);
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
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            try
            {
                if (String.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid guid))
                    return BadRequest("Invalid or missing UserId");

                bool isDeleted = await _userRepository.DeleteUser(guid);

                if (!isDeleted)
                    return NotFound("User not found");

                return NoContent();
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
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<IActionResult> ExportUsersToExcel(
            [FromQuery] int _page = 1,
            [FromQuery] int _per_page = Int32.MaxValue,
            [FromQuery] string _sort = "name")
        {
            try
            {
                // Fetching the users list without email filtering
                var pagedResult = await _userRepository.GetPagedUsers(_page, _per_page, _sort);
                var users = pagedResult.Data;

                var fileName = $"felhasznalok-{DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss", CultureInfo.InvariantCulture)}.xlsx";

                using var package = new ExcelPackage();

                var worksheet = package.Workbook.Worksheets.Add("Felhasználók");

                // Adding Header
                worksheet.Cells[1, 1].Value = "Név";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Kor";

                // Making header bold
                using (var range = worksheet.Cells[1, 1, 1, 3])
                {
                    range.Style.Font.Bold = true;
                }

                // Adding user data
                int row = 2;
                foreach (var user in users)
                {
                    worksheet.Cells[row, 1].Value = user.Name;
                    worksheet.Cells[row, 2].Value = user.Email;
                    worksheet.Cells[row, 3].Value = user.Age;
                    row++;
                }

                // AutoFit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Convert the Excel package to a byte array
                byte[]? excelData = package.GetAsByteArray();

                // Return the file
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
