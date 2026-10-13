using FerreAPI.Comun;
using FerreAPI.Productos;

namespace FerreAPI.Tests.Productos;

public class ProductoTests
{
    private static Producto CrearProducto(int stock = 10)
    {
        var producto = new Producto
        {
            Codigo = "TST-001",
            Nombre = "Producto de prueba",
            PrecioVenta = 10000m,
            StockMinimo = 1,
            CategoriaId = 1
        };
        if (stock > 0)
            producto.AumentarStock(stock);
        return producto;
    }

    [Fact]
    public void DescontarStock_ConStockSuficiente_ReduceElStock()
    {
        // Arrange
        var producto = CrearProducto(stock: 10);

        // Act
        producto.DescontarStock(3);

        // Assert
        Assert.Equal(7, producto.Stock);
    }

    [Fact]
    public void DescontarStock_MayorAlDisponible_LanzaExcepcionYNoModificaElStock()
    {
        var producto = CrearProducto(stock: 5);

        Assert.Throws<ReglaNegocioException>(() => producto.DescontarStock(6));
        Assert.Equal(5, producto.Stock);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DescontarStock_CantidadNoPositiva_LanzaExcepcion(int cantidad)
    {
        var producto = CrearProducto();

        Assert.Throws<ArgumentOutOfRangeException>(() => producto.DescontarStock(cantidad));
    }
}