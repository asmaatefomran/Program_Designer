using ProgramDesigner.Api.Domain.Enums;

namespace ProgramDesigner.Api.DTOs.Requests;

public sealed class GroupRequest : NodeRequest
{
    public GroupType GroupType { get; init; }

    public int RequiredChoiceCount { get; init; }

    public List<NodeRequest> Children { get; init; } = [];
}