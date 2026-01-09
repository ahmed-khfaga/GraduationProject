using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Command;

namespace FitZone.Core.Entitys
{
    public class BaseProgram : BaseEntity
    {

        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<ProgramTemplate> ProgramTemplates { get; set; } = new HashSet<ProgramTemplate>();


        #region special Role
        //public bool IsPremium { get; set; } // we can use it if we want to do special program for trainee 
        // 4 program 
        // 3 standard 
        // 1 premium 

        #endregion



    }
}
