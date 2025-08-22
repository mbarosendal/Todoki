using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TickTask.Server.Data;
using TickTask.Shared;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TickTask.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TaskItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TaskItems?projectId=1
        [HttpGet]
        public async Task<ActionResult<List<TaskItem>>> GetTasks(int? projectId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Use default project if none specified
            if (projectId == null)
            {
                var defaultProject = await _context.Projects.FirstOrDefaultAsync(p => p.UserId == userId);
                if (defaultProject == null)
                {
                    defaultProject = new Project { UserId = userId, Title = "Default Project" };
                    _context.Projects.Add(defaultProject);
                    await _context.SaveChangesAsync();
                }

                projectId = defaultProject.ProjectId;
            }

            var tasks = await _context.TaskItems
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            return Ok(tasks);
        }

        // POST: api/TaskItems
        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Assign default project if ProjectId is 0
            if (task.ProjectId == 0)
            {
                var defaultProject = await _context.Projects.FirstOrDefaultAsync(p => p.UserId == userId);
                if (defaultProject == null)
                {
                    defaultProject = new Project { UserId = userId, Title = "Default Project" };
                    _context.Projects.Add(defaultProject);
                    await _context.SaveChangesAsync();
                }
                task.ProjectId = defaultProject.ProjectId;
            }

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTasks), new { projectId = task.ProjectId }, task);
        }

        // PUT: api/TaskItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem task)
        {
            if (id != task.TaskItemId)
                return BadRequest();

            _context.Entry(task).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/TaskItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null)
                return NotFound();

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/TaskItems/reorder
        [HttpPut("reorder")]
        public async Task<IActionResult> ReorderTasks([FromBody] List<TaskItem> tasks)
        {
            foreach (var task in tasks)
            {
                _context.Entry(task).Property(t => t.SortOrder).IsModified = true;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
