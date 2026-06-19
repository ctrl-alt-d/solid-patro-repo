using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repositori.Abstractions;

namespace Repositori.IntegrationTests;

public class RespositoriAlumneTests
{
    [Fact]
    public async Task Crud_Alumne_Funciona()
    {
        // arrange
        await using var serviceProvider = CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        var repositori = scope.ServiceProvider.GetRequiredService<Repositori.Abstractions.IRepositoriAlumne>();
        var alumne = new DbModels.Alumne { Nom = "Ada", Email = "ada@demo.cat", Curs = 1 };

        // act
        await repositori.AfegirAlumneAsync(alumne);

        // assert
        var alumneCreat = await repositori.ObtenirPerIdAsync(alumne.Id);
        Assert.NotNull(alumneCreat);
        Assert.Equal("Ada", alumneCreat!.Nom);
        Assert.Equal("ada@demo.cat", alumneCreat.Email);

        // act
        alumne.Nom = "Ada Lovelace";
        alumne.Email = "ada.lovelace@demo.cat";
        await repositori.ActualitzarAlumneAsync(alumne);

        // assert
        var alumneActualitzat = await repositori.ObtenirPerIdAsync(alumne.Id);
        Assert.NotNull(alumneActualitzat);
        Assert.Equal("Ada Lovelace", alumneActualitzat!.Nom);
        Assert.Equal("ada.lovelace@demo.cat", alumneActualitzat.Email);

        // act
        await repositori.EsborrarAlumneAsync(alumne.Id);

        // assert
        var alumneEsborrat = await repositori.ObtenirPerIdAsync(alumne.Id);
        Assert.Null(alumneEsborrat);
    }


    private static ServiceProvider CreateServiceProvider()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();

        services.AddSingleton(connection);

        services.AddDbContext<AlumnesDbContext>(options =>
            options.UseSqlite(connection));

        services.AddScoped<IRepositoriAlumne, RepositoriAlumne>();

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<AlumnesDbContext>()
            .Database
            .EnsureCreated();

        return provider;
    }

}
