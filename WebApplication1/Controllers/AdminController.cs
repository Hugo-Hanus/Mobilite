using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class AdminController:Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly InstallationDbContext _context;

    private AdminController(ILogger<AccountController> logger,InstallationDbContext context)
    {
        _logger = logger;
        _context = context;
    }




    
    
    [HttpPost]
    public async Task<IActionResult> AjouterCamion(Camion camion, IFormFile Img)
    {
        Camion t = camion;
        if (Img != null && Img.Length > 0)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var imgFileName = $"{Guid.NewGuid()}_{Img.FileName}";
            var imgFilePath = Path.Combine(uploadsPath, imgFileName);

            using (var fileStream = new FileStream(imgFilePath, FileMode.Create))
            {
                await Img.CopyToAsync(fileStream);
            }

            camion.Img = $"~/img/{imgFileName}";
        }

        _context.Camions.Add(camion);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index","Home");
    }
    [HttpPost]
    public async Task<IActionResult> ModifierPermisChauffeur(Chauffeur chauffeur)
    {
        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    public async Task<IActionResult> ModifierCamion(Camion camion)
    {
        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    public async Task<IActionResult> SupprimerCamion(Camion camion)
    {
        return RedirectToAction("Index", "Home");
    }
    
}