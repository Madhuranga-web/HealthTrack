using HealthTrack.Data;
using HealthTrack.Models;
using HealthTrack.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables(prefix: "HEALTHTRACK_");
builder.Services.AddControllersWithViews(o => o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o => {
    o.Password.RequiredLength = 10; o.Password.RequireDigit = true; o.Password.RequireUppercase = true;
    o.User.RequireUniqueEmail = true; o.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o => { o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Home/Denied"; o.Cookie.HttpOnly = true; o.Cookie.SecurePolicy = CookieSecurePolicy.Always; o.Events.OnValidatePrincipal = async context => { var manager=context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>(); var user=await manager.GetUserAsync(context.Principal!); if(user==null || !user.IsActive) { context.RejectPrincipal(); await context.HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>().SignOutAsync(); } }; });
builder.Services.AddAuthorization(o => {
    o.AddPolicy("Admin", p => p.RequireRole("Admin"));
    o.AddPolicy("Trainer", p => p.RequireRole("Trainer"));
    o.AddPolicy("Client", p => p.RequireRole("Client"));
});
builder.Services.AddScoped<HealthService>();
builder.Services.AddScoped<DemoSeeder>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
else { app.UseDeveloperExceptionPage(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
app.MapControllerRoute(name:"default", pattern:"{controller=Home}/{action=Index}/{id?}");
if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("SeedDemoData")) {
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<DemoSeeder>().RunAsync();
}
app.Run();
public partial class Program { }
