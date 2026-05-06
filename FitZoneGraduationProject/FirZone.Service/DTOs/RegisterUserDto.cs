using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FitZone.Service.DTOs
{
    public class RegisterUserDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }


        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        public IFormFile? Photo { get; set; }
    }
}
