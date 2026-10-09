using FerreAPI.Categorias;
using FerreAPI.Productos;
using Microsoft.EntityFrameworkCore;

namespace FerreAPI.Data;

public class FerreDbContext(DbContextOptions<FerreDbContext> options) : DbContext(options)
{
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(p =>
        {
            p.HasIndex(x => x.Codigo).IsUnique();
            p.Property(x => x.Codigo).HasMaxLength(30);
            p.Property(x => x.Nombre).HasMaxLength(150);
            p.Property(x => x.Descripcion).HasMaxLength(500);
            p.Property(x => x.PrecioVenta).HasPrecision(12, 2);
        });

        modelBuilder.Entity<Categoria>(c =>
        {
            c.HasIndex(x => x.Nombre).IsUnique();
            c.Property(x => x.Nombre).HasMaxLength(60);
        });

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}