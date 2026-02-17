using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;

namespace FitZone.Core.Entitys
{
    [Table("Tracks")]
    public class Track : BaseEntity
    {

        public string Name { get; set; }

        public string Description { get; set; }
        
        public virtual ICollection<WorkoutProgram> WorkoutPrograms { get; set; } = new HashSet<WorkoutProgram>();


        #region special Role
        //public bool IsPremium { get; set; } // we can use it if we want to do special program for trainee 
        // 4 program 
        // 3 standard 
        // 1 premium 

        #endregion



    }
}
