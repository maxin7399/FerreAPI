using FerreAPI.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FerreAPI.Categorias;

public static class CategoriaEndpoints
{
    public static IEndpointRouteBuilder MapCategorias(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/categorias").WithTags("Categorias");

        grupo.MapGet("/", ObtenerTodos);
        grupo.MapGet("/{id:int}", ObtenerPorId);
        grupo.MapPost("/", CrearCategoria);

        return app;
    }

    private static async Task<Ok<List<CategoriaResponse>>> ObtenerTodos(
        FerreDbContext db, string? busqueda)
    {
        var query = db.Categorias.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(c =>
                EF.Functions.ILike(c.Nombre, $"%{busqueda}%"));

        var categorias = await query.OrderBy(c => c.Nombre).ToListAsync();
        return TypedResults.Ok(categorias.Select(c => c.ToResponse()).ToList());
    }

    private static async Task<Results<Ok<CategoriaResponse>, NotFound>> ObtenerPorId(
        int id, FerreDbContext db)
    {
        var categoria = await db.Categorias.FindAsync(id);
        if (categoria is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(categoria.ToResponse());
    }

    private static async Task<Results<Created<CategoriaResponse>, ValidationProblem, ProblemHttpResult>> CrearCategoria(
        CrearCategoriaRequest req, FerreDbContext db)
    {
        var errores = req.Validar();
        if (errores.Count > 0)
            return TypedResults.ValidationProblem(errores);

        var categoria = req.ToEntidad();

        if (await db.Categorias.AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.ToLower()))
            return NombreDuplicado(categoria.Nombre);

        db.Categorias.Add(categoria);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return NombreDuplicado(categoria.Nombre);
        }

        return TypedResults.Created($"/api/categorias/{categoria.Id}", categoria.ToResponse());
    }

    private static ProblemHttpResult NombreDuplicado(string nombre) =>
        TypedResults.Problem(
            title: "Nombre duplicado",
            detail: $"Ya existe una categoría con el nombre '{nombre}'.",
            statusCode: StatusCodes.Status409Conflict);
}