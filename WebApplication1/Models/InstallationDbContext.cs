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
}