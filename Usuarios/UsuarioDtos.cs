using System.Net.Mail;

namespace FerreAPI.Usuarios;

public record CrearUsuarioRequest(string Email, string Nombre, string Password, string Rol)
{
    public Dictionary<string, string[]> Validar()
    {
        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(Email) || !MailAddress.TryCreate(Email.Trim(), out _))
            errores["email"] = ["El email no es válido."];
        else if (Email.Trim().Length > 150)
            errores["email"] = ["El email no puede superar los 150 caracteres."];

        if (string.IsNullOrWhiteSpace(Nombre))
            errores["nombre"] = ["El nombre es obligatorio."];
        else if (Nombre.Trim().Length > 100)
            errores["nombre"] = ["El nombre no puede superar los 100 caracteres."];

        if (string.IsNullOrEmpty(Password) || Password.Length < 8)
            errores["password"] = ["La contraseña debe tener al menos 8 caracteres."];

        if (!Enum.TryParse<Rol>(Rol, ignoreCase: true, out _))
            errores["rol"] = ["El rol debe ser 'Vendedor' o 'Administrador'."];

        return errores;
    }
}

public record UsuarioResponse(int Id, string Email, string Nombre, string Rol, bool Activo);

public static class UsuarioMapeos
{
    public static UsuarioResponse ToResponse(this Usuario u) =>
        new(u.Id, u.Email, u.Nombre, u.Rol.ToString(), u.Activo);
}