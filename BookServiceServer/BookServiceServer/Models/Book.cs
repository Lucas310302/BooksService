using System.ComponentModel.DataAnnotations;

namespace BookServiceServer.Models
{
    public class Book
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        public string Title { get; set; }
        
        public string Author { get; set; }
        public bool? IsAvailable { get; set; }
        public byte[] PDFfile { get; set; }
    }
}
