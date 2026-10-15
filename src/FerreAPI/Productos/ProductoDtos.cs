using System.ComponentModel.DataAnnotations;

namespace FerreAPI.Productos;


public record CrearProductoRequest(
    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(30, ErrorMessage = "El código no puede superar los 30 caracteres.")]
    string Codigo,

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    string Nombre,

    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    string? Descripcion,

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que 0.")]
    decimal PrecioVenta,

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    int Stock,

    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    int StockMinimo,

    [Range(1, int.MaxValue, ErrorMessage = "La categoría es obligatoria.")]
    int CategoriaId);

public record ProductoResponse(
    int Id, string Codigo, string Nombre, string? Descripcion, decimal PrecioVenta,
    int Stock, int StockMinimo, int CategoriaId, string CategoriaNombre, bool Activo);

public static class ProductoMapeos
{
    public static ProductoResponse ToResponse(this Producto p) =>
        new(p.Id, p.Codigo, p.Nombre, p.Descripcion, p.PrecioVenta,
            p.Stock, p.StockMinimo, p.CategoriaId, p.Categoria!.Nombre, p.Activo);
    public static Producto ToEntidad(this CrearProductoRequest req)
    {
        var producto = new Producto {
            Codigo = req.Codigo.Trim().ToUpperInvariant(),
            Nombre = req.Nombre.Trim(),
            Descripcion = req.Descripcion?.Trim(),
            PrecioVenta = req.PrecioVenta,
            StockMinimo = req.StockMinimo,
            CategoriaId = req.CategoriaId
        };
        if (req.Stock > 0)
            producto.AumentarStock(req.Stock);
        return producto;
    }
    public static void AplicarCambios(this Producto p, ActualizarProductoRequest req)
    {
        p.Nombre = req.Nombre.Trim();
        p.Descripcion = req.Descripcion?.Trim();
        p.PrecioVenta = req.PrecioVenta;
        p.StockMinimo = req.StockMinimo;
        p.CategoriaId = req.CategoriaId;
    }
}

public record ActualizarProductoRequest(

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    string Nombre,

    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    string? Descripcion,

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que 0.")]
    decimal PrecioVenta,

    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    int StockMinimo,

    [Range(1, int.MaxValue, ErrorMessage = "La categoría es obligatoria.")]
    int CategoriaId);
