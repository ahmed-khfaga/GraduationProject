using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Command;

namespace FitZone.Core.Entitys
{
    public class Trainee : BaseEntity
    {
       
        
        [ForeignKey("User")]
        public int UserID { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } // "Male", "Female"

        public decimal? Weight { get; set; } // in kg
        public decimal? Height { get; set; } // in cm
        public string? Address { get; set; } // cairo ,alex ..etc..


        public virtual User User { get; set; }

        public virtual ICollection<TraineeMembership> TraineeMemberships { get; set; } = new HashSet<TraineeMembership>();
        public virtual ICollection<TraineeProgramTemplate> TraineeProgramTemplates { get; set; } = new HashSet<TraineeProgramTemplate>();

    }
}
