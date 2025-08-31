using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TickTask.Server.Data.Models;
using TickTask.Server.Services;
using TickTask.Shared.Data;

namespace TickTask.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TaskItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TaskItemsController> _logger;
        private readonly IProjectService _projectService;

        public TaskItemsController(ApplicationDbContext context, ILogger<TaskItemsController> logger, IProjectService projectService)
        {
            _context = context;
            _logger = logger;
            _projectService = projectService;
        }

        // GET: api/TaskItems?projectId=1
        [HttpGet]
        public async Task<ActionResult<List<TaskItemDto>>> GetTasks(int? projectId = null)
        {
            if (projectId == null)
            {
                var defaultProject = await _projectService.GetOrCreateDefaultProjectAsync(User, Request, Response);
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
            if (dto.ProjectId == 0)
            {
                var defaultProject = await _projectService.GetOrCreateDefaultProjectAsync(User, Request, Response);
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

        // PUT: api/TaskItems/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItemDto dto)
        {
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

        // DELETE: api/TaskItems/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null) return NotFound();

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/TaskItems/reorder
        [HttpPut("reorder")]
        public async Task<IActionResult> ReorderTasks([FromBody] List<TaskItemDto> dtos)
        {
            var taskIds = dtos.Select(t => t.TaskItemId).ToList();
            var tasks = await _context.TaskItems
                .Where(t => taskIds.Contains(t.TaskItemId))
                .ToListAsync();

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