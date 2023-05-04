using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Chauffeur : MembreHelmo
{

    public Chauffeur()
    {
        
    }
    
    [Key][Required]
    public int ID { get; set; } 
    
    public bool PermisB { get; set; }
    public bool PermisC { get; set; }
    public bool PermisCE { get; set; }
}