using UserServer.Data;
using UserServer.DTOs;
using UserServer.Models;
using System.Linq.Dynamic.Core;

namespace UserServer.Services
{
    public class UsersService(ApplicationDbContext context)
    {
        private readonly ApplicationDbContext _context = context;

        /// <summary>
        /// Get paginated and sorted list of users.
        /// </summary>
        public Models.PagedResult<UserDto> GetUsers(int page = 1, int perPage = 10, string sort = "name")
        {
            IQueryable<User> query = _context.Users.AsQueryable();

            // Dynamic sorting
            if (!string.IsNullOrEmpty(sort))
            {
                bool isDescending = sort.StartsWith('-');
                string sortProperty = isDescending ? sort.Substring(1) : sort;
                string orderByStr = $"{sortProperty} {(isDescending ? "descending" : "ascending")}";
                query = query.OrderBy(orderByStr);
            }

            int skip = (page - 1) * perPage;
            List<User> pagedUsers = [.. query.Skip(skip).Take(perPage)];

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / perPage);

            Models.PagedResult<UserDto> result = new()
            {
                First = 1,
                Prev = page > 1 ? page - 1 : null,
                Next = page < totalPages ? page + 1 : null,
                Last = totalPages,
                Pages = totalPages,
                Items = totalItems,
                Data = pagedUsers.Select(user => new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Age = user.Age
                }).ToList()
            };

            return result;
        }

        /// <summary>
        /// Get a user by ID.
        /// </summary>
        public UserDto? GetUserById(string id)
        {
            if (Guid.TryParse(id, out Guid guidId))
            {
                User? user = _context.Users.FirstOrDefault(u => u.Id == guidId);

                if (user != null)
                {
                    return new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Age = user.Age
                    };
                }
            }
            return null;
        }

        /// <summary>
        /// Get a user by email.
        /// </summary>
        public UserDto? GetUserByEmail(string email)
        {
            User? user = _context.Users.SingleOrDefault(u => u.Email == email);

            return user == null ? null : new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Age = user.Age
            };
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        public User CreateUser(CreateUserRequest request)
        {
            // Check if email already exists
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                throw new InvalidOperationException("Email already exists.");
            }

            // Hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            User user = new()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Age = request.Age,
                Password = hashedPassword
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        /// <summary>
        /// Update an existing user.
        /// </summary>
        public User? UpdateUser(string id, UpdateUserRequest request)
        {
            if (!Guid.TryParse(id, out var guidId)) return null;

            User? user = _context.Users.SingleOrDefault(u => u.Id == guidId);

            if (user == null) return null;

            // Check if email already exists for another user
            if (_context.Users.Any(u => u.Email == request.Email && u.Id != guidId))
            {
                throw new InvalidOperationException("Email already exists.");
            }

            user.Name = request.Name;
            user.Email = request.Email;
            user.Age = request.Age;
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            _context.Users.Update(user);
            _context.SaveChanges();

            return user;
        }

        /// <summary>
        /// Delete a user by ID.
        /// </summary>
        public bool DeleteUser(string id)
        {
            if (!Guid.TryParse(id, out var guidId)) return false;

            User? user = _context.Users.SingleOrDefault(u => u.Id == guidId);

            if (user == null) return false;

            _context.Users.Remove(user);
            _context.SaveChanges();

            return true;
        }
    }
}
