using System.Collections.Generic;

namespace RealTimeChatWithSupport.Dtos
{
    public class UserDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }
}
