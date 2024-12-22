using BookServiceServer.Models;
using BookServiceServer.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FuzzySharp;

namespace BookServiceServer.Services
{
    public class BooksService
    {
        private readonly BooksDbContext _context;

        public BooksService(BooksDbContext context)
        {
            _context = context;
        }

        // Get books
        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            return await _context.MyBooks.ToListAsync();
        }

        // Add new book
        public async Task AddBookAsync(Book book)
        {
            await _context.MyBooks.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        // Search books
        public async Task<IEnumerable<Book>> SearchBooksAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Enumerable.Empty<Book>();
            }

            var books = await _context.MyBooks.ToListAsync();

            return books
                .Where(b =>
                    // Partial word matching
                    b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    b.Author.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    // Fuzzy match, for misspelling
                    Fuzz.Ratio(b.Title, query) >= 80 ||
                    Fuzz.Ratio(b.Author, query) >= 80);
        }
    }
}
