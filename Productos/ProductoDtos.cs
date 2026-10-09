namespace FerreAPI.Productos;

    
public record CrearProductoRequest(
string Codigo, string Nombre, string? Descripcion, decimal PrecioVenta,
int Stock, int StockMinimo, int CategoriaId)
{
    public Dictionary<string, string[]> Validar()
    {
        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(Codigo))
            errores["codigo"] = ["El código es obligatorio."];
        if (string.IsNullOrWhiteSpace(Nombre))
            errores["nombre"] = ["El nombre es obligatorio."];
        if (PrecioVenta <= 0)
            errores["precioVenta"] = ["El precio debe ser mayor que 0."];
        if (Stock < 0)
            errores["stock"] = ["El stock no puede ser negativo."];
        if (StockMinimo < 0)
            errores["stockMinimo"] = ["El stock mínimo no puede ser negativo."];
        if (CategoriaId <= 0)
            errores["categoriaId"] = ["La categoría es obligatoria."];

        return errores;
    }
}
public record ProductoResponse(
    int Id, string Codigo, string Nombre, string? Descripcion, decimal PrecioVenta,
    int Stock, int StockMinimo, int CategoriaId, string CategoriaNombre, bool Activo);

public static class ProductoMapeos
{
    public static ProductoResponse ToResponse(this Producto p) =>
        new(p.Id, p.Codigo, p.Nombre, p.Descripcion, p.PrecioVenta,
            p.Stock, p.StockMinimo, p.CategoriaId, p.Categoria!.Nombre, p.Activo);
    public static Producto ToEntidad(this CrearProductoRequest req) => new()
    {
        Codigo = req.Codigo.Trim().ToUpperInvariant(),
        Nombre = req.Nombre.Trim(),
        Descripcion = req.Descripcion?.Trim(),
        PrecioVenta = req.PrecioVenta,
        Stock = req.Stock,
        StockMinimo = req.StockMinimo,
        CategoriaId = req.CategoriaId
    };
    public static void AplicarCambios(this Producto p, ActualizarProductoRequest req)
    {
        p.Nombre = req.Nombre.Trim();
        p.Descripcion = req.Descripcion?.Trim();
        p.PrecioVenta = req.PrecioVenta;
        p.Stock = req.Stock;
        p.StockMinimo = req.StockMinimo;
        p.CategoriaId = req.CategoriaId;
    }
}

public record ActualizarProductoRequest(
    string Nombre, string? Descripcion, decimal PrecioVenta,
    int Stock, int StockMinimo, int CategoriaId)
{
    public Dictionary<string, string[]> Validar()
    {
        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(Nombre))
            errores["nombre"] = ["El nombre es obligatorio."];
        if (PrecioVenta <= 0)
            errores["precioVenta"] = ["El precio debe ser mayor que 0."];
        if (Stock < 0)
            errores["stock"] = ["El stock no puede ser negativo."];
        if (StockMinimo < 0)
            errores["stockMinimo"] = ["El stock mínimo no puede ser negativo."];
        if (CategoriaId <= 0)
            errores["categoriaId"] = ["La categoría es obligatoria."];

        return errores;
    }
}
