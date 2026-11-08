using HealthTrack.Data;
using HealthTrack.Models;
using HealthTrack.Services;
using HealthTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HealthTrack.Controllers;
[Authorize(Policy="Trainer")]
public class PlansController(AppDbContext db,UserManager<ApplicationUser> users,HealthService service):Controller {
    string Id=>users.GetUserId(User)!;
    async Task LoadClients()=>ViewBag.Clients=await db.TrainerAssignments.Where(x=>x.TrainerId==Id&&x.Client.IsActive).Include(x=>x.Client).Select(x=>x.Client).ToListAsync();
    public async Task<IActionResult> Workouts() => View(await db.WorkoutPlans.Where(x=>x.TrainerId==Id&&db.TrainerAssignments.Any(a=>a.TrainerId==Id&&a.ClientId==x.ClientId&&a.Client.IsActive&&a.Trainer.IsActive)).Include(x=>x.Client).Include(x=>x.Exercises).ToListAsync());
    public async Task<IActionResult> Diets() => View(await db.DietPlans.Where(x=>x.TrainerId==Id&&db.TrainerAssignments.Any(a=>a.TrainerId==Id&&a.ClientId==x.ClientId&&a.Client.IsActive&&a.Trainer.IsActive)).Include(x=>x.Client).Include(x=>x.Meals).ToListAsync());
    public async Task<IActionResult> Workout(int? id) {
        await LoadClients(); if(id==null) return View(new PlanForm());
        var p=await db.WorkoutPlans.Include(x=>x.Exercises).FirstOrDefaultAsync(x=>x.Id==id&&x.TrainerId==Id); if(p==null) return NotFound(); if(!await service.IsAssigned(Id,p.ClientId)) return Forbid();
        ViewBag.Exercises=p.Exercises; return View(new PlanForm{Id=p.Id,ClientId=p.ClientId,Title=p.Title,Description=p.Description});
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Workout(PlanForm form) {
        if(!await service.IsAssigned(Id,form.ClientId)) return Forbid();
        if(form.Id==0 && string.IsNullOrWhiteSpace(form.ItemName)) ModelState.AddModelError(nameof(form.ItemName),"Enter the first exercise.");
        if(!ModelState.IsValid) { await LoadClients(); if(form.Id>0) ViewBag.Exercises=await db.WorkoutExercises.Where(x=>x.WorkoutPlanId==form.Id).ToListAsync(); return View(form); }
        WorkoutPlan p;
        if(form.Id==0) { p=new WorkoutPlan{TrainerId=Id,ClientId=form.ClientId}; db.WorkoutPlans.Add(p); p.Exercises.Add(new WorkoutExercise{Name=form.ItemName,Sets=form.Sets,Repetitions=form.Repetitions,DurationMinutes=form.DurationMinutes}); }
        else { var existing=await db.WorkoutPlans.FirstOrDefaultAsync(x=>x.Id==form.Id&&x.TrainerId==Id); if(existing==null) return NotFound(); p=existing; if(!await service.IsAssigned(Id,p.ClientId)) return Forbid(); p.ClientId=form.ClientId; }
        p.Title=form.Title; p.Description=form.Description; await db.SaveChangesAsync(); await service.Notify(form.ClientId,"A workout plan was assigned or updated: "+p.Title); await service.Activity(Id,"Workout plan saved"); return RedirectToAction(nameof(Workout),new{id=p.Id});
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> AddExercise(int planId,string name,int sets,int repetitions,int durationMinutes) {
        var p=await db.WorkoutPlans.FirstOrDefaultAsync(x=>x.Id==planId&&x.TrainerId==Id); if(p==null) return NotFound(); if(!await service.IsAssigned(Id,p.ClientId)) return Forbid();
        if(string.IsNullOrWhiteSpace(name)||name.Length>120||sets is <1 or >30||repetitions is <1 or >200||durationMinutes is <0 or >240) return BadRequest();
        db.WorkoutExercises.Add(new WorkoutExercise{WorkoutPlanId=planId,Name=name,Sets=sets,Repetitions=repetitions,DurationMinutes=durationMinutes}); await db.SaveChangesAsync(); return RedirectToAction(nameof(Workout),new{id=planId});
    }
    public async Task<IActionResult> EditExercise(int id) { var e=await db.WorkoutExercises.Include(x=>x.WorkoutPlan).FirstOrDefaultAsync(x=>x.Id==id&&x.WorkoutPlan.TrainerId==Id); if(e==null) return NotFound(); return await service.IsAssigned(Id,e.WorkoutPlan.ClientId)?View(e):Forbid(); }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> EditExercise(int id,string name,int sets,int repetitions,int durationMinutes) {
        var e=await db.WorkoutExercises.Include(x=>x.WorkoutPlan).FirstOrDefaultAsync(x=>x.Id==id&&x.WorkoutPlan.TrainerId==Id); if(e==null) return NotFound(); if(!await service.IsAssigned(Id,e.WorkoutPlan.ClientId)) return Forbid();
        if(string.IsNullOrWhiteSpace(name)||name.Length>120||sets is <1 or >30||repetitions is <1 or >200||durationMinutes is <0 or >240) return BadRequest();
        e.Name=name; e.Sets=sets; e.Repetitions=repetitions; e.DurationMinutes=durationMinutes; await db.SaveChangesAsync(); return RedirectToAction(nameof(Workout),new{id=e.WorkoutPlanId});
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> DeleteExercise(int id) { var e=await db.WorkoutExercises.Include(x=>x.WorkoutPlan).FirstOrDefaultAsync(x=>x.Id==id&&x.WorkoutPlan.TrainerId==Id); if(e==null) return NotFound(); if(!await service.IsAssigned(Id,e.WorkoutPlan.ClientId)) return Forbid(); db.WorkoutExercises.Remove(e); await db.SaveChangesAsync(); return RedirectToAction(nameof(Workout),new{id=e.WorkoutPlanId}); }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> DeleteWorkout(int id) { var p=await db.WorkoutPlans.FirstOrDefaultAsync(x=>x.Id==id&&x.TrainerId==Id); if(p==null) return NotFound(); if(!await service.IsAssigned(Id,p.ClientId)) return Forbid(); if(await db.WorkoutLogs.AnyAsync(x=>x.WorkoutPlanId==id)) { TempData["Error"]="This plan has workout history and cannot be deleted."; return RedirectToAction(nameof(Workouts)); } db.WorkoutPlans.Remove(p); await db.SaveChangesAsync(); return RedirectToAction(nameof(Workouts)); }
    public async Task<IActionResult> Diet(int? id) {
        await LoadClients(); if(id==null) return View(new PlanForm());
        var p=await db.DietPlans.Include(x=>x.Meals).FirstOrDefaultAsync(x=>x.Id==id&&x.TrainerId==Id); if(p==null) return NotFound(); if(!await service.IsAssigned(Id,p.ClientId)) return Forbid(); ViewBag.Meals=p.Meals;
        return View(new PlanForm{Id=p.Id,ClientId=p.ClientId,Title=p.Title,DailyCalorieTarget=p.DailyCalorieTarget});
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Diet(PlanForm form) {
        if(!await service.IsAssigned(Id,form.ClientId)) return Forbid();
        if(form.Id==0 && (string.IsNullOrWhiteSpace(form.ItemName)||string.IsNullOrWhiteSpace(form.FoodDescription))) ModelState.AddModelError("","Enter the first meal and food description.");
        if(!ModelState.IsValid) { await LoadClients(); if(form.Id>0) ViewBag.Meals=await db.DietPlanMeals.Where(x=>x.DietPlanId==form.Id).ToListAsync(); return View(form); }
        DietPlan p;
        if(form.Id==0) { p=new DietPlan{TrainerId=Id,ClientId=form.ClientId}; db.DietPlans.Add(p); p.Meals.Add(new DietPlanMeal{Name=form.ItemName,FoodDescription=form.FoodDescription,CalorieTarget=form.MealCalorieTarget,ProteinGrams=form.ProteinGrams,ScheduledTime=form.ScheduledTime}); }
        else { var existing=await db.DietPlans.FirstOrDefaultAsync(x=>x.Id==form.Id&&x.TrainerId==Id); if(existing==null) return NotFound(); p=existing; if(!await service.IsAssigned(Id,p.ClientId)) return Forbid(); p.ClientId=form.ClientId; }
        p.Title=form.Title; p.DailyCalorieTarget=form.DailyCalorieTarget; await db.SaveChangesAsync(); await service.Notify(form.ClientId,"A diet plan was assigned or updated: "+p.Title); await service.Activity(Id,"Diet plan saved"); return RedirectToAction(nameof(Diet),new{id=p.Id});
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> AddMeal(int planId,string name,string foodDescription,int calorieTarget,decimal proteinGrams,TimeOnly scheduledTime) {
        var p=await db.DietPlans.FirstOrDefaultAsync(x=>x.Id==planId&&x.TrainerId==Id); if(p==null) return NotFound(); if(!await service.IsAssigned(Id,p.ClientId)) return Forbid();
        if(string.IsNullOrWhiteSpace(name)||name.Length>100||string.IsNullOrWhiteSpace(foodDescription)||foodDescription.Length>500||calorieTarget is <1 or >3000||proteinGrams is <0 or >200) return BadRequest();
        db.DietPlanMeals.Add(new DietPlanMeal{DietPlanId=planId,Name=name,FoodDescription=foodDescription,CalorieTarget=calorieTarget,ProteinGrams=proteinGrams,ScheduledTime=scheduledTime}); await db.SaveChangesAsync(); return RedirectToAction(nameof(Diet),new{id=planId});
    }
    public async Task<IActionResult> EditMeal(int id) { var m=await db.DietPlanMeals.Include(x=>x.DietPlan).FirstOrDefaultAsync(x=>x.Id==id&&x.DietPlan.TrainerId==Id); if(m==null) return NotFound(); return await service.IsAssigned(Id,m.DietPlan.ClientId)?View(m):Forbid(); }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> EditMeal(int id,string name,string foodDescription,int calorieTarget,decimal proteinGrams,TimeOnly scheduledTime) {
        var m=await db.DietPlanMeals.Include(x=>x.DietPlan).FirstOrDefaultAsync(x=>x.Id==id&&x.DietPlan.TrainerId==Id); if(m==null) return NotFound(); if(!await service.IsAssigned(Id,m.DietPlan.ClientId)) return Forbid();
        if(string.IsNullOrWhiteSpace(name)||name.Length>100||string.IsNullOrWhiteSpace(foodDescription)||foodDescription.Length>500||calorieTarget is <1 or >3000||proteinGrams is <0 or >200) return BadRequest();
        m.Name=name; m.FoodDescription=foodDescription; m.CalorieTarget=calorieTarget; m.ProteinGrams=proteinGrams; m.ScheduledTime=scheduledTime; await db.SaveChangesAsync(); return RedirectToAction(nameof(Diet),new{id=m.DietPlanId});
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> DeleteMeal(int id) { var m=await db.DietPlanMeals.Include(x=>x.DietPlan).FirstOrDefaultAsync(x=>x.Id==id&&x.DietPlan.TrainerId==Id); if(m==null) return NotFound(); if(!await service.IsAssigned(Id,m.DietPlan.ClientId)) return Forbid(); db.DietPlanMeals.Remove(m); await db.SaveChangesAsync(); return RedirectToAction(nameof(Diet),new{id=m.DietPlanId}); }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> DeleteDiet(int id) { var p=await db.DietPlans.FirstOrDefaultAsync(x=>x.Id==id&&x.TrainerId==Id); if(p==null) return NotFound(); if(!await service.IsAssigned(Id,p.ClientId)) return Forbid(); db.DietPlans.Remove(p); await db.SaveChangesAsync(); return RedirectToAction(nameof(Diets)); }
}
