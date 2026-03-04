using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.ProfileDTOs
{
    public class TraineeProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhotoUrl { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public string? Address { get; set; }
    }
}
