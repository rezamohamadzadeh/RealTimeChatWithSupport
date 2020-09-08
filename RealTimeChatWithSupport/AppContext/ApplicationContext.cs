using RealTimeChatWithSupport.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace RealTimeChatWithSupport.AppContext
{
    public class ApplicationContext : DbContext
    {        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json")
                        .Build();

                optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            }
            
        }

        public DbSet<Answer> Answers { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
