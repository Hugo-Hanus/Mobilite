using Microsoft.AspNetCore.Identity;
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


   
    
    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> InscriptionClientPost([FromServices]UserManager<IdentityUser> userManager, [FromServices]SignInManager<IdentityUser> signInManager, Client client)
    {
        if (ModelState.IsValid)
        {

            var userIdentity = new IdentityUser(client.NomEntreprise);
            userIdentity.Email = client.Email;
            var result = await userManager.CreateAsync(userIdentity, client.MotDePasse);
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(userIdentity, "Client"); 
                await signInManager.SignInAsync(userIdentity, false);
                
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
                var uploadPath2 = Path.Combine(uploadPath, "logo", client.NomEntreprise);
                Directory.CreateDirectory(uploadPath2);
            }
            
            
        }

        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    public async Task<IActionResult> ModificationClientPost(Client client)
    {
        /**using (_context)
        {
            var userInfo = _context.Clients.Single(client => client.Id == 1);
            var camion = _context.Camions
                .Where(b => b.ID == 1)
                .Select(b => new { b.ID, b.Marque, b.Modele }).FirstOrDefault();
        }
        return RedirectToAction("Index", "Home"); **/
        return null;
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