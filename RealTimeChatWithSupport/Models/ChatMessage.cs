using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeChatWithSupport.Models
{
    public class ChatMessage : BaseEntity
    {       
        public string SenderName { get; set; }

        public string Text { get; set; }

        public Guid ChatRoomId { get; set; }

        public virtual ChatRoom ChatRoom { get; set; }
    }
}
