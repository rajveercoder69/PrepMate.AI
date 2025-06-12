using WebApiCRUDOps.DataUtility.DataRepository.IDataRepository;
using WebApiCRUDOps.DataUtility.Model;

namespace WebApiCRUDOps.DataUtility.DataRepository
{
    public class LearningPdfsRepository:DataRepository<LearningPdf>,ILearningPdfsRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public LearningPdfsRepository(ApplicationDbContext db) : base(db)
        {
            _dbContext = db;
        }
        public void update(LearningPdf data)
        {
            _dbContext.Update(data);
        }
    }
}
