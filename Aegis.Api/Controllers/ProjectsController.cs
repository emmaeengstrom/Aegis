using Microsoft.AspNetCore.Authorization;
using Aegis.Api.Data;
using Aegis.Api.DTOs;
using Aegis.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Aegis.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly AegisDbContext _context;

        public ProjectsController(AegisDbContext context)
        {
            _context = context;
        }

        // GET: /api/projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetProjects()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var projects = await _context.Projects
                .Where(project =>
                    project.OwnerId == userId ||
                    project.Members.Any(member => member.UserId == userId)
                )
                .Select(project => new ProjectResponse
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    CreatedAt = project.CreatedAt
                })
                .ToListAsync();

            return Ok(projects);
        }

        // GET: /api/projects/1
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectResponse>> GetProject(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(project =>
                    project.Id == id &&
                    (
                        project.OwnerId == userId ||
                        project.Members.Any(member => member.UserId == userId)
                    )
                );

            if (project == null)
            {
                return NotFound();
            }

            var response = new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt
            };

            return Ok(response);
        }

        // POST: /api/projects
        [HttpPost]
        public async Task<ActionResult<ProjectResponse>> CreateProject(
            CreateProjectRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
                OwnerId = userId
            };

            _context.Projects.Add(project);

            await _context.SaveChangesAsync();

            var response = new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt
            };

            return CreatedAtAction(
                nameof(GetProject),
                new { id = project.Id },
                response
            );
        }

        // PUT: /api/projects/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(
            int id,
            UpdateProjectRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(project =>
                    project.Id == id &&
                    project.OwnerId == userId
                );

            if (project == null)
            {
                return NotFound();
            }

            project.Name = request.Name;
            project.Description = request.Description;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: /api/projects/1/members
        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddProjectMember(
            int id,
            AddProjectMemberRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(project =>
                    project.Id == id &&
                    project.OwnerId == currentUserId
                );

            if (project == null)
            {
                return NotFound();
            }

            var normalizedEmail = request.Email
                .Trim()
                .ToLowerInvariant();

            var userToAdd = await _context.Users
                .FirstOrDefaultAsync(user =>
                    user.Email == normalizedEmail
                );

            if (userToAdd == null)
            {
                return NotFound("User not found.");
            }

            if (userToAdd.Id == currentUserId)
            {
                return BadRequest(
                    "The project owner cannot be added as a member."
                );
            }

            var membershipExists = await _context.ProjectMembers
                .AnyAsync(member =>
                    member.ProjectId == id &&
                    member.UserId == userToAdd.Id
                );

            if (membershipExists)
            {
                return Conflict(
                    "The user is already a member of this project."
                );
            }

            var projectMember = new ProjectMember
            {
                ProjectId = id,
                UserId = userToAdd.Id,
                Role = "Member"
            };

            _context.ProjectMembers.Add(projectMember);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                userToAdd.Id,
                userToAdd.Email,
                projectMember.Role
            });
        }

        // GET: /api/projects/1/members
        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetProjectMembers(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(project =>
                    project.Id == id &&
                    project.OwnerId == currentUserId
                );

            if (project == null)
            {
                return NotFound();
            }

            var members = await _context.ProjectMembers
                .Where(member => member.ProjectId == id)
                .Select(member => new
                {
                    member.UserId,
                    member.User.Email,
                    member.Role
                })
                .ToListAsync();

            return Ok(members);
        }

        // DELETE: /api/projects/1/members/2
        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveProjectMember(
            int id,
            int userId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(project =>
                    project.Id == id &&
                    project.OwnerId == currentUserId
                );

            if (project == null)
            {
                return NotFound();
            }

            var membership = await _context.ProjectMembers
                .FirstOrDefaultAsync(member =>
                    member.ProjectId == id &&
                    member.UserId == userId
                );

            if (membership == null)
            {
                return NotFound("Project member not found.");
            }

            _context.ProjectMembers.Remove(membership);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: /api/projects/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(project =>
                    project.Id == id &&
                    project.OwnerId == userId
                );

            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}