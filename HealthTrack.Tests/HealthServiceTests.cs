using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using HealthTrack.Controllers;
using HealthTrack.Data;
using HealthTrack.Models;
using HealthTrack.Services;
using HealthTrack.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
namespace HealthTrack.Tests;
public class HealthServiceTests {
    static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    static UserManager<ApplicationUser> Users(){var mock=new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(),null!,null!,null!,null!,null!,null!,null!,null!);mock.Setup(x=>x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((ClaimsPrincipal p)=>p.FindFirst(ClaimTypes.NameIdentifier)?.Value);return mock.Object;}
    static ClaimsPrincipal Principal(string id,string role) => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier,id),new Claim(ClaimTypes.Role,role)],"test"));
    [Theory][InlineData(70,175,22.9)][InlineData(80,180,24.7)] public void Bmi(decimal weight,decimal height,decimal expected)=>Assert.Equal(expected,HealthService.CalculateBmi(weight,height));
    [Fact] public void BmiRejectsImplausibleInput()=>Assert.Throws<ArgumentOutOfRangeException>(()=>HealthService.CalculateBmi(0,170));
    [Fact] public void BookingRejectsPastAndInvalidDuration(){var now=DateTime.UtcNow;Assert.False(HealthService.ValidBooking(now.AddMinutes(-1),60,now));Assert.False(HealthService.ValidBooking(now.AddHours(2),10,now));Assert.True(HealthService.ValidBooking(now.AddDays(1),60,now));}
    [Fact] public async Task AssignmentRestrictsTrainers(){using var db=Db();db.Users.AddRange(new ApplicationUser{Id="trainer1",UserName="t1",FullName="T"},new ApplicationUser{Id="client1",UserName="c1",FullName="C"});db.TrainerAssignments.Add(new TrainerAssignment{TrainerId="trainer1",ClientId="client1"});await db.SaveChangesAsync();var s=new HealthService(db);Assert.True(await s.IsAssigned("trainer1","client1"));Assert.False(await s.IsAssigned("trainer2","client1"));}
    [Fact] public async Task ConfirmedAppointmentConflictsButPendingDoesNot(){using var db=Db();var time=DateTime.UtcNow.AddDays(1);db.Appointments.AddRange(new Appointment{TrainerId="t",ClientId="c",StartsUtc=time,DurationMinutes=60,Status=AppointmentStatus.Approved,Purpose="A"},new Appointment{TrainerId="t",ClientId="c2",StartsUtc=time.AddHours(3),DurationMinutes=60,Status=AppointmentStatus.Pending,Purpose="B"});await db.SaveChangesAsync();var s=new HealthService(db);Assert.True(await s.HasConflict("t",time.AddMinutes(30),60));Assert.False(await s.HasConflict("t",time.AddHours(3),60));Assert.True(await s.HasClientConflict("c",time.AddMinutes(30),60));Assert.False(await s.HasClientConflict("c2",time.AddHours(3),60));}
    [Fact] public async Task ClientCannotLogAnotherClientsWorkout(){using var db=Db();db.WorkoutPlans.Add(new WorkoutPlan{Id=1,TrainerId="t",ClientId="other",Title="Plan"});await db.SaveChangesAsync();var c=new ClientController(db,Users(),new HealthService(db)){ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext{User=Principal("me","Client")}}};var result=await c.LogWorkout(new WorkoutLogForm{WorkoutPlanId=1,CompletedExercise="Run",DurationMinutes=20,CaloriesBurned=100});Assert.IsType<ForbidResult>(result);Assert.Empty(db.WorkoutLogs);}
    [Fact] public async Task TrainerCannotViewUnassignedClient(){using var db=Db();var c=new TrainerController(db,Users(),new HealthService(db)){ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext{User=Principal("trainer","Trainer")}}};Assert.IsType<ForbidResult>(await c.Progress("client"));}
    [Fact] public async Task TrainerLosesPlanAccessAfterUnassignment(){using var db=Db();db.WorkoutPlans.Add(new WorkoutPlan{Id=7,TrainerId="trainer",ClientId="client",Title="Plan"});await db.SaveChangesAsync();var c=new PlansController(db,Users(),new HealthService(db)){ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext{User=Principal("trainer","Trainer")}}};Assert.IsType<ForbidResult>(await c.Workout(7));var list=Assert.IsType<ViewResult>(await c.Workouts());Assert.Empty(Assert.IsType<List<WorkoutPlan>>(list.Model));}
    [Fact] public async Task TrainerLosesAppointmentAccessAfterUnassignment(){using var db=Db();db.Appointments.Add(new Appointment{TrainerId="trainer",ClientId="client",StartsUtc=DateTime.UtcNow.AddDays(1),Purpose="Review"});await db.SaveChangesAsync();var c=new AppointmentsController(db,Users(),new HealthService(db)){ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext{User=Principal("trainer","Trainer")}}};var list=Assert.IsType<ViewResult>(await c.Index());Assert.Empty(Assert.IsType<List<Appointment>>(list.Model));}
    [Fact] public void PlanValidationRejectsBadTargets(){var f=new PlanForm{Title="Plan",ClientId="c",DailyCalorieTarget=50,Sets=0,Repetitions=0};var errors=new List<ValidationResult>();Assert.False(Validator.TryValidateObject(f,new ValidationContext(f),errors,true));Assert.Contains(errors,e=>e.MemberNames.Contains(nameof(PlanForm.DailyCalorieTarget)));Assert.Contains(errors,e=>e.MemberNames.Contains(nameof(PlanForm.Sets)));}
    [Fact] public void RolePoliciesAreDeclared(){Assert.NotNull(typeof(ClientController).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute),true).SingleOrDefault());Assert.NotNull(typeof(TrainerController).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute),true).SingleOrDefault());}
}
