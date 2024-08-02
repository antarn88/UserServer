using Microsoft.EntityFrameworkCore;
using UserServer.Models;

namespace UserServer.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } // A User modell DbSet-je
    }
}
