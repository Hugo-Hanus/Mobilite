using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public class InstallationDbContext : IdentityDbContext
{
  public InstallationDbContext(DbContextOptions<InstallationDbContext> options) : base(options)
  {
  }
  
  public DbSet<Camion> Camions { get; set; }
  public DbSet<Client> Clients { get; set; }
  public DbSet<Dispatcher> Dispatchers { get; set; }
  public DbSet<Chauffeur> Chauffeurs { get; set; }
  public DbSet<Livraison> Livraison { get; set; }

  protected override void OnModelCreating(ModelBuilder builder)
  {
      base.OnModelCreating(builder);
      
      
      /* builder.Entity<Livraison>()
         .HasOne(e => e.ClientLivraison).WithOne().HasForeignKey<Client>("ClientId");
       builder.Entity<Livraison>()
         .HasOne(e => e.ChauffeurLivraison).WithOne().HasForeignKey<Chauffeur>("ChauffeurId");
   */
  }
}