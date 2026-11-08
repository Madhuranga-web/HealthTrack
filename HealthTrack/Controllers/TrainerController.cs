using HealthTrack.Data;
using HealthTrack.Models;
using HealthTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HealthTrack.Controllers;
[Authorize(Policy="Trainer")]
public class TrainerController(AppDbContext db,UserManager<ApplicationUser> users,HealthService service):Controller {
    string Id => users.GetUserId(User)!;
    public async Task<IActionResult> Index() { ViewBag.Announcement=await db.SystemSettings.Where(x=>x.Key=="DashboardMessage").Select(x=>x.Value).FirstOrDefaultAsync(); ViewBag.ClientCount=await db.TrainerAssignments.CountAsync(x=>x.TrainerId==Id&&x.Client.IsActive); ViewBag.Pending=await db.Appointments.CountAsync(x=>x.TrainerId==Id&&x.Status==AppointmentStatus.Pending); ViewBag.Plans=await db.WorkoutPlans.CountAsync(x=>x.TrainerId==Id); return View(); }
    public async Task<IActionResult> Clients()=>View(await db.TrainerAssignments.Where(x=>x.TrainerId==Id&&x.Client.IsActive).Include(x=>x.Client).ToListAsync());
    public async Task<IActionResult> Progress(string id) {
        if(!await service.IsAssigned(Id,id)) return Forbid();
        ViewBag.Client=await users.FindByIdAsync(id); ViewBag.Metrics=await db.HealthMetrics.Where(x=>x.ClientId==id).OrderBy(x=>x.RecordedUtc).ToListAsync();
        ViewBag.Workouts=await db.WorkoutLogs.Where(x=>x.ClientId==id).OrderByDescending(x=>x.LoggedUtc).Take(20).ToListAsync();
        ViewBag.Meals=await db.MealLogs.Where(x=>x.ClientId==id).OrderByDescending(x=>x.LoggedUtc).Take(20).ToListAsync();
        ViewBag.Feedback=await db.TrainerFeedback.Where(x=>x.ClientId==id&&x.TrainerId==Id).OrderByDescending(x=>x.CreatedUtc).ToListAsync(); return View();
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Feedback(string clientId,string message) {
        if(!await service.IsAssigned(Id,clientId)) return Forbid(); if(string.IsNullOrWhiteSpace(message)||message.Length>1000) return BadRequest();
        db.TrainerFeedback.Add(new TrainerFeedback{TrainerId=Id,ClientId=clientId,Message=message}); await db.SaveChangesAsync(); await service.Notify(clientId,"New trainer feedback received."); await service.Activity(Id,"Trainer feedback sent"); return RedirectToAction(nameof(Progress),new{id=clientId});
    }
    public async Task<IActionResult> Reports() {
        var ids=await db.TrainerAssignments.Where(x=>x.TrainerId==Id&&x.Client.IsActive).Select(x=>x.ClientId).ToListAsync();
        ViewBag.Clients=ids.Count; ViewBag.Workouts=await db.WorkoutLogs.CountAsync(x=>ids.Contains(x.ClientId)); ViewBag.Meals=await db.MealLogs.CountAsync(x=>ids.Contains(x.ClientId)); ViewBag.Metrics=await db.HealthMetrics.CountAsync(x=>ids.Contains(x.ClientId)); return View();
    }
}
