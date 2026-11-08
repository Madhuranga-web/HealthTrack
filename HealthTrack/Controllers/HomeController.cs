using Microsoft.AspNetCore.Mvc;
namespace HealthTrack.Controllers;
public class HomeController:Controller {
    public IActionResult Index() { if(User.IsInRole("Admin")) return RedirectToAction("Index","Admin"); if(User.IsInRole("Trainer")) return RedirectToAction("Index","Trainer"); if(User.IsInRole("Client")) return RedirectToAction("Index","Client"); return View(); }
    public IActionResult Denied()=>View(); public IActionResult Error()=>View();
}
