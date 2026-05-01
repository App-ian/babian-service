# Babian Backend (.NET 8 Clean Architecture)

Ce dépôt contient le backend de Babian V0 (système de bourse aux boissons), développé en **.NET 8** avec une approche **Clean Architecture** et une stricte découpe en **Vertical Slices**.

## Objectifs de l'Architecture

Le code est organisé pour maximiser la testabilité unitaire, l'isolation du domaine métier, et faciliter la maintenance future. Il implémente les patterns **CQRS** via `MediatR` et utilise `Entity Framework Core` avec MariaDB.

## Structure de la Solution (14 Projets)

### 1. Core & Domaine
- `Babian.Domain` : Entités pures (`Drink`, `MarketSession`, etc.), Interfaces (Repositories). *Aucune dépendance externe*.
- `Babian.Common` : Code partagé, Exceptions métiers.

### 2. Business Layers (Vertical Slices)
Regroupés dans le dossier `/src/BusinessLayers/`, ces projets contiennent la logique d'application spécifique pour chaque entité (Commandes, Handlers, DTOs, Validation) :
- `Babian.BusinessLayers.Drinks`
- `Babian.BusinessLayers.GlobalDrinks`
- `Babian.BusinessLayers.MarketConfigs`
- `Babian.BusinessLayers.MarketEvents`
- `Babian.BusinessLayers.MarketSessions`
- `Babian.BusinessLayers.Orders`
- `Babian.BusinessLayers.PriceHistory`
- `Babian.BusinessLayers.Profiles`
- `Babian.BusinessLayers.MarketEngine` *(Contient le PriceEngine pur)*

### 3. Infrastructure & Services
- `Babian.Infrastructure.Persistence` : Implémentations EF Core (DbContext) et mapping vers MariaDB.
- `Babian.Infrastructure.Services` : Clients HTTP externes (Refit vers CSI POS).

### 4. API (Presentation)
- `Babian.Service` : Le projet d'hôte WebAPI. Il contient les Controllers REST exposant les cas d'usages via MediatR, Swagger, et l'injection des dépendances (IoC).

### 5. Tests
Regroupés dans le dossier `/src/Tests/`, garantissant la robustesse :
- 12 Projets **Unitaires** (Couvrant le Domaine et chaque BusinessLayer).
- 1 Projet d'**Intégration** (Testant les requêtes EF Core sur une base de données MariaDB conteneurisée).

## Technologies Utilisées
- **.NET 8** (C# 12)
- **Entity Framework Core 8** (MariaDB)
- **MediatR** (Orchestration CQRS)
- **FluentValidation** (Validation des requêtes entrantes)
- **Serilog** (Logs structurés)
- **Refit** (Appels HTTP simples vers des Webhooks/APIs tierces)
- **xUnit**, **FluentAssertions**, **Moq/NSubstitute** (Tests)

## Démarrage Rapide

1. Assurez-vous d'avoir le SDK **.NET 8** installé.
2. Restaurez les dépendances :
   ```bash
   dotnet restore
   ```
3. (Recommandé) Lancez la base de données via Docker :
   ```bash
   docker-compose up -d
   ```
4. Compilez la solution :
   ```bash
   dotnet build
   ```
5. Lancez les tests :
   ```bash
   dotnet test
   ```
6. Démarrez l'API (depuis la racine `Babian.Backend`) :
   ```bash
   cd Babian.Backend
   dotnet run --project src/Babian.Service/Babian.Service.csproj
   ```
   *(Pour arrêter l'application, utilisez `Ctrl+C` dans le terminal)*.

## Dépannage

### Erreur "Address already in use" (Port 5057)
Si vous obtenez une erreur indiquant que le port `5057` est déjà utilisé (souvent après un crash ou un arrêt forcé), utilisez la commande suivante pour libérer le port :

```bash
lsof -ti:5057 | xargs kill -9
```

### Forcer l'arrêt de .NET
Pour arrêter tous les processus .NET en cours d'exécution sur votre machine :
```bash
killall dotnet
```

7. Accédez à l'interface Swagger sur `http://localhost:5057/swagger`.

## Configuration & Environnements

- **appsettings.json** : Paramètres globaux partagés.
- **appsettings.Development.json** : Paramètres spécifiques au développement (ex: Chaîne de connexion locale).
- **ASPNETCORE_ENVIRONMENT** : Défini sur `Development` dans `Properties/launchSettings.json`.

Pour modifier la base de données locale, éditez `src/Babian.Service/appsettings.Development.json`.
