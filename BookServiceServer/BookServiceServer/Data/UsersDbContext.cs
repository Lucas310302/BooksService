using BookServiceServer.Models;
using Microsoft.EntityFrameworkCore;

namespace BookServiceServer.Data
{
    public class UsersDbContext : DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } // Represents users table
    }
}
