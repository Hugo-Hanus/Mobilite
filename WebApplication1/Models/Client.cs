using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Client : Personne
{
    [Key][Required]
    public int ID { get; set; } 
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
}