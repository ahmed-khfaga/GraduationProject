using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;

namespace FitZone.Core.Specifications
{
    public interface ISpecifications<T> where T : BaseEntity
    {
        Expression<Func<T, bool>> Criteria { get; set; }

        // Lambda-based includes (single-level: w => w.Coach)
        List<Expression<Func<T, object>>> Includes { get; set; }

        // String-based includes for nested navigation (e.g. "Coach.ApplicationUser")
        List<string> IncludeStrings { get; set; }

        Expression<Func<T, object>>? OrderBy { get; set; }
        Expression<Func<T, object>>? OrderByDescending { get; set; }

        int? Take { get; set; }
        int? Skip { get; set; }
        bool IsPaginationEnabled { get; set; }
    }
}

