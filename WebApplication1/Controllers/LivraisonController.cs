using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

public class LivraisonController:Controller
{
    [HttpPost]
    public async Task<IActionResult> ValiderLivraisonPost(string livraison)
    {
        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    public async Task<IActionResult> RaterLivraisonPost(string livraison)
    {
        return RedirectToAction("Index", "Home");
    }
}