using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Camion
{
    public Camion(string marque, string modèle, string immatriculation, string type, int tonnage, string img)
    {
        this.Marque = marque;
        this.Modèle = modèle;
        this.Immatriculation = immatriculation;
        this.Type = type;
        this.Tonnage = tonnage;
        this.Img = img;
    }
    [Key][MaxLength(50)][Required]
    public string ID { get; set; }
    [Required][MaxLength(50)]
    public string Marque { get; set; }
    [Required][MaxLength(50)]
    public string Modèle { get; set; }
    [Required][MaxLength(25)]
    public string Immatriculation { get; set; }
    [Required][MaxLength(2)]
    public string Type { get; set; }
    [Required][MaxLength(10)]
    public int Tonnage { get; set; }
    [Required][MaxLength(50)]
    public string Img { get; set; }
}