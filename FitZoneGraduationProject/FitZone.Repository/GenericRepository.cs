using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Repository.Contract;
using FitZone.Repository.Data;
using Microsoft.EntityFrameworkCore;

namespace FitZone.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly FitContext _context;

        public GenericRepository(FitContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T?> GetAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
    }
}
