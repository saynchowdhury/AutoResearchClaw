using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AutoResearchClaw.Application.Interfaces;
using AutoResearchClaw.Domain.Entities;
using AutoResearchClaw.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoResearchClaw.Infrastructure.Services;

public class LlmProviderService : ILlmProviderService
{
    private readonly HttpClient _httpClient;
    private readonly ILlmProviderRepository _providerRepository;

    public LlmProviderService(HttpClient httpClient, ILlmProviderRepository providerRepository)
    {
        _httpClient = httpClient;
        _providerRepository = providerRepository;
    }

    public async Task<string> CompleteAsync(string prompt, string? systemPrompt = null)
    {
        var provider = await _providerRepository.GetDefaultAsync();
        if (provider == null) throw new InvalidOperationException("No LLM provider configured");

        return provider.Type switch
        {
            ProviderType.OpenAI => await OpenAICompleteAsync(provider, prompt, systemPrompt),
            ProviderType.Anthropic => await AnthropicCompleteAsync(provider, prompt, systemPrompt),
            ProviderType.Ollama => await OllamaCompleteAsync(provider, prompt, systemPrompt),
            ProviderType.Vllm => await VllmCompleteAsync(provider, prompt, systemPrompt),
            _ => throw new NotSupportedException($"Provider {provider.Type} not supported")
        };
    }

    public async Task<string> StreamCompleteAsync(string prompt, Action<string> onChunk, string? systemPrompt = null)
    {
        var provider = await _providerRepository.GetDefaultAsync();
        if (provider == null) throw new InvalidOperationException("No LLM provider configured");

        return provider.Type switch
        {
            ProviderType.OpenAI => await OpenAIStreamAsync(provider, prompt, onChunk, systemPrompt),
            ProviderType.Anthropic => await AnthropicStreamAsync(provider, prompt, onChunk, systemPrompt),
            ProviderType.Ollama => await OllamaStreamAsync(provider, prompt, onChunk, systemPrompt),
            _ => throw new NotSupportedException($"Streaming not supported for {provider.Type}")
        };
    }

    private async Task<string> OpenAICompleteAsync(LlmProvider provider, string prompt, string? systemPrompt)
    {
        var request = new
        {
            model = provider.Model ?? "gpt-4",
            messages = new[]
            {
                systemPrompt != null ? new { role = "system", content = systemPrompt } : null!,
                new { role = "user", content = prompt }
            }.Where(m => m != null).ToArray(),
            temperature = 0.7
        };

        _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", provider.ApiKey);
        var response = await _httpClient.PostAsJsonAsync($"{provider.BaseUrl}/chat/completions", request);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
    }

    private async Task<string> AnthropicCompleteAsync(LlmProvider provider, string prompt, string? systemPrompt)
    {
        var request = new
        {
            model = provider.Model ?? "claude-3-sonnet-20240229",
            max_tokens = 4096,
            messages = new[] { new { role = "user", content = prompt } },
            system = systemPrompt
        };

        _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", provider.ApiKey);
        var response = await _httpClient.PostAsJsonAsync($"{provider.BaseUrl}/v1/messages", request);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("content")[0].GetProperty("text").GetString() ?? "";
    }

