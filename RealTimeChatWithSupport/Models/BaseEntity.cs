using System;
using System.ComponentModel.DataAnnotations;

namespace RealTimeChatWithSupport.Models
{
    public abstract class BaseEntity
    {
        public BaseEntity()
        {
            Id = Guid.NewGuid();
            DateTime = DateTimeOffset.Now;
        }

        [Key]
        public Guid Id { get; set; }

        public DateTimeOffset DateTime { get; set; }
    }
}
