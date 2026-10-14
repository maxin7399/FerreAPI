using System.Net.Http.Headers;
using System.Net.Http.Json;
using FerreAPI.Auth;
using FerreAPI.Categorias;
using FerreAPI.Productos;

namespace FerreAPI.Tests.Integracion;

public static class ApiHelpers
{
    public static async Task<HttpClient> ClienteAutenticadoAsync(
        this ApiFactory factory, string email, string password)
    {
        var cliente = factory.CreateClient();
        var respuesta = await cliente.PostAsJsonAsync("/api/auth/login", new { email, password });
        respuesta.EnsureSuccessStatusCode();

        var login = await respuesta.Content.ReadFromJsonAsync<LoginResponse>();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
        return cliente;
    }

    public static Task<HttpClient> ClienteAdminAsync(this ApiFactory factory) =>
        factory.ClienteAutenticadoAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);

    public static async Task<HttpClient> ClienteVendedorAsync(this ApiFactory factory)
    {
        var admin = await factory.ClienteAdminAsync();
        var email = $"vendedor-{Guid.NewGuid():N}@test.local";
        const string password = "Vendedor123!";

        var respuesta = await admin.PostAsJsonAsync("/api/usuarios",
            new { email, nombre = "Vendedor de prueba", password, rol = "Vendedor" });
        respuesta.EnsureSuccessStatusCode();

        return await factory.ClienteAutenticadoAsync(email, password);
    }

    public static async Task<ProductoResponse> CrearProductoAsync(
        this HttpClient admin, int stock, decimal precio = 10000m)
    {
        var respCategoria = await admin.PostAsJsonAsync("/api/categorias",
            new { nombre = $"Cat-{Guid.NewGuid():N}" });
        respCategoria.EnsureSuccessStatusCode();
        var categoria = await respCategoria.Content.ReadFromJsonAsync<CategoriaResponse>();

        var respProducto = await admin.PostAsJsonAsync("/api/productos", new
        {
            codigo = $"T-{Guid.NewGuid():N}"[..14],
            nombre = "Producto de prueba",
            precioVenta = precio,
            stock,
            stockMinimo = 0,
            categoriaId = categoria!.Id
        });
        respProducto.EnsureSuccessStatusCode();
        return (await respProducto.Content.ReadFromJsonAsync<ProductoResponse>())!;
    }
}