using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class LivraisonController:Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly InstallationDbContext _context;

    public LivraisonController(ILogger<HomeController> logger, InstallationDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    
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
    
    public IActionResult GererLivraison()
    {
        /*Get all Chauffeur dispo pour la date*/
        Livraison livraisonList = new Livraison();


        var query = from chauffeur in _context.Users.OfType<Chauffeur>()
            join livraison in _context.Livraison
                on chauffeur equals livraison.ChauffeurLivraison
            select new { Chauffeur = chauffeur, Livraison = livraison };

        var result = query.ToList();

        return null;
    }
}