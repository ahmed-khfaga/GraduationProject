using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Command;

namespace FitZone.Core.Repository.Contract
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        // get and getAll 

        Task<T?> GetAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
    }
}
