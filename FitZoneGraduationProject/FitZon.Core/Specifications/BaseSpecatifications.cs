using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;

namespace FitZone.Core.Specifications
{
    public class BaseSpecatifications<T> : ISpecifications<T> where T : BaseEntity
    {
        public Expression<Func<T, bool>> Criteria { get; set; } = null;

        // Lambda-based single-level includes: w => w.Coach
        public List<Expression<Func<T, object>>> Includes { get; set; } = new();

        // String-based includes for nested navigation: "Coach.ApplicationUser"
        public List<string> IncludeStrings { get; set; } = new();

        public Expression<Func<T, object>>? OrderBy { get; set; }
        public Expression<Func<T, object>>? OrderByDescending { get; set; }

        public int? Take { get; set; }
        public int? Skip { get; set; }
        public bool IsPaginationEnabled { get; set; }

        public BaseSpecatifications()
        {
            // no filter — returns all rows
        }

        public BaseSpecatifications(Expression<Func<T, bool>> criteriaExpression)
        {
            Criteria = criteriaExpression;
        }

        protected void ApplyPagination(int pageIndex, int pageSize)
        {
            IsPaginationEnabled = true;
            Skip = (pageIndex - 1) * pageSize;
            Take = pageSize;
        }
    }
}

