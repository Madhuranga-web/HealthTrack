using HealthTrack.Data;
using HealthTrack.Models;
using HealthTrack.ViewModels;
using HealthTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace HealthTrack.Controllers;
public class AccountController(UserManager<ApplicationUser> users,SignInManager<ApplicationUser> signIn,RoleManager<IdentityRole> roles,AppDbContext db,HealthService service) : Controller {
    [AllowAnonymous] public IActionResult Register()=>View(new RegisterForm());
    [HttpPost,AllowAnonymous,ValidateAntiForgeryToken] public async Task<IActionResult> Register(RegisterForm form) {
        if(!ModelState.IsValid) return View(form);
        var user=new ApplicationUser{UserName=form.Email,Email=form.Email,FullName=form.FullName};
        var result=await users.CreateAsync(user,form.Password);
        if(!result.Succeeded) { foreach(var e in result.Errors) ModelState.AddModelError("",e.Description); return View(form); }
        if(!await roles.RoleExistsAsync("Client")) await roles.CreateAsync(new IdentityRole("Client"));
        await users.AddToRoleAsync(user,"Client"); db.ClientProfiles.Add(new ClientProfile{UserId=user.Id}); await db.SaveChangesAsync();
        await signIn.SignInAsync(user,false); await service.Activity(user.Id,"Client registered"); return RedirectToAction("Index","Client");
    }
    [AllowAnonymous] public IActionResult Login(string? returnUrl=null) { ViewBag.ReturnUrl=returnUrl; return View(new LoginForm()); }
    [HttpPost,AllowAnonymous,ValidateAntiForgeryToken] public async Task<IActionResult> Login(LoginForm form,string? returnUrl=null) {
        if(!ModelState.IsValid) return View(form);
        var user=await users.FindByEmailAsync(form.Email);
        if(user==null||!user.IsActive) { ModelState.AddModelError("","Invalid credentials or inactive account."); return View(form); }
        var result=await signIn.PasswordSignInAsync(user,form.Password,form.RememberMe,lockoutOnFailure:true);
        if(!result.Succeeded) { ModelState.AddModelError("","Invalid credentials or locked account."); return View(form); }
        await service.Activity(user.Id,"Signed in"); return LocalRedirect(Url.IsLocalUrl(returnUrl)?returnUrl!:"/");
    }
    [HttpPost,Authorize,ValidateAntiForgeryToken] public async Task<IActionResult> Logout() { await signIn.SignOutAsync(); return RedirectToAction("Index","Home"); }
    [Authorize] public IActionResult ChangePassword()=>View();
    [HttpPost,Authorize,ValidateAntiForgeryToken] public async Task<IActionResult> ChangePassword(string currentPassword,string newPassword,string confirmPassword) {
        if(string.IsNullOrWhiteSpace(newPassword)||newPassword!=confirmPassword) ModelState.AddModelError("","New passwords must match.");
        var user=await users.GetUserAsync(User); if(user==null) return Challenge();
        if(!ModelState.IsValid) return View();
        var result=await users.ChangePasswordAsync(user,currentPassword,newPassword);
        if(!result.Succeeded) { foreach(var error in result.Errors) ModelState.AddModelError("",error.Description); return View(); }
        await signIn.RefreshSignInAsync(user); TempData["Success"]="Password changed."; return RedirectToAction(nameof(Profile));
    }
    [Authorize] public async Task<IActionResult> Profile() => View(await users.GetUserAsync(User));
    [HttpPost,Authorize,ValidateAntiForgeryToken] public async Task<IActionResult> Profile(string fullName) {
        if(string.IsNullOrWhiteSpace(fullName)||fullName.Length>100) ModelState.AddModelError("","Enter a name up to 100 characters.");
        var user=await users.GetUserAsync(User); if(user==null) return Challenge();
        if(!ModelState.IsValid) return View(user);
        user.FullName=fullName.Trim(); await users.UpdateAsync(user); TempData["Success"]="Profile saved."; return RedirectToAction(nameof(Profile));
    }
}
