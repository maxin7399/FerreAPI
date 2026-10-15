namespace FerreAPI.Productos;

public static class ProductoConsultas
{
    public static IQueryable<Producto> Activos(this IQueryable<Producto> query) =>
        query.Where(p => p.Activo);
}