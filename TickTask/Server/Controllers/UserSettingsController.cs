using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TickTask.Shared;

namespace TickTask.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all endpoints
    public class UserSettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/UserSettings - Get current user's settings
        [HttpGet]
        public async Task<ActionResult<UserSettings>> GetMySettings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                // Create default settings for new user
                settings = new UserSettings { UserId = userId };
                _context.UserSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return Ok(settings);
        }

        // PUT: api/UserSettings - Update current user's settings
        [HttpPut]
        public async Task<IActionResult> UpdateMySettings(UserSettings userSettings)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var existingSettings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (existingSettings == null)
            {
                return NotFound("User settings not found");
            }

            // Ensure user can only update their own settings
            userSettings.UserId = userId;
            userSettings.UserSettingsId = existingSettings.UserSettingsId;

            _context.Entry(existingSettings).CurrentValues.SetValues(userSettings);

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

        // POST: api/UserSettings - Create settings for current user (if they don't exist)
        [HttpPost]
        public async Task<ActionResult<UserSettings>> CreateMySettings(UserSettings userSettings)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Check if settings already exist
            var existingSettings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (existingSettings != null)
            {
                return Conflict("User settings already exist. Use PUT to update.");
            }

            // Ensure the settings belong to the current user
            userSettings.UserId = userId;
            userSettings.UserSettingsId = 0; // Let EF generate the ID

            _context.UserSettings.Add(userSettings);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMySettings), null, userSettings);
        }

        // DELETE: api/UserSettings - Delete current user's settings
        [HttpDelete]
        public async Task<IActionResult> DeleteMySettings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
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
    }
}