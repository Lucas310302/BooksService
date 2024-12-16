using BookServiceServer.Models;

namespace BookServiceServer.Services
{
    public class BooksService
    {
        private readonly List<Book> _books = new();

        public IEnumerable<Book> GetBooks() => _books;

        public void AddBook(Book book) => _books.Add(book);
    }
}
