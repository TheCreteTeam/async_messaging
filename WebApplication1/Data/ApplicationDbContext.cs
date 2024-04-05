using Microsoft.EntityFrameworkCore;
using AsyncMessagingCommon.Entities;

namespace WebApplication1.Data;

public class ApplicationDbContext: DbContext
{
    public DbSet<AppMessage> AppMessages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("MessageDb");
        optionsBuilder.EnableSensitiveDataLogging(); 
    }
}