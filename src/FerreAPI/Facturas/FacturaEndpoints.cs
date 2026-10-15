using FerreAPI.Auth;
using FerreAPI.Comun;
using FerreAPI.Data;
using FerreAPI.Usuarios;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FerreAPI.Facturas;

public static class FacturaEndpoints
{
    public static IEndpointRouteBuilder MapFacturas(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/facturas").WithTags("Facturas");

        grupo.MapGet("/", ObtenerTodas);
        grupo.MapGet("/{id:int}", ObtenerPorId);
        grupo.MapPost("/", CrearFactura);

        return app;
    }

    private static async Task<Results<Ok<PaginaResponse<FacturaResumenResponse>>, ValidationProblem>> ObtenerTodas(
    ClaimsPrincipal usuarioActual, FerreDbContext db, int pagina = 1, int tamanio = 20)
    {
        var errores = new Dictionary<string, string[]>();
        if (pagina < 1)
            errores["pagina"] = ["La página debe ser mayor o igual a 1."];
        if (tamanio < 1 || tamanio > 100)
            errores["tamanio"] = ["El tamaño debe estar entre 1 y 100."];
        if (errores.Count > 0)
            return TypedResults.ValidationProblem(errores);

        var query = db.Facturas.AsQueryable();

        if (!usuarioActual.IsInRole(nameof(Rol.Administrador)))
        {
            var usuarioId = usuarioActual.ObtenerUsuarioId();
            query = query.Where(f => f.UsuarioId == usuarioId);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(f => f.Fecha)
            .ThenByDescending(f => f.Id)
            .Skip((pagina - 1) * tamanio)
            .Take(tamanio)
            .Select(f => new FacturaResumenResponse(
                f.Id, f.Fecha, f.Usuario!.Nombre,
                f.Subtotal, f.Iva, f.Total, f.Lineas.Count))
            .ToListAsync();

        return TypedResults.Ok(new PaginaResponse<FacturaResumenResponse>(items, pagina, tamanio, total));
    }

    private static async Task<Results<Ok<FacturaResponse>, NotFound>> ObtenerPorId(
    int id, ClaimsPrincipal usuarioActual, FerreDbContext db)
    {
        var query = db.Facturas
            .AsNoTracking()
            .Include(f => f.Usuario)
            .Include(f => f.Lineas)
                .ThenInclude(l => l.Producto)
            .Where(f => f.Id == id);

        if (!usuarioActual.IsInRole(nameof(Rol.Administrador)))
        {
            var usuarioId = usuarioActual.ObtenerUsuarioId();
            query = query.Where(f => f.UsuarioId == usuarioId);
        }

        var factura = await query.FirstOrDefaultAsync();
        if (factura is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(factura.ToResponse());
    }

    private static async Task<Results<Created<FacturaResponse>, ValidationProblem, ProblemHttpResult>> CrearFactura(
        CrearFacturaRequest req, ClaimsPrincipal usuarioActual, FerreDbContext db)
    {

        // 2. Cargar todos los productos en una sola consulta
        var ids = req.Lineas.Select(l => l.ProductoId).ToList();
        var productos = await db.Productos
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        // 3. Verificar existencia, disponibilidad y stock
        var errores = new Dictionary<string, string[]>();
        for (var i = 0; i < req.Lineas.Count; i++)
        {
            var linea = req.Lineas[i];
            if (!productos.TryGetValue(linea.ProductoId, out var producto) || !producto.Activo)
                errores[$"lineas[{i}].productoId"] = ["El producto no existe o no está disponible."];
            else if (linea.Cantidad > producto.Stock)
                errores[$"lineas[{i}].cantidad"] = [$"Stock insuficiente. Disponible: {producto.Stock}."];
        }
        if (errores.Count > 0)
            return TypedResults.ValidationProblem(errores);

        // 4. Armar la factura: cada línea descuenta stock y suma totales
        var factura = new Factura(usuarioActual.ObtenerUsuarioId());
        foreach (var linea in req.Lineas)
            factura.AgregarLinea(productos[linea.ProductoId], linea.Cantidad);

        db.Facturas.Add(factura);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return TypedResults.Problem(
                title: "Inventario modificado",
                detail: "El stock de uno o más productos cambió mientras se procesaba la venta. Intente de nuevo.",
                statusCode: StatusCodes.Status409Conflict);
        }

        await db.Entry(factura).Reference(f => f.Usuario).LoadAsync();

        return TypedResults.Created($"/api/facturas/{factura.Id}", factura.ToResponse());
    }
}