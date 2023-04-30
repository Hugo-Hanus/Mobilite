using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public class InstallationDbContext : DbContext
{
  public InstallationDbContext(DbContextOptions<InstallationDbContext> options) : base(options)
  {
  }
  
  public DbSet<Camion> Camions { get; set; }
}