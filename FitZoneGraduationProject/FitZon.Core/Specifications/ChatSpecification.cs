using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Entitys.Chat;

namespace FitZone.Core.Specifications
{
    public class ChatSpecification : BaseSpecatifications<ChatMessage>
    {
        public ChatSpecification(string user1, string user2)
        : base(m =>
            (m.SenderId == user1 && m.ReceiverId == user2) ||
            (m.SenderId == user2 && m.ReceiverId == user1))
        {
            Includes.Add(m => m.Sender);
            Includes.Add(m => m.Receiver);
        }
    }
}
