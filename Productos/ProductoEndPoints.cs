using FerreAPI.Comun;
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
    private static async Task<Results<Ok<PaginaResponse<ProductoResponse>>, ValidationProblem>> ObtenerTodos(
    FerreDbContext db, string? busqueda, int? categoriaId, int pagina = 1, int tamanio = 20)
    {
        var errores = new Dictionary<string, string[]>();
        if (pagina < 1)
            errores["pagina"] = ["La página debe ser mayor o igual a 1."];
        if (tamanio < 1 || tamanio > 100)
            errores["tamanio"] = ["El tamaño debe estar entre 1 y 100."];
        if (errores.Count > 0)
            return TypedResults.ValidationProblem(errores);

        var query = db.Productos
            .Include(p => p.Categoria)
            .AsNoTracking()
            .Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(p =>
                EF.Functions.ILike(p.Nombre, $"%{busqueda}%") ||
                EF.Functions.ILike(p.Codigo, $"%{busqueda}%"));

        if (categoriaId is not null)
            query = query.Where(p => p.CategoriaId == categoriaId);

        var total = await query.CountAsync();

        var productos = await query
            .OrderBy(p => p.Nombre)
            .ThenBy(p => p.Id)
            .Skip((pagina - 1) * tamanio)
            .Take(tamanio)
            .ToListAsync();

        var items = productos.Select(p => p.ToResponse()).ToList();
        return TypedResults.Ok(new PaginaResponse<ProductoResponse>(items, pagina, tamanio, total));
    }

    private static async Task<Results<Ok<ProductoResponse>, NotFound>> ObtenerPorId(
        int id, FerreDbContext db)
    {
        var producto = await db.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

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

        var categoria = await db.Categorias.FindAsync(req.CategoriaId);
        if (categoria is null)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["categoriaId"] = ["La categoría indicada no existe."]
            });

        var producto = req.ToEntidad();
        producto.Categoria = categoria;

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

        var categoria = await db.Categorias.FindAsync(req.CategoriaId);
        if (categoria is null)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["categoriaId"] = ["La categoría indicada no existe."]
            });

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