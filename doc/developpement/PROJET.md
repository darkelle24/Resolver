# Contraintes locales — Resolver

Ce document appartient au projet et peut être adapté. La configuration exécutable des suites est [.repo-kit/project.json](../../.repo-kit/project.json) ; reporter ici toute évolution des commandes et contrats.

## But et identité

Compiler et exécuter des fragments JSON avec diagnostics et budgets, en fournissant un cœur C# autonome et un adaptateur Godot distribué séparément.

Identifiant de dépôt pour les références de travaux : `resolver`.
URL déclarée : https://github.com/darkelle24/Resolver

## Frontières à préserver

- Le cœur sous core/ et src/central/ reste indépendant de Godot ; les règles propres aux jeux restent chez leurs consommateurs.
- L'adaptateur Godot et les ressources GDScript restent distincts du cœur ; préserver les conversions explicites et les diagnostics source.
- Préserver les budgets, le déterminisme documenté, le cache et la compatibilité des API ; toute rupture demande une décision et une migration explicites.
- Les ressources addons/resolver/ des consommateurs sont générées par le paquet : ne pas les modifier comme des sources.
- Le workflow NuGet existant fait autorité sur la publication. Ne pas créer ni pousser de tag de publication sans demande explicite.

## Entrées techniques existantes

- [README.md](../../README.md)
- [core/Resolver.Core.csproj](../../core/Resolver.Core.csproj)
- [src/central/runtime/VmEngine.cs](../../src/central/runtime/VmEngine.cs)
- [src/godot/VmEngineNode.cs](../../src/godot/VmEngineNode.cs)
- [tests/Resolver.Tests.csproj](../../tests/Resolver.Tests.csproj)
- [tools/Pack.ps1](../../tools/Pack.ps1)
- [.github/workflows/nuget.yml](../../.github/workflows/nuget.yml)
- [docs/node_catalog.json](../../docs/node_catalog.json)

## Prérequis de validation

- SDK .NET 10 sélectionné par global.json, avec les runtimes et paquets nécessaires. La restauration peut nécessiter le réseau.
- PowerShell 5.1 Windows ou PowerShell 7 pour le packaging ; Godot 4.7.2 .NET pour les recettes moteur décrites dans le README.
- Python 3.11+ et Git pour le kit ; aucun module Python externe requis.

## Suites déclarées

Les commandes sont des tableaux d'arguments exécutés depuis la racine sans interpréteur shell implicite. Les seules substitutions d'exécutable sont `$POWERSHELL` (pwsh ou Windows PowerShell) et `$GODOT` (HEX_GODOT_PATH ou godot).

### Core

```text
["dotnet", "run", "--project", "tests/Resolver.Tests.csproj", "-c", "Release"]
```

### Build

```text
["dotnet", "build", "Resolver.csproj", "-c", "Release"]
```

### Package

```text
["$POWERSHELL", "-NoProfile", "-File", "tools/Pack.ps1"]
```

## Limites de preuve

La présence d'un workflow ne prouve pas une exécution distante. La présence d'une référence de paquet ne prouve pas sa publication. La recette visuelle et l'intégration dans chaque consommateur gardent leurs propres preuves.

## Format local de la roadmap

La demande de planification conserve des tâches directes, sans mini-projets ni niveaux artificiels de milestones/objectifs. La [roadmap](TODO.md) est une vue synthétique ; les fiches sous [travaux/](travaux/) conservent le détail, les critères et les preuves selon le [guide commun](ROADMAP.md). Cette adaptation locale ne modifie pas le socle.

À la demande du 22 novembre 2026, chaque fiche propose des mini-tâches internes `resolver:RNN-TNN`, avec action, résultat vérifiable, dépendances internes et statut. Elles restent dans la fiche existante et consomment ses prérequis applicables, sans milestone ni fiche supplémentaire. Leur création ne constitue ni réalisation ni accord technique ; clôturer la tâche exige toujours ses critères et preuves d’ensemble.

Conserver les identifiants historiques `resolver:R01` à `resolver:R36` ; `resolver:R37` distingue l'exécution d'une publication de la préparation d'une candidate. Les priorités et choix non approuvés restent proposés. Les idées non retenues restent « À étudier ». La publication, la fusion et l'adoption chez un consommateur ont leurs accords et preuves propres.
