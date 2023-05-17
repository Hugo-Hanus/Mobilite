using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly InstallationDbContext _context;

    private AccountController(ILogger<AccountController> logger,InstallationDbContext context)
    {
        _logger = logger;
        _context = context;
    }


   
    
    [HttpPost]
    public async Task<IActionResult> InscriptionClientPost(Client client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index","Home");
    }
    [HttpPost]
    public async Task<IActionResult> ModificationClientPost(Client client)
    {
        using (_context)
        {
            var userInfo = _context.Clients.Single(client => client.ID == 1);
            var camion = _context.Camions
                .Where(b => b.ID == 1)
                .Select(b => new { b.ID, b.Marque, b.Modèle }).FirstOrDefault();
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> InscriptionMembreChauffeur(Chauffeur chauffeur)
    {
        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    public async Task<IActionResult> InscriptionMembreDispatcher(Dispatcher dispatcher)
    {
        return RedirectToAction("Index", "Home");
    }
    
    
    [HttpPost]
    public async Task<IActionResult> ModificationMembreChauffeur(Chauffeur chauffeur)
    {
        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    public async Task<IActionResult> ModificationMembreDispatcher(Dispatcher dispatcher)
    {
        
        return RedirectToAction("Index", "Home");
    }
        
    
    
    
}