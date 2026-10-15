using System.Net.Mail;
using System.ComponentModel.DataAnnotations;

namespace FerreAPI.Usuarios;

public record CrearUsuarioRequest(
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
    string Email,

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    string Nombre,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    string Password,

    [Required(ErrorMessage = "El rol es obligatorio.")]
    string Rol) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.TryParse<Rol>(Rol, ignoreCase: true, out _))
            yield return new ValidationResult(
                "El rol debe ser 'Vendedor' o 'Administrador'.", [nameof(Rol)]);
    }
}

public record UsuarioResponse(int Id, string Email, string Nombre, string Rol, bool Activo);

public static class UsuarioMapeos
{
    public static UsuarioResponse ToResponse(this Usuario u) =>
        new(u.Id, u.Email, u.Nombre, u.Rol.ToString(), u.Activo);
}