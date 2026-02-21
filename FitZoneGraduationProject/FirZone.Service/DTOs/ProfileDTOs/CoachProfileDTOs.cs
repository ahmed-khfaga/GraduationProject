using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.ProfileDTOs
{
    public class CoachProfileDTOs
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string About { get; set; }
        public int YearsOfExperience { get; set; }
        public decimal? Rating { get; set; }
        public decimal? Price { get; set; }
        public string? PhotoUrl { get; set; }
        public int ProgramCount { get; set; }
    }
}
