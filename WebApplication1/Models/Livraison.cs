using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

public class Livraison
{
    public Livraison(string lieuChargement, string lieuDechargement, string contenu, string heureChargement,
        string heureDechargementPrevu, Statut statutLivraison, string commentaire, string dateChargement, string dateDechargement)
    {
        LieuChargement = lieuChargement;
        LieuDechargement = lieuDechargement;
        Contenu = contenu;
        HeureChargement = heureChargement;
        HeureDechargementPrevu = heureDechargementPrevu;
        StatutLivraison = statutLivraison;
        Commentaire = commentaire;
        DateChargement = dateChargement;
        DateDechargement = dateDechargement;
    }

    public Livraison()
    {

    }

    [Key] [Required] public int ID { get; set; }
    [Required] public string LieuChargement { get; set; }
    [Required] public string LieuDechargement { get; set; }
    [Required] public string DateChargement { get; set; }
    [Required] public string DateDechargement { get; set; }

    [Required] public string Contenu { get; set; }
    [Required] public string HeureChargement { get; set; }
    [Required] public string HeureDechargementPrevu { get; set; }
    [Required] public Statut StatutLivraison { get; set; }

    public string HeureDechargementEffective { get; set; }
    public string DateDechargementEffective { get; set; }

    
    public Motif MotifLivraison
    {
        get;
        set;
    }

    public string Commentaire
    {
        get;
        set;
    }

    public enum Motif
    {
        Aucun,Accident,ClientAbsentImpossible
    }

    public enum Statut
    {
        Valide,
        Attente,
        Rate, EnCours
    }

    public Chauffeur? ChauffeurLivraison { get; set; }
    public Client ClientLivraison { get; set; }

    public Camion? CamionLivraison { get; set; }
}
