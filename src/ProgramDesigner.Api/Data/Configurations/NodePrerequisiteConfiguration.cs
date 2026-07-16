using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramDesigner.Api.Domain.Entities;

namespace ProgramDesigner.Api.Data.Configurations;

public class NodePrerequisiteConfiguration : IEntityTypeConfiguration<NodePrerequisite>
{
    public void Configure(EntityTypeBuilder<NodePrerequisite> builder)
    {
        builder.ToTable("NodePrerequisites");

        // Primary Key
        builder.HasKey(x => new
        {
            x.NodeId,
            x.PrerequisiteId
        });

        // Properties
        builder.Property(x => x.PrerequisiteTemplateId)
            .HasMaxLength(100)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.PrerequisiteTemplateId);

        // Relationships
        builder.HasOne(x => x.Node)
            .WithMany(n => n.Prerequisites)
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Prerequisite)
            .WithMany(n => n.RequiredBy)
            .HasForeignKey(x => x.PrerequisiteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}