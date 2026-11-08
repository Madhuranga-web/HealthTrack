using HealthTrack.Data;
using HealthTrack.Models;
using Microsoft.EntityFrameworkCore;
namespace HealthTrack.Services;
public class HealthService(AppDbContext db) {
    public static decimal CalculateBmi(decimal weightKg, decimal heightCm) {
        if(weightKg<20 || weightKg>500 || heightCm<50 || heightCm>250) throw new ArgumentOutOfRangeException(nameof(weightKg));
        var metres=heightCm/100m; return Math.Round(weightKg/(metres*metres),1);
    }
    public Task<bool> IsAssigned(string trainerId,string clientId) => db.TrainerAssignments.AnyAsync(x=>x.TrainerId==trainerId&&x.ClientId==clientId&&x.Trainer.IsActive&&x.Client.IsActive);
    public async Task<bool> HasConflict(string trainerId,DateTime startsUtc,int minutes,int? excludeId=null) => await db.Appointments.AnyAsync(x=>x.TrainerId==trainerId&&x.Status==AppointmentStatus.Approved&&x.Id!=excludeId&&x.StartsUtc<startsUtc.AddMinutes(minutes)&&startsUtc<x.StartsUtc.AddMinutes(x.DurationMinutes));
    public async Task<bool> HasClientConflict(string clientId,DateTime startsUtc,int minutes,int? excludeId=null) => await db.Appointments.AnyAsync(x=>x.ClientId==clientId&&x.Status==AppointmentStatus.Approved&&x.Id!=excludeId&&x.StartsUtc<startsUtc.AddMinutes(minutes)&&startsUtc<x.StartsUtc.AddMinutes(x.DurationMinutes));
    public static bool ValidBooking(DateTime startsUtc,int minutes,DateTime nowUtc) => startsUtc>nowUtc.AddMinutes(10) && startsUtc<nowUtc.AddYears(1) && minutes is >=15 and <=180;
    public async Task Notify(string userId,string message) { db.Notifications.Add(new Notification{UserId=userId,Message=message}); await db.SaveChangesAsync(); }
    public async Task Activity(string userId,string action) { db.ActivityLogs.Add(new ActivityLog{UserId=userId,Action=action}); await db.SaveChangesAsync(); }
}
