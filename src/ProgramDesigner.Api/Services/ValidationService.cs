using ProgramDesigner.Api.DTOs.Responses;
using ProgramDesigner.Api.Services.Interfaces;
using ProgramDesigner.Api.Services.Models;
using ProgramDesigner.Api.Domain.Entities;
using ProgramDesigner.Api.Domain.Enums;

namespace ProgramDesigner.Api.Services;

public class ValidationService : IValidationService
{
    public ValidationResponse Validate(BuildResult buildResult)
    {
        var errors = new List<ValidationIssueResponse>();
        var warnings = new List<ValidationIssueResponse>();

        ValidateProgramStructure(buildResult, errors);
        ValidateMissingPrerequisiteTemplates(buildResult, errors);
        ValidateSelfDependencies(buildResult, errors);
        ValidateCircularDependencies(buildResult, errors);
        ValidateImpossiblePrerequisites(buildResult, errors);

        ValidateGroupConstraints(buildResult, errors);
        ValidatePrerequisiteReachability(buildResult, warnings);
        ValidateChoiceRequiresAllChildren(buildResult, warnings);
        ValidateTemplateConsistency(buildResult, errors);

        return new ValidationResponse
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

//----------------------------------------------------------------------------------------------------------------
//1. ValidateProgramStructure  [Error]

    private static void ValidateProgramStructure(
        BuildResult buildResult,
        List<ValidationIssueResponse> errors)
    {
        var visited = new HashSet<string>();

        ValidateTree(
            buildResult.Program.RootGroup,
            visited,
            errors);
    }

    private static void ValidateTree(
        Node node,
        HashSet<string> visited,
        List<ValidationIssueResponse> errors)
    {
        if (!visited.Add(node.Id))
        {
            errors.Add(new ValidationIssueResponse
            {
                Code = "INVALID_TREE",
                NodeId = node.Id,
                Message = "A node appears more than once in the program tree."
            });

            return;
        }

        if (node is not Group group)
            return;

        foreach (var child in group.Children)
        {
            if (child.ParentGroupId != group.Id)
            {
                errors.Add(new ValidationIssueResponse
                {
                    Code = "INVALID_PARENT",
                    NodeId = child.Id,
                    Message = "Parent-child relationship is inconsistent."
                });
            }

            ValidateTree(child, visited, errors);
        }
    }
//------------------------------------------------------------------------------------------------------------

    // Note: unresolved prerequisites are never added to Node.Prerequisites in
    // the first place (there's nothing to link to), so they can't be found
    // by scanning the tree -- BuildResult.UnresolvedPrerequisites is the only
    // place this information exists. At creation time it comes straight from
    // ProgramBuilderService; at /validate time (a separate call, against an
    // already-persisted program) it comes from MissingPrerequisiteRecord rows
    // reloaded from the database -- see ProgramService.ValidateAsync.
    private static void ValidateMissingPrerequisiteTemplates(
        BuildResult buildResult,
        List<ValidationIssueResponse> errors)
    {
        foreach (var unresolved in buildResult.UnresolvedPrerequisites)
        {
            errors.Add(new ValidationIssueResponse
            {
                Code = "MISSING_PREREQUISITE",
                NodeId = unresolved.Node.Id,
                Message =
                    $"Prerequisite template '{unresolved.MissingPrerequisiteTemplateId}' does not exist."
            });
        }
    }
//---------------------------------------------------------------------------------------------------

//------------------------------------------------------------------------------------------------------------

    private static IEnumerable<Node> GetAllNodes(Group root)
    {
        yield return root;

        foreach (var child in root.Children)
        {
            if (child is Group group)
            {
                foreach (var node in GetAllNodes(group))
                {
                    yield return node;
                }
            }
            else
            {
                yield return child;
            }
        }
    }

    private static void ValidateSelfDependencies(
        BuildResult buildResult,
        List<ValidationIssueResponse> errors)
    {
        foreach (var node in GetAllNodes(buildResult.Program.RootGroup))
        {
            if (node.Prerequisites.Any(p =>
                    p.PrerequisiteTemplateId == node.TemplateId))
            {
                errors.Add(new ValidationIssueResponse
                {
                    Code = "SELF_DEPENDENCY",
                    NodeId = node.Id,
                    Message = "A node cannot depend on itself."
                });
            }
        }
    }

    private static void ValidateGroupConstraints(
        BuildResult buildResult,
        List<ValidationIssueResponse> errors)
    {
        foreach (var group in GetAllNodes(buildResult.Program.RootGroup).OfType<Group>())
        {
            if (!group.Children.Any())
            {
                errors.Add(new ValidationIssueResponse
                {
                    Code = "EMPTY_GROUP",
                    NodeId = group.Id,
                    Message = "Group must contain at least one child."
                });
            }

            if (group.RequiredSelections < 0)
            {
                errors.Add(new ValidationIssueResponse
                {
                    Code = "INVALID_REQUIRED_SELECTIONS",
                    NodeId = group.Id,
                    Message = "Required selections cannot be negative."
                });
            }

            if (group.RequiredSelections > group.Children.Count)
            {
                errors.Add(new ValidationIssueResponse
                {
                    Code = "INVALID_REQUIRED_SELECTIONS",
                    NodeId = group.Id,
                    Message = "Required selections cannot exceed the number of children."
                });
            }
        }
    }

    // A Choice group that requires every one of its children isn't actually
    // offering a choice -- every branch is mandatory, which is exactly what an
    // "in order" group already means. The structure is still valid (nothing
    // breaks), so this is a warning rather than an error: it flags a group
    // that's effectively a "complete" group wearing a Choice label, and lets
    // the designer decide whether to relabel it as "in order" instead.
    private static void ValidateChoiceRequiresAllChildren(
        BuildResult buildResult,
        List<ValidationIssueResponse> warnings)
    {
        foreach (var group in GetAllNodes(buildResult.Program.RootGroup).OfType<Group>())
        {
            if (group.GroupType == GroupType.Choice &&
                group.Children.Any() &&
                group.RequiredSelections == group.Children.Count)
            {
                warnings.Add(new ValidationIssueResponse
                {
                    Code = "CHOICE_REQUIRES_ALL_CHILDREN",
                    NodeId = group.Id,
                    Message =
                        "This choice group requires all of its options, making it effectively a complete group rather than a choice. Consider using an 'in order' group instead."
                });
            }
        }
    }

    private static Dictionary<string, int> BuildTraversalOrder(Group root)
    {
        var order = new Dictionary<string, int>();
        var index = 0;

        Traverse(root);

        return order;

        void Traverse(Node node)
        {
            order[node.Id] = index++;

            if (node is Group group)
            {
                foreach (var child in group.Children.OrderBy(c => c.OrderIndex))
                {
                    Traverse(child);
                }
            }
        }
    }

    private static bool ContainsTemplate(Node root, string templateId)
    {
        if (root.TemplateId == templateId)
            return true;

        if (root is not Group group)
            return false;

        return group.Children.Any(child => ContainsTemplate(child, templateId));
    }

    private static void ValidateImpossiblePrerequisites(
        BuildResult buildResult,
        List<ValidationIssueResponse> errors)
    {
        var order = BuildTraversalOrder(buildResult.Program.RootGroup);

        foreach (var node in GetAllNodes(buildResult.Program.RootGroup))
        {
            foreach (var prerequisite in node.Prerequisites)
            {
                // Forward reference: prerequisite appears later than the node that
                // depends on it in document/traversal order. Note: this compares
                // global pre-order position, so it is meaningful for nodes that sit
                // in a shared "in order" lineage; it does not attempt to reason about
                // mutually-exclusive choice branches (that's handled separately by
                // reachability warnings below).
                if (order.TryGetValue(node.Id, out var nodeOrder) &&
                    order.TryGetValue(prerequisite.PrerequisiteId, out var prerequisiteOrder) &&
                    prerequisiteOrder > nodeOrder)
                {
                    errors.Add(new ValidationIssueResponse
                    {
                        Code = "FORWARD_PREREQUISITE",
                        NodeId = node.Id,
                        Message =
                            "A prerequisite appears after the node that depends on it."
                    });
                }

                // Mirror case the traversal-order check above can't catch: a
                // prerequisite pointing at one of its own ancestors. An ancestor
                // always has an EARLIER traversal position than its descendant,
                // so it never trips the forward-reference check -- but it's
                // exactly as circular: the ancestor group can't be considered
                // complete until this node finishes (or, for a Choice ancestor,
                // until this branch is chosen), so this node can't also be
                // waiting on that same ancestor.
                if (IsAncestorOf(node, prerequisite.PrerequisiteId))
                {
                    errors.Add(new ValidationIssueResponse
                    {
                        Code = "PREREQUISITE_ON_ANCESTOR",
                        NodeId = node.Id,
                        Message =
                            "A prerequisite points at a group that contains it, which is circular."
                    });
                }
            }
        }
    }

    private static bool IsAncestorOf(Node node, string possibleAncestorId)
    {
        var current = node.ParentGroup;

        while (current != null)
        {
            if (current.Id == possibleAncestorId)
                return true;

            current = current.ParentGroup;
        }

        return false;
    }

//-------------------------------------------------------------------------------------------------------------------------


    private static void ValidateCircularDependencies(
        BuildResult buildResult,
        List<ValidationIssueResponse> errors)
    {
        var root = buildResult.Program.RootGroup;

        var templateLookup = BuildTemplateLookup(root);
        var visited = new HashSet<string>();

        foreach (var node in GetAllNodes(root))
        {
            DetectCycle(
                node,
                templateLookup,
                visited,
                new HashSet<string>(),
                errors);
        }
    }

    private static Dictionary<string, List<Node>> BuildTemplateLookup(Group root)
    {
        return GetAllNodes(root)
            .GroupBy(node => node.TemplateId)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    private static void DetectCycle(
        Node node,
        Dictionary<string, List<Node>> templateLookup,
        HashSet<string> visited,
        HashSet<string> currentPath,
        List<ValidationIssueResponse> errors)
    {
        if (currentPath.Contains(node.Id))
        {
            errors.Add(new ValidationIssueResponse
            {
                Code = "CIRCULAR_DEPENDENCY",
                NodeId = node.Id,
                Message = "Circular dependency detected."
            });
            return;
        }

        if (visited.Contains(node.Id))
            return;

        visited.Add(node.Id);
        currentPath.Add(node.Id);

        foreach (var prerequisite in node.Prerequisites)
        {
            if (!templateLookup.TryGetValue(
                    prerequisite.PrerequisiteTemplateId,
                    out var prerequisiteNodes))
            {
                continue;
            }

            foreach (var prerequisiteNode in prerequisiteNodes)
            {
                DetectCycle(
                    prerequisiteNode,
                    templateLookup,
                    visited,
                    currentPath,
                    errors);
                if (errors.Any(e => e.Code == "CIRCULAR_DEPENDENCY"))
                    return;
            }
        }

        currentPath.Remove(node.Id);
    }


//-----------------------------------------------------------------------------------------------------------------------------

    private static bool ContainsNode(
        Node root,
        Node target)
    {
        if (root.Id == target.Id)
            return true;

        if (root is not Group group)
            return false;

        return group.Children.Any(child => ContainsNode(child, target));
    }

    // Same as ContainsNode, but matches by a bare id rather than an object --
    // used to detect clones (the same step id intentionally placed in more
    // than one branch) without needing a specific Node instance in hand.


    /// <summary>
    /// Determines whether completing <paramref name="target"/> guarantees that
    /// a node with id <paramref name="prerequisiteId"/> was already completed,
    /// for every valid participant path.
    ///
    /// A prerequisite id may now resolve to MORE THAN ONE physical node (the
    /// same step intentionally cloned into several branches -- see
    /// ProgramBuilderService.RegisterNode). The prerequisite is guaranteed if
    /// there's at least one occurrence that sits on a path always taken
    /// whenever the target is completed, OR if a choice's branches are
    /// collectively covered by clones such that no valid selection can avoid
    /// the requirement entirely.
    /// </summary>
    private static bool IsGuaranteedPrerequisite(
        string prerequisiteTemplateId,
        Node target,
        Group root)
    {
        var occurrences = GetAllNodes(root)
            .Where(n => n.TemplateId == prerequisiteTemplateId)
            .ToList();

        // No physical occurrence to reason about -- MISSING_PREREQUISITE
        // already covers this case as an error, so there's nothing to warn
        // about here.
        if (!occurrences.Any())
            return true;

        return occurrences.Any(occurrence =>
            IsGuaranteedFromOccurrence(occurrence, target, prerequisiteTemplateId));
    }

    /// <summary>
    /// Walks up the ancestors of ONE occurrence of the prerequisite, checking
    /// whether that occurrence's position guarantees it's completed whenever
    /// the target is.
    ///
    ///  - If the target lives in the SAME branch as this occurrence,
    ///    completing the target already forces that branch to be selected --
    ///    guaranteed at this level, regardless of how many branches the
    ///    choice requires.
    ///
    ///  - If the target lives in a DIFFERENT branch (or outside the choice
    ///    group entirely), this occurrence's branch can normally be skipped.
    ///    Two things can still save the guarantee at this level:
    ///      a) every branch is mandatory (RequiredSelections == Children.Count), or
    ///      b) EVERY branch of the choice contains its own occurrence of the
    ///         same prerequisite id (the step was cloned into every branch),
    ///         so no matter which branch(es) get picked, the requirement is
    ///         still met.
    ///
    /// Example: Final Capstone requires AI Capstone, which lives inside the
    /// "AI" branch of the "Major" choice group (pick 1 of 3), with no clones
    /// elsewhere. Final Capstone sits outside "Major" entirely, so a
    /// participant who picked IT or Programming would satisfy Final
    /// Capstone's structural position without ever completing AI Capstone --
    /// not guaranteed.
    ///
    /// Counter-example: if the same step were cloned into AI, IT, and
    /// Programming under the same id, this walk (started from any one clone)
    /// would find every branch of Major covered and return true.
    ///
    /// Known limitation: this covers "every branch has a clone" cleanly, but
    /// doesn't do full combinatorics for partial coverage under a multi-select
    /// choice (e.g. clones in 2 of 3 branches with "pick 2 of 3" happens to
    /// also be guaranteed, but isn't recognized here). Flagging as a possible
    /// false warning in that specific edge case rather than silently getting
    /// it wrong.
    /// </summary>
    private static bool IsGuaranteedFromOccurrence(
        Node prerequisiteOccurrence,
        Node target,
        string prerequisiteTemplateId)
    {
        var current = prerequisiteOccurrence.ParentGroup;

        while (current != null)
        {
            if (current.GroupType == GroupType.Choice)
            {
                var branchContainingOccurrence =
                    current.Children.First(child => ContainsNode(child, prerequisiteOccurrence));

                var targetSharesBranch = ContainsNode(branchContainingOccurrence, target);
                var everyBranchMandatory = current.RequiredSelections == current.Children.Count;
                var everyBranchHasAnOccurrence =
                    current.Children.All(branch => ContainsTemplate(branch, prerequisiteTemplateId));

                if (!targetSharesBranch && !everyBranchMandatory && !everyBranchHasAnOccurrence)
                {
                    return false;
                }
            }

            current = current.ParentGroup;
        }

        return true;
    }


    private static void ValidatePrerequisiteReachability(
        BuildResult buildResult,
        List<ValidationIssueResponse> warnings)
    {
        foreach (var node in GetAllNodes(buildResult.Program.RootGroup))
        {
            foreach (var prerequisite in node.Prerequisites)
            {
                if (!IsGuaranteedPrerequisite(
                        prerequisite.PrerequisiteTemplateId,
                        node,
                        buildResult.Program.RootGroup))
                {
                    warnings.Add(new ValidationIssueResponse
                    {
                        Code = "UNREACHABLE_PREREQUISITE",
                        NodeId = node.Id,
                        Message =
                            $"Prerequisite template '{prerequisite.PrerequisiteTemplateId}' is not guaranteed for every valid participant path."
                    });
                }
            }
        }
    }

//----------------------------------------------------------------------------------------------------------------------------

    private static void ValidateTemplateConsistency(
        BuildResult buildResult,
        List<ValidationIssueResponse> errors)
    {
        var templateGroups = GetAllNodes(buildResult.Program.RootGroup)
            .GroupBy(n => n.TemplateId);

        foreach (var group in templateGroups)
        {
            if (group.Count() == 1)
                continue;

            var first = group.First();

            foreach (var node in group.Skip(1))
            {
                ValidateSameType(first, node, errors);
                ValidateSameName(first, node, errors);
                ValidateSamePrerequisites(first, node, errors);

                if (first is Group firstGroup &&
                    node is Group currentGroup)
                {
                    ValidateSameGroupConfiguration(
                        firstGroup,
                        currentGroup,
                        errors);
                }
            }
        }
    }
    
    private static void ValidateSameType(
        Node expected,
        Node actual,
        List<ValidationIssueResponse> errors)
    {
        if (expected.GetType() == actual.GetType())
            return;

        errors.Add(new ValidationIssueResponse
        {
            Code = "INCONSISTENT_TEMPLATE",
            NodeId = actual.Id,
            Message =
                $"All nodes with template '{actual.TemplateId}' must have the same type."
        });
    }
    
    private static void ValidateSameName(
        Node expected,
        Node actual,
        List<ValidationIssueResponse> errors)
    {
        if (expected.Name == actual.Name)
            return;

        errors.Add(new ValidationIssueResponse
        {
            Code = "INCONSISTENT_TEMPLATE",
            NodeId = actual.Id,
            Message =
                $"All nodes with template '{actual.TemplateId}' must have the same name."
        });
    }
    
    private static void ValidateSamePrerequisites(
        Node expected,
        Node actual,
        List<ValidationIssueResponse> errors)
    {
        var expectedTemplates = expected.Prerequisites
            .Select(p => p.PrerequisiteTemplateId)
            .Order()
            .ToList();

        var actualTemplates = actual.Prerequisites
            .Select(p => p.PrerequisiteTemplateId)
            .Order()
            .ToList();

        if (expectedTemplates.SequenceEqual(actualTemplates))
            return;

        errors.Add(new ValidationIssueResponse
        {
            Code = "INCONSISTENT_TEMPLATE",
            NodeId = actual.Id,
            Message =
                $"All nodes with template '{actual.TemplateId}' must have identical prerequisites."
        });
    }
    
    private static void ValidateSameGroupConfiguration(
        Group expected,
        Group actual,
        List<ValidationIssueResponse> errors)
    {
        if (expected.GroupType == actual.GroupType &&
            expected.RequiredSelections == actual.RequiredSelections)
        {
            return;
        }

        errors.Add(new ValidationIssueResponse
        {
            Code = "INCONSISTENT_TEMPLATE",
            NodeId = actual.Id,
            Message =
                $"All groups with template '{actual.TemplateId}' must have identical configuration."
        });
    }
}