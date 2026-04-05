using AutoResearchClaw.Api.Hubs;
using AutoResearchClaw.Application.Interfaces;
using AutoResearchClaw.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AutoResearchClaw.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RunsController : ControllerBase
{
    private readonly IRunRepository _runRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IRunLogRepository _runLogRepository;
    private readonly RunEventPublisher _eventPublisher;

    public RunsController(
        IRunRepository runRepository,
        IProjectRepository projectRepository,
        IRunLogRepository runLogRepository,
        RunEventPublisher eventPublisher)
    {
        _runRepository = runRepository;
        _projectRepository = projectRepository;
        _runLogRepository = runLogRepository;
        _eventPublisher = eventPublisher;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Run>>> GetAll([FromQuery] Guid? projectId)
    {
        var runs = projectId.HasValue
            ? await _runRepository.GetByProjectIdAsync(projectId.Value)
            : await _runRepository.GetByProjectIdAsync(Guid.Empty);
        return Ok(runs);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Run>> Get(Guid id)
    {
        var run = await _runRepository.GetByIdAsync(id);
        return run == null ? NotFound() : Ok(run);
    }

    [HttpGet("{id:guid}/logs")]
    public async Task<ActionResult<IEnumerable<RunLog>>> GetLogs(Guid id)
        => Ok(await _runLogRepository.GetByRunIdAsync(id));

    [HttpPost("start")]
    public async Task<ActionResult<Run>> Start([FromBody] RunStartRequest request)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId);
        if (project == null) return NotFound("Project not found");

        var run = new Run
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Topic = request.Topic,
            Status = RunStatus.Queued,
            CurrentStage = 0,
            StartedAt = DateTime.UtcNow
        };

        var created = await _runRepository.CreateAsync(run);

        await _eventPublisher.PublishStageChangeAsync(run.Id.ToString(), 0, "Queued");

        _ = Task.Run(async () => await ProcessRunAsync(run.Id, request));

        return Accepted(created);
    }

    [HttpPost("{id:guid}/pause")]
    public async Task<ActionResult<Run>> Pause(Guid id)
    {
        var run = await _runRepository.GetByIdAsync(id);
        if (run == null) return NotFound();

        run.Status = RunStatus.Paused;
        return Ok(await _runRepository.UpdateAsync(run));
    }

    [HttpPost("{id:guid}/resume")]
    public async Task<ActionResult<Run>> Resume(Guid id)
    {
        var run = await _runRepository.GetByIdAsync(id);
        if (run == null) return NotFound();

        run.Status = RunStatus.Running;
        return Ok(await _runRepository.UpdateAsync(run));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<Run>> Cancel(Guid id)
    {
        var run = await _runRepository.GetByIdAsync(id);
        if (run == null) return NotFound();

        run.Status = RunStatus.Cancelled;
        return Ok(await _runRepository.UpdateAsync(run));
    }

    private async Task ProcessRunAsync(Guid runId, RunStartRequest request)
    {
        var run = await _runRepository.GetByIdAsync(runId);
        if (run == null) return;

        try
        {
            run.Status = RunStatus.Running;
            await _runRepository.UpdateAsync(run);
            await _eventPublisher.PublishStageChangeAsync(runId.ToString(), 1, "Topic Initialization");

            await _runLogRepository.CreateAsync(new RunLog
            {
                Id = Guid.NewGuid(),
                RunId = runId,
                Type = LogType.AgentThought,
                Content = $"Starting research on: {run.Topic}",
                Timestamp = DateTime.UtcNow
            });

            await _eventPublisher.PublishAgentThoughtAsync(runId.ToString(), $"Starting research on: {run.Topic}");

            run.CurrentStage = 23;
            run.Status = RunStatus.Completed;
            run.CompletedAt = DateTime.UtcNow;
            await _runRepository.UpdateAsync(run);

            await _eventPublisher.PublishStageChangeAsync(runId.ToString(), 23, "Completed");
            await _eventPublisher.PublishArtifactUpdateAsync(runId.ToString(), "/paper.pdf", "created");
        }
        catch (Exception ex)
        {
            run.Status = RunStatus.Failed;
            run.ErrorMessage = ex.Message;
            run.CompletedAt = DateTime.UtcNow;
            await _runRepository.UpdateAsync(run);

            await _eventPublisher.PublishStageChangeAsync(runId.ToString(), run.CurrentStage, "Failed");
        }
    }
}

public record RunStartRequest(Guid ProjectId, string Topic);