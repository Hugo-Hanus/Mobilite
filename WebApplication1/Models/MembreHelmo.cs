namespace WebApplication1.Models;

public class MembreHelmo : Personne
{
    public string Nom { get; set; }
    public string Prenom { get; set; }
    public string Matricule { get; set; }
    
    public string DateNaissance { get; set; }
    
    public string PhotoProfil { get; set; }
}