using System.Text.Json.Serialization;
namespace ProgramDesigner.Api.DTOs.Requests;


[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(StepRequest), "step")]
[JsonDerivedType(typeof(GroupRequest), "group")]
public abstract class NodeRequest
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public List<string> PrerequisiteIds { get; init; } = [];
}