using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramDesigner.Api.Domain.Entities;

namespace ProgramDesigner.Api.Data.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.Property(g => g.RequiredSelections)
            .IsRequired();

        builder.Property(g => g.GroupType)
            .IsRequired();
    }
}