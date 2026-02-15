using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;

namespace FitZone.Core.Entitys
{
    public class ProgramTemplate : BaseEntity
    {


        [ForeignKey("BaseProgramID")]
        public int BaseProgramID { get; set; }

        [ForeignKey("CoachID")]
        public int CoachID { get; set; }


        public string Name { get; set; }

        public string Description { get; set; }

        public virtual BaseProgram BaseProgram { get; set; } // push pull leg 

        public virtual Coach Coach { get; set; }

        public virtual ICollection<ProgramDays> ProgramDays { get; set; }= new HashSet<ProgramDays>();

        public virtual ICollection<TraineeProgramTemplate> TraineeProgramTemplates { get; set; } = new HashSet<TraineeProgramTemplate>();
    }
}
