using Microsoft.EntityFrameworkCore;
using ProgramDesigner.Api.Domain.Entities;

namespace ProgramDesigner.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<LearningProgram> Programs => Set<LearningProgram>();
    public DbSet<Node> Nodes => Set<Node>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Step> Steps => Set<Step>();
    public DbSet<NodePrerequisite> NodePrerequisites => Set<NodePrerequisite>();
    public DbSet<MissingPrerequisiteRecord> MissingPrerequisites => Set<MissingPrerequisiteRecord>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}