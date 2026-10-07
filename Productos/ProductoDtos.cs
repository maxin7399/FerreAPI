namespace FerreAPI.Productos;

    
public record CrearProductoRequest(
string Codigo, string Nombre, string? Descripcion, decimal PrecioVenta,
int Stock, int StockMinimo, string Categoria)
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
        if (string.IsNullOrWhiteSpace(Categoria))
            errores["categoria"] = ["La categoría es obligatoria."];

        return errores;
    }
}
public record ProductoResponse(
    int Id, string Codigo, string Nombre, string? Descripcion, decimal PrecioVenta,
    int Stock, int StockMinimo, string Categoria, bool Activo);

public static class ProductoMapeos
{
    public static ProductoResponse ToResponse(this Producto p) =>
        new(p.Id, p.Codigo, p.Nombre, p.Descripcion, p.PrecioVenta,
            p.Stock, p.StockMinimo, p.Categoria, p.Activo);
    public static Producto ToEntidad(this CrearProductoRequest req) => new()
    {
        Codigo = req.Codigo.Trim(),
        Nombre = req.Nombre.Trim(),
        Descripcion = req.Descripcion?.Trim(),
        PrecioVenta = req.PrecioVenta,
        Stock = req.Stock,
        StockMinimo = req.StockMinimo,
        Categoria = req.Categoria.Trim()
    };
}


