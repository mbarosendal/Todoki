using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TickTask.Server.Data;
using TickTask.Shared;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TickTask.Server.Data.Models;

namespace TickTask.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TaskItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TaskItemsController> _logger;

        public TaskItemsController(ApplicationDbContext context, ILogger<TaskItemsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/TaskItems?projectId=1
        [HttpGet]
        public async Task<ActionResult<List<TaskItemDto>>> GetTasks(int? projectId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                // not logged in → return empty (or later: only "public" tasks if you add that field)
                return Ok(new List<TaskItemDto>());
            }

            if (projectId == null)
            {
                var defaultProject = await _context.Projects
                    .FirstOrDefaultAsync(p => p.UserId == userId);

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

            return Ok(tasks.Select(t => new TaskItemDto
            {
                TaskItemId = t.TaskItemId,
                Name = t.Name,
                Description = t.Description,
                ProjectId = t.ProjectId,
                SortOrder = t.SortOrder,
                IsActiveTask = t.IsActiveTask,
                IsDone = t.IsDone,
                EstimatedNumberOfPomodoros = t.EstimatedNumberOfPomodoros,
                PomodorosRanOnTask = t.PomodorosRanOnTask
            }));
        }

        // POST: api/TaskItems
        [HttpPost]
        public async Task<ActionResult<TaskItemDto>> CreateTask(TaskItemDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("You must be logged in to create tasks.");
            }

            if (dto.ProjectId == 0)
            {
                var defaultProject = await _context.Projects
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (defaultProject == null)
                {
                    defaultProject = new Project { UserId = userId, Title = "Default Project" };
                    _context.Projects.Add(defaultProject);
                    await _context.SaveChangesAsync();
                }
                dto.ProjectId = defaultProject.ProjectId;
            }

            var task = new TaskItem
            {
                Name = dto.Name,
                Description = dto.Description,
                ProjectId = dto.ProjectId,
                SortOrder = dto.SortOrder,
                IsActiveTask = dto.IsActiveTask,
                IsDone = dto.IsDone,
                EstimatedNumberOfPomodoros = dto.EstimatedNumberOfPomodoros,
                PomodorosRanOnTask = dto.PomodorosRanOnTask
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            dto.TaskItemId = task.TaskItemId;
            return CreatedAtAction(nameof(GetTasks), new { projectId = dto.ProjectId }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItemDto dto)
        {
            if (id != dto.TaskItemId) return BadRequest();

            var task = await _context.TaskItems.FindAsync(id);
            if (task == null) return NotFound();

            task.Name = dto.Name;
            task.Description = dto.Description;
            task.SortOrder = dto.SortOrder;
            task.IsActiveTask = dto.IsActiveTask;
            task.IsDone = dto.IsDone;
            task.EstimatedNumberOfPomodoros = dto.EstimatedNumberOfPomodoros;
            task.PomodorosRanOnTask = dto.PomodorosRanOnTask;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null) return NotFound();

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("reorder")]
        public async Task<IActionResult> ReorderTasks([FromBody] List<TaskItemDto> dtos)
        {
            var taskIds = dtos.Select(t => t.TaskItemId).ToList();
            var tasks = await _context.TaskItems.Where(t => taskIds.Contains(t.TaskItemId)).ToListAsync();

            foreach (var task in tasks)
            {
                var dto = dtos.First(t => t.TaskItemId == task.TaskItemId);
                task.SortOrder = dto.SortOrder;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
