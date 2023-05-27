using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
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

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly InstallationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public AccountController(IWebHostEnvironment environment,ILogger<AccountController> logger,InstallationDbContext context)
    {
        _logger = logger;
        _context = context;
        _environment = environment;
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
                return RedirectToAction("Index","Home");
            }
        }
        return RedirectToAction("Index","Home");
    }
   
    /* -----------------------------------------------------------------------------------------------
     *          CLIENT
     * -----------------------------------------------------------------------------------------------
     */
    
     [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> InscriptionClient([FromServices]UserManager<IdentityUser> userManager, [FromServices]SignInManager<IdentityUser> signInManager, Client client)
    {
        var mdp = client.MotDePasse;
        client.MotDePasse = null;
        client.UserName = client.NomEntreprise;
        await userManager.CreateAsync(client, mdp);
        await userManager.AddToRoleAsync(client, "Client"); 
        await signInManager.SignInAsync(client, false); 
        
        return RedirectToAction("Index","Home");
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
        return RedirectToAction("ProfilClient","Home");

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
        return RedirectToAction("Index","Home");
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
        return RedirectToAction("ProfilDispatcher","Home");
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
        return RedirectToAction("ProfilChauffeur","Home");

    }
    
  
}