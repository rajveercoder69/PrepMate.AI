namespace WebApiCRUDOps.DataUtility.Model.Dto
{
    public class LearningPdfDto
    {
        public string CategoryName { get; set; }
        public int PersonId { get; set; }
        public string? Description { get; set; }
        public string Title {  get; set; }
        public IFormFile PdfFile { get; set; } = null!;
    }
}
