using Microsoft.EntityFrameworkCore;
using ProgramDesigner.Api.Data;
using ProgramDesigner.Api.Domain.Entities;
using ProgramDesigner.Api.Services.Interfaces;

namespace ProgramDesigner.Api.Services;

public class ProgramLoaderService : IProgramLoaderService
{
    private readonly ApplicationDbContext _context;

    public ProgramLoaderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LearningProgram?> LoadProgramAsync(string programId)
    {
        var program = await _context.Programs
            .FirstOrDefaultAsync(p => p.Id == programId);

        if (program is null)
            return null;

        program.RootGroup = await LoadGroupAsync(program.RootGroupId);

        return program;
    }
    
    private async Task<Group> LoadGroupAsync(string groupId)
    {
        var group = await _context.Groups
            .Include(g => g.Children)
            .FirstAsync(g => g.Id == groupId);

        await LoadPrerequisites(group);

        await LoadChildren(group);

        return group;
    }
    
    private async Task LoadChildren(Group group)
    {
        var children = new List<Node>();

        foreach (var child in group.Children)
        {
            children.Add(await LoadNodeAsync(child.Id));
        }

        group.Children = children.OrderBy(c => c.OrderIndex).ToList();
    }
    
    private async Task<Node> LoadNodeAsync(string nodeId)
    {
        var isGroup = await _context.Groups.AnyAsync(g => g.Id == nodeId);

        Node node = isGroup
            ? await _context.Groups.Include(g => g.Children).FirstAsync(g => g.Id == nodeId)
            : await _context.Nodes.FirstAsync(n => n.Id == nodeId);

        await LoadPrerequisites(node);

        if (node is Group group)
            await LoadChildren(group);

        return node;
    }
    
    private async Task LoadPrerequisites(Node node)
    {
        node.Prerequisites = await _context.NodePrerequisites
            .Where(p => p.NodeId == node.Id)
            .Include(p => p.Prerequisite)
            .ToListAsync();
    }

}