using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Comman;
using FitZone.Core.Entitys.Identity;

namespace FitZone.Core.Entitys.Chat
{
    public class ChatMessage : BaseEntity
    {
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Message { get; set; }

        public ApplicationUser Sender { get; set; }

        public ApplicationUser Receiver { get; set; }
        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;


    }
}