    private async Task<string> OllamaCompleteAsync(LlmProvider provider, string prompt, string? systemPrompt)
    {
        var request = new
        {
            model = provider.Model ?? "llama2",
            prompt = $"{(systemPrompt != null ? $"{systemPrompt}\n\n" : "")}{prompt}",
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync($"{provider.BaseUrl}/api/generate", request);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("response").GetString() ?? "";
    }

    private async Task<string> VllmCompleteAsync(LlmProvider provider, string prompt, string? systemPrompt)
    {
        var request = new
        {
            model = provider.Model ?? "llama2",
            prompt = $"{(systemPrompt != null ? $"{systemPrompt}\n\n" : "")}{prompt}",
            temperature = 0.7
        };

        var response = await _httpClient.PostAsJsonAsync($"{provider.BaseUrl}/v1/completions", request);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("choices")[0].GetProperty("text").GetString() ?? "";
    }

    private async Task<string> OpenAIStreamAsync(LlmProvider provider, string prompt, Action<string> onChunk, string? systemPrompt)
    {
        var request = new
        {
            model = provider.Model ?? "gpt-4",
            messages = new[]
            {
                systemPrompt != null ? new { role = "system", content = systemPrompt } : null!,
                new { role = "user", content = prompt }
            }.Where(m => m != null).ToArray(),
            stream = true
        };

        _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", provider.ApiKey);
        var response = await _httpClient.PostAsJsonAsync($"{provider.BaseUrl}/chat/completions", request);
        var stream = await response.Content.ReadAsStreamAsync();
        var reader = new StreamReader(stream);
        var fullResponse = new StringBuilder();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line?.StartsWith("data: ") == true && line != "data: [DONE]")
            {
                var json = JsonSerializer.Deserialize<JsonElement>(line[6..]);
                var content = json.GetProperty("choices")[0].GetProperty("delta").GetProperty("content").GetString();
                if (content != null)
                {
                    fullResponse.Append(content);
                    onChunk(content);
                }
            }
        }
        return fullResponse.ToString();
    }

    private async Task<string> AnthropicStreamAsync(LlmProvider provider, string prompt, Action<string> onChunk, string? systemPrompt)
    {
        var request = new
        {
            model = provider.Model ?? "claude-3-sonnet-20240229",
            max_tokens = 4096,
            messages = new[] { new { role = "user", content = prompt } },
            system = systemPrompt,
            stream = true
        };

        _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", provider.ApiKey);
        var response = await _httpClient.PostAsJsonAsync($"{provider.BaseUrl}/v1/messages", request);
        var stream = await response.Content.ReadAsStreamAsync();
        var reader = new StreamReader(stream);
        var fullResponse = new StringBuilder();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line?.StartsWith("data: ") == true)
            {
                var json = JsonSerializer.Deserialize<JsonElement>(line[6..]);
                if (json.TryGetProperty("content", out var content))
                {
                    var text = content[0].GetProperty("text").GetString();
                    if (text != null)
                    {
                        fullResponse.Append(text);
                        onChunk(text);
                    }
                }
            }
        }
        return fullResponse.ToString();
    }

    private async Task<string> OllamaStreamAsync(LlmProvider provider, string prompt, Action<string> onChunk, string? systemPrompt)
    {
        var request = new
        {
            model = provider.Model ?? "llama2",
            prompt = $"{(systemPrompt != null ? $"{systemPrompt}\n\n" : "")}{prompt}",
            stream = true
        };

        var response = await _httpClient.PostAsJsonAsync($"{provider.BaseUrl}/api/generate", request);
        var stream = await response.Content.ReadAsStreamAsync();
        var reader = new StreamReader(stream);
        var fullResponse = new StringBuilder();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (!string.IsNullOrEmpty(line))
            {
                var json = JsonSerializer.Deserialize<JsonElement>(line);
                if (json.TryGetProperty("response", out var responseText))
                {
                    var text = responseText.GetString();
                    if (text != null)
                    {
                        fullResponse.Append(text);
                        onChunk(text);
                    }
                }
            }
        }
        return fullResponse.ToString();
    }
}

public interface ILlmProviderRepository
{
    Task<LlmProvider?> GetDefaultAsync();
    Task<IEnumerable<LlmProvider>> GetAllAsync();
    Task<LlmProvider> CreateAsync(LlmProvider provider);
    Task<LlmProvider> UpdateAsync(LlmProvider provider);
}

public class LlmProviderRepository : ILlmProviderRepository
{
    private readonly AppDbContext _context;
    public LlmProviderRepository(AppDbContext context) => _context = context;

    public async Task<LlmProvider?> GetDefaultAsync() =>
        await _context.LlmProviders.FirstOrDefaultAsync(p => p.IsDefault);

    public async Task<IEnumerable<LlmProvider>> GetAllAsync() =>
        await _context.LlmProviders.ToListAsync();

    public async Task<LlmProvider> CreateAsync(LlmProvider provider)
    {
        _context.LlmProviders.Add(provider);
        await _context.SaveChangesAsync();
        return provider;
    }

    public async Task<LlmProvider> UpdateAsync(LlmProvider provider)
    {
        _context.LlmProviders.Update(provider);
        await _context.SaveChangesAsync();
        return provider;
    }
}