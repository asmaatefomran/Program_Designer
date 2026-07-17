using ProgramDesigner.Api.Domain.Entities;
using ProgramDesigner.Api.Domain.Enums;
using ProgramDesigner.Api.DTOs.Requests;
using ProgramDesigner.Api.Services;
using Xunit;

namespace ProgramDesigner.Tests.Building;

public class ProgramBuilderServiceTests
{
    private readonly ProgramBuilderService _builder = new();

    [Fact]
    public void Build_AssignsFreshPhysicalIdsAndWiresParentChild()
    {
        var child1 = new StepRequest { TemplateId = "s1", Name = "Step One" };
        var child2 = new StepRequest { TemplateId = "s2", Name = "Step Two" };

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [child1, child2]
        };

        var result = _builder.Build(new CreateProgramRequest { Name = "Test", RootGroup = root });

        var rootGroup = result.Program.RootGroup;

        Assert.NotEqual("root", rootGroup.Id); // physical id is server-generated, distinct from TemplateId
        Assert.Equal("root", rootGroup.TemplateId);
        Assert.Equal(2, rootGroup.Children.Count);

        foreach (var child in rootGroup.Children)
        {
            Assert.Same(rootGroup, child.ParentGroup);
            Assert.Equal(rootGroup.Id, child.ParentGroupId);
        }
    }

    [Fact]
    public void Build_PreservesRequestOrderViaOrderIndex()
    {
        var stepA = new StepRequest { TemplateId = "a", Name = "A" };
        var stepB = new StepRequest { TemplateId = "b", Name = "B" };
        var stepC = new StepRequest { TemplateId = "c", Name = "C" };

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [stepA, stepB, stepC]
        };

        var result = _builder.Build(new CreateProgramRequest { Name = "Order test", RootGroup = root });

        var orderedTemplateIds = result.Program.RootGroup.Children
            .OrderBy(c => c.OrderIndex)
            .Select(c => c.TemplateId)
            .ToList();

        Assert.Equal(["a", "b", "c"], orderedTemplateIds);
    }

    [Fact]
    public void Build_ResolvesPrerequisiteToMatchingTemplate()
    {
        var stepA = new StepRequest { TemplateId = "a", Name = "A" };
        var stepB = new StepRequest { TemplateId = "b", Name = "B", PrerequisiteTemplateIds = ["a"] };

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [stepA, stepB]
        };

        var result = _builder.Build(new CreateProgramRequest { Name = "Resolve test", RootGroup = root });

        var builtB = result.Program.RootGroup.Children.Single(c => c.TemplateId == "b");

        Assert.Single(builtB.Prerequisites);
        Assert.Equal("a", builtB.Prerequisites.Single().PrerequisiteTemplateId);
        Assert.Empty(result.UnresolvedPrerequisites);
    }

    [Fact]
    public void Build_RecordsUnresolvedPrerequisite_WhenTargetTemplateDoesNotExist()
    {
        var step = new StepRequest
        {
            TemplateId = "a",
            Name = "A",
            PrerequisiteTemplateIds = ["does-not-exist"]
        };

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [step]
        };

        var result = _builder.Build(new CreateProgramRequest { Name = "Unresolved test", RootGroup = root });

        var builtStep = result.Program.RootGroup.Children.Single();

        // Nothing gets linked -- there's nothing valid to link to.
        Assert.Empty(builtStep.Prerequisites);

        var unresolved = Assert.Single(result.UnresolvedPrerequisites);
        Assert.Equal("does-not-exist", unresolved.MissingPrerequisiteTemplateId);
        Assert.Equal(builtStep.Id, unresolved.Node.Id);
    }

    [Fact]
    public void Build_AllowsTheSameTemplateIdInMultipleBranches()
    {
        var orientationInA = new StepRequest { TemplateId = "orientation", Name = "Orientation" };
        var orientationInB = new StepRequest { TemplateId = "orientation", Name = "Orientation" };

        var optionA = new GroupRequest
        {
            TemplateId = "option-a",
            Name = "Option A",
            GroupType = GroupType.All,
            Children = [orientationInA]
        };

        var optionB = new GroupRequest
        {
            TemplateId = "option-b",
            Name = "Option B",
            GroupType = GroupType.All,
            Children = [orientationInB]
        };

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.Choice,
            RequiredChoiceCount = 1,
            Children = [optionA, optionB]
        };

        var result = _builder.Build(new CreateProgramRequest { Name = "Clone test", RootGroup = root });

        // Two distinct physical Node instances, same TemplateId, no duplicate-id
        // error (their *physical* ids are still unique -- only the logical
        // TemplateId repeats).
        var allSteps = result.Program.RootGroup.Children
            .OfType<Group>()
            .SelectMany(g => g.Children)
            .ToList();

        Assert.Equal(2, allSteps.Count);
        Assert.All(allSteps, s => Assert.Equal("orientation", s.TemplateId));
        Assert.NotEqual(allSteps[0].Id, allSteps[1].Id);
    }

    [Fact]
    public void Build_SetsRootGroupIdToMatchTheBuiltRoot()
    {
        var step = new StepRequest { TemplateId = "s", Name = "S" };
        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [step]
        };

        var result = _builder.Build(new CreateProgramRequest { Name = "Root id test", RootGroup = root });

        Assert.Equal(result.Program.RootGroup.Id, result.Program.RootGroupId);
    }
}