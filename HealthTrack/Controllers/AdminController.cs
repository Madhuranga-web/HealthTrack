using HealthTrack.Data;
using HealthTrack.Models;
using HealthTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HealthTrack.Controllers;
[Authorize(Policy="Admin")]
public class AdminController(AppDbContext db,UserManager<ApplicationUser> users) : Controller {
    public async Task<IActionResult> Index() {
        ViewBag.ClientCount=await users.GetUsersInRoleAsync("Client"); ViewBag.TrainerCount=await users.GetUsersInRoleAsync("Trainer");
        ViewBag.AppointmentCount=await db.Appointments.CountAsync(); ViewBag.Activity=await db.ActivityLogs.Include(x=>x.User).OrderByDescending(x=>x.CreatedUtc).Take(10).ToListAsync();
        ViewBag.Months=Enumerable.Range(0,6).Select(i=>DateTime.UtcNow.AddMonths(i-5).ToString("MMM yy")).ToArray();
        ViewBag.Engagement=Enumerable.Range(0,6).Select(i=>db.ActivityLogs.Count(x=>x.CreatedUtc.Year==DateTime.UtcNow.AddMonths(i-5).Year&&x.CreatedUtc.Month==DateTime.UtcNow.AddMonths(i-5).Month)).ToArray();
        return View();
    }
    public async Task<IActionResult> Users(string? q) {
        var query=db.Users.AsQueryable(); if(!string.IsNullOrWhiteSpace(q)) query=query.Where(x=>x.FullName.Contains(q)||x.Email!.Contains(q));
        return View(await query.OrderBy(x=>x.FullName).ToListAsync());
    }
    public IActionResult Create()=>View();
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Create(string fullName,string email,string role,string password) {
        if(role is not ("Trainer" or "Client")||string.IsNullOrWhiteSpace(fullName)||!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email)||string.IsNullOrWhiteSpace(password)) ModelState.AddModelError("","Valid name, email and role required.");
        if(!ModelState.IsValid) return View();
        var user=new ApplicationUser{FullName=fullName,Email=email,UserName=email}; var result=await users.CreateAsync(user,password);
        if(!result.Succeeded) { foreach(var e in result.Errors) ModelState.AddModelError("",e.Description); return View(); }
        await users.AddToRoleAsync(user,role); if(role=="Trainer") db.TrainerProfiles.Add(new TrainerProfile{UserId=user.Id}); else db.ClientProfiles.Add(new ClientProfile{UserId=user.Id}); await db.SaveChangesAsync();
        TempData["Success"]="Account created."; return RedirectToAction(nameof(Users));
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Toggle(string id) {
        var user=await users.FindByIdAsync(id); if(user==null) return NotFound(); if(user.Id==users.GetUserId(User)) return BadRequest("You cannot deactivate yourself.");
        user.IsActive=!user.IsActive; await users.UpdateAsync(user); TempData["Success"]="Account status updated."; return RedirectToAction(nameof(Users));
    }
    public async Task<IActionResult> Assignments() { ViewBag.Trainers=await users.GetUsersInRoleAsync("Trainer"); ViewBag.Clients=await users.GetUsersInRoleAsync("Client"); return View(await db.TrainerAssignments.Include(x=>x.Trainer).Include(x=>x.Client).ToListAsync()); }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Assign(string trainerId,string clientId) {
        var trainer=await users.FindByIdAsync(trainerId); var client=await users.FindByIdAsync(clientId);
        if(trainer==null||client==null||!trainer.IsActive||!client.IsActive||!await users.IsInRoleAsync(trainer,"Trainer")||!await users.IsInRoleAsync(client,"Client")) return BadRequest();
        if(!await db.TrainerAssignments.AnyAsync(x=>x.TrainerId==trainerId&&x.ClientId==clientId)) { db.TrainerAssignments.Add(new TrainerAssignment{TrainerId=trainerId,ClientId=clientId}); await db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Assignments));
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Unassign(int id) { var item=await db.TrainerAssignments.FindAsync(id); if(item==null) return NotFound(); db.TrainerAssignments.Remove(item); await db.SaveChangesAsync(); return RedirectToAction(nameof(Assignments)); }
    public async Task<IActionResult> Appointments()=>View(await db.Appointments.Include(x=>x.Client).Include(x=>x.Trainer).OrderByDescending(x=>x.StartsUtc).ToListAsync());
    public async Task<IActionResult> Settings() { var item=await db.SystemSettings.FindAsync("DashboardMessage"); ViewBag.Message=item?.Value??""; return View(); }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Settings(string message) {
        if(string.IsNullOrWhiteSpace(message)||message.Length>250) { ModelState.AddModelError("","Enter a message up to 250 characters."); ViewBag.Message=message; return View(); }
        var item=await db.SystemSettings.FindAsync("DashboardMessage"); if(item==null) db.SystemSettings.Add(new SystemSetting{Key="DashboardMessage",Value=message.Trim()}); else item.Value=message.Trim();
        await db.SaveChangesAsync(); TempData["Success"]="Dashboard message saved."; return RedirectToAction(nameof(Settings));
    }
    public async Task<IActionResult> Reports() { ViewBag.Popular=await db.WorkoutLogs.GroupBy(x=>x.WorkoutPlan.Title).Select(g=>new PlanPopularity{Title=g.Key,Count=g.Count()}).OrderByDescending(x=>x.Count).Take(5).ToListAsync(); ViewBag.Workouts=await db.WorkoutLogs.CountAsync(); ViewBag.Meals=await db.MealLogs.CountAsync(); ViewBag.Metrics=await db.HealthMetrics.CountAsync(); ViewBag.Pending=await db.Appointments.CountAsync(x=>x.Status==AppointmentStatus.Pending); return View(); }
}
