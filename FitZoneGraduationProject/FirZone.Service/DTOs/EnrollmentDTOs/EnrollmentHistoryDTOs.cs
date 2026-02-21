using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.EnrollmentDTOs
{
   
    /// Returned on the history page — includes inactive enrollments so the trainee
    /// can see everything they've ever been enrolled in and re-enroll from saved progress.
    
    public class EnrollmentHistoryDto : EnrollmentDTOs
    {
        public bool IsActive { get; set; }
    }
}
