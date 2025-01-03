using BookServiceServer.Dtos;
using BookServiceServer.Models;
using BookServiceServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Text.Json;

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
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetBooks()
    {
        var books = await _bookService.GetBooksAsync();
        return Ok(books);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Route("add")]
    public async Task<IActionResult> AddBook([FromForm] BookUploadDto bookUploadDto)
    {
        if (bookUploadDto == null)
            return BadRequest("Book data cannot be null");

        if (bookUploadDto.pdfFile == null || bookUploadDto.pdfFile.Length == 0)
            return BadRequest("PDF file cannot be null or empty");

        var book = new Book
        {
            Title = bookUploadDto.Title,
            Author = bookUploadDto.Author,
            IsAvailable = bookUploadDto.IsAvailable,
        };

        // Adding pdf file to book
        using (var memorystream = new MemoryStream())
        {
            await bookUploadDto.pdfFile.CopyToAsync(memorystream);
            book.PDFfile = memorystream.ToArray();
        }

        // Save to database
        await _bookService.AddBookAsync(book);
        return CreatedAtAction(nameof(GetBooks), new { id = book.Id }, book);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    [Route("delete")]
    public async Task<IActionResult> DeleteBooks([FromQuery] List<int> ids)
    {
        // Check if ids exist
        if (ids == null || !ids.Any())
            BadRequest("No Ids Provided!");

        foreach (var id in ids)
        {
            await _bookService.RemoveBookAsync(id);
        }

        return Ok($"Deleted {ids.Count} files successfully!");
    }

    [HttpPatch]
    [Authorize(Roles = "Admin")]
    [Route("modify/{id}")]
    public async Task<IActionResult> ModifyBook(int id, [FromForm] BookUpdateDto bookUpdateDto)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
        {
            return NotFound($"Book with ID {id} not found.");
        }

        if (bookUpdateDto.Title == null)
        {
            return BadRequest("Title cannot be null");
        }

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(bookUpdateDto.Title))
        {
            book.Title = bookUpdateDto.Title;
        }

        if (!string.IsNullOrWhiteSpace(bookUpdateDto.Author))
        {
            book.Author = bookUpdateDto.Author;
        }

        if (bookUpdateDto.IsAvailable.HasValue)
        {
            book.IsAvailable = bookUpdateDto.IsAvailable.Value;
        }

        if (bookUpdateDto.PDFfile != null)
        {
            using var memorystream = new MemoryStream();
            await bookUpdateDto.PDFfile.CopyToAsync(memorystream);
            book.PDFfile = memorystream.ToArray();
        }

        await _bookService.UpdateBookAsync(book);
        return Ok($"Book with ID {id} has been updated.");
    }

    [HttpGet]
    [Authorize(Roles = "Admin,User")]
    [Route("download/{id}")]
    public async Task<IActionResult> DownloadPdf(int id)
    {
        // Check Ids
        if (id == null)
            BadRequest("No id provided.");

        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
        {
            return NotFound($"Book with ID {id} not found.");
        }

        if (book.PDFfile == null || book.PDFfile.Length == 0)
        {
            return NotFound("No pdf file found for this book");
        }

        // Serve pdf file
        return File(book.PDFfile, "application/pdf", $"{book.Title}.pdf");
    }

    [HttpGet]
    [Authorize(Roles = "Admin,User")]
    [Route("search")]
    public async Task<IActionResult> SearchBooks([FromQuery] string query)
    {
        var books = await _bookService.SearchBooksAsync(query);

        if (!books.Any())
            return NotFound("No Books matched search :(");

        return Ok(books);
    }
}
