using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.EntityConfigurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");
        builder.HasKey(p => p.Id);

        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Code)
               .IsRequired()
               .HasMaxLength(6);

        builder.Property(p => p.Price)
               .IsRequired()
               .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Size)
               .IsRequired()
               .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.AgentId).IsRequired();

        builder.HasMany(p => p.Images)
               .WithOne(i => i.Property)
               .HasForeignKey(i => i.PropertyId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Offers)
               .WithOne(o => o.Property)
               .HasForeignKey(o => o.PropertyId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Messages)
               .WithOne(m => m.Property)
               .HasForeignKey(m => m.PropertyId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Favorites)
               .WithOne(f => f.Property)
               .HasForeignKey(f => f.PropertyId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Improvements)
               .WithMany(i => i.Properties)
               .UsingEntity(j => j.ToTable("PropertyImprovements"));
    }
}