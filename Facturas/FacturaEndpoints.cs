using FerreAPI.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace FerreAPI.Facturas;

public static class FacturaEndpoints
{
    public static IEndpointRouteBuilder MapFacturas(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/facturas").WithTags("Facturas");

        grupo.MapPost("/", CrearFactura);

        return app;
    }
    private static async Task<Results<Created<FacturaResponse>, ValidationProblem, ProblemHttpResult>> CrearFactura(
    CrearFacturaRequest req, FerreDbContext db)
    {
        // 1. Validar la petición
        var errores = req.Validar();
        if (errores.Count > 0)
            return TypedResults.ValidationProblem(errores);

        // 2. Cargar todos los productos en una sola consulta
        var ids = req.Lineas.Select(l => l.ProductoId).ToList();
        var productos = await db.Productos
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        // 3. Verificar existencia, disponibilidad y stock
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
        var factura = new Factura();
        foreach (var linea in req.Lineas)
            factura.AgregarLinea(productos[linea.ProductoId], linea.Cantidad);

        // 5. Guardar todo en una sola transacción
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

        return TypedResults.Created($"/api/facturas/{factura.Id}", factura.ToResponse());
    }

}

