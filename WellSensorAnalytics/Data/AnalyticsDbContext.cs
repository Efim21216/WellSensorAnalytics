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
    }
}
