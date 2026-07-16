using System.Text.Json.Serialization;

namespace ProgramDesigner.Api.DTOs.Requests;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(StepRequest), "step")]
[JsonDerivedType(typeof(GroupRequest), "group")]
public abstract class NodeRequest
{
    // Logical identifier shared by all clones
    public required string TemplateId { get; init; }

    public required string Name { get; init; }

    // Logical prerequisite ids
    public List<string> PrerequisiteTemplateIds { get; init; } = [];
}