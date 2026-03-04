
using FitZone.Core.Comman;
using FitZone.Core.Specifications;

namespace FitZone.Core.Repository.Contract
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        // get and getAll 

        Task<T?> GetAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();

        Task<IEnumerable<T>> GetAllWithSpecAsync(ISpecifications<T> spec);

        Task<T?> GetWithSpecAsync(ISpecifications<T> spec);

        Task<int> CountAsync(ISpecifications<T> spec);

        // Write operations
        //   nothing goes to the DB until UnitOfWork.CompleteAsync()) ──
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
