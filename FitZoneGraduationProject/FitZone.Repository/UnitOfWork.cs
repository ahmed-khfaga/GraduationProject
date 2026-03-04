using FitZone.Core.Comman;
using FitZone.Core.Repository.Contract;
using FitZone.Repository.Data;
using System.Collections;


namespace FitZone.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FitContext _context;
        private readonly Hashtable _repositories;

        public UnitOfWork(FitContext context)
        {
            _context = context;
            _repositories = new Hashtable();
        }

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            var typeName = typeof(T).Name;

            if (!_repositories.ContainsKey(typeName))
                _repositories[typeName] = new GenericRepository<T>(_context);

            return (IGenericRepository<T>)_repositories[typeName]!;
        }

        public async Task<int> CompleteAsync()
            => await _context.SaveChangesAsync();

        public void Dispose()
            => _context.Dispose();
    }
}
