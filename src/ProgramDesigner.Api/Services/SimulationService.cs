using ProgramDesigner.Api.Domain.Entities;
using ProgramDesigner.Api.Domain.Enums;
using ProgramDesigner.Api.DTOs.Requests;
using ProgramDesigner.Api.DTOs.Responses;
using ProgramDesigner.Api.Services.Interfaces;

namespace ProgramDesigner.Api.Services;

public class SimulationService : ISimulationService
{
    public SimulateResponse Simulate(LearningProgram program, SimulateRequest request)
    {
        var onPath = new Dictionary<string, bool>();
        var isComplete = new Dictionary<string, bool>();
        var completedStepTemplateIds = new HashSet<string>(request.CompletedStepTemplateIds);

        ComputeOnPath(program.RootGroup, true, request.Choices, onPath);
        ComputeComplete(program.RootGroup, request.Choices, completedStepTemplateIds, isComplete);

        var complete = new List<NodeStateResponse>();
        var unlocked = new List<NodeStateResponse>();
        var blocked = new List<NodeStateResponse>();

        foreach (var node in Flatten(program.RootGroup))
        {
            // Nodes inside a choice branch the participant didn't pick (or
            // hasn't decided on yet) aren't part of their experience at all --
            // omitted entirely rather than reported as "blocked".
            if (!onPath.GetValueOrDefault(node.Id))
                continue;

            if (isComplete.GetValueOrDefault(node.Id))
            {
                complete.Add(ToState(node, "complete", null));
                continue;
            }

            var reason = GetBlockReason(node, isComplete);
            if (reason is null)
                unlocked.Add(ToState(node, "unlocked", null));
            else
                blocked.Add(ToState(node, "blocked", reason));
        }

        return new SimulateResponse { Complete = complete, Unlocked = unlocked, Blocked = blocked };
    }

    private static NodeStateResponse ToState(Node node, string status, string? reason) => new()
    {
        Id = node.Id,
        TemplateId = node.TemplateId,
        Name = node.Name,
        Status = status,
        Reason = reason,
    };

    private static IEnumerable<Node> Flatten(Node node)
    {
        yield return node;

        if (node is Group group)
        {
            foreach (var child in group.Children)
            {
                foreach (var descendant in Flatten(child))
                {
                    yield return descendant;
                }
            }
        }
    }

    private static void ComputeOnPath(
        Node node,
        bool parentOnPath,
        Dictionary<string, List<string>> choices,
        Dictionary<string, bool> onPath)
    {
        onPath[node.Id] = parentOnPath;

        if (node is not Group group)
            return;

        foreach (var child in group.Children)
        {
            var childOnPath = parentOnPath;

            if (group.GroupType == GroupType.Choice)
            {
                var chosen = choices.GetValueOrDefault(group.TemplateId) ?? [];
                childOnPath = parentOnPath && chosen.Contains(child.TemplateId);
            }

            ComputeOnPath(child, childOnPath, choices, onPath);
        }
    }

    private static bool ComputeComplete(
        Node node,
        Dictionary<string, List<string>> choices,
        HashSet<string> completedStepTemplateIds,
        Dictionary<string, bool> isComplete)
    {
        bool complete;

        if (node is Group group)
        {
            var childResults = group.Children
                .Select(c => ComputeComplete(c, choices, completedStepTemplateIds, isComplete))
                .ToList();

            if (group.GroupType == GroupType.All)
            {
                complete = childResults.Count > 0 && childResults.All(c => c);
            }
            else
            {
                var chosen = choices.GetValueOrDefault(group.TemplateId) ?? [];
                var chosenChildren = group.Children.Where(c => chosen.Contains(c.TemplateId)).ToList();

                complete = chosenChildren.Count >= (group.RequiredSelections ?? int.MaxValue)
                    && chosenChildren.Count > 0
                    && chosenChildren.All(c => isComplete.GetValueOrDefault(c.Id));
            }
        }
        else
        {
            complete = completedStepTemplateIds.Contains(node.TemplateId);
        }

        isComplete[node.Id] = complete;
        return complete;
    }

    private static string? GetBlockReason(Node node, Dictionary<string, bool> isComplete)
    {
        foreach (var prerequisite in node.Prerequisites)
        {
            var target = prerequisite.Prerequisite;
            if (target is not null && !isComplete.GetValueOrDefault(target.Id))
            {
                return $"Waiting on prerequisite '{target.Name}'.";
            }
        }

        if (node.ParentGroup is { GroupType: GroupType.All } parent)
        {
            var siblings = parent.Children.OrderBy(c => c.OrderIndex).ToList();
            var index = siblings.FindIndex(c => c.Id == node.Id);

            for (var i = 0; i < index; i++)
            {
                if (!isComplete.GetValueOrDefault(siblings[i].Id))
                    return $"Waiting on '{siblings[i].Name}' to be completed first.";
            }
        }

        return null;
    }
}