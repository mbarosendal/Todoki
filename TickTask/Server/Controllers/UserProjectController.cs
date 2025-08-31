using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TickTask.Server.Data;
using TickTask.Server.Data.Models;
using TickTask.Server.Services;
using TickTask.Shared;

namespace TickTask.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class UserProjectController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserProjectController> _logger;
        private readonly IProjectService _projectService;

        public UserProjectController(ApplicationDbContext context, ILogger<UserProjectController> logger, IProjectService projectService)
        {
            _context = context;
            _logger = logger;
            _projectService = projectService;
        }

        // GET: api/UserProject
        [HttpGet]
        public async Task<ActionResult<ProjectDto>> GetMyDefaultProject()
        {
            var project = await _projectService.GetOrCreateDefaultProjectAsync(User, Request, Response);

            var projectDto = new ProjectDto
            {
                ProjectId = project.ProjectId,
                Title = project.Title,
                Description = project.Description,
                DeadLine = project.DeadLine,
                UserId = project.UserId,
                GuestId = project.GuestId
            };

            return Ok(projectDto);
        }

        // PUT: api/UserProject
        [HttpPut]
        public async Task<IActionResult> UpdateMyDefaultProject(ProjectDto projectDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var guestId = Request.Cookies["GuestId"];
            var ownerId = userId ?? guestId;

            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => (userId != null ? p.UserId == userId : p.GuestId == guestId));

            if (existingProject == null)
                return NotFound("Default project not found");

            projectDto.ProjectId = existingProject.ProjectId;
            projectDto.UserId = userId;
            projectDto.GuestId = guestId;

            _context.Entry(existingProject).CurrentValues.SetValues(projectDto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/UserProject
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateMyDefaultProject(ProjectDto projectDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var guestId = Request.Cookies["GuestId"];

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(guestId))
            {
                guestId = Guid.NewGuid().ToString();
                Response.Cookies.Append("GuestId", guestId, new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(30),
                    HttpOnly = true
                });
            }

            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => (userId != null ? p.UserId == userId : p.GuestId == guestId));

            if (existingProject != null)
                return Conflict("Default project already exists. Use PUT to update.");

            var project = new Project
            {
                Title = projectDto.Title,
                Description = projectDto.Description,
                DeadLine = projectDto.DeadLine,
                UserId = userId,
                GuestId = guestId
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var savedProjectDto = new ProjectDto
            {
                ProjectId = project.ProjectId,
                Title = project.Title,
                Description = project.Description,
                DeadLine = project.DeadLine,
                UserId = project.UserId,
                GuestId = project.GuestId
            };

            return CreatedAtAction(nameof(GetMyDefaultProject), null, savedProjectDto);
        }
    }
}
