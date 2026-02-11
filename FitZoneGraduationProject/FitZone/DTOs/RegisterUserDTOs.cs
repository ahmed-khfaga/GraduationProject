using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitZone.APIs.DTOs
{
    public class RegisterUserDTOs
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }


        public string Password { get; set; }

        public IFormFile? Photo { get; set; }
    }
}
