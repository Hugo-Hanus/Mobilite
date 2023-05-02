using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace WebApplication1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly InstallationDbContext _context;

    public HomeController(ILogger<HomeController> logger, InstallationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return PartialView();
    }

    public IActionResult Privacy()
    {
        return PartialView();
    }
    public IActionResult InscriptionClient()
    {
        return PartialView();
    }
    public IActionResult InscriptionMembre()
    {
        return PartialView();
    }
    public IActionResult Connexion()
    {
        return PartialView();
    }
    public IActionResult Livraison()
    {
        return PartialView();
    }
    public IActionResult Dispatch()
    {
        return PartialView();
    }public IActionResult GererLivraison()
    {
        return PartialView();
    }
    
    public IActionResult CreerLivraison()
    {
        return PartialView();
    }
    public IActionResult LivraisonDispatch()
    {
        return PartialView();
    }
    public IActionResult ValiderLivraison()
    {
        return PartialView();
    }
    public IActionResult RaterLivraison()
    {
        return PartialView();
    }
    public IActionResult GestionEffectif()
    {
        return PartialView();
    }
    public IActionResult AjouterCamion()
    {
        return PartialView();
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

        return RedirectToAction("Index"); 
        /**
        if (ModelState.IsValid)
        {
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

            return RedirectToAction("Index"); 
        }
        **/

        return PartialView(camion);
    }
    public IActionResult Clientliste()
    {
        return PartialView();
    }
    public IActionResult Statistique()
    {
        return PartialView();
    }
    


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}