using HealthTrack.Data;
using HealthTrack.Models;
using HealthTrack.Services;
using HealthTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HealthTrack.Controllers;
[Authorize(Policy="Client")]
public class ClientController(AppDbContext db,UserManager<ApplicationUser> users,HealthService service):Controller {
    string Id => users.GetUserId(User)!;
    public async Task<IActionResult> Index() { ViewBag.Announcement=await db.SystemSettings.Where(x=>x.Key=="DashboardMessage").Select(x=>x.Value).FirstOrDefaultAsync();
        ViewBag.Metric=await db.HealthMetrics.Where(x=>x.ClientId==Id).OrderByDescending(x=>x.RecordedUtc).FirstOrDefaultAsync();
        ViewBag.WorkoutCount=await db.WorkoutPlans.CountAsync(x=>x.ClientId==Id); ViewBag.DietCount=await db.DietPlans.CountAsync(x=>x.ClientId==Id);
        ViewBag.Upcoming=await db.Appointments.Where(x=>x.ClientId==Id&&x.StartsUtc>DateTime.UtcNow&&x.Status==AppointmentStatus.Approved).OrderBy(x=>x.StartsUtc).FirstOrDefaultAsync();
        ViewBag.Unread=await db.Notifications.CountAsync(x=>x.UserId==Id&&!x.IsRead); return View();
    }
    public async Task<IActionResult> Progress() {
        ViewBag.Metrics=await db.HealthMetrics.Where(x=>x.ClientId==Id).OrderBy(x=>x.RecordedUtc).ToListAsync();
        ViewBag.Workouts=await db.WorkoutLogs.Where(x=>x.ClientId==Id).OrderByDescending(x=>x.LoggedUtc).ToListAsync();
        ViewBag.Meals=await db.MealLogs.Where(x=>x.ClientId==Id).OrderByDescending(x=>x.LoggedUtc).ToListAsync();
        return View();
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Metric(MetricForm form) {
        if(!ModelState.IsValid) return BadRequest("Enter weight 20–500 kg and height 50–250 cm.");
        var bmi=HealthService.CalculateBmi(form.WeightKg,form.HeightCm); db.HealthMetrics.Add(new HealthMetric{ClientId=Id,WeightKg=form.WeightKg,HeightCm=form.HeightCm,Bmi=bmi});
        var profile=await db.ClientProfiles.FindAsync(Id); if(profile!=null) profile.HeightCm=form.HeightCm;
        await db.SaveChangesAsync(); return RedirectToAction(nameof(Progress));
    }
    public async Task<IActionResult> Workouts()=>View(await db.WorkoutPlans.Where(x=>x.ClientId==Id).Include(x=>x.Exercises).Include(x=>x.Trainer).ToListAsync());
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> LogWorkout(WorkoutLogForm form) {
        if(!ModelState.IsValid) return BadRequest("Check workout log values.");
        if(!await db.WorkoutPlans.AnyAsync(x=>x.Id==form.WorkoutPlanId&&x.ClientId==Id)) return Forbid();
        db.WorkoutLogs.Add(new WorkoutLog{ClientId=Id,WorkoutPlanId=form.WorkoutPlanId,CompletedExercise=form.CompletedExercise,DurationMinutes=form.DurationMinutes,CaloriesBurned=form.CaloriesBurned}); await db.SaveChangesAsync();
        await service.Activity(Id,"Workout logged"); return RedirectToAction(nameof(Workouts));
    }
    public async Task<IActionResult> Diet()=>View(await db.DietPlans.Where(x=>x.ClientId==Id).Include(x=>x.Meals).Include(x=>x.Trainer).ToListAsync());
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> LogMeal(MealLogForm form) {
        if(!ModelState.IsValid) return BadRequest("Check meal log values."); db.MealLogs.Add(new MealLog{ClientId=Id,Food=form.Food,CaloriesConsumed=form.CaloriesConsumed,ProteinGrams=form.ProteinGrams}); await db.SaveChangesAsync(); await service.Activity(Id,"Meal logged"); return RedirectToAction(nameof(Diet));
    }
    public async Task<IActionResult> Profile() { ViewBag.User=await users.GetUserAsync(User); ViewBag.Profile=await db.ClientProfiles.FindAsync(Id); return View(); }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Goals(string goals) { if(goals==null||goals.Length>500) return BadRequest(); var p=await db.ClientProfiles.FindAsync(Id); if(p==null) return NotFound(); p.Goals=goals; await db.SaveChangesAsync(); return RedirectToAction(nameof(Profile)); }
}
