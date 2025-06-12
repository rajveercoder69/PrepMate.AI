using System.ComponentModel.DataAnnotations;

namespace WebApiCRUDOps.DataUtility.Model
{
    public class LearningPdf
    {
        [Key]
        public int Id {  get; set; }
        public string CategoryName { get; set; }
        // Navigation property
        public Person Person { get; set; } = null!;
        public string Title { get; set; }
        public int PersonId { get; set; }
        public string? Description { get; set; } 
        public DateTime CreateDateTime { get; set; }= DateTime.Now;
        public byte[] Pdf {  get; set; }
    }
}
