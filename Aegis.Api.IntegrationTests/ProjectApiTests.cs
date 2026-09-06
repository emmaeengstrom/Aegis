using Aegis.Api.Data;
using Aegis.Api.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore; 

namespace Aegis.Api.IntegrationTests
{
    public class ProjectApiTests
    {
        [Fact]
        public async Task GetProjects_WithoutAuthentication_ReturnsUnauthorized()
        {
            await using var factory = new AegisWebApplicationFactory();

            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/projects");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task GetProject_AsOutsider_ReturnsNotFound()
        {
            await using var factory = new AegisWebApplicationFactory();

            int projectId;
            int outsiderUserId;
            string outsiderEmail;

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

                var owner = new User
                {
                    Email = $"owner-{Guid.NewGuid()}@example.com",
                    PasswordHash = "not-used-in-test"
                };

                var outsider = new User
                {
                    Email = $"outsider-{Guid.NewGuid()}@example.com",
                    PasswordHash = "not-used-in-test"
                };

                db.Users.AddRange(owner, outsider);

                await db.SaveChangesAsync();

                var project = new Project
                {
                    Name = "Owner Project",
                    Description = "Project used for authorization testing",
                    OwnerId = owner.Id
                };

                db.Projects.Add(project);

                await db.SaveChangesAsync();

                projectId = project.Id;
                outsiderUserId = outsider.Id;
            }

            using var client = factory.CreateClient();

            var token = TestAuthHelper.CreateToken(
                outsiderUserId,
                "outsider@example.com");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response =
                await client.GetAsync($"/api/projects/{projectId}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task GetProject_AsOwner_ReturnsOk()
        {
            await using var factory = new AegisWebApplicationFactory();

            int projectId;
            int ownerUserId;
            string ownerEmail;

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

                ownerEmail =
                    $"owner-{Guid.NewGuid()}@example.com";

                var owner = new User
                {
                    Email = ownerEmail,
                    PasswordHash = "not-used-in-test"
                };

                db.Users.Add(owner);

                await db.SaveChangesAsync();

                var project = new Project
                {
                    Name = "Owner Accessible Project",
                    Description = "Project owned by authenticated test user",
                    OwnerId = owner.Id
                };

                db.Projects.Add(project);

                await db.SaveChangesAsync();

                projectId = project.Id;
                ownerUserId = owner.Id;
            }

            using var client = factory.CreateClient();

            var token = TestAuthHelper.CreateToken(
                ownerUserId,
                ownerEmail);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response =
                await client.GetAsync($"/api/projects/{projectId}");

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);
        }

        [Fact]
        public async Task GetProject_AsViewer_ReturnsOk()
        {
            await using var factory = new AegisWebApplicationFactory();

            int projectId;
            int viewerUserId;
            string viewerEmail;

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

                var owner = new User
                {
                    Email = $"owner-{Guid.NewGuid()}@example.com",
                    PasswordHash = "not-used-in-test"
                };

                viewerEmail =
                    $"viewer-{Guid.NewGuid()}@example.com";

                var viewer = new User
                {
                    Email = viewerEmail,
                    PasswordHash = "not-used-in-test"
                };

                db.Users.AddRange(owner, viewer);

                await db.SaveChangesAsync();

                var project = new Project
                {
                    Name = "Viewer Accessible Project",
                    Description = "Project used to test Viewer access",
                    OwnerId = owner.Id
                };

                db.Projects.Add(project);

                await db.SaveChangesAsync();

                var membership = new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = viewer.Id,
                    Role = ProjectRoles.Viewer
                };

                db.ProjectMembers.Add(membership);

                await db.SaveChangesAsync();

                projectId = project.Id;
                viewerUserId = viewer.Id;
            }

            using var client = factory.CreateClient();

