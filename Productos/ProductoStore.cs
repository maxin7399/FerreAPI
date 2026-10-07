using System.Collections.Concurrent;

namespace FerreAPI.Productos;

public class ProductoStore
{
    private readonly ConcurrentDictionary<int, Producto> _productos = new();
    private int _ultimoId;

    public ProductoStore()
    {
        Agregar(new Producto { Codigo = "HER-001", Nombre = "Martillo de uña 16 oz", PrecioVenta = 28000m, Stock = 15, StockMinimo = 5, Categoria = "Herramientas" });
        Agregar(new Producto { Codigo = "HER-002", Nombre = "Destornillador", PrecioVenta = 2000m, Stock = 15, StockMinimo = 5, Categoria = "Herramientas" });
        Agregar(new Producto { Codigo = "PIN-001", Nombre = "Pintura verde", PrecioVenta = 10000m, Stock = 10, StockMinimo = 5, Categoria = "Pinturas" });
    }
    public IEnumerable<Producto> ObtenerTodos() => _productos.Values;
    public Producto? ObtenerPorId(int id) => _productos.TryGetValue(id, out var producto) ? producto : null;
    public Producto Agregar(Producto producto)
    {
        producto.Id = Interlocked.Increment(ref _ultimoId);
        _productos[producto.Id] = producto;
        return producto;
    }
    public bool ExisteCodigo(string codigo) =>
    _productos.Values.Any(p => p.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
}