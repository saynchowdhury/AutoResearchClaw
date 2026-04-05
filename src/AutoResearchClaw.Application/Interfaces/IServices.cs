using AutoResearchClaw.Domain.Entities;

namespace AutoResearchClaw.Application.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id);
    Task<Project> CreateAsync(Project project);
    Task<Project> UpdateAsync(Project project);
    Task<IEnumerable<Project>> GetAllAsync();
}

public interface IRunRepository
{
    Task<Run?> GetByIdAsync(Guid id);
    Task<Run> CreateAsync(Run run);
    Task<Run> UpdateAsync(Run run);
    Task<IEnumerable<Run>> GetByProjectIdAsync(Guid projectId);
    Task<Run?> GetActiveRunAsync();
}

public interface IRunLogRepository
{
    Task<RunLog> CreateAsync(RunLog log);
    Task<IEnumerable<RunLog>> GetByRunIdAsync(Guid runId);
}

public interface ILlmProviderService
{
    Task<string> CompleteAsync(string prompt, string? systemPrompt = null);
    Task<string> StreamCompleteAsync(string prompt, Action<string> onChunk, string? systemPrompt = null);
}

public interface ISandboxService
{
    Task<string> StartContainerAsync(string image, string command);
    Task StopContainerAsync(string containerId);
    Task<bool> IsContainerRunningAsync(string containerId);
    IAsyncEnumerable<string> GetContainerLogsAsync(string containerId, CancellationToken cancellationToken);
}

public interface IEventPublisher
{
    Task PublishAsync(string eventType, string data);
    Task SubscribeAsync(string channel, Action<string> handler);
}