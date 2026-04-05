using System.Text;
using System.Text.Json;
using AutoResearchClaw.Application.Interfaces;
using AutoResearchClaw.Domain.Entities;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace AutoResearchClaw.Infrastructure.Services;

public class SandboxService : ISandboxService
{
    private readonly DockerClient _dockerClient;
    private readonly string _artifactsPath;

    public SandboxService(string artifactsPath = "/artifacts")
    {
        _dockerClient = new DockerClientConfiguration().CreateClient();
        _artifactsPath = artifactsPath;
    }

    public async Task<string> StartContainerAsync(string image, string command)
    {
        var response = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = image,
            Cmd = new[] { "sh", "-c", command },
            HostConfig = new HostConfig
            {
                Binds = new List<string>
                {
                    $"{_artifactsPath}:/workspace"
                },
                Memory = 16L * 1024 * 1024 * 1024,
                NanoCPUs = 4000000000L,
                Privileged = true
            },
            Tty = true,
            AttachStdout = true,
            AttachStderr = true
        });

        await _dockerClient.Containers.StartContainerAsync(response.ID, new ContainerStartParameters());
        return response.ID;
    }

    public async Task StopContainerAsync(string containerId)
    {
        try
        {
            await _dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters { WaitBeforeKillSeconds = 30 });
            await _dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters());
        }
        catch { }
    }

    public async Task<bool> IsContainerRunningAsync(string containerId)
    {
        try
        {
            var inspect = await _dockerClient.Containers.InspectContainerAsync(containerId);
            return inspect.State.Running;
        }
        catch { return false; }
    }

    public async IAsyncEnumerable<string> GetContainerLogsAsync(string containerId, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var stream = await _dockerClient.Containers.GetContainerLogsAsync(containerId, false, new ContainerLogsParameters
        {
            ShowStdout = true,
            ShowStderr = true,
            Follow = true
        });

        var buffer = new byte[4096];
        while (!cancellationToken.IsCancellationRequested)
        {
            var (streamType, data) = await ReadLogEntryAsync(stream, buffer, cancellationToken);
            if (data != null) yield return Encoding.UTF8.GetString(data);
        }
    }

    private async Task<(byte streamType, byte[]? data)> ReadLogEntryAsync(MultiplexedStream stream, byte[] buffer, CancellationToken ct)
    {
        var result = await stream.ReadOutputAsync(buffer, 0, buffer.Length, ct);
        return ((byte)result.Target, buffer.Take(result.Count).ToArray());
    }
}

public class RedisEventPublisher : IEventPublisher
{
    private readonly ISubscriber _subscriber;
    private readonly IConnectionMultiplexer _connection;

    public RedisEventPublisher(IConnectionMultiplexer connection)
    {
        _connection = connection;
        _subscriber = connection.GetSubscriber();
    }

    public async Task PublishAsync(string eventType, string data)
    {
        await _subscriber.PublishAsync(RedisChannel.Literal($"autoresearch:{eventType}"), data);
    }

    public Task SubscribeAsync(string channel, Action<string> handler)
    {
        _subscriber.Subscribe(RedisChannel.Literal(channel), (ch, message) => handler(message.ToString() ?? ""));
        return Task.CompletedTask;
    }
}