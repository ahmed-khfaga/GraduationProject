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
    public class WorkoutProgram : BaseEntity
    {


        public int TrackId { get; set; }

        public int CoachId { get; set; }


        public string Name { get; set; }

        public string Description { get; set; }


        // What the trainee will achieve by finishing this program. 
        public string? ExpectedOutcome { get; set; }

        /// What the trainee should do or move on to after completing this program.
        public string? NextSteps { get; set; }
        public int DurationOnWeeks { get; set; }

        public int SessionsPerWeeks { get; set; }

        public int SessionsDuration { get; set; }

        public string? PhotoThumbnailUrl { get; set; }

        public TrainingGoal TrainingGoal { get; set; }

        public FitnessLevel FitnessLevel { get; set; }

        public EquipmentType EquipmentType { get; set; }

        // True = visible in the public catalogue and enrollable by trainees,  Coach flips this themselves — no admin approval needed.
        public bool IsPublished { get; set; } = false;

        // Soft-delete flag. Set true by AdminDeleteProgramAsync when the program has
        // active/historical enrollments and a hard delete would violate the FK
        // (TraineeProgramEnrollment never cascade-deletes). Trainees already enrolled
        // keep full access; the program disappears from the public catalogue and the
        // coach's own list.
        public bool IsDeleted { get; set; } = false;

        public DateTime? PublishedAt { get; set; }

        public virtual Track Track { get; set; } // push pull leg 

        public virtual Coach Coach { get; set; }

        public virtual ICollection<ProgramWeek> ProgramWeeks { get; set; } = new HashSet<ProgramWeek>();

        public virtual ICollection<TraineeProgramEnrollment> TraineeProgramEnrollments { get; set; } = new HashSet<TraineeProgramEnrollment>();
    }
}