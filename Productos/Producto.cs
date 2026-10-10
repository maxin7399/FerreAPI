using FerreAPI.Categorias;

namespace FerreAPI.Productos;

public class Producto
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal PrecioVenta { get; set; }
    public int Stock { get; private set; }
    public int StockMinimo { get; set; }
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public bool Activo { get; set; } = true;
    public void Desactivar() => Activo = false;

    public void AumentarStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser positiva.");
        Stock += cantidad;
    }

    public void DescontarStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser positiva.");
        if (cantidad > Stock)
            throw new InvalidOperationException($"Stock insuficiente para '{Nombre}'. Disponible: {Stock}.");
        Stock -= cantidad;
    }
}