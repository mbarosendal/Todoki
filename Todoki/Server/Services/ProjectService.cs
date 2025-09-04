using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Todoki.Server.Data;
using Todoki.Server.Data.Models;

namespace Todoki.Server.Services
{
    public interface IProjectService
    {
        Task<Project> GetOrCreateDefaultProjectAsync(ClaimsPrincipal user, HttpRequest request, HttpResponse response);
        string GetOrCreateGuestId(HttpRequest request, HttpResponse response);
    }

    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(ApplicationDbContext context, ILogger<ProjectService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string GetOrCreateGuestId(HttpRequest request, HttpResponse response)
        {
            var guestId = request.Cookies["GuestId"];
            if (string.IsNullOrEmpty(guestId))
            {
                guestId = Guid.NewGuid().ToString();
                response.Cookies.Append("GuestId", guestId, new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(30),
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                });
                _logger.LogInformation("Created new guest ID: {GuestId}", guestId);
            }
            return guestId;
        }

        public async Task<Project> GetOrCreateDefaultProjectAsync(ClaimsPrincipal user, HttpRequest request, HttpResponse response)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var guestId = GetOrCreateGuestId(request, response);

            _logger.LogInformation("Looking for project with userId: {UserId}, guestId: {GuestId}", userId, guestId);

            Project? project = null;

            try
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    // 1. For authenticated users, first try to find by UserId
                    project = await _context.Projects
                        .Where(p => p.UserId == userId)
                        .OrderBy(p => p.ProjectId)
                        .FirstOrDefaultAsync();

                    // 2. If no user project found, check for guest projects to migrate
                    if (project == null && !string.IsNullOrEmpty(guestId))
                    {
                        var guestProject = await _context.Projects
                            .Where(p => p.GuestId == guestId && p.UserId == null)
                            .FirstOrDefaultAsync();

                        if (guestProject != null)
                        {
                            // Migrate guest project to user
                            guestProject.UserId = userId;
                            guestProject.GuestId = null;
                            await _context.SaveChangesAsync();
                            project = guestProject;
                            _logger.LogInformation("Migrated guest project {ProjectId} to user {UserId}", project.ProjectId, userId);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(guestId))
                {
                    _logger.LogInformation("Guest query: searching for GuestId='{GuestId}'", guestId);

                    // Get project data using raw SQL to avoid EF NULL issues
                    var projectData = await _context.Database.SqlQueryRaw<ProjectData>(
                        "SELECT TOP 1 ProjectId, Title, Description, DeadLine FROM Projects WHERE GuestId = {0} ORDER BY ProjectId",
                        guestId).FirstOrDefaultAsync();

                    if (projectData != null)
                    {
                        _logger.LogInformation("Found existing project with ID {ProjectId}", projectData.ProjectId);

                        project = new Project
                        {
                            ProjectId = projectData.ProjectId,
                            Title = projectData.Title ?? "Default Project",
                            Description = projectData.Description ?? "",
                            DeadLine = projectData.DeadLine,
                            UserId = null,
                            GuestId = guestId
                        };
                    }
                    else
                    {
                        _logger.LogInformation("No existing project found for GuestId");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying projects for userId: {UserId}, guestId: {GuestId}", userId, guestId);
            }

            if (project == null)
            {
                _logger.LogInformation("Creating new default project for userId: {UserId}, guestId: {GuestId}", userId, guestId);

                project = new Project
                {
                    Title = "Default Project",
                    Description = "",
                    DeadLine = DateTime.Now.AddDays(1),
                    UserId = userId,
                    GuestId = string.IsNullOrEmpty(userId) ? guestId : null
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created project with ID: {ProjectId}", project.ProjectId);
            }
            else
            {
                _logger.LogInformation("Using existing project with ID: {ProjectId}", project.ProjectId);
            }

            return project;
        }

        // Helper class for raw SQL queries
        public class ProjectData
        {
            public int ProjectId { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public DateTime DeadLine { get; set; }
        }
    }
}