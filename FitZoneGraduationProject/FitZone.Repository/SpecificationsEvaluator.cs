using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Command;
using FitZone.Core.Specifications;
using Microsoft.EntityFrameworkCore;

namespace FitZone.Repository
{
    internal static class SpecificationsEvaluator<TEntity> where TEntity : BaseEntity
    {

        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecifications<TEntity> spec)
        {
            var query = inputQuery; // dataset => DbContext

            if (spec.Criteria is not null) 
            {
                query = query.Where(spec.Criteria);
            }

            // if no where filliter 

            query = spec.Includes.Aggregate(query, (currentQuery, includeExpression) => currentQuery.Include(includeExpression));


            return query;
        }
    }
}
