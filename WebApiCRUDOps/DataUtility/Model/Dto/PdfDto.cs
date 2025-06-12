namespace WebApiCRUDOps.DataUtility.Model.Dto
{
    public class PdfDto
    {
        public int PersonId { get; set; }
        public string Email { get; set; } = null!;
        public IFormFile PdfFile { get; set; } = null!;
        public string CompressionLevel { get; set; } = "Moderate";
    }
}
