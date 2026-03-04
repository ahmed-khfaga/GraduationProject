using System.ComponentModel.DataAnnotations;

namespace FitZone.Service.DTOs
{
    public class LoginUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required] 
        public string Password { get; set; }
    }
}
