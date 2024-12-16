using BookServiceServer.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BookServiceServer.Services
{
    public class BooksService
    {
        private readonly List<Book> _books = new();

        public IEnumerable<Book> GetBooks() => _books;

        public void AddBook(Book book) => _books.Add(book);

        // Method for searching books
        public IEnumerable<Book> SearchBooks(string query, int tolerance = 3)
        {
            // Handle empty or null string
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<Book>();

            var normalizedQuery = query.ToLower();

            // Fuzzy
            return _books
                .Where(b=>
                    
                )
        }
    }
}
