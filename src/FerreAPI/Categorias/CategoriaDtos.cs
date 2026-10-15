namespace FerreAPI.Categorias;
using System.ComponentModel.DataAnnotations;

public record CrearCategoriaRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60, ErrorMessage = "El nombre no puede superar los 60 caracteres.")]
    string Nombre);

public record CategoriaResponse(int Id, string Nombre);

public static class CategoriaMapeos
{
    public static CategoriaResponse ToResponse(this Categoria c) =>
        new(c.Id, c.Nombre);
    public static Categoria ToEntidad(this CrearCategoriaRequest req) => new()
    {
        Nombre = req.Nombre.Trim()
    };
}