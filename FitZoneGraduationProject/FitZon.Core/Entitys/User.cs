using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Entitys
{
    public class User
    {
        public int ID { get; set; }
        public string F_Name { get; set; }
        public string L_Name { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // "Trainee", "Coach", "Admin"

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public virtual Trainee Trainee { get; set; }
        public virtual Coach Coach { get; set; }

    }
}
