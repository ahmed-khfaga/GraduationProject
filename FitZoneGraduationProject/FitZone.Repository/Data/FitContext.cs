using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FitZone.Repository.Data
{
    public class FitContext : DbContext
    {
        public FitContext(DbContextOptions<FitContext> option):base(option)
        {
            
        }




    }
}
