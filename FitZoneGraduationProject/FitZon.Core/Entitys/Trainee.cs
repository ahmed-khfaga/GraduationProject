using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Command;
using FitZone.Core.Entitys.Identity;

namespace FitZone.Core.Entitys
{
    public class Trainee : BaseEntity
    {
        public string ApplicationUserId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } // "Male", "Female"

        public decimal? Weight { get; set; } // in kg
        public decimal? Height { get; set; } // in cm
        public string? Address { get; set; } // cairo ,alex ..etc..
        public string? PhotoUrl { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<TraineeMembership> TraineeMemberships { get; set; } = new HashSet<TraineeMembership>();
        public virtual ICollection<TraineeProgramTemplate> TraineeProgramTemplates { get; set; } = new HashSet<TraineeProgramTemplate>();

    }
}
