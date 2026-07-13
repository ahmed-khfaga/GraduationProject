using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs
{
    public class AuthUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public int? TraineeId { get; set; }
        public int? CoachId { get; set; }

        public bool IsTrainee => Role == "Trainee";
        public bool IsCoach => Role == "Coach";
        public bool IsAdmin => Role == "Admin";
    }
}