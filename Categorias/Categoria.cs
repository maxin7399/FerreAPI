using FerreAPI.Productos;

namespace FerreAPI.Categorias;

public class Categoria
{
    public int Id { get; set; }
    public required string Nombre { get; set; }

    public List<Producto> Productos { get; set; } = [];
}