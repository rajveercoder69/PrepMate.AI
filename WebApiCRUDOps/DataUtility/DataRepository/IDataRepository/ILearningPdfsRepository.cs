using WebApiCRUDOps.DataUtility.Model;

namespace WebApiCRUDOps.DataUtility.DataRepository.IDataRepository
{
    public interface ILearningPdfsRepository:IDataRepository<LearningPdf>
    {
         void update(LearningPdf data);
    }
}
