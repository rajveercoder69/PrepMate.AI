using WebApiCRUDOps.DataUtility.Model;

namespace WebApiCRUDOps.DataUtility.DataRepository.IDataRepository
{
    public interface IPersonProduct:IDataRepository<Person>
    {
         void update(Person person); 
    }
}
