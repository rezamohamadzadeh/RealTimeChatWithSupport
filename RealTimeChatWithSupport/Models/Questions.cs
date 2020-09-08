using System.ComponentModel.DataAnnotations;

namespace RealTimeChatWithSupport.Models
{
    public class Question : BaseEntity
    {
        [Required]
        public string Title { get; set; }
    }
}
