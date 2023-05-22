using WebApplication1.Models;

namespace WebApplication1.ViewModel;

public class LivraisonNameChauffeurListModel
{
    public List<Chauffeur> ListChauffeur { get; set; }
    public int Livraison { get; set; }
}