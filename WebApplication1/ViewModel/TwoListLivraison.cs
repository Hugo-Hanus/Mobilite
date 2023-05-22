using WebApplication1.Models;

namespace WebApplication1.ViewModel;

public class TwoListLivraison
{
   public List<Livraison> ableGererList { get; set; } 
   public List<Livraison> mauvaisPayeurList { get; set; } 
}