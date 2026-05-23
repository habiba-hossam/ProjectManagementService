using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementAPI.Core.Application.Common.Models;
using ProjectManagementAPI.Core.Application.Features.Tasks;
using ProjectManagementAPI.Core.Application.Features.Tasks.Commands.CreateTask;
using ProjectManagementAPI.Core.Application.Features.Tasks.Commands.DeleteTask;
using ProjectManagementAPI.Core.Application.Features.Tasks.Commands.UpdateTaskStatus;
using ProjectManagementAPI.Core.Application.Features.Tasks.Queries.GetTasksByProject;

namespace ProjectManagementAPI.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:guid}/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get all tasks for a project.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<TaskDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProject(Guid projectId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var parameters = new GetTasksByProjectQuery(projectId, pageNumber, pageSize);
        var result = await _mediator.Send(parameters, cancellationToken);
        return Ok(ApiResponse<PaginatedList<TaskDto>>.Ok(result));
    }

    /// <summary>Create a new task within a project.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTaskCommand(projectId, request.Title, request.Description, request.DueDate, request.Priority);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByProject), new { projectId }, ApiResponse<TaskDto>.Ok(result, "Task created successfully."));
    }

    /// <summary>Update the status of a task.</summary>
    [HttpPatch("{taskId:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid projectId, Guid taskId, [FromBody] UpdateTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTaskStatusCommand(projectId, taskId, request.Status);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<TaskDto>.Ok(result, "Task status updated successfully."));
    }

    /// <summary>Delete a task.</summary>
    [HttpDelete("{taskId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid projectId, Guid taskId, CancellationToken cancellationToken)
    {
        var command = new DeleteTaskCommand(projectId, taskId);
        await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok("Task deleted successfully."));
    }
}
