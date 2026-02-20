using FitZone.Core.Comman;


namespace FitZone.Core.Repository.Contract
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : BaseEntity;

        Task<int> CompleteAsync();
    }
}
