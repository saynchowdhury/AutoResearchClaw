namespace AutoResearchClaw.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GlobalContext { get; set; } = string.Empty;
    public string? Constraints { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Run> Runs { get; set; } = new List<Run>();
}

public class Run
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public RunStatus Status { get; set; }
    public int CurrentStage { get; set; }
    public string? ContainerId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Project Project { get; set; } = null!;
    public ICollection<RunLog> Logs { get; set; } = new List<RunLog>();
}

public enum RunStatus
{
    Queued,
    Starting,
    Running,
    Paused,
    Completed,
    Failed,
    Cancelled
}

public class RunLog
{
    public Guid Id { get; set; }
    public Guid RunId { get; set; }
    public LogType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Run Run { get; set; } = null!;
}

public enum LogType
{
    AgentThought,
    TerminalLog,
    StageChange,
    ArtifactUpdate,
    Error
}

public class LlmProvider
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProviderType Type { get; set; }
    public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; }
    public string? Model { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum ProviderType
{
    OpenAI,
    Anthropic,
    GoogleGemini,
    Ollama,
    Vllm,
    LmStudio,
    Novita,
    DeepSeek
}