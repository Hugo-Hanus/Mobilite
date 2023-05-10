using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models;

public abstract class Personne : IdentityUser
{
    //public string Email { get; set; }
    public string MotDePasse { get; set; }
}