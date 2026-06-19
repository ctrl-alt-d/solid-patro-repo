# Documentació Fase 2

> Resolució a la branca:
> [github.com/ctrl-alt-d/solid-patro-repo/tree/Fase2](https://github.com/ctrl-alt-d/solid-patro-repo/tree/Fase2)

## Objectiu

En aquesta fase farem la capa de negoci. Aquesta capa concentra els casos d'ús de l'aplicació i utilitza el repositori a través de `IRepositoriAlumne`. Fixa't que les operacions de negoci no es corresponen un a un amb les operacions del repositori; per exemple, tenim una operació de negoci "Promocionar alumne" que sumarà un curs al curs actual fins a arribar a 3r i marcarà com a finalitzat quan ja estigui al darrer curs.

## 1. Crear els projectes i afegir les dependències

* Crear projecte `LogicaDeNegoci.Abstractions`: conté les interfícies i els DTOs de negoci. No té dependències.
* Crear projecte `LogicaDeNegoci`: implementa la lògica de negoci i té com a dependència `Repositori.Abstractions`.
* Crear projecte `LogicaDeNegoci.UnitTests`: valida la lògica de negoci amb mocks del repositori.

```bash
dotnet new classlib -n LogicaDeNegoci.Abstractions
dotnet new classlib -n LogicaDeNegoci
dotnet new xunit -n LogicaDeNegoci.UnitTests
```

```bash
# LogicaDeNegoci.Abstractions no té dependències
dotnet add LogicaDeNegoci reference LogicaDeNegoci.Abstractions Repositori.Abstractions
dotnet add LogicaDeNegoci.UnitTests reference LogicaDeNegoci LogicaDeNegoci.Abstractions Repositori
```

```bash
dotnet add LogicaDeNegoci.UnitTests package FluentAssertions
dotnet add LogicaDeNegoci.UnitTests package NSubstitute
```

```bash
dotnet sln add LogicaDeNegoci.Abstractions/LogicaDeNegoci.Abstractions.csproj
dotnet sln add LogicaDeNegoci/LogicaDeNegoci.csproj
dotnet sln add LogicaDeNegoci.UnitTests/LogicaDeNegoci.UnitTests.csproj
```

## 2. Definir el contracte de negoci

### 2.1 Interfície de la capa de negoci

**Responsabilitat**: exposar casos d'ús de negoci orientats a aplicació (no mètodes CRUD del repositori).

Exemple:
- `AfegirAsync`: alta d'alumne amb valors inicials de negoci.
- `PromocionarAsync`: regla de promoció/fi d'estudis.
- `SeleccionarPerIdAsync`: retorn d'una projecció d'un alumne concret.
- `SeleccionarTotsAsync`: retorn de projeccions per consum de capa superior.

Fitxer: `LogicaDeNegoci.Abstractions/ILogicaNegociAlumne.cs`

```csharp
using LogicaDeNegoci.Abstractions.Parametres;
using LogicaDeNegoci.Abstractions.Projeccions;

namespace LogicaDeNegoci.Abstractions;

public interface ILogicaNegociAlumne
{
	Task<ProjeccioAlumne> AfegirAsync(AfegirAlumneParametres parametres);
	Task<ProjeccioAlumne> CanviarDadesAsync(CanviarDadesAlumneParametres parametres);
	Task EliminarAsync(EliminarAlumneParametres parametres);
	Task<ProjeccioAlumne> SeleccionarPerIdAsync(SeleccionarPerIdAlumneParametres parametres);
	Task<ProjeccioAlumnes> SeleccionarTotsAsync(SeleccionarTotsAlumnesParametres parametres);
	Task<ProjeccioAlumne> PromocionarAsync(PromocionarAlumneParametres parametres);
}
```

### 2.2 DTOs d'entrada i sortida

**Responsabilitat**: desacoblar la capa de negoci del model de persistència.

Organització recomanada:
- Carpeta `Parametres`: classes d'entrada (`AfegirAlumneParametres`, `CanviarDadesAlumneParametres`, etc.).
- Carpeta `Projeccions`: classes de resposta (`ProjeccioAlumne`, `ProjeccioAlumnes`).

Exemple rellevant de paràmetres:

```csharp
namespace LogicaDeNegoci.Abstractions.Parametres;

public class PromocionarAlumneParametres
{
	public int Id { get; set; }
}
```

També necessitem un paràmetre per seleccionar un alumne concret per `Id`. Aquest cas d'ús serà necessari més endavant des de MVC per mostrar la pantalla de detalls sense saltar-se la capa de negoci.

```csharp
namespace LogicaDeNegoci.Abstractions.Parametres;

public class SeleccionarPerIdAlumneParametres
{
	public int Id { get; set; }
}
```

Exemple rellevant de projecció:

```csharp
namespace LogicaDeNegoci.Abstractions.Projeccions;

public class ProjeccioAlumne
{
	public int Id { get; set; }
	public string Nom { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public int Curs { get; set; }
	public bool EstudisFinalitzats { get; set; }
}
```

## 3. Implementar la lògica de negoci

**Responsabilitat**: aplicar regles de negoci i delegar persistència al repositori.

En el codi actual, `LogicaNegociAlumne` depèn de `IRepositoriAlumne`. Això permet testejar la regla de negoci amb un mock i evita que la capa de negoci quedi acoblada a Entity Framework Core.

Fitxer: `LogicaDeNegoci/LogicaNegociAlumne.cs`

Punts clau de la implementació:
- Recuperar l'alumne amb `ObtenirPerIdAsync` i llançar excepció si no existeix.
- Implementar `SeleccionarPerIdAsync` com a cas d'ús de consulta de negoci, retornant una projecció.
- Aplicar la regla de promoció a la capa de negoci.
- Persistir amb `ActualitzarAlumneAsync`.
- Retornar projeccions (no entitats de base de dades).

Snippet rellevant de selecció per id:

```csharp
public async Task<ProjeccioAlumne> SeleccionarPerIdAsync(SeleccionarPerIdAlumneParametres parametres)
{
	var alumne = await repositori.ObtenirPerIdAsync(parametres.Id)
		?? throw new InvalidOperationException($"No s'ha trobat l'alumne amb id {parametres.Id}.");

	return new ProjeccioAlumne
	{
		Id = alumne.Id,
		Nom = alumne.Nom,
		Email = alumne.Email,
		Curs = alumne.Curs,
		EstudisFinalitzats = alumne.EstudisFinalitzats,
	};
}
```

Snippet rellevant de la regla de promoció:

```csharp
public async Task<ProjeccioAlumne> PromocionarAsync(PromocionarAlumneParametres parametres)
{
	var alumne = await repositori.ObtenirPerIdAsync(parametres.Id)
		?? throw new InvalidOperationException($"No s'ha trobat l'alumne amb id {parametres.Id}.");

	if (alumne.Curs < 3)
	{
		alumne.Curs++;
	}
	else
	{
		alumne.EstudisFinalitzats = true;
	}

	await repositori.ActualitzarAlumneAsync(alumne);

	return new ProjeccioAlumne
	{
		Id = alumne.Id,
		Nom = alumne.Nom,
		Email = alumne.Email,
		Curs = alumne.Curs,
		EstudisFinalitzats = alumne.EstudisFinalitzats,
	};
}
```

## 4. Proves unitàries de la capa de negoci

**Responsabilitat**: validar regles de negoci sense tocar base de dades.

Fitxer: `LogicaDeNegoci.UnitTests/LogicaNegociAlumneTests.cs`

Casos mínims recomanats:
- `AfegirAsync`: crea alumne amb valors inicials correctes.
- `CanviarDadesAsync`: actualitza nom i email.
- `EliminarAsync`: delega l'eliminació.
- `PromocionarAsync`: aplica regla de promoció/finalització.
- `SeleccionarPerIdAsync`: obté un alumne per id i el transforma a projecció.
- `SeleccionarTotsAsync`: mapa d'entitats a projeccions.

Snippet rellevant de test de promoció:

```csharp
[Fact]
public async Task PromocionarAsync_Delega_I_Retorna_Projeccio_Actualitzada()
{
	repositori.ObtenirPerIdAsync(5).Returns(new Alumne
	{
		Id = 5,
		Nom = "Ada",
		Email = "ada@demo.cat",
		Curs = 3,
		EstudisFinalitzats = false,
	});

	var resultat = await sut.PromocionarAsync(new PromocionarAlumneParametres { Id = 5 });

	await repositori.Received(1).ActualitzarAlumneAsync(Arg.Is<Alumne>(a =>
		a.Id == 5 &&
		a.Curs == 3 &&
		a.EstudisFinalitzats));

	resultat.EstudisFinalitzats.Should().BeTrue();
}
```

## 5. Estructura esperada

```text
Alumnes/
├── LogicaDeNegoci.Abstractions/
│   ├── ILogicaNegociAlumne.cs
│   ├── Parametres/
│   │   ├── AfegirAlumneParametres.cs
│   │   ├── CanviarDadesAlumneParametres.cs
│   │   ├── EliminarAlumneParametres.cs
│   │   ├── PromocionarAlumneParametres.cs
│   │   ├── SeleccionarPerIdAlumneParametres.cs
│   │   └── SeleccionarTotsAlumnesParametres.cs
│   └── Projeccions/
│       ├── ProjeccioAlumne.cs
│       └── ProjeccioAlumnes.cs
├── LogicaDeNegoci/
│   └── LogicaNegociAlumne.cs
└── LogicaDeNegoci.UnitTests/
	└── LogicaNegociAlumneTests.cs
```

## 6. Verificar que tot funciona

```bash
dotnet build
dotnet test LogicaDeNegoci.UnitTests/LogicaDeNegoci.UnitTests.csproj
```

Si compila i passen els tests, la fase de negoci queda completada.

## 7. Preparat per ser utilitzat des de MVC

Un cop acabada aquesta fase, la capa de negoci ja es pot injectar a un projecte MVC via DI.

Registre típic al `Program.cs` del projecte web:

```csharp
builder.Services.AddScoped<ILogicaNegociAlumne, LogicaNegociAlumne>();
builder.Services.AddScoped<IRepositoriAlumne, RepositoriAlumne>();
```

I després als controladors MVC només cal dependre de la interfície de negoci:

```csharp
public class AlumnesController(ILogicaNegociAlumne logicaNegoci) : Controller
{
}
```

Això deixa una separació clara: MVC orquestra peticions/respostes HTTP, i `LogicaDeNegoci` concentra les regles del domini.
