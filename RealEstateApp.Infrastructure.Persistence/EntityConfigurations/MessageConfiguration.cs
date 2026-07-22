using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.EntityConfigurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
               .IsRequired();

        builder.Property(m => m.ClientId)
               .IsRequired();

        builder.Property(m => m.AgentId)
               .IsRequired();

        builder.Property(m => m.SenderId)
               .IsRequired();
    }
}