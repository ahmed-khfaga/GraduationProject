using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.ProfileDTOs
{
    public class UpdateCoachProfileDto
    {
        public string About { get; set; }
        public int YearsOfExperience { get; set; }
        public decimal? Price { get; set; }
    }
}
