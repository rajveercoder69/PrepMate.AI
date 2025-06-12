using WebApiCRUDOps.DataUtility.DataRepository.IDataRepository;

namespace WebApiCRUDOps.DataUtility.DataRepository
{
    public class SaveChange:ISaveChange
    {
        private ApplicationDbContext _db;
        public IPersonProduct personproduct { get; private set; }
        public IPdfUploadRepository pdfUploadRepository { get; private set; }
        public ILearningPdfsRepository learningPdfsRepository { get; private set; }
        public SaveChange(ApplicationDbContext db)
        {
            _db = db;
            personproduct = new PersonProduct(db);
            pdfUploadRepository = new PdfUploadRepository(db);
            learningPdfsRepository = new LearningPdfsRepository(db);
        }
        public void save()
        {
            _db.SaveChanges();
        }
    }
}
