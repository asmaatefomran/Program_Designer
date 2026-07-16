using ProgramDesigner.Api.Domain.Enums;

namespace ProgramDesigner.Api.DTOs.Responses;

public sealed class GroupResponse : NodeResponse
{
    public GroupType GroupType { get; init; }
    public int? RequiredSelections { get; init; }
    public List<NodeResponse> Children { get; init; } = [];
}