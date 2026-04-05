using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace AutoResearchClaw.Api.Hubs;

public class EventsHub : Hub
{
    public async Task JoinRun(string runId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, runId);
    }

    public async Task LeaveRun(string runId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, runId);
    }

    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}

public class RunEventPublisher
{
    private readonly IHubContext<EventsHub> _hubContext;

    public RunEventPublisher(IHubContext<EventsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishAgentThoughtAsync(string runId, string thought)
    {
        await _hubContext.Clients.Group(runId).SendAsync("AgentThought", new { runId, thought, timestamp = DateTime.UtcNow });
    }

    public async Task PublishTerminalLogAsync(string runId, string log)
    {
        await _hubContext.Clients.Group(runId).SendAsync("TerminalLog", new { runId, log, timestamp = DateTime.UtcNow });
    }

    public async Task PublishStageChangeAsync(string runId, int stage, string stageName)
    {
        await _hubContext.Clients.Group(runId).SendAsync("StageChange", new { runId, stage, stageName, timestamp = DateTime.UtcNow });
    }

    public async Task PublishArtifactUpdateAsync(string runId, string filePath, string action)
    {
        await _hubContext.Clients.Group(runId).SendAsync("ArtifactUpdate", new { runId, filePath, action, timestamp = DateTime.UtcNow });
    }
}