using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;
using FitZone.Core.Enums;

namespace FitZone.Core.Entitys
{
    public class TraineeProgramEnrollment : BaseEntity
    {
        public int TraineeID { get; set; }
        public int WorkoutProgramID { get; set; }
        public int TrackID { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

        public int CurrentWeekNumber { get; set; } = 1;
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual Trainee Trainee { get; set; }
        public virtual WorkoutProgram WorkoutProgram { get; set; }

        public virtual Track Track { get; set; }

    }
}
