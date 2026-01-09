using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Command;

namespace FitZone.Core.Specifications
{
    public interface ISpecifications<T> where T : BaseEntity
    {
        public Expression<Func<T,bool>> Criteria {  get; set; } // for where () filter 
        public List<Expression<Func<T,object>>> Includes { get; set; } // should get list 

    }
}
