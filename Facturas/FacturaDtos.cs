namespace FerreAPI.Facturas;

public record LineaRequest(int ProductoId, int Cantidad);

public record CrearFacturaRequest(List<LineaRequest> Lineas)
{
    public Dictionary<string, string[]> Validar()
    {
        var errores = new Dictionary<string, string[]>();

        if (Lineas is null || Lineas.Count == 0)
        {
            errores["lineas"] = ["La factura debe tener al menos una línea."];
            return errores;
        }

        for (var i = 0; i < Lineas.Count; i++)
        {
            if (Lineas[i].ProductoId <= 0)
                errores[$"lineas[{i}].productoId"] = ["El producto es obligatorio."];
            if (Lineas[i].Cantidad <= 0)
                errores[$"lineas[{i}].cantidad"] = ["La cantidad debe ser mayor que 0."];
        }

        if (Lineas.GroupBy(l => l.ProductoId).Any(g => g.Count() > 1))
            errores["lineas"] = ["Un producto no puede aparecer en más de una línea."];

        return errores;
    }
}

public record FacturaLineaResponse(
    int ProductoId, string NombreProducto, int Cantidad,
    decimal PrecioUnitario, decimal PorcentajeIva, decimal Subtotal, decimal Iva);

public record FacturaResponse(
    int Id, DateTime Fecha, decimal Subtotal, decimal Iva, decimal Total,
    List<FacturaLineaResponse> Lineas);

public static class FacturaMapeos
{
    public static FacturaResponse ToResponse(this Factura f) =>
        new(f.Id, f.Fecha, f.Subtotal, f.Iva, f.Total,
            f.Lineas.Select(l => new FacturaLineaResponse(
                l.ProductoId, l.Producto!.Nombre, l.Cantidad,
                l.PrecioUnitario, l.PorcentajeIva, l.Subtotal, l.Iva)).ToList());
}