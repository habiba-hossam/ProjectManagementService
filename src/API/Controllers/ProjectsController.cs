using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementAPI.Core.Application.Common.Models;
using ProjectManagementAPI.Core.Application.Features.Projects;
using ProjectManagementAPI.Core.Application.Features.Projects.Commands.CreateProject;
using ProjectManagementAPI.Core.Application.Features.Projects.Commands.DeleteProject;
using ProjectManagementAPI.Core.Application.Features.Projects.Commands.UpdateProject;
using ProjectManagementAPI.Core.Application.Features.Projects.Queries.GetAllProjects;
using ProjectManagementAPI.Core.Application.Features.Projects.Queries.GetProjectById;

namespace ProjectManagementAPI.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get all projects for the authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<ProjectDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var parameters = new GetAllProjectsQuery(pageNumber, pageSize);
        var result = await _mediator.Send(parameters, cancellationToken);
        return Ok(ApiResponse<PaginatedList<ProjectDto>>.Ok(result));
    }

    /// <summary>Get a project by its ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {        
        var result = await _mediator.Send(new GetProjectByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<ProjectDto>.Ok(result));
    }

    /// <summary>Create a new project.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand(request.Name, request.Description);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ProjectDto>.Ok(result, "Project created successfully."));
    }

    /// <summary>Update an existing project.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProjectCommand(id, request.Name, request.Description);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<ProjectDto>.Ok(result, "Project updated successfully."));
    }

    /// <summary>Delete a project.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteProjectCommand(id);
        await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok("Project deleted successfully."));
    }
}
