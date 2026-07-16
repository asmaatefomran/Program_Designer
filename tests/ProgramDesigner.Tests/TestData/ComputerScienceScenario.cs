using ProgramDesigner.Api.Domain.Enums;
using ProgramDesigner.Api.DTOs.Requests;

namespace ProgramDesigner.Tests.TestData;

// the given example from the file
public static class ComputerScienceScenario
{
    public static CreateProgramRequest Build(string finalCapstonePrerequisiteTemplateId = "group-major")
    {
        var introToComputing = new StepRequest { TemplateId = "step-intro", Name = "Introduction to Computing" };
        var mathForComputing = new StepRequest { TemplateId = "step-math", Name = "Mathematics for Computing" };

        var foundations = new GroupRequest
        {
            TemplateId = "group-foundations",
            Name = "Foundations",
            GroupType = GroupType.All,
            Children = [introToComputing, mathForComputing]
        };

        var mlBasics = new StepRequest { TemplateId = "step-ml-basics", Name = "Machine Learning Basics" };
        var computerVision = new StepRequest { TemplateId = "step-cv", Name = "Computer Vision" };
        var nlp = new StepRequest { TemplateId = "step-nlp", Name = "Natural Language Processing" };
        var robotics = new StepRequest { TemplateId = "step-robotics", Name = "Robotics" };

        var electives = new GroupRequest
        {
            TemplateId = "group-electives",
            Name = "Electives",
            GroupType = GroupType.Choice,
            RequiredChoiceCount = 2,
            Children = [computerVision, nlp, robotics]
        };

        var aiCapstone = new StepRequest
        {
            TemplateId = "step-ai-capstone",
            Name = "AI Capstone",
            PrerequisiteTemplateIds = ["group-electives"]
        };

        var aiTrack = new GroupRequest
        {
            TemplateId = "group-ai",
            Name = "AI",
            GroupType = GroupType.All,
            Children = [mlBasics, electives, aiCapstone]
        };

        var networks = new StepRequest { TemplateId = "step-networks", Name = "Networks & Security" };
        var sysAdmin = new StepRequest { TemplateId = "step-sysadmin", Name = "Systems Administration" };

        var itTrack = new GroupRequest
        {
            TemplateId = "group-it",
            Name = "IT",
            GroupType = GroupType.All,
            Children = [networks, sysAdmin]
        };

        var algorithms = new StepRequest { TemplateId = "step-algorithms", Name = "Algorithms & Data Structures" };
        var softwareEng = new StepRequest { TemplateId = "step-swe", Name = "Software Engineering" };

        var programmingTrack = new GroupRequest
        {
            TemplateId = "group-programming",
            Name = "Programming",
            GroupType = GroupType.All,
            Children = [algorithms, softwareEng]
        };

        var major = new GroupRequest
        {
            TemplateId = "group-major",
            Name = "Major",
            GroupType = GroupType.Choice,
            RequiredChoiceCount = 1,
            PrerequisiteTemplateIds = ["group-foundations"],
            Children = [aiTrack, itTrack, programmingTrack]
        };

        var finalCapstone = new StepRequest
        {
            TemplateId = "step-final-capstone",
            Name = "Final Capstone",
            PrerequisiteTemplateIds = [finalCapstonePrerequisiteTemplateId]
        };

        var root = new GroupRequest
        {
            TemplateId = "group-root",
            Name = "Computer Science",
            GroupType = GroupType.All,
            Children = [foundations, major, finalCapstone]
        };

        return new CreateProgramRequest { Name = "Computer Science", RootGroup = root };
    }
}