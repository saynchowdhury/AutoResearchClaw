using AutoResearchClaw.Application.Interfaces;
using AutoResearchClaw.Domain.Entities;
using AutoResearchClaw.Infrastructure.Data;
using AutoResearchClaw.Infrastructure.Repositories;
using AutoResearchClaw.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost"));

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IRunRepository, RunRepository>();
builder.Services.AddScoped<IRunLogRepository, RunLogRepository>();
builder.Services.AddScoped<ILlmProviderService, LlmProviderService>();
builder.Services.AddSingleton<ISandboxService, SandboxService>();
builder.Services.AddHostedService<ResearchWorker>();

var host = builder.Build();
host.Run();

public class ResearchWorker : BackgroundService
{
    private readonly ILogger<ResearchWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISandboxService _sandboxService;

    public ResearchWorker(
        ILogger<ResearchWorker> logger,
        IServiceProvider serviceProvider,
        ISandboxService sandboxService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _sandboxService = sandboxService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Research Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var runRepository = scope.ServiceProvider.GetRequiredService<IRunRepository>();

                var activeRun = await runRepository.GetActiveRunAsync();
                if (activeRun != null)
                {
                    await ProcessRunAsync(activeRun, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in worker loop");
            }

            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task ProcessRunAsync(Run run, CancellationToken ct)
    {
        _logger.LogInformation("Processing run {RunId}", run.Id);

        try
        {
            var stages = new[]
            {
                "Topic Initialization", "Problem Decomposition", "Search Strategy",
                "Literature Collect", "Literature Screen", "Knowledge Extract",
                "Synthesis", "Hypothesis Generation", "Experiment Design",
                "Code Generation", "Resource Planning", "Experiment Run",
                "Iterative Refinement", "Result Analysis", "Research Decision",
                "Paper Outline", "Paper Draft", "Peer Review", "Paper Revision",
                "Quality Gate", "Knowledge Archive", "Export & Publish", "Citation Verify"
            };

            for (int i = 0; i < stages.Length; i++)
            {
                run.CurrentStage = i + 1;
                _logger.LogInformation("Stage {Stage}: {StageName}", i + 1, stages[i]);
                await Task.Delay(2000, ct);
            }

            run.Status = RunStatus.Completed;
            run.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            run.Status = RunStatus.Failed;
            run.ErrorMessage = ex.Message;
            run.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "Run {RunId} failed", run.Id);
        }
    }
}