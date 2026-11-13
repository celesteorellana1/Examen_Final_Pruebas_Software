using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ClienteApp.Tests.Api;

public class ClientesApiTests
{
    [Fact]
    public async Task Registrar_ClienteValido_DebeRetornar201()
    {
        // Arrange
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        var solicitud = new
        {
            Nombre = "Ana López",
            Correo = $"ana_{Guid.NewGuid():N}@example.com"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Clientes",
            solicitud);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var cliente = await response.Content
            .ReadFromJsonAsync<ClienteRespuesta>();

        Assert.NotNull(cliente);
        Assert.Equal(solicitud.Nombre, cliente.Nombre);
        Assert.Equal(solicitud.Correo, cliente.Correo);
    }

    [Fact]
    public async Task Registrar_NombreVacio_DebeRetornar400()
    {
        // Arrange
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        var solicitud = new
        {
            Nombre = "",
            Correo = $"correo_{Guid.NewGuid():N}@example.com"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Clientes",
            solicitud);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    // PRUEBA 3 - EXAMEN FINAL
    // Consultar un cliente existente y comprobar 200 OK.
    [Fact]
    public async Task ObtenerPorId_ClienteExistente_DebeRetornar200()
    {
        // Arrange
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        var solicitud = new
        {
            Nombre = "Carlos Pérez",
            Correo = $"carlos_{Guid.NewGuid():N}@example.com"
        };

        // Registrar un cliente para obtener su ID.
        var registro = await client.PostAsJsonAsync(
            "/api/Clientes",
            solicitud);

        Assert.Equal(
            HttpStatusCode.Created,
            registro.StatusCode);

        var clienteRegistrado = await registro.Content
            .ReadFromJsonAsync<ClienteRespuesta>();

        Assert.NotNull(clienteRegistrado);

        // Act: consultar el cliente existente.
        var response = await client.GetAsync(
            $"/api/Clientes/{clienteRegistrado.Id}");

        // Assert: comprobar 200 OK.
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var clienteObtenido = await response.Content
            .ReadFromJsonAsync<ClienteRespuesta>();

        Assert.NotNull(clienteObtenido);
        Assert.Equal(
            clienteRegistrado.Id,
            clienteObtenido.Id);
        Assert.Equal(
            solicitud.Nombre,
            clienteObtenido.Nombre);
        Assert.Equal(
            solicitud.Correo,
            clienteObtenido.Correo);
    }

    // PRUEBA 4 - EXAMEN FINAL
    // Consultar un cliente inexistente y comprobar 404.
    [Fact]
    public async Task ObtenerPorId_ClienteInexistente_DebeRetornar404()
    {
        // Arrange
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        int idInexistente = 99999;

        // Act: consultar un cliente que no existe.
        var response = await client.GetAsync(
            $"/api/Clientes/{idInexistente}");

        // Assert: comprobar 404 Not Found.
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    private sealed record ClienteRespuesta(
        int Id,
        string Nombre,
        string Correo);
}

public sealed class ClienteApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"clientes_api_test_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:ClientesDb"] =
                        $"Data Source={_dbPath};Pooling=False"
                });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
