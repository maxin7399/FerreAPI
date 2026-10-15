namespace FerreAPI.Facturas;

internal static partial class FacturaLogs
{
    [LoggerMessage(EventId = 2001, Level = LogLevel.Information,
        Message = "Factura {FacturaId} creada por el usuario {UsuarioId}: {CantidadLineas} líneas, total {Total}")]
    public static partial void FacturaCreada(
        this ILogger logger, int facturaId, int usuarioId, int cantidadLineas, decimal total);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning,
        Message = "Conflicto de inventario al facturar; el usuario {UsuarioId} debe reintentar")]
    public static partial void ConflictoInventario(this ILogger logger, int usuarioId);
}