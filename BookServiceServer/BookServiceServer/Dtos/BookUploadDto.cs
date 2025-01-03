using System.ComponentModel.DataAnnotations;

namespace BookServiceServer.Dtos
{
    public class BookUploadDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Author { get; set; }
        public bool IsAvailable { get; set; }
        public IFormFile? pdfFile { get; set; }
    }
}
