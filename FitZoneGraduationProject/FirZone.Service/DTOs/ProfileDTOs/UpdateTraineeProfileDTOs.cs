using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.ProfileDTOs
{
    public class UpdateTraineeProfileDTOs
    {
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

}
