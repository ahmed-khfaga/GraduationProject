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


        [ForeignKey("TrackID")]
        public int TrackID { get; set; }

        [ForeignKey("CoachID")]
        public int CoachID { get; set; }


        public string Name { get; set; }

        public string Description { get; set; }

        public int DurationOnWeeks {  get; set; }

        public int SessionsPerWeeks {  get; set; }

        public int SessionsDuration {  get; set; }

        public string? PhotoThumbnailUrl {  get; set; }

        public TrainingGoal  TrainingGoal { get; set; }

        public FitnessLevel FitnessLevel { get; set; }

        public EquipmentType EquipmentType { get; set; }

        // Admin approval workflow
        public ProgramStatus Status { get; set; } = ProgramStatus.Draft;

        public string? RejectionNote { get; set; }

        public DateTime? PublishedAt { get; set; }

        public virtual Track Track { get; set; } // push pull leg 

        public virtual Coach Coach { get; set; }

        public virtual ICollection<ProgramWeek> ProgramWeeks { get; set; }= new HashSet<ProgramWeek>();

        public virtual ICollection<TraineeProgramEnrollment> TraineeProgramEnrollments { get; set; } = new HashSet<TraineeProgramEnrollment>();
    }
}
