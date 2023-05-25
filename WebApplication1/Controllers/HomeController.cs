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

    public IActionResult Index()
    {
        List<Client> listClients=new List<Client>();
        listClients= _context.Clients.ToList();
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
        var mdp = client.MotDePasse;
        client.MotDePasse = null;
        client.UserName = client.NomEntreprise;
        await userManager.CreateAsync(client, mdp);
        await userManager.AddToRoleAsync(client, "Client"); 
        await signInManager.SignInAsync(client, false); 
        
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
    public async Task<IActionResult> Livraison()
    {
        //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userCasted = await _context.Users.OfType<Client>().SingleOrDefaultAsync(u => u.Id == userId);

        var livraisonsEnAttente = await _context.Livraison.Where(l => l.ClientLivraison == userCasted && l.StatutLivraison==Models.Livraison.Statut.Attente).ToListAsync();
        var livraisonFini = await _context.Livraison.Where(l => l.ClientLivraison == userCasted && (l.StatutLivraison == Models.Livraison.Statut.Valide || l.StatutLivraison == Models.Livraison.Statut.Rate)).ToListAsync();
        var LivrasionVm = new LivraisonViewModel();
        LivrasionVm.LivraisonsFini = livraisonFini;
        LivrasionVm.LivraisonsEnAttente = livraisonsEnAttente;
        return View(LivrasionVm);
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

    public IActionResult ModifierLivraison(int id)
    {
        var livraison = _context.Livraison.FirstOrDefault(l => l.ID == id);
        if (livraison == null)
        {
            return RedirectToAction("Index");
        }
        return View(livraison);
    }

    
    [HttpPost][ValidateAntiForgeryToken][Authorize(Roles = "Client")]
    public IActionResult ModifierLivraison(Livraison model)
    {
        
        var livraison = _context.Livraison.FirstOrDefault(l => l.ID == model.ID);
        if (livraison == null)
        {
            return RedirectToAction("Index");
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

        return RedirectToAction("Livraison");
    }
    
    [Authorize(Roles = "Client")]
    public IActionResult CreerLivraison()
    {
        return PartialView();
    }
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

        return RedirectToAction("Livraison");
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
        return View(livraison);
    }
    [HttpPost]
    [Authorize(Roles = "Chauffeur")]
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

        return RedirectToAction("LivraisonDispatch");
    }
    
    [Authorize(Roles = "Chauffeur")]
    public IActionResult RaterLivraison(int id)
    {
        var livraison = _context.Livraison.Find(id);
        if(livraison == null)
        {
            return NotFound();
        }
        return View(livraison);
    }
    
    [Authorize(Roles = "Chauffeur")][HttpPost]
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

        return RedirectToAction("LivraisonDispatch");
    }
    
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GestionEffectif()
    {
        List<Chauffeur> chauffeurs = await _context.Users.OfType<Chauffeur>().ToListAsync();
        List<Camion> camions = await _context.Camions.ToListAsync();
        var ViewModel = new GestionEffectifViewModel();
        ViewModel.Camions = camions;
        ViewModel.Chauffeurs = chauffeurs;
        return View(ViewModel);
    }
    
    [Authorize(Roles = "Admin")]
    public IActionResult AjouterCamion()
    {
        return PartialView();
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
            }else
            {
                View(camion);
            }
            
            Regex regex = new Regex(@"^\d-[A-Za-z]{3}-\d{3}$");
            if (regex.IsMatch(camion.Immatriculation))
            {
                _context.Camions.Add(camion);
                await _context.SaveChangesAsync();
            }
            else
            {
                return View(camion);
            }
            
            
            return RedirectToAction("GestionEffectif");
    }
    
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Clientliste([FromServices]UserManager<IdentityUser>userManager)
    {
        var users = await userManager.GetUsersInRoleAsync("Client");
        var clients = users.OfType<Client>().ToList();
        return PartialView(clients);
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

    [Authorize(Roles = "Client")]
    
    public IActionResult ModificationClient()
    {
        return PartialView();
    }
    [Authorize(Roles = "Client")][HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> ModificationClient([FromServices]UserManager<IdentityUser>userManager,IFormFile Logo, [FromForm] IFormCollection form)
    {
        if (User.Identity.IsAuthenticated)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userCasted = await _context.Users.OfType<Client>().SingleOrDefaultAsync(u => u.Id == userId);
            
            if (!form["Pays"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.Pays = form["Pays"].FirstOrDefault();

            } 
            if (!form["codePostal"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.CodePostal = int.Parse(form["codePostal"].FirstOrDefault());

            } 
            if (!form["rue"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.Rue = form["rue"].FirstOrDefault();

            } 
            if (!form["numero"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.Numero = int.Parse(form["numero"].FirstOrDefault());

            }
            if (!form["entrepriseName"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.NomEntreprise = form["entrepriseName"].FirstOrDefault();

            }

            if (!form["localite"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.Localite = form["localite"].FirstOrDefault();

            }
            
                if (Logo != null && Logo.Length > 0)
                {
                    var user = await userManager.GetUserAsync(User);
                    var userEmail = user?.Email;
            
                    var fileExtension = Path.GetExtension(Logo.FileName);

                    string newFileName = $"{userEmail}{fileExtension}";
                    var path = Path.Combine(_environment.WebRootPath, "img", newFileName);


                      using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await Logo.CopyToAsync(stream);
                    }
                    userCasted.logo = $"~/img/{newFileName}";

                }
                await userManager.UpdateAsync(userCasted);
                await _context.SaveChangesAsync();
            }
        return RedirectToAction("ProfilClient");

    }
    
    [Authorize(Roles = "Dispatcher")]
    public IActionResult ModificationDispatcher()
    {
        return PartialView();
    }

    [Authorize(Roles = "Dispatcher")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModificationDispatcher([FromServices] UserManager<IdentityUser> userManager,IFormFile profil, [FromForm] IFormCollection form)
    {
        if (User.Identity.IsAuthenticated)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userCasted = await _context.Users.OfType<Dispatcher>().SingleOrDefaultAsync(u => u.Id == userId);
            
            if (!form["dateNaissance"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.DateNaissance = form["dateNaissance"].FirstOrDefault();

            }
            if (!form["nom"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.Nom = form["nom"].FirstOrDefault();

            }
            if (!form["prenom"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.Prenom = form["prenom"].FirstOrDefault();

            }
            if (!form["matricule"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.Matricule = form["matricule"].FirstOrDefault();

            }
            if (profil != null && profil.Length > 0)
            {
                var user = await userManager.GetUserAsync(User);
                var userEmail = user?.Email;
            
                var fileExtension = Path.GetExtension(profil.FileName);

                string newFileName = $"{userEmail}{fileExtension}";
                var path = Path.Combine(_environment.WebRootPath, "img", newFileName);


                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await profil.CopyToAsync(stream);
                }
                userCasted.PhotoProfil = $"~/img/{newFileName}";

            }
            
            await userManager.UpdateAsync(userCasted);
            await _context.SaveChangesAsync();
            
        }
        return RedirectToAction("ProfilDispatcher");
    }

    [Authorize(Roles = "Chauffeur")]
    public IActionResult ModificationChauffeur()
    {
        return PartialView();
    }

    [Authorize(Roles = "Chauffeur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModificationChauffeur([FromServices] UserManager<IdentityUser> userManager, IFormFile profil, [FromForm] IFormCollection form)
    {
        if (User.Identity.IsAuthenticated)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userCasted = await _context.Users.OfType<Chauffeur>().SingleOrDefaultAsync(u => u.Id == userId);
            
            if (!form["dateNaissance"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.DateNaissance = form["dateNaissance"].FirstOrDefault();

            }
            if (!form["nom"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.Nom = form["nom"].FirstOrDefault();

            }
            if (!form["prenom"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.Prenom = form["prenom"].FirstOrDefault();

            }
            if (!form["matricule"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.Matricule = form["matricule"].FirstOrDefault();

            }
            if (profil != null && profil.Length > 0)
            {
                var user = await userManager.GetUserAsync(User);
                var userEmail = user?.Email;
            
                var fileExtension = Path.GetExtension(profil.FileName);

                string newFileName = $"{userEmail}{fileExtension}";
                var path = Path.Combine(_environment.WebRootPath, "img", newFileName);


                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await profil.CopyToAsync(stream);
                }
                userCasted.PhotoProfil = $"~/img/{newFileName}";

            }
            
            await userManager.UpdateAsync(userCasted);
            await _context.SaveChangesAsync();
            
        }
        return RedirectToAction("ProfilChauffeur");

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

        return RedirectToAction("Clientliste");

    }
    
    [Authorize(Roles = "Admin")][HttpGet]
    public async Task<IActionResult> ModificationCamion(int id)
    {
        var camion = await _context.Camions.FindAsync(id);

        if (camion == null)
        {
            return RedirectToAction("GestionEffectif");
        }

        return View(camion);
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
            return View(camion);
        }
        existingCamion.Marque = camion.Marque;
        existingCamion.Modele = camion.Modele;
        existingCamion.Tonnage = camion.Tonnage;
        existingCamion.Type = camion.Type;
        _context.Update(existingCamion);
        await _context.SaveChangesAsync();

        return RedirectToAction("GestionEffectif");
    }

    [HttpPost][Authorize(Roles = "Admin")]
    public async Task<IActionResult> SupprimerCamion([FromForm] int id)
    {
        var camion = await _context.Camions.FindAsync(id);

        if (camion == null)
        {
            return RedirectToAction("GestionEffectif");
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

        _context.Camions.Remove(camion);
        await _context.SaveChangesAsync();

        return RedirectToAction("GestionEffectif");
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

        return RedirectToAction("GestionEffectif");
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

        return View(client);
    }
    
    [Authorize(Roles = "Dispatcher")]
    public async Task<IActionResult> ProfilDispatcher([FromServices] UserManager<IdentityUser> userManager)
    {
        var user = await userManager.GetUserAsync(User);
        var dispatcher = await _context.Users.OfType<Dispatcher>().FirstOrDefaultAsync(c => c.Id == user.Id);

        return View(dispatcher);
        
    }
}
