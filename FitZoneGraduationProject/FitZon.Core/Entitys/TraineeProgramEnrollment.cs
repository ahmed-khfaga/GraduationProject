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
        public int TraineeId { get; set; }
        public int WorkoutProgramId { get; set; }
        public int TrackId { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

        /// The highest week number the trainee currently has access to. Computed from StartDate on read and persisted
        public int MaxWeekUnlocked { get; set; } = 1;
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual Trainee Trainee { get; set; }
        public virtual WorkoutProgram WorkoutProgram { get; set; }

        public virtual Track Track { get; set; }

    }
}
