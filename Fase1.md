# Documentació Fase 1

> Resolució a la branca:
> [github.com/ctrl-alt-d/solid-patro-repo/tree/Fase1](https://github.com/ctrl-alt-d/solid-patro-repo/tree/Fase1)

## Objectiu

En aquesta fase es crea l'estructura base del projecte per implementar el patró Repository, separant models, contractes, implementació i proves d'integració.

## 1. Crear la solució

```bash
dotnet new sln -n Alumnes
```

Aquesta comanda crea la solució `Alumnes.slnx`, que actuarà com a contenidor de tots els projectes.

## 2. Crear els projectes

### 2.1 Projecte `DbModels`

```bash
dotnet new classlib -n DbModels
```

**Responsabilitat**: definir els models de dades persistents.

Exemple:
- `Alumne`: model amb propietats com `Id`, `Nom`, `Email`, `Curs` i `EstudisFinalitzats`.

### 2.2 Projecte `Repositori.Abstractions`

```bash
dotnet new classlib -n Repositori.Abstractions
```

**Responsabilitat**: definir les interfícies i contractes d'accés a dades (CRUD).

Exemple:
- `IRepositoriAlumne`: contracte per obtenir, llistar, afegir, actualitzar i esborrar alumnes.

**Dependència**:

```bash
dotnet add Repositori.Abstractions reference DbModels
```

### 2.3 Projecte `Repositori`

```bash
dotnet new classlib -n Repositori
```

**Responsabilitat**: implementar les interfícies de `Repositori.Abstractions`.

Exemple:
- `RepositoriAlumne`: implementació d'`IRepositoriAlumne` amb Entity Framework Core.
- `AlumnesDbContext`: context d'EF Core.

**Dependències**:

```bash
dotnet add Repositori reference DbModels Repositori.Abstractions
dotnet add Repositori package Microsoft.EntityFrameworkCore.Sqlite
```

### 2.4 Projecte `Repositori.IntegrationTests`

```bash
dotnet new xunit -n Repositori.IntegrationTests
```

**Responsabilitat**: validar que la persistència funciona amb una base de dades SQLite real (en memòria).

**Dependències**:

```bash
dotnet add Repositori.IntegrationTests reference DbModels Repositori.Abstractions Repositori
dotnet add Repositori.IntegrationTests package Microsoft.EntityFrameworkCore.Sqlite
```

## 3. Afegir els projectes a la solució

```bash
dotnet sln add DbModels/DbModels.csproj
dotnet sln add Repositori.Abstractions/Repositori.Abstractions.csproj
dotnet sln add Repositori/Repositori.csproj
dotnet sln add Repositori.IntegrationTests/Repositori.IntegrationTests.csproj
```

Alternativa ràpida:

```bash
dotnet sln add **/*.csproj
```

## 4. Estructura esperada

Un cop finalitzada la fase, l'estructura mínima hauria de ser semblant a:

```text
Alumnes/
├── Alumnes.slnx
├── DbModels/
│   ├── DbModels.csproj
│   └── Alumne.cs
├── Repositori.Abstractions/
│   ├── Repositori.Abstractions.csproj
│   └── IRepositoriAlumne.cs
├── Repositori/
│   ├── Repositori.csproj
│   ├── AlumnesDbContext.cs
│   └── RepositoriAlumne.cs
└── Repositori.IntegrationTests/
    ├── Repositori.IntegrationTests.csproj
    └── RespositoriAlumneTests.cs
```

Nota: el fitxer de test actual del repositori està anomenat `RespositoriAlumneTests.cs` (amb una errada tipogràfica al nom). Pots mantenir-lo així o renombrar-lo a `RepositoriAlumneTests.cs` per consistència.

## 5. Verificar que tot funciona

```bash
dotnet build
dotnet test
```

Si tot compila i les proves passen, la fase queda tancada correctament.

## 6. Exemples de codi base

### 6.1 Model `Alumne`

Fitxer: `DbModels/Alumne.cs`

```csharp
namespace DbModels;

public class Alumne
{
    public int Id { get; set; }
    public string Nom { get; set; } = "";
    public string Email { get; set; } = "";
    public int Curs { get; set; } = 1;
    public bool EstudisFinalitzats { get; set; } = false;
}
```

### 6.2 Interfície del repositori

Fitxer: `Repositori.Abstractions/IRepositoriAlumne.cs`

```csharp
using DbModels;

namespace Repositori.Abstractions;

public interface IRepositoriAlumne
{
    Task<Alumne?> ObtenirPerIdAsync(int id);
    Task<IEnumerable<Alumne>> LlistaAlumnesAsync();
    Task AfegirAlumneAsync(Alumne alumne);
    Task ActualitzarAlumneAsync(Alumne alumne);
    Task EsborrarAlumneAsync(int id);
}
```

Fixa't que el repositori **no** té cap mètode `PromocionarAlumneAsync`: promocionar és una regla de negoci i s'implementarà a la capa `LogicaDeNegoci`.

### 6.3 Esquelet de la implementació del repositori

Fitxer: `Repositori/RepositoriAlumne.cs`

```csharp
using Repositori.Abstractions;

namespace Repositori;

public class RepositoriAlumne : IRepositoriAlumne
{
}
```

Pots usar l'acció de VS Code per implementar la interfície automàticament (generarà mètodes amb `throw new NotImplementedException();`).

### 6.4 Context de base de dades

Fitxer: `Repositori/AlumnesDbContext.cs`

```csharp
using DbModels;
using Microsoft.EntityFrameworkCore;

namespace Repositori;

public class AlumnesDbContext(DbContextOptions<AlumnesDbContext> options) : DbContext(options)
{
    public DbSet<Alumne> Alumnes { get; set; }
}
```

### 6.5 Prova d'integració CRUD

Fitxer: `Repositori.IntegrationTests/RespositoriAlumneTests.cs`

```csharp
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
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        var repositori = scope.ServiceProvider.GetRequiredService<IRepositoriAlumne>();
        var alumne = new DbModels.Alumne { Nom = "Ada", Email = "ada@demo.cat", Curs = 1 };

        // Act
        await repositori.AfegirAlumneAsync(alumne);

        // Assert
        var alumneCreat = await repositori.ObtenirPerIdAsync(alumne.Id);
        Assert.NotNull(alumneCreat);
        Assert.Equal("Ada", alumneCreat!.Nom);
        Assert.Equal("ada@demo.cat", alumneCreat.Email);

        // Act
        alumne.Nom = "Ada Lovelace";
        alumne.Email = "ada.lovelace@demo.cat";
        await repositori.ActualitzarAlumneAsync(alumne);

        // Assert
        var alumneActualitzat = await repositori.ObtenirPerIdAsync(alumne.Id);
        Assert.NotNull(alumneActualitzat);
        Assert.Equal("Ada Lovelace", alumneActualitzat!.Nom);
        Assert.Equal("ada.lovelace@demo.cat", alumneActualitzat.Email);

        // Act
        await repositori.EsborrarAlumneAsync(alumne.Id);

        // Assert
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
```

## 7. Aclariments importants

- `IntegrationTests` valida la integració completa (repositori + EF Core + SQLite), no només mètodes aïllats.
- Fer servir SQLite en memòria (`DataSource=:memory:`) fa que les proves siguin ràpides i repetibles.
- `EnsureCreated()` prepara l'esquema de BD només per a proves; en entorns reals es recomanen migracions (`dotnet ef migrations`).
- A la propera fase afegirem la capa de negoci, que consumirà aquest repositori mitjançant la interfície `IRepositoriAlumne`.
