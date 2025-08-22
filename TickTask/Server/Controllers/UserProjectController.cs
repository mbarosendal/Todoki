using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TickTask.Shared;

namespace TickTask.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserProjectController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserProjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/UserProject - Get current user's default project
        [HttpGet]
        public async Task<ActionResult<Project>> GetMyDefaultProject()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var project = await _context.Projects.FirstOrDefaultAsync(p => p.UserId == userId);
                if (project == null)
                {
                    project = new Project { UserId = userId, Title = "Default Project" };
                    _context.Projects.Add(project);
                    await _context.SaveChangesAsync();
                }

                return Ok(project);
            }
            catch (Exception ex)
            {
                // Log and return the error
                Console.WriteLine("An error occured❗!!:", ex);
                return StatusCode(500, ex.Message);
            }
        }


        // PUT: api/UserProject - Update current user's default project
        [HttpPut]
        public async Task<IActionResult> UpdateMyDefaultProject(Project updatedProject)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingProject == null)
                return NotFound("Default project not found");

            // Ensure the user can only update their own project
            updatedProject.UserId = userId;
            updatedProject.ProjectId = existingProject.ProjectId;

            _context.Entry(existingProject).CurrentValues.SetValues(updatedProject);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/UserProject - Create a default project (if none exists)
        [HttpPost]
        public async Task<ActionResult<Project>> CreateMyDefaultProject(Project newProject)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingProject != null)
                return Conflict("Default project already exists. Use PUT to update.");

            newProject.UserId = userId;
            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyDefaultProject), null, newProject);
        }
    }
}
