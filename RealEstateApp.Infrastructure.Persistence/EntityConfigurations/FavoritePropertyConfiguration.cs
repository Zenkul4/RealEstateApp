using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.EntityConfigurations;

public class FavoritePropertyConfiguration : IEntityTypeConfiguration<FavoriteProperty>
{
    public void Configure(EntityTypeBuilder<FavoriteProperty> builder)
    {
        builder.ToTable("FavoriteProperties");
        builder.HasKey(fp => fp.Id);

        builder.Property(fp => fp.ClientId)
               .IsRequired();
    }
}