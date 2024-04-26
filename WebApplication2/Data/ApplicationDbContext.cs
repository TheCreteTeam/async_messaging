using Microsoft.EntityFrameworkCore;
using AsyncMessagingCommon.Entities;
using MassTransit;

namespace WebApplication2.Data;

public class ApplicationDbContext: DbContext
{
    public DbSet<AppMessage>? AppMessages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // In memory db
        // optionsBuilder.UseInMemoryDatabase("MessageDb");
        
        // Postgres
        optionsBuilder.UseNpgsql("Host=host.docker.internal;Database=AsyncMessageDb;Username=postgres;Password=jijikos;Port=5432", opt =>
        {
            opt.EnableRetryOnFailure(5);
        });
        optionsBuilder.EnableSensitiveDataLogging(); 
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        
        // EnsureCreated
        
    }
    
    public void InitializeDatabase()
    {
        this.Database.EnsureCreated();
    }
}