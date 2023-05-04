using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Dispatcher : MembreHelmo
{
    [Key][Required]
    public int ID { get; set; } 
    public enum NiveauEtude
    {
        CESS,
        Bachelier,
        Licencier
    }

    public NiveauEtude NiveauEtudeMax { get; set; }
}