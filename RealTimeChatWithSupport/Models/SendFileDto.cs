using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RealTimeChatWithSupport.Models
{
    public class SendFileDto
    {
        [Required]
        public string RoomId { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }
}
