using System.ComponentModel.DataAnnotations;

namespace WebApiCRUDOps.DataUtility.Model.Dto
{
    public class PersonDto
    {
        public int personId { get; set; }
        public string firstName { get; set; }

        public string? lastName { get; set; }


        public string gender { get; set; }

        public double price { get; set; }

        public int size { get; set; }

        // Navigation property
        //public ICollection<PdfUpload> PdfUploads { get; set; } = new List<PdfUpload>();
    }
}