            var token = TestAuthHelper.CreateToken(
                viewerUserId,
                viewerEmail);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response =
                await client.GetAsync($"/api/projects/{projectId}");

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);
        }

        [Fact]
        public async Task UpdateProject_AsViewer_ReturnsNotFound()
        {
            await using var factory = new AegisWebApplicationFactory();

            int projectId;
            int viewerUserId;
            string viewerEmail;

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

                var owner = new User
                {
                    Email = $"owner-{Guid.NewGuid()}@example.com",
                    PasswordHash = "not-used-in-test"
                };

                viewerEmail =
                    $"viewer-{Guid.NewGuid()}@example.com";

                var viewer = new User
                {
                    Email = viewerEmail,
                    PasswordHash = "not-used-in-test"
                };

                db.Users.AddRange(owner, viewer);

                await db.SaveChangesAsync();

                var project = new Project
                {
                    Name = "Viewer Restricted Project",
                    Description = "Viewer should not be able to update this project",
                    OwnerId = owner.Id
                };

                db.Projects.Add(project);

                await db.SaveChangesAsync();

                var membership = new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = viewer.Id,
                    Role = ProjectRoles.Viewer
                };

                db.ProjectMembers.Add(membership);

                await db.SaveChangesAsync();

                projectId = project.Id;
                viewerUserId = viewer.Id;
            }

            using var client = factory.CreateClient();

            var token = TestAuthHelper.CreateToken(
                viewerUserId,
                viewerEmail);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Name = "Viewer attempted update",
                Description = "This update should be blocked"
            };

            var response = await client.PutAsJsonAsync(
                $"/api/projects/{projectId}",
                request);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task UpdateProject_AsEditor_ReturnsNoContent()
        {
            await using var factory = new AegisWebApplicationFactory();

            int projectId;
            int editorUserId;
            string editorEmail;

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

                var owner = new User
                {
                    Email = $"owner-{Guid.NewGuid()}@example.com",
                    PasswordHash = "not-used-in-test"
                };

                editorEmail =
                    $"editor-{Guid.NewGuid()}@example.com";

                var editor = new User
                {
                    Email = editorEmail,
                    PasswordHash = "not-used-in-test"
                };

                db.Users.AddRange(owner, editor);

                await db.SaveChangesAsync();

                var project = new Project
                {
                    Name = "Editor Project",
                    Description = "Project used to test Editor update access",
                    OwnerId = owner.Id
                };

                db.Projects.Add(project);

                await db.SaveChangesAsync();

                var membership = new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = editor.Id,
                    Role = ProjectRoles.Editor
                };

                db.ProjectMembers.Add(membership);

                await db.SaveChangesAsync();

                projectId = project.Id;
                editorUserId = editor.Id;
            }

            using var client = factory.CreateClient();

            var token = TestAuthHelper.CreateToken(
                editorUserId,
                editorEmail);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Name = "Updated by Editor",
                Description = "Editor was allowed to update this project"
            };

            var response = await client.PutAsJsonAsync(
                $"/api/projects/{projectId}",
                request);

            Assert.Equal(
                HttpStatusCode.NoContent,
                response.StatusCode);
        }

        [Fact]
        public async Task UpdateProject_AsOutsider_ReturnsNotFound()
        {
            await using var factory = new AegisWebApplicationFactory();

            int projectId;
            int outsiderUserId;
            string outsiderEmail;

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

                var owner = new User
                {
                    Email = $"owner-{Guid.NewGuid()}@example.com",
                    PasswordHash = "not-used-in-test"
                };

                outsiderEmail =
                    $"outsider-{Guid.NewGuid()}@example.com";

                var outsider = new User
                {
                    Email = outsiderEmail,
                    PasswordHash = "not-used-in-test"
                };

                db.Users.AddRange(owner, outsider);

                await db.SaveChangesAsync();

                var project = new Project
                {
                    Name = "Owner Only Project",
                    Description = "Outsider should not be able to update this project",
                    OwnerId = owner.Id
                };

                db.Projects.Add(project);

                await db.SaveChangesAsync();

                projectId = project.Id;
                outsiderUserId = outsider.Id;
            }

            using var client = factory.CreateClient();

            var token = TestAuthHelper.CreateToken(
                outsiderUserId,
                outsiderEmail);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Name = "Unauthorized update attempt",
                Description = "This update must not be applied"
            };

            var response = await client.PutAsJsonAsync(
                $"/api/projects/{projectId}",
                request);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task GetProject_AfterMembershipRemoved_ReturnsNotFound()
        {
            await using var factory = new AegisWebApplicationFactory();

            int projectId;
            int memberUserId;
            string memberEmail;

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

                var owner = new User
                {
                    Email = $"owner-{Guid.NewGuid()}@example.com",
                    PasswordHash = "not-used-in-test"
                };

                memberEmail =
                    $"member-{Guid.NewGuid()}@example.com";

                var member = new User
                {
                    Email = memberEmail,
                    PasswordHash = "not-used-in-test"
                };

                db.Users.AddRange(owner, member);

                await db.SaveChangesAsync();

                var project = new Project
                {
                    Name = "Membership Removal Project",
                    Description = "Project used to test revoked access",
                    OwnerId = owner.Id
                };

                db.Projects.Add(project);

                await db.SaveChangesAsync();

                var membership = new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = member.Id,
                    Role = ProjectRoles.Viewer
                };

                db.ProjectMembers.Add(membership);

                await db.SaveChangesAsync();

                projectId = project.Id;
                memberUserId = member.Id;

                db.ProjectMembers.Remove(membership);

                await db.SaveChangesAsync();
            }

            using var client = factory.CreateClient();

            var token = TestAuthHelper.CreateToken(
                memberUserId,
                memberEmail);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response =
                await client.GetAsync($"/api/projects/{projectId}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task UpdateMemberRole_AsViewer_ReturnsNotFound()
        {
            await using var factory = new AegisWebApplicationFactory();

            int projectId;
            int viewerUserId;
            string viewerEmail;

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

                var owner = new User
                {
                    Email = $"owner-{Guid.NewGuid()}@example.com",
                    PasswordHash = "not-used-in-test"
                };

                viewerEmail =
                    $"viewer-{Guid.NewGuid()}@example.com";

                var viewer = new User
                {
                    Email = viewerEmail,
                    PasswordHash = "not-used-in-test"
                };

                db.Users.AddRange(owner, viewer);

                await db.SaveChangesAsync();

                var project = new Project
                {
                    Name = "Privilege Escalation Project",
                    Description = "Project used to test role escalation protection",
                    OwnerId = owner.Id
                };

                db.Projects.Add(project);

                await db.SaveChangesAsync();

                var membership = new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = viewer.Id,
                    Role = ProjectRoles.Viewer
                };

                db.ProjectMembers.Add(membership);

                await db.SaveChangesAsync();

                projectId = project.Id;
                viewerUserId = viewer.Id;
            }

            using var client = factory.CreateClient();

            var token = TestAuthHelper.CreateToken(
                viewerUserId,
                viewerEmail);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Role = ProjectRoles.Editor
            };

            var response = await client.PutAsJsonAsync(
                $"/api/projects/{projectId}/members/{viewerUserId}/role",
                request);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        }

        [Fact]
        public async Task DeleteProject_PreservesAuditLogs()
        {
            await using var factory =
                new AegisWebApplicationFactory();

            int projectId;
            int ownerUserId;
            string ownerEmail;

            using (var scope =
                factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

                ownerEmail =
                    $"owner-{Guid.NewGuid()}@example.com";

                var owner = new User
                {
                    Email = ownerEmail,
                    PasswordHash = "not-used-in-test"
                };

                db.Users.Add(owner);

                await db.SaveChangesAsync();

                var project = new Project
                {
                    Name = "Audit Preservation Project",
                    Description =
                        "Project used to test audit preservation",
                    OwnerId = owner.Id
                };

                db.Projects.Add(project);

                await db.SaveChangesAsync();

                var existingAuditLog = new AuditLog
                {
                    UserId = owner.Id,
                    ProjectId = project.Id,
                    Action = "TestAction",
                    Details = "Audit log created before deletion."
                };

                db.AuditLogs.Add(existingAuditLog);

                await db.SaveChangesAsync();

                projectId = project.Id;
                ownerUserId = owner.Id;
            }

            using var client = factory.CreateClient();

            var token = TestAuthHelper.CreateToken(
                ownerUserId,
                ownerEmail);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response = await client.DeleteAsync(
                $"/api/projects/{projectId}");

            Assert.Equal(
                HttpStatusCode.NoContent,
                response.StatusCode);

            using (var scope =
                factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<AegisDbContext>();

                var projectStillExists =
                    await db.Projects.AnyAsync(
                        project =>
                            project.Id == projectId);

                Assert.False(projectStillExists);

                var preservedLogs =
                    await db.AuditLogs
                        .Where(log =>
                            log.UserId == ownerUserId &&
                            (
                                log.Action == "TestAction" ||
                                log.Action == "ProjectDeleted"
                            ))
                        .ToListAsync();

                Assert.Equal(2, preservedLogs.Count);

                Assert.All(
                    preservedLogs,
                    log => Assert.Null(log.ProjectId));

                Assert.Contains(
                    preservedLogs,
                    log =>
                        log.Action == "TestAction");

                Assert.Contains(
                    preservedLogs,
                    log =>
                        log.Action == "ProjectDeleted");
            }
        }
    }
}