
using Microsoft.EntityFrameworkCore;
using PetCareAnalytics.Core.Models;

namespace PetCareAnalytics.Core.Storage;

public class PetCareDbContext : DbContext
{
    public PetCareDbContext(DbContextOptions<PetCareDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pet> Pets => Set<Pet>();

    public DbSet<PetMeasurement> Measurements =>
        Set<PetMeasurement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Species)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.Breed)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<PetMeasurement>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.MeasurementType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(m => m.Unit)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne<Pet>()
                .WithMany()
                .HasForeignKey(m => m.PetId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
