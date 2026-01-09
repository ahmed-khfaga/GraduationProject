using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Command;

namespace FitZone.Core.Entitys
{
    public class TraineeProgramTemplate : BaseEntity
    {

        [ForeignKey("TraineeID")]
        public int TraineeID { get; set; }

        [ForeignKey("ProgramTemplateID")]
        public int ProgramTemplateID { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual Trainee Trainee { get; set; }
        public virtual ProgramTemplate ProgramTemplate { get; set; }
    }
}
