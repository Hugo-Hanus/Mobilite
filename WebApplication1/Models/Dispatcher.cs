using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Dispatcher : MembreHelmo
{
    public enum NiveauEtude
    {
        CESS,
        Bachelier,
        Licencier
    }

    public NiveauEtude NiveauEtudeMax { get; set; }
}