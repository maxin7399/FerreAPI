using System.Net;
using System.Net.Http.Json;

namespace FerreAPI.Tests.Integracion;

[Collection(ApiCollection.Nombre)]
public class ProductosApiTests(ApiFactory factory)
{
    [Fact]
    public async Task ListarProductos_SinToken_Devuelve401()
    {
        var cliente = factory.CreateClient();

        var respuesta = await cliente.GetAsync("/api/productos");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task CrearProducto_ComoVendedor_Devuelve403()
    {
        var vendedor = await factory.ClienteVendedorAsync();

        var respuesta = await vendedor.PostAsJsonAsync("/api/productos", new
        {
            codigo = "X-001",
            nombre = "No permitido",
            precioVenta = 1000,
            stock = 1,
            stockMinimo = 0,
            categoriaId = 1
        });

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task CrearProducto_CodigoDuplicadoConOtrasMayusculas_Devuelve409()
    {
        var admin = await factory.ClienteAdminAsync();
        var existente = await admin.CrearProductoAsync(stock: 1);

        var respuesta = await admin.PostAsJsonAsync("/api/productos", new
        {
            codigo = existente.Codigo.ToLowerInvariant(),
            nombre = "Otro producto",
            precioVenta = 1000,
            stock = 1,
            stockMinimo = 0,
            categoriaId = existente.CategoriaId
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }
}