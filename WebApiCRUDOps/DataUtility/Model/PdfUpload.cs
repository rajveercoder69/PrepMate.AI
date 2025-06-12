using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WebApiCRUDOps.DataUtility.Model

{
    public class PdfUpload
    {
        [Key]
        public int Id { get; set; }

        // Foreign key
        public int PersonId { get; set; }

        // Navigation property
        public Person Person { get; set; } = null!;
        // PDF fields
        public string Email { get; set; } = null!;
        public byte[]? OriginalPdf { get; set; }
        public byte[]? CompressedPdf { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
