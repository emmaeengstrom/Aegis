using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Aegis.Api.Data;
using Aegis.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aegis.Api.IntegrationTests
{
    public class ProjectTaskApiTests
    {
        [Fact]
        public async Task Owner_Can_Create_Task()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

            var owner = new User
            {
                Email = $"owner-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            dbContext.Users.Add(owner);

            await dbContext.SaveChangesAsync();

            var project = new Project
            {
                Name = "Task Test Project",
                Description = "Project for task integration tests",
                OwnerId = owner.Id
            };

            dbContext.Projects.Add(project);

            await dbContext.SaveChangesAsync();

            var client = factory.CreateClient();

            var token =
                TestAuthHelper.CreateToken(
                    owner.Id,
                    owner.Email);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var request = new
            {
                title = "Implement authentication",
                description = "Add JWT authentication"
            };

            var response =
                await client.PostAsJsonAsync(
                    $"/api/projects/{project.Id}/tasks",
                    request);

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);
        }

        [Fact]
        public async Task Viewer_Cannot_Create_Task()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

            var owner = new User
            {
                Email = $"owner-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            var viewer = new User
            {
                Email = $"viewer-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            dbContext.Users.AddRange(owner, viewer);

            await dbContext.SaveChangesAsync();

            var project = new Project
            {
                Name = "Viewer Task Test Project",
                Description = "Project for viewer authorization test",
                OwnerId = owner.Id
            };

            dbContext.Projects.Add(project);

            await dbContext.SaveChangesAsync();

            var membership = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = viewer.Id,
                Role = ProjectRoles.Viewer
            };

            dbContext.ProjectMembers.Add(membership);

            await dbContext.SaveChangesAsync();

            var client = factory.CreateClient();

            var token =
                TestAuthHelper.CreateToken(
                    viewer.Id,
                    viewer.Email);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var request = new
            {
                title = "Unauthorized task",
                description = "Viewer should not be able to create this"
            };

            var response =
                await client.PostAsJsonAsync(
                    $"/api/projects/{project.Id}/tasks",
                    request);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Editor_Can_Create_Task()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

            var owner = new User
            {
                Email = $"owner-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            var editor = new User
            {
                Email = $"editor-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            dbContext.Users.AddRange(owner, editor);

            await dbContext.SaveChangesAsync();

            var project = new Project
            {
                Name = "Editor Task Test Project",
                Description = "Project for editor authorization test",
                OwnerId = owner.Id
            };

            dbContext.Projects.Add(project);

            await dbContext.SaveChangesAsync();

            var membership = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = editor.Id,
                Role = ProjectRoles.Editor
            };

            dbContext.ProjectMembers.Add(membership);

            await dbContext.SaveChangesAsync();

            var client = factory.CreateClient();

            var token =
                TestAuthHelper.CreateToken(
                    editor.Id,
                    editor.Email);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var request = new
            {
                title = "Editor-created task",
                description = "Editor should be able to create this"
            };

            var response =
                await client.PostAsJsonAsync(
                    $"/api/projects/{project.Id}/tasks",
                    request);

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);
        }

        [Fact]
        public async Task Outsider_Cannot_Read_Tasks()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

            var owner = new User
            {
                Email = $"owner-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            var outsider = new User
            {
                Email = $"outsider-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            dbContext.Users.AddRange(owner, outsider);

            await dbContext.SaveChangesAsync();

            var project = new Project
            {
                Name = "Private Task Project",
                Description = "Project inaccessible to outsider",
                OwnerId = owner.Id
            };

            dbContext.Projects.Add(project);

            await dbContext.SaveChangesAsync();

            var task = new ProjectTask
            {
                Title = "Private task",
                Description = "Outsider must not see this",
                Status = ProjectTaskStatuses.Todo,
                ProjectId = project.Id
            };

            dbContext.ProjectTasks.Add(task);

            await dbContext.SaveChangesAsync();

            var client = factory.CreateClient();

            var token =
                TestAuthHelper.CreateToken(
                    outsider.Id,
                    outsider.Email);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response =
                await client.GetAsync(
                    $"/api/projects/{project.Id}/tasks");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Viewer_Can_Read_Tasks()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

            var owner = new User
            {
                Email = $"owner-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            var viewer = new User
            {
                Email = $"viewer-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            dbContext.Users.AddRange(owner, viewer);

            await dbContext.SaveChangesAsync();

            var project = new Project
            {
                Name = "Viewer Read Test Project",
                Description = "Project for viewer read access",
                OwnerId = owner.Id
            };

            dbContext.Projects.Add(project);

            await dbContext.SaveChangesAsync();

            var membership = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = viewer.Id,
                Role = ProjectRoles.Viewer
            };

            dbContext.ProjectMembers.Add(membership);

            var task = new ProjectTask
            {
                Title = "Visible task",
                Description = "Viewer should be able to read this",
                Status = ProjectTaskStatuses.Todo,
                ProjectId = project.Id
            };

            dbContext.ProjectTasks.Add(task);

            await dbContext.SaveChangesAsync();

            var client = factory.CreateClient();

            var token =
                TestAuthHelper.CreateToken(
                    viewer.Id,
                    viewer.Email);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response =
                await client.GetAsync(
                    $"/api/projects/{project.Id}/tasks");

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);
        }

        [Fact]
        public async Task Editor_Can_Update_Task()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

            var owner = new User
            {
                Email = $"owner-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            var editor = new User
            {
                Email = $"editor-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            dbContext.Users.AddRange(owner, editor);

            await dbContext.SaveChangesAsync();

            var project = new Project
            {
                Name = "Task Update Test Project",
                Description = "Project for task update authorization test",
                OwnerId = owner.Id
            };

            dbContext.Projects.Add(project);

            await dbContext.SaveChangesAsync();

            var membership = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = editor.Id,
                Role = ProjectRoles.Editor
            };

            dbContext.ProjectMembers.Add(membership);

            var task = new ProjectTask
            {
                Title = "Original title",
                Description = "Original description",
                Status = ProjectTaskStatuses.Todo,
                ProjectId = project.Id
            };

            dbContext.ProjectTasks.Add(task);

            await dbContext.SaveChangesAsync();

            var client = factory.CreateClient();

            var token =
                TestAuthHelper.CreateToken(
                    editor.Id,
                    editor.Email);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var request = new
            {
                title = "Updated title",
                description = "Updated by editor",
                status = ProjectTaskStatuses.InProgress
            };

            var response =
                await client.PutAsJsonAsync(
                    $"/api/projects/{project.Id}/tasks/{task.Id}",
                    request);

            Assert.Equal(
                HttpStatusCode.NoContent,
                response.StatusCode);
        }

        [Fact]
        public async Task Viewer_Cannot_Update_Task()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

            var owner = new User
            {
                Email = $"owner-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            var viewer = new User
            {
                Email = $"viewer-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            dbContext.Users.AddRange(owner, viewer);

            await dbContext.SaveChangesAsync();

            var project = new Project
            {
                Name = "Viewer Update Test Project",
                Description = "Project for viewer update authorization test",
                OwnerId = owner.Id
            };

            dbContext.Projects.Add(project);

            await dbContext.SaveChangesAsync();

            var membership = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = viewer.Id,
                Role = ProjectRoles.Viewer
            };

            dbContext.ProjectMembers.Add(membership);

            var task = new ProjectTask
            {
                Title = "Original task",
                Description = "Viewer must not update this",
                Status = ProjectTaskStatuses.Todo,
                ProjectId = project.Id
            };

            dbContext.ProjectTasks.Add(task);

            await dbContext.SaveChangesAsync();

            var client = factory.CreateClient();

            var token =
                TestAuthHelper.CreateToken(
                    viewer.Id,
                    viewer.Email);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var request = new
            {
                title = "Unauthorized update",
                description = "Viewer tried to update this",
                status = ProjectTaskStatuses.Done
            };

            var response =
                await client.PutAsJsonAsync(
                    $"/api/projects/{project.Id}/tasks/{task.Id}",
                    request);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Owner_Can_Delete_Task()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

            var owner = new User
            {
                Email = $"owner-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            dbContext.Users.Add(owner);

            await dbContext.SaveChangesAsync();

            var project = new Project
            {
                Name = "Task Delete Test Project",
                Description = "Project for task delete authorization test",
                OwnerId = owner.Id
            };

            dbContext.Projects.Add(project);

            await dbContext.SaveChangesAsync();

            var task = new ProjectTask
            {
                Title = "Task to delete",
                Description = "Owner should be able to delete this",
                Status = ProjectTaskStatuses.Todo,
                ProjectId = project.Id
            };

            dbContext.ProjectTasks.Add(task);

            await dbContext.SaveChangesAsync();

            var client = factory.CreateClient();

            var token =
                TestAuthHelper.CreateToken(
                    owner.Id,
                    owner.Email);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response =
                await client.DeleteAsync(
                    $"/api/projects/{project.Id}/tasks/{task.Id}");

            Assert.Equal(
                HttpStatusCode.NoContent,
                response.StatusCode);
        }

        [Fact]
        public async Task Editor_Cannot_Delete_Task()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

            var owner = new User
            {
                Email = $"owner-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            var editor = new User
            {
                Email = $"editor-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            dbContext.Users.AddRange(owner, editor);

            await dbContext.SaveChangesAsync();

            var project = new Project
            {
                Name = "Editor Delete Test Project",
                Description = "Project for editor delete authorization test",
                OwnerId = owner.Id
            };

            dbContext.Projects.Add(project);

            await dbContext.SaveChangesAsync();

            var membership = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = editor.Id,
                Role = ProjectRoles.Editor
            };

            dbContext.ProjectMembers.Add(membership);

            var task = new ProjectTask
            {
                Title = "Protected task",
                Description = "Editor must not be able to delete this",
                Status = ProjectTaskStatuses.Todo,
                ProjectId = project.Id
            };

            dbContext.ProjectTasks.Add(task);

            await dbContext.SaveChangesAsync();

            var client = factory.CreateClient();

            var token =
                TestAuthHelper.CreateToken(
                    editor.Id,
                    editor.Email);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response =
                await client.DeleteAsync(
                    $"/api/projects/{project.Id}/tasks/{task.Id}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Task_Cannot_Be_Accessed_Through_Another_Project()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

            var owner = new User
            {
                Email = $"owner-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            dbContext.Users.Add(owner);

            await dbContext.SaveChangesAsync();

            var projectA = new Project
            {
                Name = "Project A",
                Description = "Contains the task",
                OwnerId = owner.Id
            };

            var projectB = new Project
            {
                Name = "Project B",
                Description = "Used for manipulated URL",
                OwnerId = owner.Id
            };

            dbContext.Projects.AddRange(projectA, projectB);

            await dbContext.SaveChangesAsync();

            var task = new ProjectTask
            {
                Title = "Project A task",
                Description = "Must only be accessible through Project A",
                Status = ProjectTaskStatuses.Todo,
                ProjectId = projectA.Id
            };

            dbContext.ProjectTasks.Add(task);

            await dbContext.SaveChangesAsync();

            var client = factory.CreateClient();

            var token =
                TestAuthHelper.CreateToken(
                    owner.Id,
                    owner.Email);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response =
                await client.GetAsync(
                    $"/api/projects/{projectB.Id}/tasks/{task.Id}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task Invalid_Task_Status_Returns_BadRequest()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

            var owner = new User
            {
                Email = $"owner-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            dbContext.Users.Add(owner);

            await dbContext.SaveChangesAsync();

            var project = new Project
            {
                Name = "Task Status Test Project",
                Description = "Project for task status validation",
                OwnerId = owner.Id
            };

            dbContext.Projects.Add(project);

            await dbContext.SaveChangesAsync();

            var task = new ProjectTask
            {
                Title = "Status test task",
                Description = "Task used for status validation",
                Status = ProjectTaskStatuses.Todo,
                ProjectId = project.Id
            };

            dbContext.ProjectTasks.Add(task);

            await dbContext.SaveChangesAsync();

            var client = factory.CreateClient();

            var token =
                TestAuthHelper.CreateToken(
                    owner.Id,
                    owner.Email);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var request = new
            {
                title = "Status test task",
                description = "Attempting invalid status",
                status = "HackedStatus"
            };

            var response =
                await client.PutAsJsonAsync(
                    $"/api/projects/{project.Id}/tasks/{task.Id}",
                    request);

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);
        }

        [Fact]
        public async Task Creating_Task_Creates_Audit_Log()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

            var owner = new User
            {
                Email = $"owner-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            dbContext.Users.Add(owner);

            await dbContext.SaveChangesAsync();

            var project = new Project
            {
                Name = "Task Audit Test Project",
                Description = "Project for task audit logging test",
                OwnerId = owner.Id
            };

            dbContext.Projects.Add(project);

            await dbContext.SaveChangesAsync();

            var client = factory.CreateClient();

            var token =
                TestAuthHelper.CreateToken(
                    owner.Id,
                    owner.Email);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var request = new
            {
                title = "Audited task",
                description = "This creation should be logged"
            };

            var response =
                await client.PostAsJsonAsync(
                    $"/api/projects/{project.Id}/tasks",
                    request);

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);

            var auditLog =
                await dbContext.AuditLogs
                    .FirstOrDefaultAsync(log =>
                        log.ProjectId == project.Id &&
                        log.UserId == owner.Id &&
                        log.Action == "TaskCreated");

            Assert.NotNull(auditLog);

            Assert.Contains(
                "Audited task",
                auditLog.Details);
        }

        [Fact]
        public async Task Updating_Task_Creates_Audit_Log()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

            var owner = new User
            {
                Email = $"owner-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            dbContext.Users.Add(owner);

            await dbContext.SaveChangesAsync();

            var project = new Project
            {
                Name = "Task Update Audit Project",
                Description = "Project for task update audit test",
                OwnerId = owner.Id
            };

            dbContext.Projects.Add(project);

            await dbContext.SaveChangesAsync();

            var task = new ProjectTask
            {
                Title = "Audit update task",
                Description = "Original description",
                Status = ProjectTaskStatuses.Todo,
                ProjectId = project.Id
            };

            dbContext.ProjectTasks.Add(task);

            await dbContext.SaveChangesAsync();

            var client = factory.CreateClient();

            var token =
                TestAuthHelper.CreateToken(
                    owner.Id,
                    owner.Email);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var request = new
            {
                title = "Audit update task",
                description = "Updated description",
                status = ProjectTaskStatuses.InProgress
            };

            var response =
                await client.PutAsJsonAsync(
                    $"/api/projects/{project.Id}/tasks/{task.Id}",
                    request);

            Assert.Equal(
                HttpStatusCode.NoContent,
                response.StatusCode);

            var auditLog =
                await dbContext.AuditLogs
                    .FirstOrDefaultAsync(log =>
                        log.ProjectId == project.Id &&
                        log.UserId == owner.Id &&
                        log.Action == "TaskUpdated");

            Assert.NotNull(auditLog);

            Assert.Contains(
                "Todo",
                auditLog.Details);

            Assert.Contains(
                "InProgress",
                auditLog.Details);
        }

        [Fact]
        public async Task Deleting_Task_Creates_Audit_Log()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            using var scope =
                factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

            var owner = new User
            {
                Email = $"owner-{Guid.NewGuid()}@example.com",
                PasswordHash = "test"
            };

            dbContext.Users.Add(owner);

            await dbContext.SaveChangesAsync();

            var project = new Project
            {
                Name = "Task Delete Audit Project",
                Description = "Project for task delete audit test",
                OwnerId = owner.Id
            };

            dbContext.Projects.Add(project);

            await dbContext.SaveChangesAsync();

            var task = new ProjectTask
            {
                Title = "Task to audit delete",
                Description = "This task will be deleted",
                Status = ProjectTaskStatuses.Todo,
                ProjectId = project.Id
            };

            dbContext.ProjectTasks.Add(task);

            await dbContext.SaveChangesAsync();

            var taskId = task.Id;

            var client = factory.CreateClient();

            var token =
                TestAuthHelper.CreateToken(
                    owner.Id,
                    owner.Email);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response =
                await client.DeleteAsync(
                    $"/api/projects/{project.Id}/tasks/{taskId}");

            Assert.Equal(
                HttpStatusCode.NoContent,
                response.StatusCode);

            var auditLog =
                await dbContext.AuditLogs
                    .FirstOrDefaultAsync(log =>
                        log.ProjectId == project.Id &&
                        log.UserId == owner.Id &&
                        log.Action == "TaskDeleted");

            Assert.NotNull(auditLog);

            Assert.Contains(
                "Task to audit delete",
                auditLog.Details);

            var deletedTask =
                await dbContext.ProjectTasks
                    .FirstOrDefaultAsync(task =>
                        task.Id == taskId);

            Assert.Null(deletedTask);
        } 
    }
} 