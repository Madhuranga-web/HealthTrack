using HealthTrack.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthTrack.Models;
namespace HealthTrack.Controllers;
[Authorize]
public class NotificationsController(AppDbContext db,UserManager<ApplicationUser> users):Controller {
    public async Task<IActionResult> Index()=>View(await db.Notifications.Where(x=>x.UserId==users.GetUserId(User)).OrderByDescending(x=>x.CreatedUtc).Take(100).ToListAsync());
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Read(int id) { var item=await db.Notifications.FirstOrDefaultAsync(x=>x.Id==id&&x.UserId==users.GetUserId(User)); if(item==null) return NotFound(); item.IsRead=true; await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
}
