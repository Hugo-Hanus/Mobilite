﻿using System.Diagnostics;
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
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult InscriptionClient()
    {
        return View();
    }
    public IActionResult InscriptionMembre()
    {
        return View();
    }
    public IActionResult Connexion()
    {
        return View();
    }
    public IActionResult Livraison()
    {
        return View();
    }
    public IActionResult Dispatch()
    {
        return View();
    }public IActionResult GererLivraison()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}