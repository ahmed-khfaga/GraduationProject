using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Command;
using FitZone.Core.Enums;

namespace FitZone.Core.Entitys
{
    public class ProgramDays : BaseEntity
    {

        [ForeignKey("ProgramTemplateID")]
        public int ProgramTemplateID { get; set; }

        public WeekDay Day { get; set; }
        
        public DifficultyLevel Difficulty { get; set; }

        public string Focus { get; set; } // Push, Pull, Legs, Cardio, Recovery


        public virtual ProgramTemplate ProgramTemplate { get; set; }

    }
}
