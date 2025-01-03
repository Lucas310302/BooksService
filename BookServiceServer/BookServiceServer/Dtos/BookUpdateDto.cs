namespace BookServiceServer.Dtos
{
    public class BookUpdateDto
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public bool? IsAvailable { get; set; }
        public IFormFile? PDFfile { get; set; }
    }
}
