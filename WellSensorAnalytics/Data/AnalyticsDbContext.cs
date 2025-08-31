using System;
using Microsoft.EntityFrameworkCore;
using WellSensorAnalytics.Models.Entities;

namespace WellSensorAnalytics.Data;

public class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<Algorithm> Algorithms { get; set; }
    public DbSet<AnalysisResult> AnalysisResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Algorithm>()
            .HasMany<AnalysisResult>()
            .WithOne(e => e.Algorithm)
            .HasForeignKey(e => e.AlgorithmId)
            .IsRequired();
        modelBuilder.Entity<Algorithm>()
            .HasIndex(a => a.Name)
            .IsUnique(false);
        modelBuilder.Entity<Algorithm>()
            .HasIndex(a => a.Enabled)
            .IsUnique(false);

        modelBuilder.Entity<Algorithm>()
            .Property(a => a.LastModified)
            .HasDefaultValueSql("NOW() AT TIME ZONE 'utc'");
    }
    public override int SaveChanges()
    {
        UpdateLastModified();
        return base.SaveChanges();
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateLastModified();
        return base.SaveChangesAsync(cancellationToken);
    }
    private void UpdateLastModified()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IAuditable && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            ((IAuditable)entityEntry.Entity).LastModified = DateTimeOffset.UtcNow;
        }
    }
}
