using System.Text.Json.Serialization;

namespace ProgramDesigner.Api.DTOs.Responses;

// The domain entities (Node/Group/Step) can't be returned directly: Node is
// abstract with no JsonPolymorphic discriminator, and Group.Children /
// Node.Prerequisites / Node.ParentGroup form reference cycles that System.Text.Json
// can't serialize by default. These response types are the API's actual public
// contract -- flat, acyclic, and safe to serialize.
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(StepResponse), "step")]
[JsonDerivedType(typeof(GroupResponse), "group")]
public abstract class NodeResponse
{
    public required string Id { get; init; }
    public required string TemplateId { get; init; }
    public required string Name { get; init; }
    public List<string> PrerequisiteTemplateIds { get; init; } = [];
}