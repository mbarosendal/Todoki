using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TickTask.Server.Data.Models;
using TickTask.Shared.Data;

namespace TickTask.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class UserSettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/UserSettings
        [HttpGet]
        public async Task<ActionResult<UserSettingsDto>> GetMySettings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Ok(GetDefaultSettingsDto());
            }

            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                settings = new UserSettings { UserId = userId };
                _context.UserSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return Ok(ConvertToDto(settings));
        }

        // PUT: api/UserSettings
        [HttpPut]
        public async Task<IActionResult> UpdateMySettings(UserSettingsDto settingsDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return NoContent();
            }

            var existingSettings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (existingSettings == null)
            {
                return NotFound("User settings not found");
            }

            UpdateEntityFromDto(existingSettings, settingsDto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserSettingsExistsForUser(userId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/UserSettings
        [HttpPost]
        public async Task<ActionResult<UserSettingsDto>> CreateMySettings(UserSettingsDto settingsDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Ok(settingsDto);
            }

            var existingSettings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (existingSettings != null)
            {
                return Conflict("User settings already exist. Use PUT to update.");
            }

            var userSettings = ConvertToEntity(settingsDto, userId);

            _context.UserSettings.Add(userSettings);
            await _context.SaveChangesAsync();

            settingsDto.UserSettingsId = userSettings.UserSettingsId;
            settingsDto.UserId = userId;

            return CreatedAtAction(nameof(GetMySettings), null, settingsDto);
        }

        // DELETE: api/UserSettings
        [HttpDelete]
        public async Task<IActionResult> DeleteMySettings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return NoContent();
            }

            var userSettings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (userSettings == null)
            {
                return NotFound();
            }

            _context.UserSettings.Remove(userSettings);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> UserSettingsExistsForUser(string userId)
        {
            return await _context.UserSettings.AnyAsync(e => e.UserId == userId);
        }

        private UserSettingsDto GetDefaultSettingsDto()
        {
            return new UserSettingsDto
            {
                UserSettingsId = 0,
                UserId = null,
                PomodoroDurationMinutes = TimeSpan.FromMinutes(25),
                ShortBreakDurationMinutes = TimeSpan.FromMinutes(5),
                LongBreakDurationMinutes = TimeSpan.FromMinutes(15),
                PomodoroText = "",
                ShortBreakText = "",
                LongBreakText = "",
                HideTasks = false,
                HideActiveTask = false,
                IsAutoStart = false,
                IsAutoStartAfterRestart = true,
                AutomaticallyMarkDoneTasks = true,
                AutomaticallyProceedToNextTaskAfterDone = true,
                AutomaticallyClearDoneTasks = false,
                EnableNotifications = false,
                NumberOfPomodorosRun = 0,
                RunsBeforeLongBreak = 4
            };
        }

        private UserSettingsDto ConvertToDto(UserSettings settings)
        {
            return new UserSettingsDto
            {
                UserSettingsId = settings.UserSettingsId,
                UserId = settings.UserId,
                PomodoroDurationMinutes = settings.PomodoroDurationMinutes,
                ShortBreakDurationMinutes = settings.ShortBreakDurationMinutes,
                LongBreakDurationMinutes = settings.LongBreakDurationMinutes,
                PomodoroText = settings.PomodoroText,
                ShortBreakText = settings.ShortBreakText,
                LongBreakText = settings.LongBreakText,
                HideTasks = settings.HideTasks,
                HideActiveTask = settings.HideActiveTask,
                IsAutoStart = settings.IsAutoStart,
                IsAutoStartAfterRestart = settings.IsAutoStartAfterRestart,
                AutomaticallyMarkDoneTasks = settings.AutomaticallyMarkDoneTasks,
                AutomaticallyProceedToNextTaskAfterDone = settings.AutomaticallyProceedToNextTaskAfterDone,
                AutomaticallyClearDoneTasks = settings.AutomaticallyClearDoneTasks,
                EnableNotifications = settings.EnableNotifications,
                NumberOfPomodorosRun = settings.NumberOfPomodorosRun,
                RunsBeforeLongBreak = settings.RunsBeforeLongBreak
            };
        }

        private UserSettings ConvertToEntity(UserSettingsDto dto, string userId)
        {
            return new UserSettings
            {
                UserId = userId,
                UserSettingsId = 0,
                PomodoroDurationMinutes = dto.PomodoroDurationMinutes,
                ShortBreakDurationMinutes = dto.ShortBreakDurationMinutes,
                LongBreakDurationMinutes = dto.LongBreakDurationMinutes,
                PomodoroText = dto.PomodoroText,
                ShortBreakText = dto.ShortBreakText,
                LongBreakText = dto.LongBreakText,
                HideTasks = dto.HideTasks,
                HideActiveTask = dto.HideActiveTask,
                IsAutoStart = dto.IsAutoStart,
                IsAutoStartAfterRestart = dto.IsAutoStartAfterRestart,
                AutomaticallyMarkDoneTasks = dto.AutomaticallyMarkDoneTasks,
                AutomaticallyProceedToNextTaskAfterDone = dto.AutomaticallyProceedToNextTaskAfterDone,
                AutomaticallyClearDoneTasks = dto.AutomaticallyClearDoneTasks,
                EnableNotifications = dto.EnableNotifications,
                NumberOfPomodorosRun = dto.NumberOfPomodorosRun,
                RunsBeforeLongBreak = dto.RunsBeforeLongBreak
            };
        }

        private void UpdateEntityFromDto(UserSettings entity, UserSettingsDto dto)
        {
            entity.PomodoroDurationMinutes = dto.PomodoroDurationMinutes;
            entity.ShortBreakDurationMinutes = dto.ShortBreakDurationMinutes;
            entity.LongBreakDurationMinutes = dto.LongBreakDurationMinutes;
            entity.PomodoroText = dto.PomodoroText;
            entity.ShortBreakText = dto.ShortBreakText;
            entity.LongBreakText = dto.LongBreakText;
            entity.HideTasks = dto.HideTasks;
            entity.HideActiveTask = dto.HideActiveTask;
            entity.IsAutoStart = dto.IsAutoStart;
            entity.IsAutoStartAfterRestart = dto.IsAutoStartAfterRestart;
            entity.AutomaticallyMarkDoneTasks = dto.AutomaticallyMarkDoneTasks;
            entity.AutomaticallyProceedToNextTaskAfterDone = dto.AutomaticallyProceedToNextTaskAfterDone;
            entity.AutomaticallyClearDoneTasks = dto.AutomaticallyClearDoneTasks;
            entity.EnableNotifications = dto.EnableNotifications;
            entity.NumberOfPomodorosRun = dto.NumberOfPomodorosRun;
            entity.RunsBeforeLongBreak = dto.RunsBeforeLongBreak;
        }
    }
}