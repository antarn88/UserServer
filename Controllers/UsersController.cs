using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Globalization;
using UserServer.DTOs;
using UserServer.Models;
using UserServer.Services;

namespace UserServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;

        public UsersController(UsersService usersService)
        {
            _usersService = usersService;
        }

        /// <summary>
        /// Get a paginated and sorted list of users, or retrieve a user by email.
        /// </summary>
        /// <param name="_page">Page number for pagination, defaults to 1.</param>
        /// <param name="_per_page">Number of items per page, defaults to 10.</param>
        /// <param name="_sort">Sort order, defaults to "name".</param>
        /// <param name="email">Optional email to search for a specific user.</param>
        /// <returns>
        /// Returns a paginated list of users, or a single user if email is provided.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(PagedResult<UserDto>))]
        [ProducesResponseType(404, Type = typeof(string))]
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
                var user = _usersService.GetUserByEmail(email);

                return user == null ? NotFound("Not Found") : Ok(user);
            }

            var users = _usersService.GetUsers(_page, _per_page, _sort);

            return Ok(users);
        }

        //TODO: Get User By Email saját különálló hívásban!

        /// <summary>
        /// Get a user by ID.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<UserDto> GetUserById(string id)
        {
            var user = _usersService.GetUserById(id);

            return user == null ? NotFound("Not Found") : Ok(user);
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        [HttpPost]
        public ActionResult<UserDto> CreateUser([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var user = _usersService.CreateUser(request);
                var userDto = new UserDto()
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
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var user = _usersService.UpdateUser(id, request);
                if (user == null) return NotFound("Not Found");

                var userDto = new UserDto()
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
            bool isSuccess = _usersService.DeleteUser(id);

            return isSuccess ? NoContent() : NotFound("Not Found");
        }

        /// <summary>
        /// Export users to Excel
        /// </summary>
        [HttpGet("export")]
        public IActionResult ExportUsersToExcel(
            [FromQuery] int _page = 1,
            [FromQuery] int _per_page = Int32.MaxValue,
            [FromQuery] string _sort = "name")
        {
            _per_page = _per_page <= 0 ? 10 : _per_page;
            _page = _page <= 0 ? 1 : _page;

            // Fetching the users list without email filtering
            var users = _usersService.GetUsers(_page, _per_page, _sort).Data;
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
    }
}
