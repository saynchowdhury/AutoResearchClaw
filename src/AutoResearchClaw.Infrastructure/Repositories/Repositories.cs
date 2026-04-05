using AutoResearchClaw.Application.Interfaces;
using AutoResearchClaw.Domain.Entities;
using AutoResearchClaw.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoResearchClaw.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;
    public ProjectRepository(AppDbContext context) => _context = context;

    public async Task<Project?> GetByIdAsync(Guid id) => await _context.Projects
        .Include(p => p.Runs).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Project> CreateAsync(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<IEnumerable<Project>> GetAllAsync() => await _context.Projects.ToListAsync();
}

public class RunRepository : IRunRepository
{
    private readonly AppDbContext _context;
    public RunRepository(AppDbContext context) => _context = context;

    public async Task<Run?> GetByIdAsync(Guid id) => await _context.Runs
        .Include(r => r.Logs).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Run> CreateAsync(Run run)
    {
        _context.Runs.Add(run);
        await _context.SaveChangesAsync();
        return run;
    }

    public async Task<Run> UpdateAsync(Run run)
    {
        _context.Runs.Update(run);
        await _context.SaveChangesAsync();
        return run;
    }

    public async Task<IEnumerable<Run>> GetByProjectIdAsync(Guid projectId) =>
        await _context.Runs.Where(r => r.ProjectId == projectId).ToListAsync();

    public async Task<Run?> GetActiveRunAsync() => await _context.Runs
        .Where(r => r.Status == RunStatus.Running || r.Status == RunStatus.Starting)
        .FirstOrDefaultAsync();
}

public class RunLogRepository : IRunLogRepository
{
    private readonly AppDbContext _context;
    public RunLogRepository(AppDbContext context) => _context = context;

    public async Task<RunLog> CreateAsync(RunLog log)
    {
        _context.RunLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<IEnumerable<RunLog>> GetByRunIdAsync(Guid runId) =>
        await _context.RunLogs.Where(l => l.RunId == runId).OrderBy(l => l.Timestamp).ToListAsync();
}