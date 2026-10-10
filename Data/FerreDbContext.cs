using FerreAPI.Categorias;
using FerreAPI.Facturas;
using FerreAPI.Productos;
using FerreAPI.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FerreAPI.Data;

public class FerreDbContext(DbContextOptions<FerreDbContext> options) : DbContext(options)
{
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(p =>
        {
            p.HasIndex(x => x.Codigo).IsUnique();
            p.Property(x => x.Codigo).HasMaxLength(30);
            p.Property(x => x.Nombre).HasMaxLength(150);
            p.Property(x => x.Descripcion).HasMaxLength(500);
            p.Property(x => x.PrecioVenta).HasPrecision(12, 2);
            p.Property(x => x.Version).IsRowVersion();
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

        modelBuilder.Entity<Factura>(f =>
        {
            f.Property(x => x.Subtotal).HasPrecision(14, 2);
            f.Property(x => x.Iva).HasPrecision(14, 2);
            f.Property(x => x.Total).HasPrecision(14, 2);

            f.HasMany(x => x.Lineas)
                .WithOne()
                .HasForeignKey(l => l.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);

            f.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<FacturaLinea>(l =>
        {
            l.Property(x => x.PrecioUnitario).HasPrecision(12, 2);
            l.Property(x => x.PorcentajeIva).HasPrecision(5, 2);
            l.Property(x => x.Subtotal).HasPrecision(14, 2);
            l.Property(x => x.Iva).HasPrecision(14, 2);

            l.HasOne(x => x.Producto)
                .WithMany()
                .HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<Usuario>(u =>
        {
            u.HasIndex(x => x.Email).IsUnique();
            u.Property(x => x.Email).HasMaxLength(150);
            u.Property(x => x.Nombre).HasMaxLength(100);
            u.Property(x => x.Rol).HasConversion<string>().HasMaxLength(20);
        });
    }
}