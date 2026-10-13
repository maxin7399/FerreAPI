using FerreAPI.Comun;
using FerreAPI.Productos;
using FerreAPI.Usuarios;

namespace FerreAPI.Facturas;

public class Factura
{
    public const decimal TarifaIvaGeneral = 19m;

    private readonly List<FacturaLinea> _lineas = [];

    public int Id { get; private set; }
    public DateTime Fecha { get; private set; } = DateTime.UtcNow;
    public decimal Subtotal { get; private set; }
    public decimal Iva { get; private set; }
    public decimal Total { get; private set; }
    public IReadOnlyCollection<FacturaLinea> Lineas => _lineas;
    public int UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    private Factura() { }

    public void AgregarLinea(Producto producto, int cantidad)
    {
        if (!producto.Activo)
            throw new ReglaNegocioException($"El producto '{producto.Nombre}' no está disponible para la venta.");

        producto.DescontarStock(cantidad);

        var linea = new FacturaLinea(producto, cantidad, TarifaIvaGeneral);
        _lineas.Add(linea);

        Subtotal += linea.Subtotal;
        Iva += linea.Iva;
        Total = Subtotal + Iva;
    }
    public Factura(int usuarioId)
    {
        UsuarioId = usuarioId;
    }
}