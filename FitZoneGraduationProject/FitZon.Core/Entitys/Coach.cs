using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;
using FitZone.Core.Entitys.Identity;

namespace FitZone.Core.Entitys
{
    public class Coach : BaseEntity
    {

        public string ApplicationUserId { get; set; }

        public int YearsOfExperience { get; set; }
        public decimal? Rating { get; set; } // 0.0 to 5.0
        public string About { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal? Price { get; set; } // Hourly rate

        public DateTime HireDate { get; set; } = DateTime.Now;
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<WorkoutProgram> WorkoutPrograms { get; set; } = new HashSet<WorkoutProgram>();

        // Each coach owns their own exercise library
        public virtual ICollection<Exercise> Exercises { get; set; } = new HashSet<Exercise>();

    }
}
