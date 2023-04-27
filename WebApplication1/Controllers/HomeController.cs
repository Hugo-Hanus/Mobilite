using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
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
    public IActionResult InscriptionMembre()
    {
        return PartialView();
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