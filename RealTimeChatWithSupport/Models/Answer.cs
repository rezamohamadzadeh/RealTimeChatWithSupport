using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealTimeChatWithSupport.Models
{
    public class Question : BaseEntity
    {
        public string QuestionTitle { get; set; }

        [Required]
        public string FirstOption { get; set; }

        [Required]
        public string SecondOption { get; set; }

        [Required]
        public string ThirdOption { get; set; }

        [Required]
        public string FourthOption { get; set; }


        public AnswerOptions TrueAnswer { get; set; }

    }

}
