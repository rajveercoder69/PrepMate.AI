using WebApiCRUDOps.DataUtility.DataRepository.IDataRepository;
using WebApiCRUDOps.DataUtility.Model;

namespace WebApiCRUDOps.DataUtility.DataRepository
{
    public class PersonProduct : DataRepository<Person>, IPersonProduct
    {
            private readonly ApplicationDbContext _context;
            public PersonProduct(ApplicationDbContext db) : base(db)
            {
                _context = db;
            }
            public void update(Person person)
            {
                _context.Update(person);
            }
        
    }

}

