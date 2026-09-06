using System.Security.Claims;
using Aegis.Api.Data;
using Aegis.Api.DTOs;
using Aegis.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aegis.Api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/tasks")]
    [Authorize]
    public class ProjectTasksController : ControllerBase
    {
        private readonly AegisDbContext _dbContext;

        public ProjectTasksController(AegisDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectTaskResponse>>> GetTasks(
            int projectId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var hasAccess =
                await UserHasProjectAccess(
                    projectId,
                    userId.Value);

            if (!hasAccess)
            {
                return NotFound();
            }

            var tasks = await _dbContext.ProjectTasks
                .Where(task => task.ProjectId == projectId)
                .OrderByDescending(task => task.CreatedAt)
                .Select(task => new ProjectTaskResponse
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status,
                    CreatedAt = task.CreatedAt,
                    ProjectId = task.ProjectId
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("{taskId}")]
        public async Task<ActionResult<ProjectTaskResponse>> GetTask(
            int projectId,
            int taskId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var hasAccess =
                await UserHasProjectAccess(
                    projectId,
                    userId.Value);

            if (!hasAccess)
            {
                return NotFound();
            }

            var task = await _dbContext.ProjectTasks
                .Where(task =>
                    task.ProjectId == projectId &&
                    task.Id == taskId)
                .Select(task => new ProjectTaskResponse
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status,
                    CreatedAt = task.CreatedAt,
                    ProjectId = task.ProjectId
                })
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound();
            }

            return Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectTaskResponse>> CreateTask(
            int projectId,
            CreateProjectTaskRequest request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var canEdit =
                await UserCanEditProject(
                    projectId,
                    userId.Value);

            if (!canEdit)
            {
                return NotFound();
            }

            var title = request.Title.Trim();
            var description = request.Description.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Task title cannot be empty.");
            }

            var task = new ProjectTask
            {
                Title = title,
                Description = description,
                Status = ProjectTaskStatuses.Todo,
                CreatedAt = DateTime.UtcNow,
                ProjectId = projectId
            };

            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync();

            try
            {
                _dbContext.ProjectTasks.Add(task);

                await _dbContext.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    UserId = userId.Value,
                    ProjectId = projectId,
                    Action = "TaskCreated",
                    Details =
                        $"Created task {task.Id} '{task.Title}'."
                };

                _dbContext.AuditLogs.Add(auditLog);

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }


            var response = new ProjectTaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                CreatedAt = task.CreatedAt,
                ProjectId = task.ProjectId
            };

            return CreatedAtAction(
                nameof(GetTask),
                new
                {
                    projectId,
                    taskId = task.Id
                },
                response);
        }

        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTask(
            int projectId,
            int taskId,
            UpdateProjectTaskRequest request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var canEdit =
                await UserCanEditProject(
                    projectId,
                    userId.Value);

            if (!canEdit)
            {
                return NotFound();
            }

            if (!IsValidStatus(request.Status))
            {
                return BadRequest("Invalid task status.");
            }

            var title = request.Title.Trim();
            var description = request.Description.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Task title cannot be empty.");
            }

            var task = await _dbContext.ProjectTasks
                .FirstOrDefaultAsync(task =>
                    task.ProjectId == projectId &&
                    task.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            var oldStatus = task.Status;

            task.Title = title;
            task.Description = description;
            task.Status = request.Status;

            var auditLog = new AuditLog
            {
                UserId = userId.Value,
                ProjectId = projectId,
                Action = "TaskUpdated",
                Details =
                    $"Updated task {task.Id} '{task.Title}'. " +
                    $"Status changed from {oldStatus} to {task.Status}."
            };

            _dbContext.AuditLogs.Add(auditLog);

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(
            int projectId,
            int taskId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var isOwner = await _dbContext.Projects
                .AnyAsync(project =>
                    project.Id == projectId &&
                    project.OwnerId == userId.Value);

            if (!isOwner)
            {
                return NotFound();
            }

            var task = await _dbContext.ProjectTasks
                .FirstOrDefaultAsync(task =>
                    task.ProjectId == projectId &&
                    task.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            var taskIdForLog = task.Id;
            var taskTitleForLog = task.Title;

            _dbContext.ProjectTasks.Remove(task);

            var auditLog = new AuditLog
            {
                UserId = userId.Value,
                ProjectId = projectId,
                Action = "TaskDeleted",
                Details =
                    $"Deleted task {taskIdForLog} '{taskTitleForLog}'."
            };

            _dbContext.AuditLogs.Add(auditLog);

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return null;
            }

            return userId;
        }

        private async Task<bool> UserHasProjectAccess(
            int projectId,
            int userId)
        {
            return await _dbContext.Projects
                .AnyAsync(project =>
                    project.Id == projectId &&
                    (
                        project.OwnerId == userId ||
                        project.Members.Any(member =>
                            member.UserId == userId)
                    ));
        }

        private async Task<bool> UserCanEditProject(
            int projectId,
            int userId)
        {
            return await _dbContext.Projects
                .AnyAsync(project =>
                    project.Id == projectId &&
                    (
                        project.OwnerId == userId ||
                        project.Members.Any(member =>
                            member.UserId == userId &&
                            member.Role == ProjectRoles.Editor)
                    ));
        }

        private static bool IsValidStatus(
            string status)
        {
            return
                status == ProjectTaskStatuses.Todo ||
                status == ProjectTaskStatuses.InProgress ||
                status == ProjectTaskStatuses.Done;
        }
    }
}  