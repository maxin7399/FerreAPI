using System.Net;
using System.Net.Http.Json;
using FerreAPI.Facturas;
using FerreAPI.Productos;

namespace FerreAPI.Tests.Integracion;

[Collection(ApiCollection.Nombre)]
public class FacturasApiTests(ApiFactory factory)
{
    [Fact]
    public async Task CrearFactura_StockInsuficiente_Devuelve400YNoDescuentaStock()
    {
        var admin = await factory.ClienteAdminAsync();
        var producto = await admin.CrearProductoAsync(stock: 3);

        var respuesta = await admin.PostAsJsonAsync("/api/facturas",
            new { lineas = new[] { new { productoId = producto.Id, cantidad = 5 } } });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var actual = await admin.GetFromJsonAsync<ProductoResponse>($"/api/productos/{producto.Id}");
        Assert.Equal(3, actual!.Stock);
    }

    [Fact]
    public async Task ObtenerFactura_DeOtroUsuario_Devuelve404()
    {
        var admin = await factory.ClienteAdminAsync();
        var producto = await admin.CrearProductoAsync(stock: 10);
        var creada = await admin.PostAsJsonAsync("/api/facturas",
            new { lineas = new[] { new { productoId = producto.Id, cantidad = 1 } } });
        var factura = await creada.Content.ReadFromJsonAsync<FacturaResponse>();

        var vendedor = await factory.ClienteVendedorAsync();
        var respuesta = await vendedor.GetAsync($"/api/facturas/{factura!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task CrearFactura_VentasSimultaneas_NuncaVendeMasDelStockDisponible()
    {
        var admin = await factory.ClienteAdminAsync();
        var producto = await admin.CrearProductoAsync(stock: 5);
        var venta = new { lineas = new[] { new { productoId = producto.Id, cantidad = 4 } } };

        var respuestas = await Task.WhenAll(
            Enumerable.Range(0, 5).Select(_ => admin.PostAsJsonAsync("/api/facturas", venta)));

        Assert.Single(respuestas, r => r.StatusCode == HttpStatusCode.Created);
        Assert.All(respuestas.Where(r => r.StatusCode != HttpStatusCode.Created),
            r => Assert.Contains(r.StatusCode, new[] { HttpStatusCode.Conflict, HttpStatusCode.BadRequest }));

        var actual = await admin.GetFromJsonAsync<ProductoResponse>($"/api/productos/{producto.Id}");
        Assert.Equal(1, actual!.Stock);
    }
}