using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Livraison
{
    public Livraison(string lieuChargement, string lieuDechargement, string contenu, DateTime heureChargement,
        DateTime heureDechargementPrevu, Statut statutLivraison, string commentaire)
    {
        LieuChargement = lieuChargement;
        LieuDechargement = lieuDechargement;
        Contenu = contenu;
        HeureChargement = heureChargement;
        HeureDechargementPrevu = heureDechargementPrevu;
        StatutLivraison = statutLivraison;
        Commentaire = commentaire;
    }

    public Livraison()
    {

    }

    [Key] [Required] public int ID { get; set; }
    [Required] public string LieuChargement { get; set; }
    [Required] public string LieuDechargement { get; set; }
    [Required] public string Contenu { get; set; }
    [Required] public DateTime HeureChargement { get; set; }
    [Required] public DateTime HeureDechargementPrevu { get; set; }
    [Required][DefaultValue("Attente")] public Statut StatutLivraison { get; set; }

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
        Motif,Accident,ClientAbsentImpossible
    }

public enum Statut
    {
        Valide,
        Attente,
        Rate
    }
}
