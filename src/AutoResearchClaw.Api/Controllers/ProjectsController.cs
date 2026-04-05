using AutoResearchClaw.Application.Interfaces;
using AutoResearchClaw.Domain.Entities;
using AutoResearchClaw.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AutoResearchClaw.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;
    public ProjectsController(IProjectRepository projectRepository) => _projectRepository = projectRepository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetAll()
        => Ok(await _projectRepository.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Project>> Get(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        return project == null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<Project>> Create([FromBody] ProjectCreateRequest request)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            GlobalContext = request.GlobalContext,
            Constraints = request.Constraints,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var created = await _projectRepository.CreateAsync(project);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Project>> Update(Guid id, [FromBody] ProjectUpdateRequest request)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) return NotFound();

        project.Name = request.Name ?? project.Name;
        project.Description = request.Description ?? project.Description;
        project.GlobalContext = request.GlobalContext ?? project.GlobalContext;
        project.Constraints = request.Constraints ?? project.Constraints;
        project.UpdatedAt = DateTime.UtcNow;

        return Ok(await _projectRepository.UpdateAsync(project));
    }
}

public record ProjectCreateRequest(string Name, string Description, string GlobalContext, string? Constraints);
public record ProjectUpdateRequest(string? Name, string? Description, string? GlobalContext, string? Constraints);