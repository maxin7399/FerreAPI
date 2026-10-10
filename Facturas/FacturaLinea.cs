using FerreAPI.Productos;

namespace FerreAPI.Facturas;

public class FacturaLinea
{
    public int Id { get; private set; }
    public int FacturaId { get; private set; }
    public int ProductoId { get; private set; }
    public Producto? Producto { get; private set; }
    public int Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal PorcentajeIva { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Iva { get; private set; }

    private FacturaLinea() { }   // lo usa EF Core al cargar desde la base de datos

    internal FacturaLinea(Producto producto, int cantidad, decimal porcentajeIva)
    {
        Producto = producto;
        ProductoId = producto.Id;
        Cantidad = cantidad;
        PrecioUnitario = producto.PrecioVenta;   // snapshot del precio actual
        PorcentajeIva = porcentajeIva;
        Subtotal = cantidad * PrecioUnitario;
        Iva = Math.Round(Subtotal * porcentajeIva / 100, 2, MidpointRounding.AwayFromZero);
    }
}