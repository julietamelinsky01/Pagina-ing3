using LasMelis.Api.Exceptions;
using LasMelis.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace LasMelis.Tests;

public class ExceptionHandlingMiddlewareTests
{
    private static async Task<DefaultHttpContext> Ejecutar(Exception? aLanzar)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => aLanzar is null ? Task.CompletedTask : throw aLanzar,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);
        return context;
    }

    private static string Cuerpo(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        return new StreamReader(context.Response.Body).ReadToEnd();
    }

    // Cada excepción de negocio se traduce a su código HTTP: es el contrato que ve el frontend.
    [Theory]
    [InlineData(typeof(NotFoundAppException), 404)]
    [InlineData(typeof(ValidationAppException), 400)]
    [InlineData(typeof(ConflictAppException), 409)]
    [InlineData(typeof(UnauthorizedAppException), 401)]
    public async Task UnaExcepcionDeNegocio_SeTraduceASuCodigoHttp(Type tipo, int codigoEsperado)
    {
        var excepcion = (Exception)Activator.CreateInstance(tipo, "el motivo")!;

        var context = await Ejecutar(excepcion);

        Assert.Equal(codigoEsperado, context.Response.StatusCode);
        Assert.Contains("el motivo", Cuerpo(context));
    }

    [Fact]
    public async Task UnaExcepcionInesperada_Devuelve500SinFiltrarElDetalleInterno()
    {
        var context = await Ejecutar(new InvalidOperationException("detalle interno secreto"));

        Assert.Equal(500, context.Response.StatusCode);
        Assert.DoesNotContain("detalle interno secreto", Cuerpo(context));
    }

    [Fact]
    public async Task SinExcepcion_DejaPasarElPedidoSinTocarLaRespuesta()
    {
        var context = await Ejecutar(null);

        Assert.Equal(200, context.Response.StatusCode);
        Assert.Equal(string.Empty, Cuerpo(context));
    }
}
