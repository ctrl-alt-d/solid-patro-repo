# Documentació Fase 3

> Resolució a la branca:
> [github.com/ctrl-alt-d/solid-patro-repo/tree/Fase3](https://github.com/ctrl-alt-d/solid-patro-repo/tree/Fase3)

## Objectiu

En aquesta fase creem la capa de presentació amb **ASP.NET Core MVC**. L'aplicació web consumirà la lògica de negoci de la Fase 2 a través de la seva interfície, sense acoblar-se mai a la implementació concreta.

La idea important és aquesta:

```text
MVC → LogicaDeNegoci.Abstractions → LogicaDeNegoci → Repositori.Abstractions → Repositori → DbModels
```

El projecte `Web` és el **composition root**: l'únic punt de l'aplicació on es decideix quines implementacions concretes s'injecten. Els controladors només coneixen contractes de negoci.

Funcionalitats implementades:

- Crear alumne.
- Llistar alumnes.
- Veure dades d'un alumne.
- Editar les dades d'un alumne.
- Eliminar alumne.
- Promocionar alumne.
- Accedir a la gestió d'alumnes des de la pàgina d'inici.

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

Afegim el projecte a la solució:

```bash
dotnet sln add Web/Web.csproj
```

### Per què `Web` depèn de tants projectes?

- Depèn de `LogicaDeNegoci.Abstractions` perquè els controladors injecten `ILogicaNegociAlumne`.
- Depèn de `LogicaDeNegoci` perquè `Program.cs` registra la implementació `LogicaNegociAlumne`.
- Depèn de `Repositori.Abstractions` perquè `Program.cs` registra el contracte `IRepositoriAlumne`.
- Depèn de `Repositori` perquè `Program.cs` registra `RepositoriAlumne` i `AlumnesDbContext`.

Això no trenca l'arquitectura perquè aquestes dependències concretes només apareixen al **composition root**, no als controladors.

## 2. Configurar la injecció de dependències

Fitxer: `Web/Program.cs`

```csharp
using LogicaDeNegoci;
using LogicaDeNegoci.Abstractions;
using Microsoft.EntityFrameworkCore;
using Repositori;
using Repositori.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AlumnesDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Alumnes")));

builder.Services.AddScoped<IRepositoriAlumne, RepositoriAlumne>();
builder.Services.AddScoped<ILogicaNegociAlumne, LogicaNegociAlumne>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AlumnesDbContext>();
    dbContext.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
```

### 2.1 Cadena de DI

La cadena completa queda així:

```text
AlumnesController
└── ILogicaNegociAlumne → LogicaNegociAlumne
    └── IRepositoriAlumne → RepositoriAlumne
        └── AlumnesDbContext → SQLite
```

El controlador **no sap res** de `RepositoriAlumne`, `AlumnesDbContext` ni Entity Framework Core. Aquesta separació és el punt central de la fase.

### 2.2 Cadena de connexió

Fitxer: `Web/appsettings.json`

```json
{
  "ConnectionStrings": {
    "Alumnes": "Data Source=alumnes.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

En aquesta fase usem `EnsureCreated()` per crear la base de dades automàticament si no existeix. És correcte per a una pràctica o prototip, però en un projecte real faríem servir migracions d'Entity Framework Core.

## 3. Cas d'ús de negoci necessari per a detalls

Per poder implementar la vista de detalls, MVC necessita demanar un alumne concret per id.

La solució **no** és injectar el repositori al controlador. Això saltaria la capa de negoci i trencaria la separació de responsabilitats.

Per això, aquest cas d'ús ja ha d'estar definit a la Fase 2 dins la capa de negoci:

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

Nou paràmetre:

Fitxer: `LogicaDeNegoci.Abstractions/Parametres/SeleccionarPerIdAlumneParametres.cs`

```csharp
namespace LogicaDeNegoci.Abstractions.Parametres;

public class SeleccionarPerIdAlumneParametres
{
    public int Id { get; set; }
}
```

Això manté la regla arquitectònica:

```text
Web no consulta el repositori directament.
Web demana casos d'ús a LogicaDeNegoci.
```

## 4. Crear el controlador `AlumnesController`

Fitxer: `Web/Controllers/AlumnesController.cs`

El controlador rep `ILogicaNegociAlumne` per constructor. **No** rep ni el repositori ni el `DbContext`.

```csharp
using LogicaDeNegoci.Abstractions;
using LogicaDeNegoci.Abstractions.Parametres;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class AlumnesController(ILogicaNegociAlumne logicaNegoci) : Controller
{
    public async Task<IActionResult> Index()
    {
        var alumnes = await logicaNegoci.SeleccionarTotsAsync(new SeleccionarTotsAlumnesParametres());
        return View(alumnes);
    }

    public async Task<IActionResult> Detalls(int id)
    {
        var alumne = await logicaNegoci.SeleccionarPerIdAsync(new SeleccionarPerIdAlumneParametres { Id = id });
        return View(alumne);
    }

    public IActionResult Crear()
    {
        return View(new AfegirAlumneParametres());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(AfegirAlumneParametres parametres)
    {
        if (!ModelState.IsValid)
        {
            return View(parametres);
        }

        await logicaNegoci.AfegirAsync(parametres);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var alumne = await logicaNegoci.SeleccionarPerIdAsync(new SeleccionarPerIdAlumneParametres { Id = id });

        return View(new CanviarDadesAlumneParametres
        {
            Id = alumne.Id,
            Nom = alumne.Nom,
            Email = alumne.Email,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(CanviarDadesAlumneParametres parametres)
    {
        if (!ModelState.IsValid)
        {
            return View(parametres);
        }

        await logicaNegoci.CanviarDadesAsync(parametres);
        return RedirectToAction(nameof(Detalls), new { id = parametres.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Promocionar(int id)
    {
        await logicaNegoci.PromocionarAsync(new PromocionarAlumneParametres { Id = id });
        return RedirectToAction(nameof(Detalls), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        await logicaNegoci.EliminarAsync(new EliminarAlumneParametres { Id = id });
        return RedirectToAction(nameof(Index));
    }
}
```

### 4.1 Accions del controlador

| Acció | Mètode HTTP | Ruta | Responsabilitat |
|---|---|---|---|
| `Index` | GET | `/Alumnes` | Demana tots els alumnes a negoci i els mostra. |
| `Detalls` | GET | `/Alumnes/Detalls/{id}` | Demana un alumne concret a negoci i el mostra. |
| `Crear` | GET | `/Alumnes/Crear` | Mostra el formulari buit. |
| `Crear` | POST | `/Alumnes/Crear` | Valida el formulari, crea l'alumne i redirigeix a `Index`. |
| `Editar` | GET | `/Alumnes/Editar/{id}` | Carrega l'alumne i mostra el formulari d'edició. |
| `Editar` | POST | `/Alumnes/Editar` | Valida el formulari, canvia nom/email i redirigeix a `Detalls`. |
| `Promocionar` | POST | `/Alumnes/Promocionar/{id}` | Demana a negoci que promocioni l'alumne i redirigeix a `Detalls`. |
| `Eliminar` | POST | `/Alumnes/Eliminar/{id}` | Demana a negoci que elimini l'alumne i redirigeix a `Index`. |

### 4.2 Per què `Promocionar` és POST?

Promocionar modifica l'estat del servidor. Per tant, no ha de ser una petició GET.

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Promocionar(int id)
{
    await logicaNegoci.PromocionarAsync(new PromocionarAlumneParametres { Id = id });
    return RedirectToAction(nameof(Detalls), new { id });
}
```

El controlador no sap res de cursos, ni de quan finalitzen els estudis. Això ho decideix `LogicaNegociAlumne`.

## 5. Crear les vistes

Les vistes es creen a la carpeta:

```text
Web/Views/Alumnes/
```

Fitxers necessaris:

```text
Web/Views/Alumnes/
├── Index.cshtml       → llista d'alumnes amb enllaços a Detalls/Editar i formulari d'eliminació
├── Detalls.cshtml     → fitxa d'un alumne amb botons Promocionar, Editar i Eliminar
├── Crear.cshtml       → formulari per donar d'alta un alumne
└── Editar.cshtml      → formulari per canviar nom i email
```

### 5.1 Vista `Index.cshtml`

Model: `ProjeccioAlumnes`

Responsabilitat:

- Mostrar una taula amb els alumnes.
- Mostrar un enllaç a `Detalls` per cada alumne.
- Mostrar un enllaç a `Editar` per cada alumne.
- Permetre eliminar un alumne amb un formulari `POST`.
- Mostrar un enllaç per crear un alumne nou.

Exemple:

```cshtml
@model LogicaDeNegoci.Abstractions.Projeccions.ProjeccioAlumnes

<h1>Alumnes</h1>

<p>
    <a asp-action="Crear">Crear alumne</a>
</p>

<table class="table">
    <thead>
        <tr>
            <th>Nom</th>
            <th>Email</th>
            <th>Curs</th>
            <th>Estudis finalitzats</th>
            <th></th>
        </tr>
    </thead>
    <tbody>
    @foreach (var alumne in Model.Alumnes)
    {
        <tr>
            <td>@alumne.Nom</td>
            <td>@alumne.Email</td>
            <td>@alumne.Curs</td>
            <td>@(alumne.EstudisFinalitzats ? "Sí" : "No")</td>
            <td>
                <a asp-action="Detalls" asp-route-id="@alumne.Id">Detalls</a>
                |
                <a asp-action="Editar" asp-route-id="@alumne.Id">Editar</a>
                |
                <form asp-action="Eliminar" asp-route-id="@alumne.Id" method="post" class="d-inline">
                    <button type="submit" class="btn btn-link p-0 align-baseline">Eliminar</button>
                </form>
            </td>
        </tr>
    }
    </tbody>
</table>
```

### 5.2 Vista `Detalls.cshtml`

Model: `ProjeccioAlumne`

Responsabilitat:

- Mostrar les dades d'un alumne.
- Permetre promocionar-lo si encara no ha finalitzat els estudis.
- Fer la promoció amb un formulari `POST`.
- Permetre anar a editar-lo o eliminar-lo.

```cshtml
@model LogicaDeNegoci.Abstractions.Projeccions.ProjeccioAlumne

<h1>@Model.Nom</h1>

<dl>
    <dt>Email</dt>
    <dd>@Model.Email</dd>

    <dt>Curs</dt>
    <dd>@Model.Curs</dd>

    <dt>Estudis finalitzats</dt>
    <dd>@(Model.EstudisFinalitzats ? "Sí" : "No")</dd>
</dl>

@if (!Model.EstudisFinalitzats)
{
    <form asp-action="Promocionar" asp-route-id="@Model.Id" method="post">
        <button type="submit">Promocionar</button>
    </form>
}

<a asp-action="Editar" asp-route-id="@Model.Id">Editar</a>

<form asp-action="Eliminar" asp-route-id="@Model.Id" method="post">
    <button type="submit">Eliminar</button>
</form>

<p>
    <a asp-action="Index">Tornar a la llista</a>
</p>
```

### 5.3 Vista `Crear.cshtml`

Model: `AfegirAlumneParametres`

Responsabilitat:

- Mostrar un formulari amb els camps `Nom` i `Email`.
- Enviar el formulari per `POST` a l'acció `Crear`.
- No demanar `Curs` ni `EstudisFinalitzats`, perquè aquests valors s'inicialitzen a la capa de negoci.

```cshtml
@model LogicaDeNegoci.Abstractions.Parametres.AfegirAlumneParametres

<h1>Crear alumne</h1>

<form asp-action="Crear" method="post">
    <div>
        <label asp-for="Nom"></label>
        <input asp-for="Nom" />
        <span asp-validation-for="Nom"></span>
    </div>

    <div>
        <label asp-for="Email"></label>
        <input asp-for="Email" />
        <span asp-validation-for="Email"></span>
    </div>

    <button type="submit">Crear</button>
</form>

<p>
    <a asp-action="Index">Tornar a la llista</a>
</p>
```

### 5.4 Vista `Editar.cshtml`

Model: `CanviarDadesAlumneParametres`

Responsabilitat:

- Mostrar un formulari amb les dades actuals de l'alumne.
- Permetre canviar `Nom` i `Email`.
- Mantenir l'`Id` en un camp ocult.
- Enviar el formulari per `POST` a l'acció `Editar`.

```cshtml
@model LogicaDeNegoci.Abstractions.Parametres.CanviarDadesAlumneParametres

<h1>Editar alumne</h1>

<form asp-action="Editar" method="post">
    <input asp-for="Id" type="hidden" />

    <div>
        <label asp-for="Nom"></label>
        <input asp-for="Nom" />
        <span asp-validation-for="Nom"></span>
    </div>

    <div>
        <label asp-for="Email"></label>
        <input asp-for="Email" />
        <span asp-validation-for="Email"></span>
    </div>

    <button type="submit">Desar</button>
</form>
```

### 5.5 Enllaç des de la pàgina d'inici

La pàgina `Web/Views/Home/Index.cshtml` inclou un enllaç cap a la gestió d'estudiants:

```cshtml
<a asp-controller="Alumnes" asp-action="Index" class="btn btn-primary btn-lg">Anar a la gestió d'estudiants</a>
```

## 6. Estructura esperada

```text
Alumnes/
├── Alumnes.slnx
├── DbModels/
├── Repositori.Abstractions/
├── Repositori/
├── Repositori.IntegrationTests/
├── LogicaDeNegoci.Abstractions/
│   ├── ILogicaNegociAlumne.cs
│   └── Parametres/
│       └── SeleccionarPerIdAlumneParametres.cs
├── LogicaDeNegoci/
├── LogicaDeNegoci.UnitTests/
└── Web/
    ├── Web.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── Controllers/
    │   └── AlumnesController.cs
    └── Views/
        ├── Shared/
        │   └── _Layout.cshtml
        └── Alumnes/
            ├── Index.cshtml
            ├── Detalls.cshtml
            ├── Crear.cshtml
            └── Editar.cshtml
```

## 7. Verificar que tot funciona

Compilar tota la solució:

```bash
dotnet build
```

Executar les proves:

```bash
dotnet test
```

Executar l'aplicació web:

```bash
dotnet run --project Web
```

Comprovacions manuals:

- `/Alumnes/Crear` → pots donar d'alta un alumne.
- `/Alumnes` → l'alumne creat apareix a la llista.
- `/Alumnes/Detalls/{id}` → veus les dades de l'alumne.
- `/Alumnes/Editar/{id}` → pots canviar nom i email.
- `Eliminar` → elimina l'alumne i torna a la llista.
- Botó `Promocionar` → puja el curs fins a 3r.
- Quan l'alumne ja és a 3r, promocionar-lo marca `EstudisFinalitzats = true`.
- Quan `EstudisFinalitzats = true`, ja no es mostra el botó de promocionar.

## 8. Aclariments importants

- MVC és capa de presentació. No conté regles de negoci.
- El controlador no calcula el curs següent ni decideix si un alumne ha finalitzat els estudis.
- El controlador no utilitza `AlumnesDbContext` directament.
- El controlador no utilitza `IRepositoriAlumne` directament.
- La promoció és un `POST` perquè modifica dades.
- `EnsureCreated()` és acceptable per aquesta fase didàctica; en una aplicació real caldria usar migracions.

## 9. Resultat final de la Fase 3

En acabar aquesta fase tenim una aplicació MVC funcional que respecta la separació de capes:

```text
Usuari
↓
Vista Razor
↓
AlumnesController
↓
ILogicaNegociAlumne
↓
LogicaNegociAlumne
↓
IRepositoriAlumne
↓
RepositoriAlumne
↓
SQLite
```

La capa web només orquestra peticions HTTP i respostes HTML. La lògica de negoci continua centralitzada a `LogicaDeNegoci`, que és exactament el que buscàvem amb aquesta arquitectura.
