using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FerreAPI.Comun;

public sealed class ManejadorGlobalExcepciones(
    IProblemDetailsService problemDetailsService,
    ILogger<ManejadorGlobalExcepciones> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problema = exception switch
        {
            ReglaNegocioException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Regla de negocio no cumplida",
                Detail = exception.Message
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno del servidor",
                Detail = "Ocurrió un error inesperado. Intente de nuevo más tarde."
            }
        };

        if (problema.Status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Error no controlado en {Metodo} {Ruta}",
                httpContext.Request.Method, httpContext.Request.Path);
        else
            logger.LogWarning("Regla de negocio: {Mensaje}", exception.Message);

        httpContext.Response.StatusCode = problema.Status ?? StatusCodes.Status500InternalServerError;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problema
        });
    }
}