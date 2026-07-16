namespace ProgramDesigner.Api.Domain.Entities;

public abstract class Node
{
    // Unique per PHYSICAL INSTANCE in the tree. Every node, including every
    // clone of a shared step, has its own distinct Id. Tree structure
    // (ParentGroupId), per-instance graph algorithms (cycle detection,
    // self-dependency), and INVALID_TREE duplication checks all key off this
    // -- they're about this exact placement, not the underlying concept.
    public required string Id { get; init; }

    // Identifies the underlying step/group this instance represents. Two
    // physical nodes sharing the same TemplateId are the SAME logical
    // requirement placed in more than one branch: completing either one
    // satisfies a prerequisite that points at this TemplateId, and
    // reachability logic must search for every node with a matching
    // TemplateId rather than trusting a single wired instance.
    //
    // For an ordinary (non-cloned) node, TemplateId should simply equal Id.
    public required string TemplateId { get; init; }

    public required string Name { get; init; }

    public string? ParentGroupId { get; set; }

    public Group? ParentGroup { get; set; }

    // Position among siblings as originally submitted. Group.Children is an
    // unordered EF collection, so without this, "does X appear before Y"
    // (used by the forward-prerequisite and cycle checks) isn't guaranteed
    // to survive a database round-trip -- only the in-memory object graph
    // built fresh by ProgramBuilderService happens to preserve request order.
    public int OrderIndex { get; set; }

    // Nodes this node depends on
    public ICollection<NodePrerequisite> Prerequisites { get; set; }
        = new List<NodePrerequisite>();

    // Nodes that depend on this node
    public ICollection<NodePrerequisite> RequiredBy { get; set; }
        = new List<NodePrerequisite>();
}