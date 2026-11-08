using HealthTrack.Data;
using HealthTrack.Models;
using HealthTrack.Services;
using HealthTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
namespace HealthTrack.Controllers;
[Authorize]
public class AppointmentsController(AppDbContext db,UserManager<ApplicationUser> users,HealthService service):Controller {
    string Id=>users.GetUserId(User)!;
    public async Task<IActionResult> Index() {
        if(!User.IsInRole("Client")&&!User.IsInRole("Trainer")) return Forbid();
        var q=db.Appointments.Include(x=>x.Client).Include(x=>x.Trainer).AsQueryable(); q=User.IsInRole("Client")?q.Where(x=>x.ClientId==Id):q.Where(x=>x.TrainerId==Id&&db.TrainerAssignments.Any(a=>a.TrainerId==Id&&a.ClientId==x.ClientId&&a.Client.IsActive&&a.Trainer.IsActive));
        return View(await q.OrderByDescending(x=>x.StartsUtc).ToListAsync());
    }
    [Authorize(Policy="Client")] public async Task<IActionResult> Book() { ViewBag.Trainers=await db.TrainerAssignments.Where(x=>x.ClientId==Id&&x.Trainer.IsActive).Include(x=>x.Trainer).Select(x=>x.Trainer).ToListAsync(); return View(new AppointmentForm{StartsUtc=DateTime.UtcNow.AddDays(1)}); }
    [HttpPost,Authorize(Policy="Client"),ValidateAntiForgeryToken] public async Task<IActionResult> Book(AppointmentForm form) {
        if(!await service.IsAssigned(form.TrainerId,Id)) return Forbid();
        var start=DateTime.SpecifyKind(form.StartsUtc,DateTimeKind.Utc);
        if(!HealthService.ValidBooking(start,form.DurationMinutes,DateTime.UtcNow)) ModelState.AddModelError(nameof(form.StartsUtc),"Choose a UTC time at least 10 minutes ahead and within one year.");
        if(!ModelState.IsValid) { ViewBag.Trainers=await db.TrainerAssignments.Where(x=>x.ClientId==Id&&x.Trainer.IsActive).Include(x=>x.Trainer).Select(x=>x.Trainer).ToListAsync(); return View(form); }
        if(await service.HasConflict(form.TrainerId,start,form.DurationMinutes)||await service.HasClientConflict(Id,start,form.DurationMinutes)) { ModelState.AddModelError("","Trainer has a confirmed appointment at this time."); ViewBag.Trainers=await db.TrainerAssignments.Where(x=>x.ClientId==Id&&x.Trainer.IsActive).Include(x=>x.Trainer).Select(x=>x.Trainer).ToListAsync(); return View(form); }
        db.Appointments.Add(new Appointment{ClientId=Id,TrainerId=form.TrainerId,StartsUtc=start,DurationMinutes=form.DurationMinutes,Purpose=form.Purpose}); await db.SaveChangesAsync(); await service.Notify(form.TrainerId,"New appointment request received."); await service.Activity(Id,"Appointment requested"); return RedirectToAction(nameof(Index));
    }
    [HttpPost,Authorize(Policy="Trainer"),ValidateAntiForgeryToken] public async Task<IActionResult> Decide(int id,string decision) {
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var a=await db.Appointments.FirstOrDefaultAsync(x=>x.Id==id&&x.TrainerId==Id); if(a==null) return NotFound(); if(!await service.IsAssigned(Id,a.ClientId)) return Forbid(); if(a.Status!=AppointmentStatus.Pending) return BadRequest();
        if(decision=="Approve") { if(await service.HasConflict(Id,a.StartsUtc,a.DurationMinutes,a.Id)||await service.HasClientConflict(a.ClientId,a.StartsUtc,a.DurationMinutes,a.Id)) { TempData["Error"]="Appointment overlaps a confirmed booking."; return RedirectToAction(nameof(Index)); } a.Status=AppointmentStatus.Approved; }
        else if(decision=="Reject") a.Status=AppointmentStatus.Rejected; else return BadRequest();
        await db.SaveChangesAsync(); await service.Notify(a.ClientId,"Appointment "+a.Status.ToString().ToLowerInvariant()+"."); await service.Activity(Id,"Appointment "+a.Status); await tx.CommitAsync(); return RedirectToAction(nameof(Index));
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Cancel(int id) {
        if(!User.IsInRole("Client")&&!User.IsInRole("Trainer")) return Forbid();
        var a=await db.Appointments.FirstOrDefaultAsync(x=>x.Id==id&&(User.IsInRole("Client")?x.ClientId==Id:x.TrainerId==Id)); if(a==null) return NotFound(); if(User.IsInRole("Trainer")&&!await service.IsAssigned(Id,a.ClientId)) return Forbid();
        if(a.Status is AppointmentStatus.Cancelled or AppointmentStatus.Rejected) return BadRequest(); a.Status=AppointmentStatus.Cancelled; await db.SaveChangesAsync(); await service.Notify(User.IsInRole("Client")?a.TrainerId:a.ClientId,"Appointment cancelled."); await service.Activity(Id,"Appointment cancelled"); return RedirectToAction(nameof(Index));
    }
    [Authorize(Policy="Trainer")] public async Task<IActionResult> Reschedule(int id) { var a=await db.Appointments.FirstOrDefaultAsync(x=>x.Id==id&&x.TrainerId==Id); if(a==null) return NotFound(); if(!await service.IsAssigned(Id,a.ClientId)) return Forbid(); return View(new AppointmentForm{Id=a.Id,TrainerId=Id,StartsUtc=a.StartsUtc,DurationMinutes=a.DurationMinutes,Purpose=a.Purpose}); }
    [HttpPost,Authorize(Policy="Trainer"),ValidateAntiForgeryToken] public async Task<IActionResult> Reschedule(AppointmentForm form) {
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var a=await db.Appointments.FirstOrDefaultAsync(x=>x.Id==form.Id&&x.TrainerId==Id); if(a==null) return NotFound(); if(!await service.IsAssigned(Id,a.ClientId)) return Forbid(); if(a.Status is AppointmentStatus.Cancelled or AppointmentStatus.Rejected) return BadRequest();
        var start=DateTime.SpecifyKind(form.StartsUtc,DateTimeKind.Utc); if(!ModelState.IsValid||!HealthService.ValidBooking(start,form.DurationMinutes,DateTime.UtcNow)) { ModelState.AddModelError("","Check date, duration and purpose."); return View(form); }
        if(await service.HasConflict(Id,start,form.DurationMinutes,a.Id)||await service.HasClientConflict(a.ClientId,start,form.DurationMinutes,a.Id)) { ModelState.AddModelError("","Time overlaps another approved appointment."); return View(form); }
        a.StartsUtc=start; a.DurationMinutes=form.DurationMinutes; a.Purpose=form.Purpose; a.Status=AppointmentStatus.Approved; await db.SaveChangesAsync(); await service.Notify(a.ClientId,"Appointment rescheduled and approved."); await service.Activity(Id,"Appointment rescheduled"); await tx.CommitAsync(); return RedirectToAction(nameof(Index));
    }
}
