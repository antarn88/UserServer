using OfficeOpenXml;
using System.Globalization;
using UserServer.DTOs;
using UserServer.Interfaces;
using UserServer.Models;
using UserServer.Repositories;

namespace UserServer.Services
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public string exportUsersFileName = $"felhasznalok-{DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss", CultureInfo.InvariantCulture)}.xlsx";
        public string exportUsersFileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public async Task<Models.PagedResult<UserDto>> GetPagedUsers(int page, int perPage, string sort)
        {
            return await _userRepository.GetPagedUsers(page, perPage, sort);
        }

        public async Task<UserDto?> GetUserById(Guid id)
        {
            return await _userRepository.GetUserDtoById(id);
        }

        public async Task<UserDto?> GetUserByEmail(string email)
        {
            return await _userRepository.GetUserDtoByEmail(email);
        }

        public async Task<User> CreateUser(CreateUserRequest request)
        {
            return await _userRepository.CreateUser(request);
        }

        public async Task<User?> UpdateUser(Guid id, UpdateUserRequest request)
        {
            return await _userRepository.UpdateUser(id, request);
        }

        public async Task<bool> DeleteUser(Guid id)
        {
            return await _userRepository.DeleteUser(id);
        }

        public async Task<byte[]> ExportUsersToExcel(int page, int perPage, string sort)
        {
            // Fetching the users list without email filtering
            var pagedResult = await GetPagedUsers(page, perPage, sort);
            var users = pagedResult.Data;

            using var package = new ExcelPackage();
            {
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
                return package.GetAsByteArray();
            }
        }
    }
}
