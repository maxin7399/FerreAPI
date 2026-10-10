namespace FerreAPI.Comun;

public record PaginaResponse<T>(List<T> Items, int Pagina, int Tamanio, int Total)
{
    public int TotalPaginas => (int)Math.Ceiling(Total / (double)Tamanio);
}