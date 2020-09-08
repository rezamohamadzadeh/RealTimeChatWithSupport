namespace RealTimeChatWithSupport.Models
{
    public class ChatRoom : BaseEntity
    {
        
        public string OwnerConnectionId { get; set; }

        public string Name { get; set; }

        public string UserId { get; set; }

    }
}
