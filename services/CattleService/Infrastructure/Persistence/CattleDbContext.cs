using Microsoft.EntityFrameworkCore;
using SmartFarm.Cattle.Api.Domain.Entities;

namespace SmartFarm.Cattle.Api.Infrastructure.Persistence;

public class CattleDbContext : DbContext
{
    public CattleDbContext(
        DbContextOptions<CattleDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ranch> Ranches => Set<Ranch>();
    public DbSet<Lot> Lots => Set<Lot>();
    public DbSet<CattleAnimal> Cattle => Set<CattleAnimal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("cattle");

        modelBuilder.Entity<Ranch>(entity =>
        {
            entity.ToTable("ranches");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("ranch_id");
            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(120)
                .IsRequired();
            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .IsRequired();
        });

        modelBuilder.Entity<Lot>(entity =>
        {
            entity.ToTable("lots");
            entity.HasKey(x => x.Id);
            entity.HasAlternateKey(x => new { x.Id, x.RanchId });

            entity.Property(x => x.Id).HasColumnName("lot_id");
            entity.Property(x => x.RanchId).HasColumnName("ranch_id");
            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(120)
                .IsRequired();
            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne<Ranch>()
                .WithMany()
                .HasForeignKey(x => x.RanchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CattleAnimal>(entity =>
        {
            entity.ToTable("cattle");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("cattle_id");
            entity.Property(x => x.RanchId).HasColumnName("ranch_id");
            entity.Property(x => x.LotId).HasColumnName("lot_id");
            entity.Property(x => x.EarTag)
                .HasColumnName("ear_tag")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(x => x.Version)
                .HasColumnName("version")
                .IsConcurrencyToken();

            entity.HasIndex(x => new { x.RanchId, x.EarTag })
                .IsUnique();

            entity.HasOne<Ranch>()
                .WithMany()
                .HasForeignKey(x => x.RanchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Lot>()
                .WithMany()
                .HasForeignKey(x => new { x.LotId, x.RanchId })
                .HasPrincipalKey(x => new { x.Id, x.RanchId })
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}