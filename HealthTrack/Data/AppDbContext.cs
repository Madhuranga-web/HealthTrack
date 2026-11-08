using HealthTrack.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace HealthTrack.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options) {
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>(); public DbSet<TrainerProfile> TrainerProfiles => Set<TrainerProfile>(); public DbSet<TrainerAssignment> TrainerAssignments => Set<TrainerAssignment>(); public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>(); public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>(); public DbSet<WorkoutLog> WorkoutLogs => Set<WorkoutLog>(); public DbSet<DietPlan> DietPlans => Set<DietPlan>(); public DbSet<DietPlanMeal> DietPlanMeals => Set<DietPlanMeal>(); public DbSet<MealLog> MealLogs => Set<MealLog>(); public DbSet<HealthMetric> HealthMetrics => Set<HealthMetric>(); public DbSet<Appointment> Appointments => Set<Appointment>(); public DbSet<Notification> Notifications => Set<Notification>(); public DbSet<TrainerFeedback> TrainerFeedback => Set<TrainerFeedback>(); public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    protected override void OnModelCreating(ModelBuilder b) {
        base.OnModelCreating(b);
        b.Entity<TrainerAssignment>().HasIndex(x=>new{x.TrainerId,x.ClientId}).IsUnique();
        b.Entity<TrainerAssignment>().HasOne(x=>x.Trainer).WithMany().HasForeignKey(x=>x.TrainerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<TrainerAssignment>().HasOne(x=>x.Client).WithMany().HasForeignKey(x=>x.ClientId).OnDelete(DeleteBehavior.Restrict);
        foreach (var t in new[]{typeof(WorkoutPlan),typeof(DietPlan),typeof(Appointment),typeof(TrainerFeedback)}) {
            b.Entity(t).HasOne(typeof(ApplicationUser),"Trainer").WithMany().HasForeignKey("TrainerId").OnDelete(DeleteBehavior.Restrict);
            b.Entity(t).HasOne(typeof(ApplicationUser),"Client").WithMany().HasForeignKey("ClientId").OnDelete(DeleteBehavior.Restrict);
        }
        foreach (var t in new[]{typeof(WorkoutLog),typeof(MealLog),typeof(HealthMetric)}) b.Entity(t).HasOne(typeof(ApplicationUser),"Client").WithMany().HasForeignKey("ClientId").OnDelete(DeleteBehavior.Restrict);
        b.Entity<WorkoutLog>().HasOne(x=>x.WorkoutPlan).WithMany().HasForeignKey(x=>x.WorkoutPlanId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<WorkoutExercise>().HasOne(x=>x.WorkoutPlan).WithMany(x=>x.Exercises).HasForeignKey(x=>x.WorkoutPlanId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<DietPlanMeal>().HasOne(x=>x.DietPlan).WithMany(x=>x.Meals).HasForeignKey(x=>x.DietPlanId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Appointment>().HasIndex(x=>new{x.TrainerId,x.StartsUtc,x.Status});
        b.Entity<HealthMetric>().HasIndex(x=>new{x.ClientId,x.RecordedUtc});
        b.Entity<Notification>().HasIndex(x=>new{x.UserId,x.IsRead,x.CreatedUtc});
        b.Entity<ActivityLog>().HasIndex(x=>new{x.UserId,x.CreatedUtc});
        foreach(var t in new[]{typeof(ClientProfile),typeof(TrainerProfile),typeof(DietPlanMeal),typeof(WorkoutExercise),typeof(Notification),typeof(ActivityLog)}) {
            var fk=t==typeof(ClientProfile)||t==typeof(TrainerProfile)?"UserId":null;
            if(fk!=null) b.Entity(t).HasOne(typeof(ApplicationUser),"User").WithMany().HasForeignKey(fk).OnDelete(DeleteBehavior.Restrict);
        }
        b.Entity<Notification>().HasOne(x=>x.User).WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ActivityLog>().HasOne(x=>x.User).WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<TrainerFeedback>().Property(x=>x.Message).HasMaxLength(1000);
        foreach(var entity in new[]{typeof(ClientProfile),typeof(HealthMetric),typeof(MealLog),typeof(DietPlan),typeof(DietPlanMeal)}) {
            foreach(var property in b.Entity(entity).Metadata.GetProperties().Where(p=>p.ClrType==typeof(decimal)||p.ClrType==typeof(decimal?))) b.Entity(entity).Property(property.Name).HasPrecision(9,2);
        }
    }
}
