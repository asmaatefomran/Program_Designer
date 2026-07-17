using ProgramDesigner.Api.Domain.Enums;
using ProgramDesigner.Api.DTOs.Requests;
using ProgramDesigner.Api.Services;
using ProgramDesigner.Tests.TestData;
using Xunit;

namespace ProgramDesigner.Tests.Validation;

public class ValidationServiceTests
{
    private readonly ProgramBuilderService _builder = new();
    private readonly ValidationService _validator = new();

    [Fact]
    public void ComputerScienceScenario_ValidatesWithoutErrorsOrWarnings()
    {
        var buildResult = _builder.Build(ComputerScienceScenario.Build());

        var result = _validator.Validate(buildResult);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void DirectPrerequisiteCycle_IsRejected()
    {
        var stepA = new StepRequest { TemplateId = "a", Name = "A", PrerequisiteTemplateIds = ["b"] };
        var stepB = new StepRequest { TemplateId = "b", Name = "B", PrerequisiteTemplateIds = ["a"] };

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [stepA, stepB]
        };

        var buildResult = _builder.Build(new CreateProgramRequest { Name = "Cycle test", RootGroup = root });

        var result = _validator.Validate(buildResult);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "CIRCULAR_DEPENDENCY");
    }

    [Fact]
    public void PrerequisiteInsideAnUnpickedChoiceOption_GeneratesWarningNotRejection()
    {
        // Redirect Final Capstone's prerequisite from Major (safe -- guaranteed
        // regardless of which option is picked) to AI Capstone specifically
        // (only reachable if the participant picks the AI option).
        var request = ComputerScienceScenario.Build(finalCapstonePrerequisiteTemplateId: "step-ai-capstone");
        var buildResult = _builder.Build(request);

        var result = _validator.Validate(buildResult);

        Assert.True(result.IsValid); // still valid -- a warning, not a rejection
        Assert.Empty(result.Errors);
        Assert.Contains(result.Warnings, w => w.Code == "UNREACHABLE_PREREQUISITE");
    }

    [Fact]
    public void SelfReferencingPrerequisite_IsRejected()
    {
        var step = new StepRequest
        {
            TemplateId = "step-1",
            Name = "Self referencing step",
            PrerequisiteTemplateIds = ["step-1"]
        };

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [step]
        };

        var buildResult = _builder.Build(new CreateProgramRequest { Name = "Self ref test", RootGroup = root });

        var result = _validator.Validate(buildResult);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "SELF_DEPENDENCY");
    }

    [Fact]
    public void PrerequisitePointingAtALaterStep_IsRejected()
    {
        var stepA = new StepRequest { TemplateId = "a", Name = "A", PrerequisiteTemplateIds = ["b"] };
        var stepB = new StepRequest { TemplateId = "b", Name = "B" };

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [stepA, stepB]
        };

        var buildResult = _builder.Build(new CreateProgramRequest { Name = "Forward ref test", RootGroup = root });

        var result = _validator.Validate(buildResult);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "FORWARD_PREREQUISITE");
    }

    [Fact]
    public void MissingPrerequisiteTemplate_IsRejected()
    {
        var step = new StepRequest
        {
            TemplateId = "step-1",
            Name = "Orphan prerequisite",
            PrerequisiteTemplateIds = ["does-not-exist"]
        };

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [step]
        };

        var buildResult = _builder.Build(new CreateProgramRequest { Name = "Missing prereq test", RootGroup = root });

        var result = _validator.Validate(buildResult);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "MISSING_PREREQUISITE");
    }

    [Fact]
    public void ChoiceGroupThatRequiresAllOptions_NoUnreachablePrerequisiteWarning()
    {
        // A Choice group with RequiredChoiceCount == number of children has no
        // real choice -- everything is mandatory -- so it must not cause a
        // false "might not be picked" warning.
        var optionA = new StepRequest { TemplateId = "opt-a", Name = "Option A" };
        var optionB = new StepRequest { TemplateId = "opt-b", Name = "Option B" };

        var pickBoth = new GroupRequest
        {
            TemplateId = "pick-both",
            Name = "Pick both",
            GroupType = GroupType.Choice,
            RequiredChoiceCount = 2, // == Children.Count
            Children = [optionA, optionB]
        };

        var finalStep = new StepRequest
        {
            TemplateId = "final",
            Name = "Final",
            PrerequisiteTemplateIds = ["opt-a"]
        };

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [pickBoth, finalStep]
        };

        var buildResult = _builder.Build(new CreateProgramRequest { Name = "Pick-all test", RootGroup = root });

        var result = _validator.Validate(buildResult);

        Assert.DoesNotContain(result.Warnings, w => w.Code == "UNREACHABLE_PREREQUISITE");
    }

    [Fact]
    public void PrerequisitePointingAtAncestor_IsRejected()
    {
        // A group's prerequisite pointing at its own child would be caught by
        // the forward-reference check (a descendant always has a later
        // traversal position). The mirror case -- a node depending on one of
        // its own ANCESTORS -- has an *earlier* traversal position, so it
        // needs its own check.
        var innerStep = new StepRequest
        {
            TemplateId = "inner",
            Name = "Inner step",
            PrerequisiteTemplateIds = ["outer"]
        };

        var outerGroup = new GroupRequest
        {
            TemplateId = "outer",
            Name = "Outer group",
            GroupType = GroupType.All,
            Children = [innerStep]
        };

        var buildResult = _builder.Build(new CreateProgramRequest { Name = "Ancestor ref test", RootGroup = outerGroup });

        var result = _validator.Validate(buildResult);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "PREREQUISITE_ON_ANCESTOR");
    }

    [Fact]
    public void ClonedStepInEveryChoiceBranch_MakesPrerequisiteGuaranteed()
    {
        // The same TemplateId ("orientation") placed in every branch of a
        // choice group. No matter which options a participant picks, they
        // complete an occurrence of it -- so a prerequisite on "orientation"
        // should NOT warn, even though no single occurrence sits on a path
        // guaranteed regardless of choice.
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

        var choice = new GroupRequest
        {
            TemplateId = "choice",
            Name = "Choice",
            GroupType = GroupType.Choice,
            RequiredChoiceCount = 1,
            Children = [optionA, optionB]
        };

        var finalStep = new StepRequest
        {
            TemplateId = "final",
            Name = "Final",
            PrerequisiteTemplateIds = ["orientation"]
        };

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [choice, finalStep]
        };

        var buildResult = _builder.Build(new CreateProgramRequest { Name = "Clone coverage test", RootGroup = root });

        var result = _validator.Validate(buildResult);

        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Warnings, w => w.Code == "UNREACHABLE_PREREQUISITE");
    }

    [Fact]
    public void ClonedStepInOnlyOneChoiceBranch_PrerequisiteIsUnreachableWarning()
    {
        // Same shape as above, but the clone only exists in one branch --
        // a participant who picks Option B never completes it.
        var orientationInA = new StepRequest { TemplateId = "orientation", Name = "Orientation" };
        var somethingElseInB = new StepRequest { TemplateId = "something-else", Name = "Something else" };

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
            Children = [somethingElseInB]
        };

        var choice = new GroupRequest
        {
            TemplateId = "choice",
            Name = "Choice",
            GroupType = GroupType.Choice,
            RequiredChoiceCount = 1,
            Children = [optionA, optionB]
        };

        var finalStep = new StepRequest
        {
            TemplateId = "final",
            Name = "Final",
            PrerequisiteTemplateIds = ["orientation"]
        };

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [choice, finalStep]
        };

        var buildResult = _builder.Build(new CreateProgramRequest { Name = "Partial clone coverage test", RootGroup = root });

        var result = _validator.Validate(buildResult);

        Assert.True(result.IsValid);
        Assert.Contains(result.Warnings, w => w.Code == "UNREACHABLE_PREREQUISITE");
    }

    [Fact]
    public void DuplicateTemplateIdsWithDifferentNames_IsRejectedAsInconsistent()
    {
        // Two physical nodes sharing a TemplateId ("clones") must agree on
        // definition -- otherwise "the same step" means two different things
        // depending which instance a participant happens to hit.
        var stepA = new StepRequest { TemplateId = "shared", Name = "Version One" };
        var stepB = new StepRequest { TemplateId = "shared", Name = "Version Two" };

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [stepA, stepB]
        };

        var buildResult = _builder.Build(new CreateProgramRequest { Name = "Inconsistent clone test", RootGroup = root });

        var result = _validator.Validate(buildResult);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "INCONSISTENT_TEMPLATE");
    }
}