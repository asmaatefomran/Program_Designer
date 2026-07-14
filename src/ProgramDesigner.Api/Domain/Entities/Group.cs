namespace ProgramDesigner.Api.Domain.Entities;
using ProgramDesigner.Api.Domain.Enums;

public sealed class Group : Node
{
    public GroupType GroupType { get; init; }

    public int? RequiredSelections { get; init; }

    public List<Node> Children { get; init; } = [];
}