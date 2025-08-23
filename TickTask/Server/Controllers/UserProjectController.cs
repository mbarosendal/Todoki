using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TickTask.Server.Controllers.TickTask.Server.Controllers;
using TickTask.Server.Data.Models;
using TickTask.Shared;

namespace TickTask.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class UserProjectController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthenticationController> _logger;

        public UserProjectController(ApplicationDbContext context, ILogger<AuthenticationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/UserProject - Get current user's default project
        [HttpGet]
        public async Task<ActionResult<ProjectDto>> GetMyDefaultProject()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var existingProject = await _context.Projects.FirstOrDefaultAsync(p => p.UserId == userId);
                if (existingProject == null)
                {
                    existingProject = new Project()
                    {
                        Title = "Default Project",
                        Description = "",
                        DeadLine = DateTime.Now.AddDays(1),
                        UserId = userId
                    };

                    _context.Projects.Add(existingProject);
                    await _context.SaveChangesAsync();
                }

                // Map EF model to DTO to return to client
                var projectDto = new ProjectDto
                {
                    ProjectId = existingProject.ProjectId,
                    Title = existingProject.Title,
                    Description = existingProject.Description,
                    DeadLine = existingProject.DeadLine,
                    UserId = existingProject.UserId
                };

                return Ok(projectDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("GetMyDefaultProject failed: {Error}", ex);
                return StatusCode(500, ex.Message);
            }
        }


        // PUT: api/UserProject - Update current user's default project
        [HttpPut]
        public async Task<IActionResult> UpdateMyDefaultProject(ProjectDto projectDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingProject == null)
            {
                return NotFound("Default project not found");
            }

            projectDto.UserId = userId;
            projectDto.ProjectId = existingProject.ProjectId;

            _context.Entry(existingProject).CurrentValues.SetValues(projectDto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/UserProject - Create a default project (if none exists)
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateMyDefaultProject(ProjectDto projectDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingProject != null)
            {
                _logger.LogWarning("CreateMyDefaultProject failed:Default project already exists for ProjectId: {ProjectId}", projectDto.ProjectId.ToString());
                return Conflict("Default project already exists. Use PUT to update.");
            }

            // Map DTO → EF model
            var project = new Project
            {
                Title = projectDto.Title,
                Description = projectDto.Description,
                DeadLine = projectDto.DeadLine,
                UserId = userId
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Map back to DTO for response
            var savedProjectDto = new ProjectDto
            {
                ProjectId = project.ProjectId,
                Title = project.Title,
                Description = project.Description,
                DeadLine = project.DeadLine,
                UserId = project.UserId
            };

            return CreatedAtAction(nameof(GetMyDefaultProject), null, savedProjectDto);
        }

    }
}
