using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

public class Camion
{
    public Camion(MarqueCamion marque, string modele, string immatriculation, string type, int tonnage)
    {
        this.Marque = marque;
        this.Modele = modele;
        this.Immatriculation = immatriculation;
        this.Type = type;
        this.Tonnage = tonnage;
    }

    public Camion()
    {
        
    }
    
    [Key][Required]
    public int ID { get; set; } 
    
    [Required]
    public MarqueCamion Marque { get; set; }
    
    public enum MarqueCamion
    {
        Volvo,
        Scania,
        MercedesBenz,
        DAF,
        Iveco,
        MAN,
        Peugeot,
        Renault,
        Fiat,
        Nissan,
        FRUEHAUF
    }
    [Required][MaxLength(50)]
    public string Modele { get; set; }
    
    [Required][MaxLength(25)][RegularExpression(@"^\d-[A-Za-z]{3}-\d{3}$", ErrorMessage = "Format d'immatriculation non valide.")]
    public string Immatriculation { get; set; }
    [Required][MaxLength(2)]
    public string Type { get; set; }
    [Required]
    public int Tonnage { get; set; }
    public string Img { get; set; }
}