﻿using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

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
        return PartialView();
    }

    public IActionResult Privacy()
    {
        return PartialView();
    }
    public IActionResult InscriptionClient()
    {
        return PartialView();
    }
    
    [HttpPost]
    public async Task<IActionResult> InscriptionClient(Client client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    } 
    public IActionResult InscriptionMembre()
    {
        return PartialView();
    }
    
    [HttpPost]
    public async Task<IActionResult> InscriptionMembres([FromForm] IFormCollection form)
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

            if (password.Equals(passwordConfirm))
            {
                dispatcher.MotDePasse= password;
            }

            _context.Dispatchers.Add(dispatcher);
            await _context.SaveChangesAsync();


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

            if (password.Equals(passwordConfirm))
            {
                chauffeur.MotDePasse= password;
            }
            _context.Chauffeurs.Add(chauffeur);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    } 
    public IActionResult Connexion()
    {
        return PartialView();
    }
    public IActionResult Livraison()
    {
        return PartialView();
    }
    public IActionResult Dispatch()
    {
        return PartialView();
    }public IActionResult GererLivraison()
    {
        return PartialView();
    }
    
    public IActionResult CreerLivraison()
    {
        return PartialView();
    }
    public IActionResult LivraisonDispatch()
    {
        return PartialView();
    }
    public IActionResult ValiderLivraison()
    {
        return PartialView();
    }
    public IActionResult RaterLivraison()
    {
        return PartialView();
    }
    public IActionResult GestionEffectif()
    {
        return PartialView();
    }
    public IActionResult AjouterCamion()
    {
        return PartialView();
    }
    
    
    [HttpPost]
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

        _context.Camions.Add(camion);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
    public IActionResult Clientliste()
    {
        return PartialView();
    }
    public IActionResult Statistique()
    {
        return PartialView();
    }
    


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}