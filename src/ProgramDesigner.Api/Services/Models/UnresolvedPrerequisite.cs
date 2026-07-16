using ProgramDesigner.Api.Domain.Entities;
namespace ProgramDesigner.Api.Services.Models;

public class UnresolvedPrerequisite
{
    public required Node Node { get; init; }

    public required string MissingPrerequisiteTemplateId { get; init; }
}