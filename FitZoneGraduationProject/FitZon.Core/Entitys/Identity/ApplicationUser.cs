using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace FitZone.Core.Entitys.Identity
{
    public class ApplicationUser : IdentityUser
    {

        public string F_Name { get; set; }
        public string L_Name { get; set; }

        [NotMapped]
        public string FullName => $"{F_Name} {L_Name}"; // mohamed omer 
      
        public UserRole Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public virtual Trainee Trainee { get; set; }
        public virtual Coach Coach { get; set; }
    }
}
