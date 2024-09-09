using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using UserServer.Data;
using UserServer.DTOs;
using UserServer.Interfaces;
using UserServer.Models;

namespace UserServer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Models.PagedResult<UserDto>> GetPagedUsers(int page, int perPage, string sort)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                if (!string.IsNullOrEmpty(sort))
                {
                    bool isDescending = sort.StartsWith('-');
                    string sortProperty = isDescending ? sort.Substring(1) : sort;
                    string orderByStr = $"{sortProperty} {(isDescending ? "descending" : "ascending")}";
                    query = query.OrderBy(orderByStr);
                }

                page = page <= 0 ? 1 : page;
                perPage = perPage <= 0 ? 10 : perPage;

                int skip = (page - 1) * perPage;
                var pagedUsers = await query.Skip(skip).Take(perPage).ToListAsync();
                int totalItems = await query.CountAsync();
                int totalPages = (int)Math.Ceiling((double)totalItems / perPage);

                return new Models.PagedResult<UserDto>()
                {
                    First = 1,
                    Prev = page > 1 ? page - 1 : null,
                    Next = page < totalPages ? page + 1 : null,
                    Last = totalPages,
                    Pages = totalPages,
                    Items = totalItems,
                    Data = pagedUsers.Select(user => _mapper.Map<UserDto>(user)).ToList(),
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error getting paged users", ex);
            }
        }

        public async Task<UserDto?> GetUserDtoById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetUserDtoByEmail(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                return user == null ? null : _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error getting user by email", ex);
            }
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error getting user by email", ex);
            }
        }

        public async Task<User> CreateUser(CreateUserRequest request)
        {
            try
            {
                var user = _mapper.Map<User>(request);
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

                user.Id = Guid.NewGuid();
                user.Password = hashedPassword;

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error creating user", ex);
            }
        }

        public async Task<User?> UpdateUser(Guid id, UpdateUserRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                    return null;

                user.Name = request.Name;
                user.Email = request.Email;
                user.Age = request.Age;
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

                _context.Users.Update(user);

                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error updating user", ex);
            }
        }

        public async Task<bool> DeleteUser(Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                    return false;

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error deleting user", ex);
            }
        }
    }
}
