using System.ComponentModel.DataAnnotations;

namespace FerreAPI.Facturas;

public record LineaRequest(
    [Range(1, int.MaxValue, ErrorMessage = "El producto es obligatorio.")]
    int ProductoId,

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 0.")]
    int Cantidad);

public record CrearFacturaRequest(
    [Required(ErrorMessage = "La factura debe tener líneas.")]
    [MinLength(1, ErrorMessage = "La factura debe tener al menos una línea.")]
    List<LineaRequest> Lineas) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Lineas.GroupBy(l => l.ProductoId).Any(g => g.Count() > 1))
            yield return new ValidationResult(
                "Un producto no puede aparecer en más de una línea.", [nameof(Lineas)]);
    }
}

public record FacturaLineaResponse(
    int ProductoId, string NombreProducto, int Cantidad,
    decimal PrecioUnitario, decimal PorcentajeIva, decimal Subtotal, decimal Iva);

public record FacturaResponse(
    int Id, DateTime Fecha, int VendedorId, string VendedorNombre,
    decimal Subtotal, decimal Iva, decimal Total,
    List<FacturaLineaResponse> Lineas);

public record FacturaResumenResponse(
    int Id, DateTime Fecha, string VendedorNombre,
    decimal Subtotal, decimal Iva, decimal Total, int CantidadLineas);


public static class FacturaMapeos
{
    public static FacturaResponse ToResponse(this Factura f) =>
        new(f.Id, f.Fecha, f.UsuarioId, f.Usuario!.Nombre,
            f.Subtotal, f.Iva, f.Total,
            f.Lineas.Select(l => new FacturaLineaResponse(
                l.ProductoId, l.Producto!.Nombre, l.Cantidad,
                l.PrecioUnitario, l.PorcentajeIva, l.Subtotal, l.Iva)).ToList());
}