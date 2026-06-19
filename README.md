# solid-patro-repo

## Pràctica patró repositori

Basat en el document [Implementing the Repository and Unit of Work Patterns in an ASP.NET MVC Application](https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application)

> The repository and unit of work patterns are intended to create an abstraction layer between the data access layer and the business logic layer of an application. Implementing these patterns can help insulate your application from changes in the data store and can facilitate automated unit testing or test-driven development (TDD).

## Objectiu

Crear una aplicació MVC on les operacions de negoci estiguin encapsulades en IoW (Unitats de Treball) i que no depenguin d'una infraestructura concreta per persistir i recuperar les dades de la base de dades.

## Aplicació

Farem una aplicació [CRUD](https://ca.wikipedia.org/wiki/Crear,_llegir,_actualitzar_i_esborrar) i a més tindrà una operació de negoci:

* Crear alumne
* Esborrar alumne
* Actualitzar alumne
* Llegit un alumne
* Llegir tots els alumnes
* Promocionar alumne (operació de negoci)

## Per on comencem?

Crearem dos projectes on posar estructures de dades i definir les operacions que farem contra la base de dades:
* `DbModels`: Models que utilitza el negoci i que es persistiran a la base de dades (ex: `Alumne`).
* `Repositori.Abstractions`: Operacions contra el repositori (CRUD, ex: `LlegirAlumnePerId`, `PersisteixAlumne`, `InsertaAlumne`, ...). Aquest projecte depèn de `DbModels`, ha de conèixer els models a persistir. Es tracta d'una capa d'infraestructura.
* `Repositori`: Implementa `Repositori.Abstractions`
* `Repositori.IntegrationTests`: Comprova que podem persistir.

## TDD

Podem fer TDD, podriem fer que VS Code generi la implementació de `Repositori` a partir de `Repositori.Abstractions`, llavors, escrivim els testos que fallaran i anem implementant.

## Fase 2

En aquesta fase farem la capa de negoci. La capa de negoci és la que té UoW (Unitats de treball). Les unitats de treball utilitzen el repositori, fixa't que les operacions de negoci no es corresponen un a un amb les operacions del repositori, per exempl, tenim una operació de negoci "Promocionar alumne" que sumarà un curs al curs actual fins arribar a 2n i marcarà com a finalitzat si ha completat 2n curs.