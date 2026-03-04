using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.DTOs.EnrollmentDTOs
{
    public class AdminReviewDto
    {
        public bool Approve { get; set; }
        public string? RejectionNote { get; set; }
    }
}
