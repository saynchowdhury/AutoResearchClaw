using AutoResearchClaw.Application.Interfaces;
using AutoResearchClaw.Domain.Entities;
using AutoResearchClaw.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoResearchClaw.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ILlmProviderRepository _providerRepository;

    public SettingsController(ILlmProviderRepository providerRepository)
        => _providerRepository = providerRepository;

    [HttpGet("providers")]
    public async Task<ActionResult<IEnumerable<LlmProvider>>> GetProviders()
        => Ok(await _providerRepository.GetAllAsync());

    [HttpGet("providers/default")]
    public async Task<ActionResult<LlmProvider>> GetDefaultProvider()
    {
        var provider = await _providerRepository.GetDefaultAsync();
        return provider == null ? NotFound() : Ok(provider);
    }

    [HttpPost("providers")]
    public async Task<ActionResult<LlmProvider>> CreateProvider([FromBody] LlmProviderCreateRequest request)
    {
        if (request.IsDefault)
        {
            var existing = await _providerRepository.GetDefaultAsync();
            if (existing != null)
            {
                existing.IsDefault = false;
                await _providerRepository.UpdateAsync(existing);
            }
        }

        var provider = new LlmProvider
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = request.Type,
            ApiKey = request.ApiKey,
            BaseUrl = request.BaseUrl ?? GetDefaultBaseUrl(request.Type),
            Model = request.Model,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow
        };

        return Ok(await _providerRepository.CreateAsync(provider));
    }

    [HttpPut("providers/{id:guid}")]
    public async Task<ActionResult<LlmProvider>> UpdateProvider(Guid id, [FromBody] LlmProviderUpdateRequest request)
    {
        var providers = await _providerRepository.GetAllAsync();
        var provider = providers.FirstOrDefault(p => p.Id == id);
        if (provider == null) return NotFound();

        if (request.IsDefault && !provider.IsDefault)
        {
            var existing = await _providerRepository.GetDefaultAsync();
            if (existing != null)
            {
                existing.IsDefault = false;
                await _providerRepository.UpdateAsync(existing);
            }
        }

        provider.Name = request.Name ?? provider.Name;
        provider.ApiKey = request.ApiKey ?? provider.ApiKey;
        provider.BaseUrl = request.BaseUrl ?? provider.BaseUrl;
        provider.Model = request.Model ?? provider.Model;
        provider.IsDefault = request.IsDefault;

        return Ok(await _providerRepository.UpdateAsync(provider));
    }

    [HttpDelete("providers/{id:guid}")]
    public async Task<ActionResult> DeleteProvider(Guid id)
    {
        var providers = await _providerRepository.GetAllAsync();
        var provider = providers.FirstOrDefault(p => p.Id == id);
        if (provider == null) return NotFound();

        provider.ApiKey = null;
        await _providerRepository.UpdateAsync(provider);
        return Ok();
    }

    private static string GetDefaultBaseUrl(ProviderType type) => type switch
    {
        ProviderType.OpenAI => "https://api.openai.com",
        ProviderType.Anthropic => "https://api.anthropic.com",
        ProviderType.GoogleGemini => "https://generativelanguage.googleapis.com",
        ProviderType.Ollama => "http://localhost:11434",
        ProviderType.Vllm => "http://localhost:8000",
        ProviderType.LmStudio => "http://localhost:1234/v1",
        ProviderType.Novita => "https://api.novita.ai",
        ProviderType.DeepSeek => "https://api.deepseek.com",
        _ => "http://localhost:11434"
    };
}

public record LlmProviderCreateRequest(
    string Name,
    ProviderType Type,
    string? ApiKey,
    string? BaseUrl,
    string? Model,
    bool IsDefault);

public record LlmProviderUpdateRequest(
    string? Name,
    string? ApiKey,
    string? BaseUrl,
    string? Model,
    bool IsDefault);