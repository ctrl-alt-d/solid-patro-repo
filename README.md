# Pràctica: patró Repository, lògica de negoci i MVC

> Pràctica disponible a GitHub:
> https://github.com/ctrl-alt-d/solid-patro-repo

Basat en el document [Implementing the Repository and Unit of Work Patterns in an ASP.NET MVC Application](https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application)

> The repository and unit of work patterns are intended to create an abstraction layer between the data access layer and the business logic layer of an application. Implementing these patterns can help insulate your application from changes in the data store and can facilitate automated unit testing or test-driven development (TDD).

## Objectiu

Crear una aplicació MVC on les operacions de negoci estiguin encapsulades en una capa pròpia i no depenguin d'una infraestructura concreta per persistir i recuperar les dades de la base de dades.

El que busquem és separar responsabilitats:

```text
MVC → Lògica de negoci → Repositori → Base de dades
```

La capa MVC només ha d'orquestrar peticions HTTP i respostes HTML. Les regles de negoci han de viure a la capa de negoci. El repositori només s'ha d'encarregar de persistir i recuperar dades.

## Què és el patró Repository en aquesta pràctica?

El patró **Repository** crea una abstracció entre el codi que necessita dades i el mecanisme concret que les desa. En aquest projecte, la capa de negoci no parla amb Entity Framework Core ni amb SQLite: parla amb `IRepositoriAlumne`.

```text
LogicaNegociAlumne → IRepositoriAlumne → RepositoriAlumne → AlumnesDbContext → SQLite
```

Això té dues conseqüències importants:

* La lògica de negoci expressa casos d'ús (`AfegirAsync`, `CanviarDadesAsync`, `PromocionarAsync`, etc.) sense conèixer la base de dades.
* El repositori expressa operacions de persistència (`ObtenirPerIdAsync`, `LlistaAlumnesAsync`, `AfegirAlumneAsync`, `ActualitzarAlumneAsync`, `EsborrarAlumneAsync`) sense decidir regles de negoci.

Per això **promocionar un alumne no és responsabilitat del repositori**. El repositori pot carregar i desar alumnes; la regla de si un alumne passa de curs o finalitza els estudis pertany a `LogicaDeNegoci`.

## Aplicació

Farem una aplicació [CRUD](https://ca.wikipedia.org/wiki/Crear,_llegir,_actualitzar_i_esborrar) i a més tindrà una operació de negoci:

* Crear alumne.
* Esborrar alumne.
* Actualitzar alumne.
* Llegir un alumne.
* Llegir tots els alumnes.
* Promocionar alumne — operació de negoci.

## Fase 1: repositori i persistència

> Documentació completa fase 1:
> [github.com/ctrl-alt-d/solid-patro-repo/blob/main/Fase1.md](https://github.com/ctrl-alt-d/solid-patro-repo/blob/main/Fase1.md)

Crearem els projectes on posar estructures de dades i definir les operacions que farem contra la base de dades:

* `DbModels`: Models que utilitza el negoci i que es persistiran a la base de dades (ex: `Alumne`).
* `Repositori.Abstractions`: Contracte d'accés a dades (`ObtenirPerIdAsync`, `LlistaAlumnesAsync`, `AfegirAlumneAsync`, `ActualitzarAlumneAsync`, `EsborrarAlumneAsync`). Aquest projecte depèn de `DbModels`, perquè el repositori treballa amb els models persistents.
* `Repositori`: Implementa `Repositori.Abstractions`.
* `Repositori.IntegrationTests`: Comprova que podem persistir.

Consulta els detalls a [`Fase1.md`](./Fase1.md).

## Fase 2

> Documentació completa fase 2:
> [github.com/ctrl-alt-d/solid-patro-repo/blob/main/Fase2.md](https://github.com/ctrl-alt-d/solid-patro-repo/blob/main/Fase2.md)


En aquesta fase farem la capa de negoci. La capa de negoci és la que concentra els casos d'ús. Aquests casos d'ús utilitzen el repositori, però no es corresponen un a un amb les operacions del repositori. Per exemple, tenim una operació de negoci "Promocionar alumne" que sumarà un curs al curs actual fins a arribar a 3r i marcarà com a finalitzat quan ja estigui al darrer curs.

La idea important és que el controlador MVC no hauria de saber aquesta regla. El repositori tampoc. Aquesta regla pertany a la lògica de negoci:

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

Aquest tall de codi és el cor de l'arquitectura: la regla de promoció està encapsulada en un cas d'ús de negoci, el repositori només persisteix el resultat i MVC només demana que l'acció es faci.

Consulta els detalls a [`Fase2.md`](./Fase2.md).

## Fase 3: ASP.NET Core MVC

> Documentació completa fase 3:
> [github.com/ctrl-alt-d/solid-patro-repo/blob/main/Fase3.md](https://github.com/ctrl-alt-d/solid-patro-repo/blob/main/Fase3.md)

En aquesta fase afegim la capa de presentació amb un projecte `Web` basat en ASP.NET Core MVC.

Funcionalitats principals:

* Crear alumne des d'un formulari.
* Llistar alumnes.
* Veure els detalls d'un alumne.
* Editar nom i email.
* Eliminar alumne amb una acció `POST`.
* Promocionar alumne amb una acció `POST`.
* Accedir a la gestió d'alumnes des de la pàgina d'inici.

El projecte `Web` és el **composition root**: registra les implementacions concretes al contenidor de dependències:

```text
ILogicaNegociAlumne → LogicaNegociAlumne
IRepositoriAlumne   → RepositoriAlumne
AlumnesDbContext    → SQLite
```

Però els controladors només depenen de `ILogicaNegociAlumne`. Això és clau: MVC no coneix ni Entity Framework Core ni el repositori.

Consulta els detalls a [`Fase3.md`](./Fase3.md).

## Verificació

```bash
dotnet build
dotnet test
dotnet run --project Web
```
