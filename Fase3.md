# Documentació Fase 3

## Objectiu

En aquesta fase creem la capa de presentació amb ASP.NET Core MVC. L'aplicació web consumirà la lògica de negoci de la Fase 2 a través de la seva interfície, sense acoblar-se mai a la implementació concreta. Es demostrarà com la injecció de dependències (DI) uneix totes les capes.

Les funcionalitats a implementar:
- Crear alumne
- Veure dades d'un alumne
- Promocionar alumne

## 1. Crear el projecte MVC i afegir les dependències

```bash
dotnet new mvc -n Web
```

**Dependències del projecte**:

```bash
dotnet add Web reference LogicaDeNegoci.Abstractions
dotnet add Web reference LogicaDeNegoci
dotnet add Web reference Repositori.Abstractions
dotnet add Web reference Repositori
dotnet add Web package Microsoft.EntityFrameworkCore.Sqlite
```

```bash
dotnet sln add Web/Web.csproj
```

> `Web` depèn de `LogicaDeNegoci.Abstractions` per poder declarar el tipus a injectar.
> Depèn de `LogicaDeNegoci` i `Repositori` perquè és el punt d'entrada (composition root) que registra les implementacions concretes al contenidor de DI.

## 2. Configurar la injecció de dependències

El fitxer `Program.cs` és el **composition root**: l'únic lloc de tota l'aplicació on es coneix quines implementacions concretes s'utilitzen.

Fitxer: `Web/Program.cs`

Passos clau:
- Registrar `AlumnesDbContext` amb SQLite.
- Registrar `IRepositoriAlumne` → `RepositoriAlumne`.
- Registrar `ILogicaNegociAlumne` → `LogicaNegociAlumne`.

> Els controladors només veuran `ILogicaNegociAlumne`. No coneixeran ni el repositori ni EF Core.

## 3. Crear el controlador `AlumnesController`

Fitxer: `Web/Controllers/AlumnesController.cs`

El controlador rep `ILogicaNegociAlumne` per constructor (DI). **No** rep el repositori ni el DbContext directament.

Accions a implementar:

| Acció | Mètode HTTP | Ruta | Descripció |
|---|---|---|---|
| `Index` | GET | `/Alumnes` | Llista tots els alumnes |
| `Detalls` | GET | `/Alumnes/Detalls/{id}` | Veure dades d'un alumne |
| `Crear` | GET | `/Alumnes/Crear` | Formulari de creació |
| `Crear` | POST | `/Alumnes/Crear` | Processa el formulari |
| `Promocionar` | POST | `/Alumnes/Promocionar/{id}` | Promociona un alumne |

> L'acció `Crear` GET mostra el formulari buit. L'acció `Crear` POST recull el model, crida `AfegirAsync` i redirigeix.

### 3.1 Acció `Promocionar`

L'acció és un POST perquè modifica l'estat del servidor. El `id` arriba per ruta. Després de promocionar redirigeix a `Detalls` per mostrar el nou estat de l'alumne.

```csharp
[HttpPost]
public async Task<IActionResult> Promocionar(int id)
{
    await logica.PromocionarAsync(new PromocionarAlumneParametres { Id = id });
    return RedirectToAction(nameof(Detalls), new { id });
}
```

> Fixa't que el controlador no sap res de cursos ni de regles de negoci. Delega tot a `ILogicaNegociAlumne`.

## 4. Crear les vistes

Les vistes s'han de crear **a mà** a la carpeta `Web/Views/Alumnes/`.

Fitxers necessaris:

```text
Web/Views/Alumnes/
├── Index.cshtml       → llista d'alumnes amb enllaç a Detalls
├── Detalls.cshtml     → fitxa d'un alumne amb botó Promocionar
└── Crear.cshtml       → formulari per donar d'alta un alumne
```

### 4.1 Vista `Index.cshtml`

Model: `ProjeccioAlumnes`

Mostra una taula amb els alumnes i un enllaç a `Detalls` per cada fila.

### 4.2 Vista `Detalls.cshtml`

Model: `ProjeccioAlumne`

Mostra les dades de l'alumne: Nom, Email, Curs i si ha finalitzat els estudis.

Inclou un formulari `<form method="post">` que apunta a l'acció `Promocionar` per poder promocionar l'alumne amb un botó.

```cshtml
@model LogicaDeNegoci.Abstractions.Projeccions.ProjeccioAlumne

<h2>@Model.Nom</h2>

<dl>
    <dt>Email</dt>
    <dd>@Model.Email</dd>
    <dt>Curs</dt>
    <dd>@Model.Curs</dd>
    <dt>Estudis finalitzats</dt>
    <dd>@Model.EstudisFinalitzats</dd>
</dl>

@if (!Model.EstudisFinalitzats)
{
    <form asp-action="Promocionar" asp-route-id="@Model.Id" method="post">
        <button type="submit">Promocionar</button>
    </form>
}

### 4.3 Vista `Crear.cshtml`

Model: `AfegirAlumneParametres`

Formulari amb camps: Nom i Email (el Curs i l'estat s'inicialitzen a la capa de negoci).

## 5. Estructura esperada

```text
Alumnes/
├── Web/
│   ├── Web.csproj
│   ├── Program.cs
│   ├── Controllers/
│   │   └── AlumnesController.cs
│   └── Views/
│       ├── Shared/
│       │   └── _Layout.cshtml   (generat per la plantilla)
│       └── Alumnes/
│           ├── Index.cshtml
│           ├── Detalls.cshtml
│           └── Crear.cshtml
```

## 6. Verificar que tot funciona

```bash
dotnet build
dotnet run --project Web
```

Comprova manualment:
- `/Alumnes/Crear` → pots donar d'alta un alumne.
- `/Alumnes` → l'alumne creat apareix a la llista.
- `/Alumnes/Detalls/{id}` → veus les dades i pots promocionar.
- Promocionar diverses vegades puja el curs fins a 3r i després marca `EstudisFinalitzats = true`.

