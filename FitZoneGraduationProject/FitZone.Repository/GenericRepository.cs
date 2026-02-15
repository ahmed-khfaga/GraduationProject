using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;
using FitZone.Core.Repository.Contract;
using FitZone.Core.Specifications;
using FitZone.Repository.Data;
using Microsoft.EntityFrameworkCore;

namespace FitZone.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
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


        public async Task<IEnumerable<T>> GetAllWithSpecAsync(ISpecifications<T> spec)
        {
            return await SpecificationHelp(spec).ToListAsync();
        }


        public async Task<T?> GetWithSpecAsync(ISpecifications<T> spec)
        {
            return await SpecificationHelp(spec).FirstOrDefaultAsync();
        }



        // private helper method 

        private IQueryable<T> SpecificationHelp(ISpecifications<T> spec)
        {
            return SpecificationsEvaluator<T>.GetQuery(_context.Set<T>(), spec);
        }
    }
}
