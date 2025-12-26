using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Enums;

namespace FitZone.Core.Entitys
{
    public class User
    {
        public int ID { get; set; }
        public string F_Name { get; set; }
        public string L_Name { get; set; }

        [NotMapped]
        public string FullName => $"{F_Name} {L_Name}"; // mohamed omer 

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        // "Trainee", "Coach", "Admin"
        // 0 , 1, 2

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public virtual Trainee Trainee { get; set; }
        public virtual Coach Coach { get; set; }

    }
}
