using WebApplication1.Models;

namespace WebApplication1.ViewModel;

public class StatsViewModel
{
    public List<Livraison> completeListLivraison;


    public Dictionary<string,int> rateChaffeur { get; set; }
    public Dictionary<string,int> livraisonMarque { get; set; }
    public Dictionary<string, int> livraisonStatut { get; set; }
    public Dictionary<string, int> livraisonParClient { get; set; }
}