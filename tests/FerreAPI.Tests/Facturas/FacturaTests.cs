using FerreAPI.Comun;
using FerreAPI.Facturas;
using FerreAPI.Productos;

namespace FerreAPI.Tests.Facturas;

public class FacturaTests
{
    private const int UsuarioId = 1;

    private static Producto CrearProducto(decimal precio, int stock = 100)
    {
        var producto = new Producto
        {
            Codigo = "TST-001",
            Nombre = "Producto de prueba",
            PrecioVenta = precio,
            StockMinimo = 1,
            CategoriaId = 1
        };
        producto.AumentarStock(stock);
        return producto;
    }

    [Fact]
    public void AgregarLinea_CalculaSubtotalIvaYTotal()
    {
        var factura = new Factura(UsuarioId);
        var producto = CrearProducto(precio: 10000m);

        factura.AgregarLinea(producto, 2);

        Assert.Equal(20000m, factura.Subtotal);
        Assert.Equal(3800m, factura.Iva);
        Assert.Equal(23800m, factura.Total);
    }

    [Fact]
    public void AgregarLinea_VariasLineas_SumaLosTotales()
    {
        var factura = new Factura(UsuarioId);

        factura.AgregarLinea(CrearProducto(precio: 10000m), 1);
        factura.AgregarLinea(CrearProducto(precio: 5000m), 2);

        Assert.Equal(2, factura.Lineas.Count);
        Assert.Equal(20000m, factura.Subtotal);
        Assert.Equal(3800m, factura.Iva);
        Assert.Equal(23800m, factura.Total);
    }

    [Fact]
    public void AgregarLinea_DescuentaElStockDelProducto()
    {
        var factura = new Factura(UsuarioId);
        var producto = CrearProducto(precio: 10000m, stock: 10);

        factura.AgregarLinea(producto, 4);

        Assert.Equal(6, producto.Stock);
    }

    [Fact]
    public void AgregarLinea_CambioDePrecioPosterior_NoAfectaLaLinea()
    {
        var factura = new Factura(UsuarioId);
        var producto = CrearProducto(precio: 10000m);
        factura.AgregarLinea(producto, 1);

        producto.PrecioVenta = 15000m;

        var linea = Assert.Single(factura.Lineas);
        Assert.Equal(10000m, linea.PrecioUnitario);
        Assert.Equal(10000m, factura.Subtotal);
    }

    [Fact]
    public void AgregarLinea_RedondeaElIvaAlejandoseDelCero()
    {
        // 1,50 × 19 % = 0,285. El redondeo bancario daría 0,28; en facturación debe ser 0,29.
        var factura = new Factura(UsuarioId);

        factura.AgregarLinea(CrearProducto(precio: 1.50m), 1);

        Assert.Equal(0.29m, factura.Iva);
    }

    [Fact]
    public void AgregarLinea_ProductoInactivo_LanzaExcepcionYNoAgregaLaLinea()
    {
        var factura = new Factura(UsuarioId);
        var producto = CrearProducto(precio: 10000m, stock: 10);
        producto.Desactivar();

        Assert.Throws<ReglaNegocioException>(() => factura.AgregarLinea(producto, 1));
        Assert.Empty(factura.Lineas);
        Assert.Equal(10, producto.Stock);
    }
}