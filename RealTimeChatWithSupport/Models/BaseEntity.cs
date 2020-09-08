using System;
using System.ComponentModel.DataAnnotations;

namespace RealTimeChatWithSupport.Models
{
    public abstract class BaseEntity
    {
        public BaseEntity()
        {
            Id = Guid.NewGuid();
            DateTime = DateTime.Now;
        }

        [Key]
        public Guid Id { get; set; }

        public DateTime DateTime { get; set; }
    }
}
