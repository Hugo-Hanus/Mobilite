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

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly InstallationDbContext _context;
    private readonly IWebHostEnvironment _environment;


    public HomeController(IWebHostEnvironment environment,ILogger<HomeController> logger, InstallationDbContext context)
    {
        _logger = logger;
        _context = context;
        _environment = environment;

    }

    public async Task<IActionResult> Index()
    {
        List<Client> listClients=new List<Client>();
        listClients=  _context.Clients.ToList();
        return  PartialView(listClients);
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

    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Livraison()
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userCasted = await _context.Users.OfType<Client>().SingleOrDefaultAsync(u => u.Id == userId);

        var livraisonsEnAttente = await _context.Livraison.Where(l => l.ClientLivraison == userCasted && l.StatutLivraison==Models.Livraison.Statut.Attente).ToListAsync();
        var livraisonFini = await _context.Livraison.Where(l => l.ClientLivraison == userCasted && (l.StatutLivraison == Models.Livraison.Statut.Valide || l.StatutLivraison == Models.Livraison.Statut.Rate|| l.StatutLivraison == Models.Livraison.Statut.EnCours)).ToListAsync();
        var LivrasionVm = new LivraisonViewModel();
        LivrasionVm.LivraisonsFini = livraisonFini;
        LivrasionVm.LivraisonsEnAttente = livraisonsEnAttente;
        return PartialView(LivrasionVm);
    }
    
    [Authorize(Roles = "Dispatcher")]
    public IActionResult Dispatch()
    {
        var listLivraisonDispo = _context.Livraison.Where(l => l.StatutLivraison == Models.Livraison.Statut.Attente  & l.ClientLivraison.isMauvaisPayeur==false);
        var listLivraisonMauvaisPayeur = _context.Livraison.Where(l => l.StatutLivraison == Models.Livraison.Statut.Attente & l.ClientLivraison.isMauvaisPayeur==true);
        TwoListLivraison liv = new TwoListLivraison();
         liv.ableGererList = listLivraisonDispo.ToList();
        liv.mauvaisPayeurList = listLivraisonMauvaisPayeur.ToList();
        return PartialView("Dispatch",liv);
    }
    
    [Authorize(Roles = "Dispatcher")]
    public IActionResult GererLivraison(int id)
    {
        var livraisonGerer = _context.Livraison.SingleOrDefault(l => l.ID == id);
            var heureChargement = DateTime.Parse(livraisonGerer.HeureChargement);
            var heureDechargementPrevu = DateTime.Parse(livraisonGerer.HeureDechargementPrevu);
            var chauffeurLivraisonId = livraisonGerer?.ChauffeurLivraison?.Id;
            var chauffeurs = _context.Users.OfType<Chauffeur>()
                .Where(anu => anu.Id != chauffeurLivraisonId && !_context.Livraison.Any(l => l.ChauffeurLivraison.Id == anu.Id && l.DateChargement == livraisonGerer.DateChargement && l.DateDechargement == livraisonGerer.DateDechargement)).ToList();
            var chauffeursNonIssusDeLivraison = chauffeurs.Where(anu => !_context.Livraison
                    .Where(l => l.ChauffeurLivraison != null && l.ChauffeurLivraison.Id == anu.Id && l.DateChargement == livraisonGerer.DateChargement && l.DateDechargement == livraisonGerer.DateDechargement).AsEnumerable()
                    .Any(l => DateTime.Parse(l.HeureChargement) >= heureChargement &&
                              DateTime.Parse(l.HeureChargement) <= heureDechargementPrevu &&
                              (DateTime.Parse(l.HeureDechargementPrevu)) >= heureDechargementPrevu.AddHours(1) &&
                              (DateTime.Parse(l.HeureDechargementPrevu)) <= heureDechargementPrevu.AddHours(1)))
                .ToList();
            var  viewModel = new LivraisonNameChauffeurListModel
            {
                Livraison = livraisonGerer.ID,
                ListChauffeur = chauffeursNonIssusDeLivraison.ToList()
            };
            return PartialView("GererLivraison",viewModel);
    }

    [Authorize(Roles = "Client")]
    public IActionResult ModifierLivraison(int id)
    {
        var livraison = _context.Livraison.FirstOrDefault(l => l.ID == id);
        if (livraison == null)
        {
            return RedirectToAction("Index");
        }
        return PartialView(livraison);
    }
    
    [Authorize(Roles = "Client")]
    public IActionResult CreerLivraison()
    {
        return PartialView();
    }

    [Authorize(Roles = "Chauffeur")]
    public async Task<IActionResult> LivraisonDispatch()
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var livraisons = _context.Livraison
            .Where(l => l.ChauffeurLivraison.UserName == User.Identity.Name && l.StatutLivraison == Models.Livraison.Statut.EnCours)
            .AsEnumerable()
            .Select(l => new
            {
                Livraison = l,
                DateChargement = DateTime.ParseExact(l.DateChargement, "yyyy-MM-dd", CultureInfo.InvariantCulture)
            })
            .OrderBy(l => l.DateChargement)
            .Select(l => l.Livraison).ToList();
        
        return PartialView(livraisons);
    }
    
    [Authorize(Roles = "Chauffeur")]
    public IActionResult ValiderLivraison(int id)
    {
        var livraison = _context.Livraison.Find(id);
        if(livraison == null)
        {
            return NotFound();
        }
        return PartialView(livraison);
    }

    [Authorize(Roles = "Chauffeur")]
    public IActionResult RaterLivraison(int id)
    {
        var livraison = _context.Livraison.Find(id);
        if(livraison == null)
        {
            return NotFound();
        }
        return PartialView(livraison);
    }
    
    
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GestionEffectif()
    {
        List<Chauffeur> chauffeurs = await _context.Users.OfType<Chauffeur>().ToListAsync();
        List<Camion> camions = await _context.Camions.ToListAsync();
        var ViewModel = new GestionEffectifViewModel();
        ViewModel.Camions = camions;
        ViewModel.Chauffeurs = chauffeurs;
        return PartialView(ViewModel);
    }
    
    [Authorize(Roles = "Admin")]
    public IActionResult AjouterCamion()
    {
        return PartialView();
    }
    
    
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Clientliste([FromServices]UserManager<IdentityUser>userManager)
    {
        var users = await userManager.GetUsersInRoleAsync("Client");
        var clients = users.OfType<Client>().ToList();
        return PartialView(clients);
    }


    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Statistique(string searchString = "")
    {
        var livraisonParMarque = _context.Livraison.Include(l => l.CamionLivraison)
            .Where(liv => liv.StatutLivraison == Models.Livraison.Statut.Valide |
                          liv.StatutLivraison == Models.Livraison.Statut.Rate).GroupBy(l => l.CamionLivraison.Marque)
            .Select(group => new { Marque = group.Key.ToString(), Number = group.Count() }).ToDictionary(w=>w.Marque,z=>z.Number);
        var livraisonRateParChauffeur = _context.Livraison.Include(l => l.ChauffeurLivraison)
            .Where(liv => liv.StatutLivraison == Models.Livraison.Statut.Rate).GroupBy(l => l.ChauffeurLivraison)
            .Select(group => new
                {Chauffeur =group.Key.Prenom +" "+group.Key.Nom,NumberRate=group.Count()}).ToDictionary(x=>x.Chauffeur,y=>y.NumberRate);
        var livraisonStatut = _context.Livraison.GroupBy(liv => liv.StatutLivraison)
            .Select(group => new { Statut = group.Key.ToString(), Number = group.Count() })
            .ToDictionary(f => f.Statut, g => g.Number);
        var livraisonParClient = _context.Livraison.Include(l => l.ClientLivraison)
            .Where(liv => liv.StatutLivraison == Models.Livraison.Statut.Valide).GroupBy(livr => livr.ClientLivraison)
            .Select(groupe => new { Entreprise = groupe.Key.NomEntreprise.ToString(), Number = groupe.Count() })
            .ToDictionary(m => m.Entreprise, n => n.Number);

        
    var listLivraison = _context.Livraison.Include(l=>l.ChauffeurLivraison).Include(l=>l.ClientLivraison).Where(liv => liv.StatutLivraison == Models.Livraison.Statut.Valide|liv.StatutLivraison==Models.Livraison.Statut.Rate).ToList();
        
    var viewM = new StatsViewModel()
    {
        completeListLivraison = listLivraison,
        rateChaffeur=livraisonRateParChauffeur,
        livraisonMarque=livraisonParMarque,
        livraisonStatut=livraisonStatut,
        livraisonParClient=livraisonParClient
    };
        if (searchString.IsNullOrEmpty())
        {
            return PartialView(viewM);
        }
        else
        {
            var listLivraisonSearh=_context.Livraison.Include(l=>l.ChauffeurLivraison).Include(l=>l.ClientLivraison).Where(liv => liv.StatutLivraison == Models.Livraison.Statut.Valide|liv.StatutLivraison==Models.Livraison.Statut.Rate)
                .Where(l=>l.ChauffeurLivraison.Nom.Contains(searchString)|l.ChauffeurLivraison.Prenom.Contains(searchString)|l.ClientLivraison.NomEntreprise.Contains(searchString)).ToList();
            viewM.completeListLivraison = listLivraisonSearh;
            return PartialView(viewM);
        }
    }
   
    public IActionResult ErrorPage()
    {
        return PartialView();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return PartialView(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    public async Task<IActionResult> Deconnexion([FromServices]SignInManager<IdentityUser> signInManager)
    {
        
        foreach (var cookie in Request.Cookies.Keys)
        {
            Response.Cookies.Delete(cookie);
        }
        await signInManager.SignOutAsync();
        return RedirectToAction("Index");

    }

    [Authorize(Roles = "Client")]
    public IActionResult ModificationClient()
    {
        return PartialView();
    }

    [Authorize(Roles = "Dispatcher")]
    public IActionResult ModificationDispatcher()
    {
        return PartialView();
    }


    [Authorize(Roles = "Chauffeur")]
    public IActionResult ModificationChauffeur()
    {
        return PartialView();
    }

    [Authorize(Roles = "Admin")][HttpGet]
    public async Task<IActionResult> ModificationCamion(int id)
    {
        var camion = await _context.Camions.FindAsync(id);

        if (camion == null)
        {
            return RedirectToAction("GestionEffectif");
        }

        return PartialView(camion);
    }
    

    [Authorize(Roles = "Client")]
    public async Task<IActionResult> ProfilClient([FromServices] UserManager<IdentityUser> userManager)
    {
        var user = await userManager.GetUserAsync(User);
        var client = await _context.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == user.Id);

        return View(client);
    }
    
    [Authorize(Roles = "Chauffeur")]
    public async Task<IActionResult> ProfilChauffeur([FromServices] UserManager<IdentityUser> userManager)
    {
        var user = await userManager.GetUserAsync(User);
        var client = await _context.Users.OfType<Chauffeur>().FirstOrDefaultAsync(c => c.Id == user.Id);

        return PartialView(client);
    }
    
    [Authorize(Roles = "Dispatcher")]
    public async Task<IActionResult> ProfilDispatcher([FromServices] UserManager<IdentityUser> userManager)
    {
        var user = await userManager.GetUserAsync(User);
        var dispatcher = await _context.Users.OfType<Dispatcher>().FirstOrDefaultAsync(c => c.Id == user.Id);

        return PartialView(dispatcher);
        
    }
}
