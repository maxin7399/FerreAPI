using FerreAPI.Productos;
using Microsoft.EntityFrameworkCore;

namespace FerreAPI.Data;

public class FerreDbContext(DbContextOptions<FerreDbContext> options) : DbContext(options)
{
    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(p =>
        {
            p.HasIndex(x => x.Codigo).IsUnique();
            p.Property(x => x.Codigo).HasMaxLength(30);
            p.Property(x => x.Nombre).HasMaxLength(150);
            p.Property(x => x.Categoria).HasMaxLength(60);
            p.Property(x => x.Descripcion).HasMaxLength(500);
            p.Property(x => x.PrecioVenta).HasPrecision(12, 2);
        });
    }
}