using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.ViewModel;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        List<Client> listClients = new List<Client>();
            listClients.Add(new Client());
            return PartialView("Index",listClients);
    }

    public IActionResult Privacy()
    {
        return PartialView();
    }
    public IActionResult InscriptionClient()
    {
        return PartialView();
    }
    
    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> InscriptionClient([FromServices]UserManager<IdentityUser> userManager, [FromServices]SignInManager<IdentityUser> signInManager, Client client)
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
            }
        }

        return RedirectToAction("Index");
    } 
    public IActionResult InscriptionMembre()
    {
        return PartialView();
    }
    
    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> InscriptionMembre([FromServices]UserManager<IdentityUser> userManager, [FromServices]SignInManager<IdentityUser> signInManager, [FromForm] IFormCollection form)
    {
        
        string type = form["role"].ToString();
        string matricule = form["matriculeValue"].ToString();
        string nom = form["nameValue"].ToString();
        string prenom = form["surnameValue"].ToString();
        string mail = form["mailValue"].ToString();
        string password = form["passwordValue"].ToString();
        string passwordConfirm = form["passwordConfirmValue"].ToString();
        string naissance = form["birthdateValue"].ToString();
        
        

        if (type.Equals("dispatcher"))
        {
            string diplome = form["divName"].ToString();
            Dispatcher dispatcher = new Dispatcher();
            if (diplome.Equals("CESS"))
            {
                dispatcher.NiveauEtudeMax = Dispatcher.NiveauEtude.CESS;

            }else if (diplome.Equals("Bachelier"))
            {
                dispatcher.NiveauEtudeMax = Dispatcher.NiveauEtude.Bachelier;

            }
            else
            {
                dispatcher.NiveauEtudeMax = Dispatcher.NiveauEtude.Licencier;
            }

            dispatcher.Email = mail;
            dispatcher.DateNaissance = naissance;
            dispatcher.Matricule = matricule;
            dispatcher.Nom = nom;
            dispatcher.Prenom = prenom;
            dispatcher.UserName = matricule;

            if (password.Equals(passwordConfirm))
            {
                await userManager.CreateAsync(dispatcher, password);
                await userManager.AddToRoleAsync(dispatcher, "Dispatcher");
                await signInManager.SignInAsync(dispatcher, false);
            }
            
        }else if (type.Equals("chauffeur"))
        {
            var list = form["divPermis"];
            Chauffeur chauffeur = new Chauffeur();

            foreach (string s in list)
            {
                if (s.Equals("B"))
                {
                    chauffeur.PermisB = true;
                }else if (s.Equals("C"))
                {
                    chauffeur.PermisC = true;
                }
                else
                {
                    chauffeur.PermisCE = true;
                }
            }
            
            
            chauffeur.Email = mail;
            chauffeur.DateNaissance = naissance;
            chauffeur.Matricule = matricule;
            chauffeur.Nom = nom;
            chauffeur.Prenom = prenom;
            chauffeur.UserName = matricule;

            if (chauffeur.PermisC == false && chauffeur.PermisCE == true)
            {
                chauffeur.PermisC=true;
            }
            
            if (password.Equals(passwordConfirm))
            {
                await userManager.CreateAsync(chauffeur, password);
                await userManager.AddToRoleAsync(chauffeur, "Chauffeur");
                await signInManager.SignInAsync(chauffeur, false);
                
            }
        }
        return RedirectToAction("Index");
    } 
    public IActionResult Connexion()
    {
           
        return PartialView();
    }
    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Connexion([FromServices]UserManager<IdentityUser>userManager, [FromServices]SignInManager<IdentityUser> signInManager,[FromForm] IFormCollection form)
    {
        var userIdentity = await userManager.FindByEmailAsync(form["email"].ToString());
        if (userIdentity != null)
        {
            var signInResult = await signInManager.PasswordSignInAsync(userIdentity, form["password"].ToString(), false, false);
            if (signInResult.Succeeded)
            {
                return RedirectToAction("Index");
            }
        }
        return PartialView();
    }
    
    [Authorize(Roles = "Client")]
    public IActionResult Livraison()
    {
        return PartialView();
    }
    
    [Authorize(Roles = "Dispatcher")]
    public IActionResult Dispatch()
    {
        var listLivraisonDispo = _context.Livraison.Where(l => l.StatutLivraison == Models.Livraison.Statut.Attente & l.ChauffeurLivraison==null & l.ClientLivraison.isMauvaisPayeur==false);
        var listLivraisonMauvaisPayeur = _context.Livraison.Where(l => l.StatutLivraison == Models.Livraison.Statut.Attente & l.ChauffeurLivraison==null & l.ClientLivraison.isMauvaisPayeur==true);

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
                              DateTime.Parse(l.HeureDechargementPrevu) >= heureDechargementPrevu &&
                              DateTime.Parse(l.HeureDechargementPrevu) <= heureDechargementPrevu))
                .ToList();
            var  viewModel = new LivraisonNameChauffeurListModel
            {
                Livraison = livraisonGerer.ID,
                ListChauffeur = chauffeursNonIssusDeLivraison.ToList()
            };
        
        
        

        return PartialView("GererLivraison",viewModel);
    }
    [Produces("application/json")]
    [ValidateAntiForgeryToken]
    public  IActionResult GetCamionDispo(string selectedChauffeurId,int livraisonId)
    {
        var chauffeur = _context.Users.OfType<Chauffeur>().SingleOrDefault(chauf => chauf.Id == selectedChauffeurId);
        var livraisonChoisie = _context.Livraison.SingleOrDefault(liv => liv.ID == livraisonId);
        var heureChargement = DateTime.Parse(livraisonChoisie.HeureChargement);
        var heureDechargementPrevu = DateTime.Parse(livraisonChoisie.HeureDechargementPrevu);

        var camionsDisponibles = _context.Camions.Where(camion =>
            (chauffeur.PermisB && camion.Type == "B") ||
            (chauffeur.PermisC && camion.Type == "C") ||
            (chauffeur.PermisCE && camion.Type == "CE")
        ).ToList();
     
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
                          DateTime.Parse(l.HeureDechargementPrevu) >= heureDechargementPrevu &&
                          DateTime.Parse(l.HeureDechargementPrevu) <= heureDechargementPrevu))
            .ToList();


        return Ok(camionsDisponiblesAuHoraire.ToList());
    
}


    
    [Authorize(Roles = "Client")]
    public IActionResult CreerLivraison()
    {
        return PartialView();
    }
    
    [Authorize(Roles = "Chauffeur")]
    public IActionResult LivraisonDispatch()
    {
        List<Livraison> listLivraison= new List<Livraison>();

        using (_context )
        {
            listLivraison = _context.Livraison.Where(l=>l.StatutLivraison==Models.Livraison.Statut.Attente).ToList();
        }
        return PartialView("LivraisonDispatch",listLivraison);
    }
    
    [Authorize(Roles = "Chauffeur")]
    public IActionResult ValiderLivraison()
    {
        return PartialView();
    }
    
    [Authorize(Roles = "Chauffeur")]
    public IActionResult RaterLivraison()
    {
        return PartialView();
    }
    
    [Authorize(Roles = "Admin")]
    public IActionResult GestionEffectif()
    {
        return PartialView();
    }
    
    [Authorize(Roles = "Admin")]
    public IActionResult AjouterCamion()
    {
        return PartialView();
    }
    
    
    [HttpPost][ValidateAntiForgeryToken][Authorize(Roles = "Admin")]
    public async Task<IActionResult> AjouterCamion(Camion camion, IFormFile Img)
    {
        if (ModelState.IsValid)
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
        }

        return RedirectToAction("Index");
    }
    
    [Authorize(Roles = "Admin")]
    public IActionResult Clientliste()
    {
        return PartialView();
    }
    
    [Authorize(Roles = "Admin")]
    public IActionResult Statistique()
    {
        //get all livraison effectuer et récuper le chauffeur/le client /date
        var listLivraison = _context.Livraison.Include(l=>l.ChauffeurLivraison).Include(l=>l.ClientLivraison).Where(liv => liv.StatutLivraison == Models.Livraison.Statut.Valide).ToList();
       
        return PartialView(listLivraison);
    }
    [Produces("application/json")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SearchList(string searchList)
    {
        var listChauffeur = await _context.Livraison.Include(l=>l.ChauffeurLivraison).Where(liv =>
            liv.ChauffeurLivraison.UserName.Contains(searchList)).Select(liv => liv.ChauffeurLivraison).Distinct().ToListAsync();
        var listClient = await _context.Livraison.Include(l=>l.ClientLivraison).Where(liv =>
            liv.ClientLivraison.UserName.Contains(searchList) & liv.StatutLivraison==Models.Livraison.Statut.Attente).Select(liv => liv.ClientLivraison).Distinct().ToListAsync();
        var listString = new List<string>();
        listChauffeur.ForEach(e => listString.Add(e.UserName));
        listClient.ForEach(e=>listString.Add(e.UserName));
        return Ok(listString);
    }
    public IActionResult ErrorPage()
    {
        return PartialView();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    public async Task<IActionResult> Deconnexion([FromServices]SignInManager<IdentityUser> signInManager)
    {
        await signInManager.SignOutAsync();

        foreach (var cookie in Request.Cookies.Keys)
        {
            Response.Cookies.Delete(cookie);
        }

        return RedirectToAction("Index");

    }
}