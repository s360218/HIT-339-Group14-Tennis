using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tennis.Areas.Identity.Data;
using tennis.Data;

namespace tennis.Controllers
{
    [Authorize(Roles = "Coach")]
    public class CoachController : Controller
    {
        private readonly tennisContext _context;
        private readonly UserManager<tennisUser> _userManager;

        public CoachController(tennisContext context, UserManager<tennisUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET : Coach/Index
        public async Task<IActionResult> MySchedules()
        {
            var userId = _userManager.GetUserId(User);
            var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UserId == userId);
            if (coach == null)
            {
                return NotFound("Coach not found.");
            }

            var schedules = await _context.Schedules
                .Where(s => s.CoachId == coach.CoachId)
                .ToListAsync();

            return View("MySchedules", schedules);
        }

        // GET : Coach/EnrolledMembers/5
        public async Task<IActionResult> EnrolledMembers(int id)
        {
            var userId = _userManager.GetUserId(User);
            var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UserId == userId);
            if (coach == null)
            {
                return NotFound("Coach not found.");
            }

            var schedule = await _context.Schedules
                .Where(s => s.CoachId == coach.CoachId && s.ScheduleId == id)
                .Include(s => s.MemberSchedules)
                .ThenInclude(ms => ms.Member)
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                return NotFound("Schedule not found or you do not have access to it.");
            }

            return View("EnrolledMembers", schedule);
        }

    }
}
