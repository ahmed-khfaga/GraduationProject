using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


    }
}
