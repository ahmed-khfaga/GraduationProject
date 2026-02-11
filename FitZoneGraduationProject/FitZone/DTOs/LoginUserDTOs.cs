using System.ComponentModel.DataAnnotations;

namespace FitZone.APIs.DTOs
{
    public class LoginUserDTOs
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required] 
        public string Password { get; set; }
    }
}
