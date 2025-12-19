using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Entitys
{
    public class Coach
    {
        public int ID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        public int YearsOfExperience { get; set; }
        public decimal? Rating { get; set; } // 0.0 to 5.0
        public string About { get; set; }
        
       
        //public string PhotoUrl { get; set; }

        public decimal? Price { get; set; } // Hourly rate

        public DateTime HireDate { get; set; } = DateTime.Now;


        public virtual User User { get; set; }


    }
}
