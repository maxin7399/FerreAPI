namespace FerreAPI.Usuarios;

public class Usuario
{
    public int Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public Rol Rol { get; private set; }
    public bool Activo { get; private set; } = true;

    private Usuario() { }   // para EF Core

    public Usuario(string email, string nombre, Rol rol)
    {
        Email = email.Trim().ToLowerInvariant();
        Nombre = nombre.Trim();
        Rol = rol;
    }

    public void EstablecerPasswordHash(string hash) => PasswordHash = hash;
    public void Desactivar() => Activo = false;
}