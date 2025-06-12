using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace WebApiCRUDOps.DataUtility.Model
{
    public class Person
    {
        [Key]
        [Required]
        public int personId { get; set; }
        public string firstName { get; set; }
        [Required]
        public string? lastName { get; set; }
        [UserDefinedValidateioncs]
        [Required]
        public string gender { get; set; }
        [Required]
        public double? price { get; set; }
        [UserDefinedValidateioncs]
        public int size { get; set; }

        // Navigation property
       // public ICollection<PdfUpload>? PdfUploads { get; set; } = new List<PdfUpload>();
    }
}
