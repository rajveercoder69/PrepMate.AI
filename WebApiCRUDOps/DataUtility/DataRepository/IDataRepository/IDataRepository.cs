using System.Linq.Expressions;
namespace WebApiCRUDOps.DataUtility.DataRepository.IDataRepository
{
    public interface IDataRepository<T> where T : class
    {
        IEnumerable<T> GetAll(string? includeProperties=null);
        T GetFirstOrDefault(Expression<Func<T,bool>>filter ,string? includeProperties=null);
        void Add(T entity);
        void Remove(T entity);
        T GetSingle(Expression<Func<T, bool>> predicate);

    }
}
