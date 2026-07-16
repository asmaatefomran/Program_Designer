namespace ProgramDesigner.Api.Domain.Entities;
using ProgramDesigner.Api.Domain.Enums;


public sealed class Group : Node
{
    public GroupType GroupType { get; init; }

    public int? RequiredSelections { get; init; }
    // Used only when GroupType == Choice.
    // Ignored for All groups.

    public ICollection<Node> Children { get; set; } = new List<Node>();
}