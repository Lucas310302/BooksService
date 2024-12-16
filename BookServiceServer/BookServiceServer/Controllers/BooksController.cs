using BookServiceServer.Models;
using BookServiceServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookServiceServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BooksService _bookService;

    public BooksController(BooksService bookService)
    {
        _bookService = bookService;

        _bookService.AddBook(new Book { Id = 1, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", IsAvailable = true });
        _bookService.AddBook(new Book { Id = 2, Title = "1984", Author = "George Orwell", IsAvailable = false });
    }

    [HttpGet]
    public IActionResult GetBooks()
    {
        return Ok(_bookService.GetBooks());
    }

    [HttpPost]
    public IActionResult AddBook([FromBody] Book book)
    {
        _bookService.AddBook(book);
        return CreatedAtAction(nameof(GetBooks), new { id = book.Id }, book);
    }
}
