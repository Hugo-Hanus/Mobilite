using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Client : Personne
{
    
    [Required]
    public string NomEntreprise { get; set; }
    [Required]
    public string Rue { get; set; }
    [Required]
    public int Numero { get; set; }
    [Required]
    public int CodePostal { get; set; }
    [Required]
    public string Localite { get; set; }
    [Required]
    public string Pays { get; set; }
    
    public bool isMauvaisPayeur { get; set; }
    
    public string logo { get; set; }
}