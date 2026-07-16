using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramDesigner.Api.Domain.Entities;

namespace ProgramDesigner.Api.Data.Configurations;

public class NodeConfiguration : IEntityTypeConfiguration<Node>
{
    public void Configure(EntityTypeBuilder<Node> builder)
    {
        builder.ToTable("Nodes");

        // Primary Key
        builder.HasKey(n => n.Id);

        // Properties
        builder.Property(n => n.Id)
            .IsRequired();

        builder.Property(n => n.TemplateId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(n => n.Name)
            .HasMaxLength(200)
            .IsRequired();

        // Indexes
        builder.HasIndex(n => n.TemplateId);

        builder.HasIndex(n => n.ParentGroupId);

        // Relationships
        builder.HasOne(n => n.ParentGroup)
            .WithMany(g => g.Children)
            .HasForeignKey(n => n.ParentGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // TPH Inheritance
        builder.HasDiscriminator<string>("NodeType")
            .HasValue<Group>("Group")
            .HasValue<Step>("Step");
    }
}