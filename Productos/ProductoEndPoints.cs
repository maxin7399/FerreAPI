using Microsoft.AspNetCore.Http.HttpResults;

namespace FerreAPI.Productos;

public static class ProductoEndpoints
{
    public static IEndpointRouteBuilder MapProductos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/productos").WithTags("Productos");

        grupo.MapGet("/", ObtenerTodos);
        grupo.MapGet("/{id:int}", ObtenerPorId);

        return app;
    }
    private static Ok<List<ProductoResponse>> ObtenerTodos(
    ProductoStore store, string? busqueda, string? categoria)
    {
        var productos = store.ObtenerTodos();

        if (!string.IsNullOrWhiteSpace(busqueda))
            productos = productos.Where(p =>
                p.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                p.Codigo.Contains(busqueda, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(categoria))
            productos = productos.Where(p =>
                p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase));

        return TypedResults.Ok(productos.Select(p => p.ToResponse()).ToList());
    }

    private static Results<Ok<ProductoResponse>, NotFound> ObtenerPorId(int id, ProductoStore store)
    {
        var producto = store.ObtenerPorId(id);

        if (producto is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(producto.ToResponse());
    }
}