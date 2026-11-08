using HealthTrack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HealthTrack.Data;
public class DemoSeeder(AppDbContext db,UserManager<ApplicationUser> users,RoleManager<IdentityRole> roles,IConfiguration config) {
    public async Task RunAsync() {
        var password=config["SeedPassword"];
        if(string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Set SeedPassword with user secrets or HEALTHTRACK_SeedPassword before enabling SeedDemoData.");
        await db.Database.MigrateAsync();
        foreach(var role in new[]{"Admin","Trainer","Client"}) if(!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));
        async Task<ApplicationUser> Add(string name,string email,string role) {
            var user=await users.FindByEmailAsync(email);
            if(user!=null) return user;
            user=new ApplicationUser{FullName=name,Email=email,UserName=email,EmailConfirmed=true};
            var result=await users.CreateAsync(user,password);
            if(!result.Succeeded) throw new InvalidOperationException(string.Join("; ",result.Errors.Select(x=>x.Description)));
            await users.AddToRoleAsync(user,role); return user;
        }
        var admin=await Add("Demo Administrator","admin@healthtrack.test","Admin");
        var trainer=await Add("Morgan Trainer","trainer@healthtrack.test","Trainer");
        var client=await Add("Alex Client","client@healthtrack.test","Client");
        if(!await db.TrainerProfiles.AnyAsync(x=>x.UserId==trainer.Id)) db.TrainerProfiles.Add(new TrainerProfile{UserId=trainer.Id,Specialty="Strength and mobility"});
        if(!await db.ClientProfiles.AnyAsync(x=>x.UserId==client.Id)) db.ClientProfiles.Add(new ClientProfile{UserId=client.Id,HeightCm=172,Goals="Build a consistent routine"});
        if(!await db.TrainerAssignments.AnyAsync(x=>x.TrainerId==trainer.Id&&x.ClientId==client.Id)) db.TrainerAssignments.Add(new TrainerAssignment{TrainerId=trainer.Id,ClientId=client.Id});
        if(!await db.HealthMetrics.AnyAsync(x=>x.ClientId==client.Id)) db.HealthMetrics.Add(new HealthMetric{ClientId=client.Id,HeightCm=172,WeightKg=74,Bmi=25,RecordedUtc=DateTime.UtcNow.AddDays(-7)});
        if(!await db.WorkoutPlans.AnyAsync(x=>x.ClientId==client.Id)) db.WorkoutPlans.Add(new WorkoutPlan{TrainerId=trainer.Id,ClientId=client.Id,Title="Foundation strength",Description="Three sessions weekly",Exercises=[new WorkoutExercise{Name="Bodyweight squat",Sets=3,Repetitions=12,DurationMinutes=10}]});
        if(!await db.DietPlans.AnyAsync(x=>x.ClientId==client.Id)) db.DietPlans.Add(new DietPlan{TrainerId=trainer.Id,ClientId=client.Id,Title="Balanced meals",DailyCalorieTarget=2200,Meals=[new DietPlanMeal{Name="Breakfast",FoodDescription="Oats, fruit and yogurt",CalorieTarget=500,ProteinGrams=20,ScheduledTime=new TimeOnly(8,0)}]});
        await db.SaveChangesAsync();
    }
}
