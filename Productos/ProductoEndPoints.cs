namespace FerreAPI.Productos;

public static class ProductoEndpoints
{
    public static IEndpointRouteBuilder MapProductos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/productos").WithTags("Productos");

        // grupo.MapGet("/", ObtenerTodos);
        // grupo.MapGet("/{id:int}", ObtenerPorId);
        // ... los demás

        return app;
    }
}