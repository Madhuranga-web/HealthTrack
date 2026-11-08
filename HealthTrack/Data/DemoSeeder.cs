using HealthTrack.Models;
using HealthTrack.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthTrack.Data;

/// <summary>Idempotent fictional data for local demonstrations only.</summary>
public class DemoSeeder(AppDbContext db, UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles, IConfiguration config)
{
    public async Task RunAsync()
    {
        var password = config["SeedPassword"];
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Set SeedPassword in an environment variable or user secrets before enabling SeedDemoData.");
        foreach (var validator in users.PasswordValidators)
        {
            var check = await validator.ValidateAsync(users, new ApplicationUser(), password);
            if (!check.Succeeded)
                throw new InvalidOperationException("SeedPassword does not meet the Identity password policy: " + string.Join("; ", check.Errors.Select(e => e.Description)));
        }

        await using var transaction = await db.Database.BeginTransactionAsync();
        foreach (var role in new[] { "Admin", "Trainer", "Client" }) await EnsureRole(role);
        await EnsureUser("Demo Administrator", "admin@healthtrack.test", "Admin", password);

        var trainers = new[]
        {
            (Name: "Morgan Trainer", Email: "trainer@healthtrack.test", Specialty: "Strength and mobility"),
            (Name: "Jamie Trainer", Email: "trainer2@healthtrack.test", Specialty: "Cardio and endurance")
        };
        var clients = new[]
        {
            (Name: "Alex Client", Email: "client@healthtrack.test", Height: 172m, Weight: 74m, Goals: "Build a consistent routine", Workout: "Foundation strength", Diet: "Balanced meals"),
            (Name: "Taylor Client", Email: "client2@healthtrack.test", Height: 165m, Weight: 68m, Goals: "Improve mobility", Workout: "Mobility routine", Diet: "Everyday nutrition"),
            (Name: "Casey Client", Email: "client3@healthtrack.test", Height: 180m, Weight: 82m, Goals: "Increase endurance", Workout: "Cardio conditioning", Diet: "Training day fuel"),
            (Name: "Riley Client", Email: "client4@healthtrack.test", Height: 168m, Weight: 71m, Goals: "Stay active each week", Workout: "Full-body basics", Diet: "Healthy habits")
        };

        var trainerUsers = new List<ApplicationUser>();
        foreach (var trainer in trainers)
        {
            var user = await EnsureUser(trainer.Name, trainer.Email, "Trainer", password);
            trainerUsers.Add(user);
            if (!await db.TrainerProfiles.AnyAsync(x => x.UserId == user.Id))
                db.TrainerProfiles.Add(new TrainerProfile { UserId = user.Id, Specialty = trainer.Specialty });
        }

        var clientUsers = new List<ApplicationUser>();
        for (var i = 0; i < clients.Length; i++)
        {
            var item = clients[i];
            var client = await EnsureUser(item.Name, item.Email, "Client", password);
            var trainer = trainerUsers[i / 2];
            clientUsers.Add(client);
            if (!await db.ClientProfiles.AnyAsync(x => x.UserId == client.Id))
                db.ClientProfiles.Add(new ClientProfile { UserId = client.Id, HeightCm = item.Height, Goals = item.Goals });
            if (!await db.TrainerAssignments.AnyAsync(x => x.TrainerId == trainer.Id && x.ClientId == client.Id))
                db.TrainerAssignments.Add(new TrainerAssignment { TrainerId = trainer.Id, ClientId = client.Id });

            for (var point = 0; point < 2; point++)
            {
                var weight = item.Weight - point * 0.6m;
                if (!await db.HealthMetrics.AnyAsync(x => x.ClientId == client.Id && x.WeightKg == weight && x.HeightCm == item.Height))
                    db.HealthMetrics.Add(new HealthMetric
                    {
                        ClientId = client.Id, HeightCm = item.Height, WeightKg = weight,
                        Bmi = HealthService.CalculateBmi(weight, item.Height),
                        RecordedUtc = DateTime.UtcNow.Date.AddDays(-14 + point * 7).AddHours(8)
                    });
            }
            if (!await db.WorkoutPlans.AnyAsync(x => x.ClientId == client.Id && x.TrainerId == trainer.Id && x.Title == item.Workout))
                db.WorkoutPlans.Add(new WorkoutPlan
                {
                    TrainerId = trainer.Id, ClientId = client.Id, Title = item.Workout,
                    Description = "Two approachable sessions each week.",
                    Exercises =
                    [
                        new WorkoutExercise { Name = "Bodyweight squat", Sets = 3, Repetitions = 12, DurationMinutes = 10 },
                        new WorkoutExercise { Name = "Brisk walk", Sets = 1, Repetitions = 1, DurationMinutes = 25 }
                    ]
                });
            if (!await db.DietPlans.AnyAsync(x => x.ClientId == client.Id && x.TrainerId == trainer.Id && x.Title == item.Diet))
                db.DietPlans.Add(new DietPlan
                {
                    TrainerId = trainer.Id, ClientId = client.Id, Title = item.Diet, DailyCalorieTarget = 2100 + i * 100,
                    Meals =
                    [
                        new DietPlanMeal { Name = "Breakfast", FoodDescription = "Oats, fruit and yogurt", CalorieTarget = 500, ProteinGrams = 20, ScheduledTime = new TimeOnly(8, 0) },
                        new DietPlanMeal { Name = "Lunch", FoodDescription = "Rice, vegetables and beans", CalorieTarget = 650, ProteinGrams = 25, ScheduledTime = new TimeOnly(13, 0) }
                    ]
                });
        }
        await db.SaveChangesAsync();

        for (var i = 0; i < clients.Length; i++)
        {
            var item = clients[i];
            var client = clientUsers[i];
            var trainer = trainerUsers[i / 2];
            var plan = await db.WorkoutPlans.SingleAsync(x => x.ClientId == client.Id && x.TrainerId == trainer.Id && x.Title == item.Workout);
            if (!await db.WorkoutExercises.AnyAsync(x => x.WorkoutPlanId == plan.Id && x.Name == "Brisk walk"))
                db.WorkoutExercises.Add(new WorkoutExercise { WorkoutPlanId = plan.Id, Name = "Brisk walk", Sets = 1, Repetitions = 1, DurationMinutes = 25 });
            var diet = await db.DietPlans.SingleAsync(x => x.ClientId == client.Id && x.TrainerId == trainer.Id && x.Title == item.Diet);
            if (!await db.DietPlanMeals.AnyAsync(x => x.DietPlanId == diet.Id && x.Name == "Lunch"))
                db.DietPlanMeals.Add(new DietPlanMeal { DietPlanId = diet.Id, Name = "Lunch", FoodDescription = "Rice, vegetables and beans", CalorieTarget = 650, ProteinGrams = 25, ScheduledTime = new TimeOnly(13, 0) });
            foreach (var exercise in new[] { "Bodyweight squat", "Brisk walk" })
                if (!await db.WorkoutLogs.AnyAsync(x => x.ClientId == client.Id && x.WorkoutPlanId == plan.Id && x.CompletedExercise == exercise))
                    db.WorkoutLogs.Add(new WorkoutLog
                    {
                        ClientId = client.Id, WorkoutPlanId = plan.Id, CompletedExercise = exercise,
                        DurationMinutes = exercise == "Brisk walk" ? 25 : 15,
                        CaloriesBurned = exercise == "Brisk walk" ? 130 : 90,
                        LoggedUtc = DateTime.UtcNow.Date.AddDays(exercise == "Brisk walk" ? -2 : -5).AddHours(9)
                    });
            foreach (var meal in new[] { (Food: "Oat bowl", Calories: 480, Protein: 19m), (Food: "Rice and vegetables", Calories: 620, Protein: 23m) })
                if (!await db.MealLogs.AnyAsync(x => x.ClientId == client.Id && x.Food == meal.Food && x.CaloriesConsumed == meal.Calories))
                    db.MealLogs.Add(new MealLog
                    {
                        ClientId = client.Id, Food = meal.Food, CaloriesConsumed = meal.Calories, ProteinGrams = meal.Protein,
                        LoggedUtc = DateTime.UtcNow.Date.AddDays(-2).AddHours(meal.Food == "Oat bowl" ? 8 : 13)
                    });
            var purpose = $"Demo progress check-in {i + 1}";
            if (!await db.Appointments.AnyAsync(x => x.ClientId == client.Id && x.TrainerId == trainer.Id && x.Purpose == purpose))
                db.Appointments.Add(new Appointment
                {
                    ClientId = client.Id, TrainerId = trainer.Id, Purpose = purpose,
                    StartsUtc = DateTime.UtcNow.Date.AddDays(i + 3).AddHours(10 + i),
                    DurationMinutes = 45, Status = (AppointmentStatus)i
                });
            const string welcome = "Welcome to your HealthTrack demonstration account.";
            if (!await db.Notifications.AnyAsync(x => x.UserId == client.Id && x.Message == welcome))
                db.Notifications.Add(new Notification { UserId = client.Id, Message = welcome });
        }
        foreach (var trainer in trainerUsers)
        {
            const string assigned = "Your demonstration clients are ready to review.";
            if (!await db.Notifications.AnyAsync(x => x.UserId == trainer.Id && x.Message == assigned))
                db.Notifications.Add(new Notification { UserId = trainer.Id, Message = assigned });
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private async Task EnsureRole(string role)
    {
        if (await roles.RoleExistsAsync(role)) return;
        var result = await roles.CreateAsync(new IdentityRole(role));
        if (!result.Succeeded)
            throw new InvalidOperationException("Could not create demonstration role: " + string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    private async Task<ApplicationUser> EnsureUser(string name, string email, string role, string password)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is not null)
        {
            if (!await users.IsInRoleAsync(user, role))
                throw new InvalidOperationException($"The existing demonstration email {email} has another role. Seeding stopped without changing it.");
            return user;
        }
        user = new ApplicationUser { FullName = name, Email = email, UserName = email, EmailConfirmed = true };
        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException("Could not create demonstration user: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        result = await users.AddToRoleAsync(user, role);
        if (!result.Succeeded)
            throw new InvalidOperationException("Could not assign demonstration role: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        return user;
    }
}
