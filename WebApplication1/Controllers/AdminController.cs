using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.ViewModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using HostingEnvironmentExtensions = Microsoft.AspNetCore.Hosting.HostingEnvironmentExtensions;
namespace WebApplication1.Controllers;

public class AdminController:Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly InstallationDbContext _context;

    public AdminController(ILogger<AccountController> logger,InstallationDbContext context)
    {
        _logger = logger;
        _context = context;
    }


    [HttpPost][ValidateAntiForgeryToken][Authorize(Roles = "Admin")]
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
            
        Regex regex = new Regex(@"^\d-[A-Za-z]{3}-\d{3}$");
        if (regex.IsMatch(camion.Immatriculation))
        {
            _context.Camions.Add(camion);
            await _context.SaveChangesAsync();
        }
        else
        {
            return RedirectToAction("AjouterCamion","Home",camion);
        }
            
            
        return RedirectToAction("GestionEffectif","Home");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ModificationCamion(Camion camion, IFormFile Img)
    {
        var existingCamion = await _context.Camions.FindAsync(camion.ID);
        if (Img != null && Img.Length > 0 )
        {
            var oldImgPath = existingCamion.Img;
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var imgFileName = $"{Guid.NewGuid()}_{Img.FileName}";
            var imgFilePath = Path.Combine(uploadsPath, imgFileName);
            
            if (!string.Equals(oldImgPath, $"~/img/{imgFileName}", StringComparison.OrdinalIgnoreCase))
            {
                var imgFullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImgPath.TrimStart('~', '/'));

                if (System.IO.File.Exists(imgFullPath))
                {
                    System.IO.File.Delete(imgFullPath);
                }
                
                using (var fileStream = new FileStream(imgFilePath, FileMode.Create))
                {
                    await Img.CopyToAsync(fileStream);
                }

                existingCamion.Img = $"~/img/{imgFileName}";
            }
        }

        Regex regex = new Regex(@"^\d-[A-Za-z]{3}-\d{3}$");
        if (regex.IsMatch(camion.Immatriculation))
        {
            existingCamion.Immatriculation = camion.Immatriculation;
        }
        else
        {
            return RedirectToAction("ModificationCamion","Home",camion);
        }
        existingCamion.Marque = camion.Marque;
        existingCamion.Modele = camion.Modele;
        existingCamion.Tonnage = camion.Tonnage;
        existingCamion.Type = camion.Type;
        _context.Update(existingCamion);
        await _context.SaveChangesAsync();

        return RedirectToAction("GestionEffectif","Home");
    }
    
    [Produces("application/json")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SearchList(string searchList)
    {
        var listChauffeur = await _context.Livraison.Include(l=>l.ChauffeurLivraison).Where(liv =>
            liv.ChauffeurLivraison.Prenom.Contains(searchList)|liv.ChauffeurLivraison.Nom.Contains(searchList)).Where(liv=>liv.StatutLivraison==Models.Livraison.Statut.Valide|liv.StatutLivraison==Models.Livraison.Statut.Rate).Select(liv => liv.ChauffeurLivraison).Distinct().ToListAsync();
        var listClient = await _context.Livraison.Include(l=>l.ClientLivraison).Where(liv =>
            liv.ClientLivraison.UserName.Contains(searchList) & (liv.StatutLivraison==Models.Livraison.Statut.Valide|liv.StatutLivraison==Models.Livraison.Statut.Rate)).Select(liv => liv.ClientLivraison).Distinct().ToListAsync();
        var listString = new List<string>();
        listChauffeur.ForEach(e =>
        {
            listString.Add(e.Prenom);
            listString.Add(e.Nom);
        });
        listClient.ForEach(e=>listString.Add(e.NomEntreprise));
        return Ok(listString);
    }
    
    [HttpPost][Authorize(Roles = "Admin")]

    public async Task<IActionResult> UpdateClientStatus(string id, bool isMauvaisPayeur)
    {
        
        var client = await _context.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == id);
        if (client == null)
        {
            return NotFound();
        }

        client.isMauvaisPayeur = isMauvaisPayeur;
        await _context.SaveChangesAsync();

        return RedirectToAction("Clientliste","Home");

    }
    
    [HttpPost][Authorize(Roles = "Admin")]
    public async Task<IActionResult> SupprimerCamion([FromForm] int id)
    {
        var camion = await _context.Camions.FindAsync(id);

        if (camion == null)
        {
            return RedirectToAction("GestionEffectif","Home");
        }
        
        var imgFullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", camion.Img.TrimStart('~', '/'));

        if (System.IO.File.Exists(imgFullPath))
        {
            System.IO.File.Delete(imgFullPath);
        }
        
        var livraisons = await _context.Livraison.Where(l => l.CamionLivraison.ID == camion.ID && l.StatutLivraison ==Models.Livraison.Statut.EnCours).ToListAsync();
        
        foreach(var livraison in livraisons)
        {
            livraison.ChauffeurLivraison = null;
            livraison.CamionLivraison = null;
            livraison.StatutLivraison = Models.Livraison.Statut.Attente;
            _context.Update(livraison);        
            await _context.SaveChangesAsync();


        }
        var livraisons2 = await _context.Livraison.Where(l => l.CamionLivraison.ID == camion.ID && l.StatutLivraison ==Models.Livraison.Statut.Valide || l.StatutLivraison==Models.Livraison.Statut.Rate).ToListAsync();
        foreach(var livraison in livraisons2)
        {
            livraison.CamionLivraison = null;
            _context.Update(livraison);        
            await _context.SaveChangesAsync();
        }
        _context.Camions.Remove(camion);
        await _context.SaveChangesAsync();

        return RedirectToAction("GestionEffectif","Home");
    }
    
    
    [HttpPost][Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateChauffeurPermis([FromServices] UserManager<IdentityUser> userManager,[FromForm] IFormCollection form)
    {
        var id = form["ChauffeurId"].ToString();
        var chauffeur = await _context.Users.OfType<Chauffeur>().FirstOrDefaultAsync(c => c.Id == id);
        if (chauffeur == null)
        {
            return NotFound();
        }

        var permisB = form["PermisB"].ToString();
        if (permisB.IsNullOrEmpty())
        {
            chauffeur.PermisB = false;
        }
        else
        {
            chauffeur.PermisB = true;
        }
        var permisC = form["PermisC"].ToString();
        if (permisC.IsNullOrEmpty())
        {
            chauffeur.PermisC = false;
        }
        else
        {
            chauffeur.PermisC = true;
        }
        var permisCE = form["PermisCE"].ToString();
        if (permisCE.IsNullOrEmpty())
        {
            chauffeur.PermisCE = false;
        }
        else
        {
            chauffeur.PermisCE = true;
        }

        if (!chauffeur.PermisB && !chauffeur.PermisC && !chauffeur.PermisCE)
        {
            var livraisons = await _context.Livraison.Where(l => l.ChauffeurLivraison.Id == chauffeur.Id && l.StatutLivraison == Models.Livraison.Statut.EnCours).ToListAsync();
            foreach(var livraison in livraisons)
            {
                livraison.ChauffeurLivraison = null;
                livraison.CamionLivraison = null;
                livraison.StatutLivraison = Models.Livraison.Statut.Attente;
            }

        }
        await _context.SaveChangesAsync();

        return RedirectToAction("GestionEffectif","Home");
    }
    
    
}