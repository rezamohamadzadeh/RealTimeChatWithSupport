using System.ComponentModel.DataAnnotations;

namespace RealTimeChatWithSupport.Models
{
    public class Answer : BaseEntity
    {
        [Required]
        public string FirstOption { get; set; }

        [Required]
        public string SecondOption { get; set; }

        [Required]
        public string ThirdOption { get; set; }

        [Required]
        public string FourthOption { get; set; }

        public AnswerOptions TrueAnswer { get; set; }

        public string UserId { get; set; }

    }

    public enum AnswerOptions
    {
        FirstOption = 1,
        SecondOption = 2,
        ThirdOption = 3,
        FourthOption = 4,

    }
}
