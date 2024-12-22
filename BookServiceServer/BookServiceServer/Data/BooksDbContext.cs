using BookServiceServer.Models;
using Microsoft.EntityFrameworkCore;

namespace BookServiceServer.Data
{
    public class BooksDbContext : DbContext
    {
        public BooksDbContext(DbContextOptions<BooksDbContext> options) : base (options) { }

        public DbSet<Book> MyBooks { get; set; } // Represents the book table
    }
}
