using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

public class Camion
{
    public Camion(string marque, string modèle, string immatriculation, string type, int tonnage)
    {
        this.Marque = marque;
        this.Modèle = modèle;
        this.Immatriculation = immatriculation;
        this.Type = type;
        this.Tonnage = tonnage;
    }

    public Camion()
    {
        
    }
    
    [Key][Required]
    public int ID { get; set; } 
    
    [Required][MaxLength(50)]
    public string Marque { get; set; }
    [Required][MaxLength(50)]
    public string Modèle { get; set; }
    [Required][MaxLength(25)]
    public string Immatriculation { get; set; }
    [Required][MaxLength(2)]
    public string Type { get; set; }
    [Required]
    public int Tonnage { get; set; }
    [Required]
    public string Img { get; set; }
}