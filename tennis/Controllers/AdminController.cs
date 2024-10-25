using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using tennis.Data;
using tennis.Models;
using tennis.Services;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly tennisContext _context;
    private readonly IEmailService _emailService;

    public AdminController(tennisContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // GET: Admin/CreateSchedule
    public IActionResult CreateSchedule()
    {
        ViewData["Coaches"] = new SelectList(_context.Coaches, "CoachId", "FirstName");
        return View();
    }

    // GET: Admin/Index
    public async Task<IActionResult> Index()
    {
        var schedules = await _context.Schedules
            .Include(s => s.Coach)
            .ToListAsync();
        return View(schedules);
    }


    // POST: Admin/CreateSchedule
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSchedule([Bind("ScheduleId,Name,Location,Date,CoachId")] Schedule schedule)
    {
        if (ModelState.IsValid)
        {
            _context.Add(schedule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["Coaches"] = new SelectList(_context.Coaches, "CoachId", "FirstName", schedule.CoachId);
        return View(schedule);
    }

    // GET: Admin/ViewMembers
    public async Task<IActionResult> ViewMembers()
    {
        var members = await _context.Members
            .Include(m => m.User)
            .Include(m => m.MemberSchedules)
            .ThenInclude(ms => ms.Schedule)
            .ToListAsync();
        return View(members);
    }

    // POST: Admin/SendNotifications
    [HttpPost]
    public async Task<IActionResult> SendNotifications()
    {
        var upcomingSchedules = await _context.Schedules
            .Include(s => s.MemberSchedules)
            .ThenInclude(ms => ms.Member)
            .Where(s => s.Date > DateTime.Now)
            .ToListAsync();

        foreach (var schedule in upcomingSchedules)
        {
            foreach (var memberSchedule in schedule.MemberSchedules)
            {
                var member = memberSchedule.Member;
                if (member != null && member.Active && !string.IsNullOrEmpty(member.Email))
                {
                    await _emailService.SendEmailAsync(member.Email, "Upcoming Schedule Notification", $"Dear {member.FirstName},\n\nYou have an upcoming schedule: {schedule.Name} on {schedule.Date}.\n\nBest regards,\nTennis Club");
                }
            }
        }

        return RedirectToAction(nameof(ViewMembers));
    }

    // GET: Admin/AssignCoach
    public async Task<IActionResult> AssignCoach()
    {
        var upcomingSchedules = await _context.Schedules
            .Include(s => s.Coach)
            .Where(s => s.Date > DateTime.Now)
            .Select(s => new ScheduleViewModel
            {
                ScheduleId = s.ScheduleId,
                Name = s.Name,
                Location = s.Location,
                Date = s.Date,
                CoachName = s.Coach != null ? s.Coach.FirstName + " " + s.Coach.LastName : "None",
                CoachId = s.CoachId
            })
            .ToListAsync();

        ViewData["Coaches"] = new SelectList(_context.Coaches, "CoachId", "FirstName");
        return View(upcomingSchedules);
    }



    // POST: Admin/AssignCoach
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignCoach(int scheduleId, int coachId)
    {
        // Check if the CoachId exists
        var coachExists = await _context.Coaches.AnyAsync(c => c.CoachId == coachId);
        if (!coachExists)
        {
            ModelState.AddModelError("", "The selected coach does not exist.");
            return RedirectToAction(nameof(AssignCoach));
        }

        var schedule = await _context.Schedules.FindAsync(scheduleId);
        if (schedule == null)
        {
            return NotFound();
        }

        schedule.CoachId = coachId;
        _context.Update(schedule);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(AssignCoach));
    }



    // GET: Admin/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var schedule = await _context.Schedules
            .Include(s => s.Coach)
            .Include(s => s.MemberSchedules)
                .ThenInclude(ms => ms.Member)
            .FirstOrDefaultAsync(m => m.ScheduleId == id);

        if (schedule == null)
        {
            return NotFound();
        }

        return View(schedule);
    }


    // GET: Admin/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule == null)
        {
            return NotFound();
        }
        ViewData["Coaches"] = new SelectList(_context.Coaches, "CoachId", "FirstName", schedule.CoachId);
        return View(schedule);
    }

    // POST: Admin/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ScheduleId,Name,Location,Date,CoachId")] Schedule schedule)
    {
        if (id != schedule.ScheduleId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(schedule);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleExists(schedule.ScheduleId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["Coaches"] = new SelectList(_context.Coaches, "CoachId", "FirstName", schedule.CoachId);
        return View(schedule);
    }

    // POST: Admin/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule != null)
        {
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool ScheduleExists(int id)
    {
        return _context.Schedules.Any(e => e.ScheduleId == id);
    }
}
