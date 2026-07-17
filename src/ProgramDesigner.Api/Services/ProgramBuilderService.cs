using ProgramDesigner.Api.Domain.Entities;
using ProgramDesigner.Api.DTOs.Requests;
using ProgramDesigner.Api.Services.Interfaces;
using ProgramDesigner.Api.Services.Models;

namespace ProgramDesigner.Api.Services;

public class ProgramBuilderService : IProgramBuilderService
{
    // Physical instances (unique Id)
    private readonly Dictionary<string, Node> _nodeInstances = new();

    // Logical nodes (same TemplateId may have many instances)
    private readonly Dictionary<string, List<Node>> _templateNodes = new();

    private readonly Dictionary<Node, List<string>> _pendingPrerequisites = new();

    private readonly List<UnresolvedPrerequisite> _unresolvedPrerequisites = [];

    private readonly HashSet<string> _duplicateNodeIds = [];

    public BuildResult Build(CreateProgramRequest request)
    {
        _nodeInstances.Clear();
        _templateNodes.Clear();
        _pendingPrerequisites.Clear();
        _unresolvedPrerequisites.Clear();
        _duplicateNodeIds.Clear();

        var root = BuildGroup(request.RootGroup);

        ResolvePrerequisites();
        return new BuildResult
        {
            Program = new LearningProgram
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                RootGroup = root,
                RootGroupId = root.Id
            },
            UnresolvedPrerequisites = _unresolvedPrerequisites.ToList()
        };
        
    }

    private Group BuildGroup(GroupRequest request)
    {
        var group = new Group
        {
            Id = Guid.NewGuid().ToString(),
            TemplateId = request.TemplateId,
            Name = request.Name,
            GroupType = request.GroupType,
            RequiredSelections = request.RequiredChoiceCount
        };

        RegisterNode(group);

        _pendingPrerequisites[group] = request.PrerequisiteTemplateIds;

        BuildChildren(group, request.Children);

        return group;
    }

    private Step BuildStep(StepRequest request)
    {
        var step = new Step
        {
            Id = Guid.NewGuid().ToString(),
            TemplateId = request.TemplateId,
            Name = request.Name
        };

        RegisterNode(step);

        _pendingPrerequisites[step] = request.PrerequisiteTemplateIds;

        return step;
    }

    private void BuildChildren(Group parent, List<NodeRequest> children)
    {
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];

            Node node = child switch
            {
                GroupRequest g => BuildGroup(g),
                StepRequest s => BuildStep(s),
                _ => throw new InvalidOperationException("Unknown node type.")
            };

            node.ParentGroup = parent;
            node.ParentGroupId = parent.Id;
            node.OrderIndex = i;

            parent.Children.Add(node);
        }
    }

    private void RegisterNode(Node node)
    {
        // Physical ids must always be unique.
        if (!_nodeInstances.TryAdd(node.Id, node))
        {
            _duplicateNodeIds.Add(node.Id);
            return;
        }

        // Register the logical template.
        if (!_templateNodes.TryGetValue(node.TemplateId, out var instances))
        {
            instances = [];
            _templateNodes[node.TemplateId] = instances;
        }

        instances.Add(node);
    }

    private void ResolvePrerequisites()
    {
        foreach (var (node, prerequisiteTemplateIds) in _pendingPrerequisites)
        {
            foreach (var templateId in prerequisiteTemplateIds)
            {
                if (!_templateNodes.TryGetValue(templateId, out var candidates))
                {
                    _unresolvedPrerequisites.Add(new UnresolvedPrerequisite
                    {
                        Node = node,
                        MissingPrerequisiteTemplateId = templateId
                    });

                    continue;
                }

                // Wire one representative instance.
                // Validation that reasons about reachability across clones
                // should always use TemplateId and search _all_ occurrences.
                var prerequisite = candidates.First();

                node.Prerequisites.Add(new NodePrerequisite
                {
                    Node = node,
                    NodeId = node.Id,

                    Prerequisite = prerequisite,
                    PrerequisiteId = prerequisite.Id,

                    PrerequisiteTemplateId = prerequisite.TemplateId
                });
            }
        }
    }
}