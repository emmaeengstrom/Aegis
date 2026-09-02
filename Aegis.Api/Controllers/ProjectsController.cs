using Microsoft.AspNetCore.Authorization;
using Aegis.Api.Data;
using Aegis.Api.DTOs;
using Aegis.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var projects = await _context.Projects
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
            var project = await _context.Projects.FindAsync(id);

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
            var project = new Project
            {
                Name = request.Name,
                Description = request.Description
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
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            project.Name = request.Name;
            project.Description = request.Description;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: /api/projects/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);

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