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
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        var books = await _bookService.GetBooksAsync();
        return Ok(books);
    }

    [HttpPost]
    [Route("add")]
    public async Task<IActionResult> AddBook([FromBody] Book book, IFormFile pdfFile)
    {
        if (book == null )
            return BadRequest("Book cannot be null");

        // Adding pdf file to book
        using (var memorystream = new MemoryStream())
        {
            await pdfFile.CopyToAsync(memorystream);
            book.PDFfile = memorystream.ToArray();
        }

        // Save to database
        await _bookService.AddBookAsync(book);
        return CreatedAtAction(nameof(GetBooks), new { id = book.Id }, book);
    }

    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> SearchBooks([FromQuery] string query)
    {
        var books = await _bookService.SearchBooksAsync(query);

        if (!books.Any())
            return NotFound("No Books matched search :(");

        return Ok(books);
    }
}
