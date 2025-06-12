using WebApiCRUDOps.DataUtility.DataRepository.IDataRepository;
using WebApiCRUDOps.DataUtility.Model;

namespace WebApiCRUDOps.DataUtility.DataRepository
{
   
    public class PdfUploadRepository : DataRepository<PdfUpload>, IPdfUploadRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public PdfUploadRepository(ApplicationDbContext db) : base(db)
        {
            _dbContext=db;
        }
        public void update(PdfUpload pdfUpload)
        {
            _dbContext.SaveChanges();
        }
    }
}
