using HealthTrack.Data;
using HealthTrack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthTrack.Tests;

public class DatabaseSeedTests
{
    [Fact]
    public void SqlServerModelContainsExpectedTablesAndRelationships()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=HealthTrack;Trusted_Connection=True")
            .Options);
        var expected = new[]
        {
            "AspNetUsers", "AspNetRoles", "AspNetUserRoles", "AspNetUserClaims", "AspNetUserLogins",
            "AspNetUserTokens", "AspNetRoleClaims", "ClientProfiles", "TrainerProfiles", "TrainerAssignments",
            "WorkoutPlans", "WorkoutExercises", "WorkoutLogs", "DietPlans", "DietPlanMeals", "MealLogs",
            "HealthMetrics", "Appointments", "Notifications", "TrainerFeedback", "ActivityLogs", "SystemSettings"
        };
        var actual = db.Model.GetEntityTypes().Select(e => e.GetTableName()).ToHashSet();
        Assert.Equal(expected.OrderBy(x => x), actual.OrderBy(x => x));
        Assert.All(db.Model.GetEntityTypes(), entity => Assert.NotNull(entity.FindPrimaryKey()));
        Assert.Equal(26, db.Model.GetEntityTypes().Sum(entity => entity.GetForeignKeys().Count()));
        var assignment = db.Model.FindEntityType(typeof(TrainerAssignment))!;
        Assert.Contains(assignment.GetIndexes(), i => i.IsUnique &&
            i.Properties.Select(p => p.Name).SequenceEqual(new[] { "TrainerId", "ClientId" }));
        Assert.Equal(2, assignment.GetForeignKeys().Count());
        var metric = db.Model.FindEntityType(typeof(HealthMetric))!;
        Assert.Equal(9, metric.FindProperty(nameof(HealthMetric.WeightKg))!.GetPrecision());
        Assert.Equal(2, metric.FindProperty(nameof(HealthMetric.WeightKg))!.GetScale());
        Assert.Equal("datetime2", metric.FindProperty(nameof(HealthMetric.RecordedUtc))!.GetColumnType());
        var meal = db.Model.FindEntityType(typeof(DietPlanMeal))!;
        Assert.Equal("time", meal.FindProperty(nameof(DietPlanMeal.ScheduledTime))!.GetColumnType());
    }
    [Fact]
    public async Task DemoSeedIsRepeatableAndUsesIdentityPasswordHashes()
    {
        var password = Guid.NewGuid().ToString("N") + "Aa1!";
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(connection));
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["SeedPassword"] = password }).Build());
        services.AddScoped<DemoSeeder>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        var seeder = scope.ServiceProvider.GetRequiredService<DemoSeeder>();
        await seeder.RunAsync();
        await seeder.RunAsync();

        Assert.Equal(7, await db.Users.CountAsync());
        Assert.Equal(3, await db.Roles.CountAsync());
        Assert.Equal(7, await db.UserRoles.CountAsync());
        Assert.Equal(2, await db.TrainerProfiles.CountAsync());
        Assert.Equal(4, await db.ClientProfiles.CountAsync());
        Assert.Equal(4, await db.TrainerAssignments.CountAsync());
        Assert.Equal(4, await db.WorkoutPlans.CountAsync());
        Assert.Equal(8, await db.WorkoutExercises.CountAsync());
        Assert.Equal(4, await db.DietPlans.CountAsync());
        Assert.Equal(8, await db.DietPlanMeals.CountAsync());
        Assert.Equal(8, await db.WorkoutLogs.CountAsync());
        Assert.Equal(8, await db.MealLogs.CountAsync());
        Assert.Equal(8, await db.HealthMetrics.CountAsync());
        Assert.Equal(4, await db.Appointments.CountAsync());
        Assert.Equal(4, await db.Appointments.Select(a => a.Status).Distinct().CountAsync());
        Assert.Equal(6, await db.Notifications.CountAsync());
        Assert.All(await db.Users.ToListAsync(), u => Assert.NotEqual(password, u.PasswordHash));
        var user = await db.Users.SingleAsync(u => u.Email == "client@healthtrack.test");
        Assert.Equal(PasswordVerificationResult.Success,
            new PasswordHasher<ApplicationUser>().VerifyHashedPassword(user, user.PasswordHash!, password));

        var assignment = await db.TrainerAssignments.FirstAsync();
        db.TrainerAssignments.Add(new TrainerAssignment { TrainerId = assignment.TrainerId, ClientId = assignment.ClientId });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        db.Entry(db.TrainerAssignments.Local.Last()).State = EntityState.Detached;
        db.Notifications.Add(new Notification { UserId = "nonexistent-user", Message = "Invalid relationship" });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}
