using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramDesigner.Api.Domain.Entities;

namespace ProgramDesigner.Api.Data.Configurations;

public class LearningProgramConfiguration : IEntityTypeConfiguration<LearningProgram>
{
    public void Configure(EntityTypeBuilder<LearningProgram> builder)
    {
        builder.ToTable("Programs");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne(p => p.RootGroup)
            .WithMany()
            .HasForeignKey(p => p.RootGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}