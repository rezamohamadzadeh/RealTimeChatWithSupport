using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealTimeChatWithSupport.Models
{
    public class QuestionAnswer : BaseEntity
    {
        [Required]
        public Guid QuestionId { get; set; }

        public AnswerOptions UserAnswer { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public Guid FormId { get; set; }


        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }

    }


    public enum AnswerOptions
    {
        FirstOption = 1,
        SecondOption = 2,
        ThirdOption = 3,
        FourthOption = 4,

    }
}
