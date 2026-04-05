using AutoResearchClaw.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoResearchClaw.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Run> Runs => Set<Run>();
    public DbSet<RunLog> RunLogs => Set<RunLog>();
    public DbSet<LlmProvider> LlmProviders => Set<LlmProvider>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.HasMany(e => e.Runs).WithOne(r => r.Project).HasForeignKey(r => r.ProjectId);
        });

        modelBuilder.Entity<Run>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Topic).HasMaxLength(2000).IsRequired();
            entity.HasMany(e => e.Logs).WithOne(l => l.Run).HasForeignKey(l => l.RunId);
        });

        modelBuilder.Entity<RunLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).HasMaxLength(10000);
        });

        modelBuilder.Entity<LlmProvider>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.BaseUrl).HasMaxLength(500);
        });
    }
}