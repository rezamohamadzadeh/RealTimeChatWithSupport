using System;

namespace RealTimeChatWithSupport.Models
{
    public class ChatRoom
    {
        public ChatRoom()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }

        public string OwnerConnectionId { get; set; }

        public string Name { get; set; }
        public string UserId { get; set; }

    }
}
