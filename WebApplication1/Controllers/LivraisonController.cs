using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.ViewModel;

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
    
    /*------------------------------------------------------------------------------------
    *         DISPATCHER
    * ---------------------------------------------------------------------------------
    */
    [Authorize(Roles = "Dispatcher")]  
    public async Task<IActionResult> refreshWindow()
    {
        return RedirectToAction("Dispatch","Home");
    }
    
    [Produces("application/json")]
    [ValidateAntiForgeryToken][HttpGet]
    public async Task<IActionResult> GetCamionDispo(string selectedChauffeurId,int livraisonId)
    {
        var chauffeur = _context.Users.OfType<Chauffeur>().SingleOrDefault(chauf => chauf.Id == selectedChauffeurId);
        var livraisonChoisie = _context.Livraison.SingleOrDefault(liv => liv.ID == livraisonId);
        var heureChargement = DateTime.Parse(livraisonChoisie.HeureChargement);
        var heureDechargementPrevu = DateTime.Parse(livraisonChoisie.HeureDechargementPrevu);
        
     
        var camions = _context.Camions
            .Where(camion => !_context.Livraison.Any(l => l.CamionLivraison.ID == camion.ID && l.DateChargement == livraisonChoisie.DateChargement && l.DateDechargement == livraisonChoisie.DateDechargement))
            .Where(camion =>
                (chauffeur.PermisB && camion.Type == "B") ||
                (chauffeur.PermisC && camion.Type == "C") ||
                (chauffeur.PermisCE && camion.Type == "CE"))
            .ToList();
        var camionsDisponiblesAuHoraire = camions.Where(cam => !_context.Livraison
                .Where(l => l.ChauffeurLivraison != null && l.CamionLivraison.ID == cam.ID && l.DateChargement == livraisonChoisie.DateChargement && l.DateDechargement == livraisonChoisie.DateDechargement).AsEnumerable()
                .Any(l => DateTime.Parse(l.HeureChargement) >= heureChargement &&
                          DateTime.Parse(l.HeureChargement) <= heureDechargementPrevu &&
                          DateTime.Parse(l.HeureChargement) >= heureDechargementPrevu &&
                          DateTime.Parse(l.HeureDechargementPrevu) <= heureDechargementPrevu))
            .ToList();


        return Ok(camionsDisponiblesAuHoraire.ToList());
    
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Dispatcher")]
    public IActionResult AssignerChauffeurCamion( [FromForm] IFormCollection form)
    {
        var idChauffeur = form["chauffeurSelect"].ToString();
        var plaqueCamion=form["camionSelect"].ToString();
        var idLivraison=-1; 
        try
        {
            idLivraison = int.Parse(form["livraisonId"].ToString());
        }
        catch (FormatException ex)
        {
            return RedirectToAction("Index","Home");
        }

        var camionToAssigne = _context.Camions.SingleOrDefault(cam => cam.Immatriculation == plaqueCamion);
        var chauffeurToAssigne = _context.Users.OfType<Chauffeur>().SingleOrDefault(cha=> cha.Id == idChauffeur);

        var livraison = _context.Livraison.SingleOrDefault(liv => liv.ID == idLivraison);
        if (livraison == null)
        {
            return RedirectToAction("Index","Home");
        }

        livraison.CamionLivraison = camionToAssigne;
        livraison.ChauffeurLivraison = chauffeurToAssigne;
        livraison.StatutLivraison = Models.Livraison.Statut.EnCours;
        _context.Update(livraison);
        _context.SaveChanges();
        return RedirectToAction("Dispatch","Home");

    }
    /*------------------------------------------------------------------------------------
     *         CLIENT
     * ---------------------------------------------------------------------------------
     */
    
    [Authorize(Roles = "Client")][HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> CreerLivraison([FromServices]UserManager<IdentityUser>userManager,Livraison livraison)
    {
        livraison.StatutLivraison = Models.Livraison.Statut.Attente;
        livraison.Commentaire = ".";
        livraison.MotifLivraison = Models.Livraison.Motif.Aucun;
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userCasted = await _context.Users.OfType<Client>().SingleOrDefaultAsync(u => u.Id == userId);
        livraison.ClientLivraison = userCasted;
        livraison.CamionLivraison=null;
        livraison.DateDechargementEffective = " ";
        livraison.HeureDechargementEffective = " ";
        DateTime chargement = DateTime.ParseExact(livraison.DateChargement+" "+livraison.HeureChargement,"yyyy-MM-dd HH:mm",System.Globalization.CultureInfo.InvariantCulture);
        DateTime dechargement = DateTime.ParseExact(livraison.DateDechargement+" "+livraison.HeureDechargementPrevu,"yyyy-MM-dd HH:mm",System.Globalization.CultureInfo.InvariantCulture);

        if (DateTime.Compare(chargement, DateTime.Now) >= 0)
        {
            if (DateTime.Compare(chargement, dechargement) < 0)
            {
                _context.Livraison.Add(livraison);
                await _context.SaveChangesAsync();

            }
        }

        return RedirectToAction("Livraison","Home");
    }
    
    [HttpPost][ValidateAntiForgeryToken][Authorize(Roles = "Client")]
    public IActionResult ModifierLivraison(Livraison model)
    {
        
        var livraison = _context.Livraison.FirstOrDefault(l => l.ID == model.ID);
        if (livraison == null)
        {
            return RedirectToAction("Index","Home");
        }

        
        livraison.LieuChargement = model.LieuChargement;
        livraison.DateChargement = model.DateChargement;
        livraison.HeureChargement = model.HeureChargement;
        livraison.HeureDechargementPrevu = model.HeureDechargementPrevu;
        livraison.Contenu = model.Contenu;
        livraison.DateDechargement = model.DateDechargement;
        livraison.LieuDechargement = model.LieuDechargement;
        
        _context.Update(livraison);
        _context.SaveChanges();

        return RedirectToAction("Livraison","Home");
    }
    
    
    /*------------------------------------------------------------------------------------
     *          CHAUFFEUR
     * ---------------------------------------------------------------------------------
     */
    [HttpPost]
    [Authorize(Roles = "Chauffeur")][ValidateAntiForgeryToken]
    public async Task<IActionResult> ValiderLivraison([FromForm] IFormCollection form, int id)
    {
        var livraison = _context.Livraison.Find(id);
        if(livraison == null)
        {
            return NotFound();
        }

        livraison.Commentaire = form["commentaire"].ToString();
        livraison.DateDechargementEffective = form["dateDeChargementEf"].ToString();
        livraison.HeureDechargementEffective = form["heureDeChargementEf"].ToString();
        livraison.StatutLivraison = Models.Livraison.Statut.Valide;
        _context.Update(livraison);
        await _context.SaveChangesAsync();

        return RedirectToAction("LivraisonDispatch","Home");
    }
    [Authorize(Roles = "Chauffeur")][HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> RaterLivraison(int id, [FromForm] IFormCollection form)
    {
        var livraison = _context.Livraison.Find(id);
        if(livraison == null)
        {
            return NotFound();
        }

        livraison.Commentaire = form["commentaire"].ToString();
        livraison.MotifLivraison = Enum.Parse<Livraison.Motif>(form["motif"]);
        livraison.StatutLivraison = Models.Livraison.Statut.Rate;
        _context.Update(livraison);
        await _context.SaveChangesAsync();

        return RedirectToAction("LivraisonDispatch","Home");
    }

}