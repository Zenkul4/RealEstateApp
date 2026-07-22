using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.EntityConfigurations;

public sealed class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.ToTable("Offers");
        builder.HasKey(offer => offer.Id);

        builder.Property(offer => offer.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(offer => offer.ClientId)
            .IsRequired();

        builder.HasOne(offer => offer.Property)
            .WithMany(property => property.Offers)
            .HasForeignKey(offer => offer.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
