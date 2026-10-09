namespace FerreAPI.Categorias;

public record CrearCategoriaRequest(string Nombre)
{
    public Dictionary<string, string[]> Validar()
    {
        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(Nombre))
            errores["nombre"] = ["El nombre es obligatorio."];
        else if (Nombre.Trim().Length > 60)
            errores["nombre"] = ["El nombre no puede superar los 60 caracteres."];

        return errores;
    }
}

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