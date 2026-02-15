using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Service.HelperAuth
{
    public class AuthResultDto
    {
        public bool IsSuccess { get; set; }
        public string ?Message { get; set; }
        public string? Token { get; set; }

    }
}
