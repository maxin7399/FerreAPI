using Microsoft.AspNetCore.Http.HttpResults;

namespace FerreAPI.Productos;

public static class ProductoEndpoints
{
    public static IEndpointRouteBuilder MapProductos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/productos").WithTags("Productos");

        grupo.MapGet("/", ObtenerTodos);
        grupo.MapGet("/{id:int}", ObtenerPorId);
        grupo.MapPost("/", CrearProducto);
        grupo.MapPut("/{id:int}", ActualizarProducto);
        grupo.MapDelete("/{id:int}", EliminarProducto);

        return app;
    }
    private static Ok<List<ProductoResponse>> ObtenerTodos(
    ProductoStore store, string? busqueda, string? categoria)
    {
        var productos = store.ObtenerTodos().Where(p => p.Activo);

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
    private static Results<Created<ProductoResponse>, ValidationProblem, ProblemHttpResult> CrearProducto(
    CrearProductoRequest req, ProductoStore store)
    {
        var errores = req.Validar();
        if (errores.Count > 0)
            return TypedResults.ValidationProblem(errores);

        // TODO: race condition, se resuelve con índice único en la semana 4
        if (store.ExisteCodigo(req.Codigo))
            return TypedResults.Problem(
                title: "Código duplicado",
                detail: $"Ya existe un producto con el código '{req.Codigo}'.",
                statusCode: StatusCodes.Status409Conflict);

        var producto = store.Agregar(req.ToEntidad());
        return TypedResults.Created($"/api/productos/{producto.Id}", producto.ToResponse());
    }
    private static Results<NoContent, ValidationProblem, NotFound> ActualizarProducto(
        int id, ActualizarProductoRequest req, ProductoStore store)
    {
        var errores = req.Validar();
        if (errores.Count > 0)
            return TypedResults.ValidationProblem(errores);
        var producto = store.ObtenerPorId(id);
        if (producto is null)
            return TypedResults.NotFound();
        producto.AplicarCambios(req);
        return TypedResults.NoContent();
    }
    private static Results<NoContent, NotFound> EliminarProducto(int id, ProductoStore store)
    {
        var producto = store.ObtenerPorId(id);

        if (producto is null)
            return TypedResults.NotFound();
        producto.Desactivar();
        return TypedResults.NoContent();
    }
}