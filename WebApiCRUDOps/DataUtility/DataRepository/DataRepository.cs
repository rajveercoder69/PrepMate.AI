using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using WebApiCRUDOps.DataUtility.DataRepository.IDataRepository;
using WebApiCRUDOps.DataUtility.Model;

namespace WebApiCRUDOps.DataUtility.DataRepository
{
    public class DataRepository<T> : IDataRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> DbSet;

        public DataRepository(ApplicationDbContext db)
        {
            _db = db;
            this.DbSet = _db.Set<T>();
        }
        public IEnumerable<T> GetAll(string? includeProperties = null)
        {
            IQueryable<T> query = DbSet;
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();

        }
        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            IQueryable<T> query = DbSet;
            query.Where(filter);
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }
        public void Add(T entity)
        {
            _db.Add(entity);
        }
        public void Remove(T entity)
        {
            _db.Remove(entity);
        }
        public T GetSingle(Expression<Func<T, bool>> predicate)
        {
            return DbSet.Single(predicate); // Will throw if not exactly one match
        }
    }

}
