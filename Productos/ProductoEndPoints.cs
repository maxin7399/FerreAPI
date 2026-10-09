using FerreAPI.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
    private static async Task<Ok<List<ProductoResponse>>> ObtenerTodos(
    FerreDbContext db, string? busqueda, int? categoriaId)
    {
        var query = db.Productos.AsNoTracking().Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(p =>
                EF.Functions.ILike(p.Nombre, $"%{busqueda}%") ||
                EF.Functions.ILike(p.Codigo, $"%{busqueda}%"));

        if (categoriaId is not null)
            query = query.Where(p => p.CategoriaId == categoriaId);

        var productos = await query.ToListAsync();
        return TypedResults.Ok(productos.Select(p => p.ToResponse()).ToList());
    }

    private static async Task<Results<Ok<ProductoResponse>, NotFound>> ObtenerPorId(
        int id, FerreDbContext db)
    {
        var producto = await db.Productos.FindAsync(id);
        if (producto is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(producto.ToResponse());
    }
    private static async Task<Results<Created<ProductoResponse>, ValidationProblem, ProblemHttpResult>> CrearProducto(
    CrearProductoRequest req, FerreDbContext db)
    {
        var errores = req.Validar();
        if (errores.Count > 0)
            return TypedResults.ValidationProblem(errores);

        var producto = req.ToEntidad();

        if (await db.Productos.AnyAsync(p => p.Codigo == producto.Codigo))
            return CodigoDuplicado(producto.Codigo);

        db.Productos.Add(producto);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return CodigoDuplicado(producto.Codigo);
        }

        return TypedResults.Created($"/api/productos/{producto.Id}", producto.ToResponse());
    }

    private static ProblemHttpResult CodigoDuplicado(string codigo) =>
        TypedResults.Problem(
            title: "Código duplicado",
            detail: $"Ya existe un producto con el código '{codigo}'.",
            statusCode: StatusCodes.Status409Conflict);
    private static async Task<Results<NoContent, ValidationProblem, NotFound>> ActualizarProducto(
        int id, ActualizarProductoRequest req, FerreDbContext db)
    {
        var errores = req.Validar();
        if (errores.Count > 0)
            return TypedResults.ValidationProblem(errores);

        var producto = await db.Productos.FindAsync(id);
        if (producto is null)
            return TypedResults.NotFound();

        producto.AplicarCambios(req);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }
    private static async Task<Results<NoContent, NotFound>> EliminarProducto(int id, FerreDbContext db)
    {
        var producto = await db.Productos.FindAsync(id);

        if (producto is null)
            return TypedResults.NotFound();

        producto.Desactivar();
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}