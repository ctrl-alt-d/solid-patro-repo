# Documentació Fase 1

## Creació dels projectes des de CLI

En aquesta fase creem l'estructura base del projecte amb tots els assemblies necessaris per implementar el patró Repository i Unit of Work.

### 1. Crear la solució

```bash
dotnet new sln -n Alumnes
```

Això crea un arxiu `Alumnes.sln` que actuarà com a contenidor de tots els projectes.

### 2. Crear els projectes

#### 2.1 Projecte `DbModels`

```bash
dotnet new classlib -n DbModels
```

**Responsabilitat**: Definir els models de dades que es persistiran a la base de dades.
- `Alumne`: Classe que representa un alumne amb propietats com `Id`, `Nom`, `Cognoms`, `DataNaixement`, etc.

#### 2.2 Projecte `Repositori.Abstractions`

```bash
dotnet new classlib -n Repositori.Abstractions
```

**Responsabilitat**: Definir les interfícies i contractes per accedir a les dades (CRUD).
- `IRepositoriAlumne`: Interfície amb mètodes com `LlegirAlumnePerId()`, `LlegirTots()`, `InsertaAlumne()`, `ActualitzaAlumne()`, `EsborraAlumne()`, etc.
- `IUnityOfWork`: Interfície que agrupa els repositoris i gestiona les transaccions.

**Dependències**:
```bash
dotnet add Repositori.Abstractions reference DbModels
```

#### 2.3 Projecte `Repositori`

```bash
dotnet new classlib -n Repositori
```

**Responsabilitat**: Implementar les interfícies definides en `Repositori.Abstractions`.
- `RepositoriAlumne`: Implementa `IRepositoriAlumne` amb la lògica d'accés a dades.
- `UnityOfWork`: Implementa `IUnityOfWork`.

**Dependències**:
```bash
dotnet add Repositori reference DbModels Repositori.Abstractions
```

**Paquet NuGet** (per a Entity Framework Core):
```bash
dotnet add Repositori package Microsoft.EntityFrameworkCore.Sqlite
```

#### 2.4 Projecte `Repositori.IntegrationTests`

```bash
dotnet new xunit -n Repositori.IntegrationTests
```

**Responsabilitat**: Validar que la persistència funciona correctament amb una base de dades real.
- Tests d'integració que verifiquen les operacions CRUD.

**Dependències**:
```bash
dotnet add Repositori.IntegrationTests reference DbModels Repositori.Abstractions Repositori
dotnet add Repositori.IntegrationTests package Microsoft.EntityFrameworkCore.Sqlite
```

### 3. Afegir els projectes a la solució

```bash
dotnet sln add DbModels/DbModels.csproj
dotnet sln add Repositori.Abstractions/Repositori.Abstractions.csproj
dotnet sln add Repositori/Repositori.csproj
dotnet sln add Repositori.IntegrationTests/Repositori.IntegrationTests.csproj
```

O de forma més ràpida:

```bash
dotnet sln add **/*.csproj
```

### 4. Estructura del projecte

Un cop finalitzada aquesta fase, l'estructura ha de ser:

```
Alumnes/
├── Alumnes.sln
├── DbModels/
│   ├── DbModels.csproj
│   └── Alumne.cs
├── Repositori.Abstractions/
│   ├── Repositori.Abstractions.csproj
│   ├── IRepositoriAlumne.cs
│   └── IUnityOfWork.cs
├── Repositori/
│   ├── Repositori.csproj
│   ├── RepositoriAlumne.cs
│   └── UnityOfWork.cs
└── Repositori.IntegrationTests/
    ├── Repositori.IntegrationTests.csproj
    ├── RepositoriAlumneTests.cs
    └── UnitOfWorkTests.cs
```

### 5. Verificar que tot funciona

```bash
dotnet build
dotnet test
```

Aquesta fase estableix les bases per implementar el patró Repository i Unit of Work de forma correcta.

