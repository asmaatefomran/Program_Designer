using ProgramDesigner.Api.Domain.Entities;
using ProgramDesigner.Api.DTOs.Responses;

namespace ProgramDesigner.Api.DTOs.Mapping;

public static class ProgramMapper
{
    public static ProgramResponse ToResponse(LearningProgram program)
    {
        return new ProgramResponse
        {
            Id = program.Id,
            Name = program.Name,
            RootGroup = (GroupResponse)ToResponse((Node)program.RootGroup)
        };
    }

    private static NodeResponse ToResponse(Node node)
    {
        var prerequisiteTemplateIds = node.Prerequisites
            .Select(p => p.PrerequisiteTemplateId)
            .ToList();

        return node switch
        {
            Group group => new GroupResponse
            {
                Id = group.Id,
                TemplateId = group.TemplateId,
                Name = group.Name,
                GroupType = group.GroupType,
                RequiredSelections = group.RequiredSelections,
                PrerequisiteTemplateIds = prerequisiteTemplateIds,
                Children = group.Children
                    .OrderBy(c => c.OrderIndex)
                    .Select(ToResponse)
                    .ToList()
            },
            Step step => new StepResponse
            {
                Id = step.Id,
                TemplateId = step.TemplateId,
                Name = step.Name,
                PrerequisiteTemplateIds = prerequisiteTemplateIds
            },
            _ => throw new InvalidOperationException("Unknown node type.")
        };
    }
}