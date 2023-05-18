using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
        
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img","logo",client.Email);
        Directory.CreateDirectory(uploadPath);
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
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img","profilPic",dispatcher.Email);
                Directory.CreateDirectory(uploadPath);
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
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img","profilPic",chauffeur.Email);
                Directory.CreateDirectory(uploadPath);
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
        return PartialView();
    }
    public IActionResult GererLivraison()
    {
        return PartialView();
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
        return PartialView("Dispatch",listLivraison);
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
        return PartialView();
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
            if (!form["email"].FirstOrDefault().IsNullOrEmpty())
            {
               /** userCasted.Email = form["email"].FirstOrDefault();
                userCasted.NormalizedEmail = form["email"].FirstOrDefault().ToUpper(); **/
            } 
            if (!form["entrepriseName"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.NomEntreprise = form["entrepriseName"].FirstOrDefault();

            }

            if (!form["localite"].FirstOrDefault().IsNullOrEmpty())
            {
                userCasted.Localite = form["localite"].FirstOrDefault();

            }
            if (!form["passwordUserConfirm"].FirstOrDefault().IsNullOrEmpty() && !form["passwordUser"].FirstOrDefault().IsNullOrEmpty())
            {
                if (form["passwordUserConfirm"].FirstOrDefault().Equals(form["passwordUser"].FirstOrDefault()))
                {
                    //userManager.ResetPasswordAsync(userCasted,)
                }
            }
            
                if (Logo != null && Logo.Length > 0)
                {
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo",userCasted.Email);

                    var imgFileName = $"{Guid.NewGuid()}_{Logo.FileName}";
                    var imgFilePath = Path.Combine(uploadsPath, imgFileName);

                    using (var fileStream = new FileStream(imgFilePath, FileMode.Create))
                    {
                        await Logo.CopyToAsync(fileStream);
                    }
                    userCasted.logo = $"~/img/{imgFileName}";

                }
                await userManager.UpdateAsync(userCasted);
                await _context.SaveChangesAsync();
            }
        return RedirectToAction("Index");

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
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "profilPic",userCasted.Email);

                var imgFileName = $"{Guid.NewGuid()}_{profil.FileName}";
                var imgFilePath = Path.Combine(uploadsPath, imgFileName);

                using (var fileStream = new FileStream(imgFilePath, FileMode.Create))
                {
                    await profil.CopyToAsync(fileStream);
                }
                userCasted.PhotoProfil = $"~/img/{imgFileName}";

            }
            
            await userManager.UpdateAsync(userCasted);
            await _context.SaveChangesAsync();
            
        }
        return RedirectToAction("Index");
    }



}