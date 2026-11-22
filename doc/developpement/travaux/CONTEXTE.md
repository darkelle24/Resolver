# Contexte de planification — Resolver

Révision documentaire du 21 novembre 2026. Point d’entrée : [roadmap synthétique](../TODO.md). Ce contexte conserve les constats et la méthode ; les statuts et critères de réalisation appartiennent aux fiches R01–R37.

## Snapshot et limites du constat

- Racine : dépôt [Resolver](https://github.com/darkelle24/Resolver), branche `docs/roadmap-resolver` réutilisée après examen.
- Base Git de cette révision : `51e47cb556f83ba8789aa43f718709f393b42ee2` ; arbre et index propres au démarrage. Cette base inclut déjà l’ancienne roadmap et le kit `1.2.0` ; elle n’est pas le commit de la présente révision.
- L’ancienne roadmap décrivait encore la base `ead75c3dd8bc20ce04a616d8cdaad5d69f66a0ff` et des changements non commités. Cette information appartient à la première rédaction, désormais historique.
- La comparaison Git de cette ancienne base à la base actuelle ne montre aucune modification des sources runtime, tests, projets, packaging et workflow NuGet examinés. Les constats ci-dessous sont conservés comme lecture ciblée, sans les transformer en nouvelles exécutions de code.
- Sources, projets, versions et commandes utiles ont été relus ; Python `3.11.5` et SDK effectif `10.0.403` constatés sur Windows. Moteur Godot, CI, NuGet distant et consommateurs non vérifiés à nouveau.

## Capacités existantes et prérequis

| Domaine | Constat dans les sources et documents | Conséquence pour la roadmap |
|---|---|---|
| Compilation et cache | [VmEngine](../../../src/central/runtime/VmEngine.cs) expose compilation textuelle, fragments parsés, cache et handles réutilisables | Consolider les contrats et limites, sans présenter ces API comme une fonctionnalité à créer |
| Langage et extensions | [Catalogue exporté](../../../docs/node_catalog.json), registre de nœuds, callbacks et compilateurs avancés présents ; le smoke Godot attend 41 nœuds et 5 types littéraux | Contrôler cohérence, évolution, compatibilité et qualité des extensions |
| Exécution | [Options](../../../src/central/runtime/VmContracts.cs) : limites d'instructions, trace, allocations, collections et chaînes | Vérifier les frontières et interactions, plutôt que promettre une sandbox complète |
| Entrée JSON | [Lecteur](../../../src/central/compiler/LocatedJson.cs) : limite 16 Mio, profondeur 128 et refus des clés dupliquées | Mesurer aussi les allocations et traitements avant/après ces contrôles |
| Diagnostics | Erreurs typées, JSON Pointer, provenance, export du programme et explications présents | Stabiliser les formats et leur consommation externe |
| Objets hôtes | [Registre](../../../src/central/security/ExposedMethodRegistry.cs) pour les méthodes ; [adaptateur Godot](../../../src/godot/GodotHostObjects.cs) pour propriétés, appels et libération | Examiner séparément chaque capacité : le filtrage des méthodes ne définit pas celui des propriétés |
| Distribution | [Packaging](../../../tools/Pack.ps1), sources Godot générées et [workflow NuGet](../../../.github/workflows/nuget.yml) présents | Vérifier les paquets, mises à niveau et exécutions réelles avant livraison |
| Tests | [Programme de tests C#](../../../tests/Program.cs) et [smoke GDScript](../../../tests/godot/smoke.gd) présents | Présence de scénarios distincte de leur exécution sur une version donnée |
| Historique de validation | Le [guide de développement](../README.md) consigne un essai local du 21 novembre : 64/64 tests C#, build et packaging réussis, avec ses limites | Preuve historique déclarée dans ce guide, pas nouvelle exécution ni recette moteur de cette roadmap |
| Publication et adoption | Version de développement `0.1.0` ; le README indique qu'aucun paquet n'a encore été publié ; aucune vérification distante NuGet faite ici | Disponibilité des noms, version publiée, CI distante et version adoptée restent à constater |

Cette lecture ciblée n'est pas un audit exhaustif du code. Une tâche de consolidation peut aboutir à « contrat déjà satisfait » avec une preuve ; elle n'impose pas une réécriture. Aucun pourcentage d'avancement n'est déduit de la présence du code.

### Versions et prérequis constatés

| Élément consommé | Version ou contrat constaté | Disponibilité / limite de preuve |
|---|---|---|
| SDK .NET | [global.json](../../../global.json) : `10.0.100`, `latestFeature`, sans prerelease ; projets runtime `net10.0` | `dotnet --version` retourne `10.0.403` lors de cette révision |
| Godot | [Projet adaptateur](../../../Resolver.csproj) : `Godot.NET.Sdk/4.7.2` ; workflow : moteur .NET `4.7.2` | Configuration lue ; installation moteur, téléchargement et recette non vérifiés ici |
| Générateur | [Projet Roslyn](../../../generator/Generator.csproj) : `netstandard2.0`, CodeAnalysis.CSharp `4.8.0`, Analyzers `3.3.4` | Références lues ; compatibilité effective à vérifier avec les SDK supportés |
| Paquets Resolver | [Version centrale](../../../Directory.Build.props), script Pack et workflow : `0.1.0` par défaut ; références consommateur exactes | Valeur déclarée, sans preuve de publication ; vérifier leur cohérence à chaque livraison |
| Contrats exportés | Catalogue JSON `format_version: 1`, versions/empreintes de registre et nœuds ; explication versionnée | Ne pas confondre version de paquet, de format, de nœud, version du registre et empreintes |
| Kit de documentation | Python `3.11+`, Git et fichiers locaux ; PowerShell 5.1 Windows ou 7 pour Pack | Python `3.11.5` constaté ; contrôles de cette révision dans la [revue](REVUE_ROADMAP.md) |
| Consommateur connu | [HexGameGodot et guide d'intégration](../DEPENDANCES.md) | Version adoptée et preuves côté jeu non consultées ; ne pas les supposer |

La [stack d’assistant optionnelle](../ASSISTANTS_DOCKER.md) est un outil de développement séparé du runtime Resolver. Graphify, Obsidian, Hermes, Docker et un modèle local ne sont pas des prérequis de cette roadmap ou de la bibliothèque. Leur présence ne prouve ni leur recette ni une revue indépendante ; aucun setup ou service n’est lancé par cette révision.

## Méthode inspirée d’Elephant Goldfish

Source consultée le 21 novembre 2026 : [README EGM](https://github.com/google/elephant-goldfish-model/blob/7d1b721e51448518cc9b0336465041230200dd10/README.md), révision affichée `7d1b721e51448518cc9b0336465041230200dd10`. Le modèle distingue la conception avec contexte, la revue dans un contexte neuf et l'arbitrage humain.

Adaptation proposée pour Resolver, sans installer le plugin :

1. **Contexte durable (« Elephant »).** Partir des sources, contrats, contraintes, décisions et preuves du dépôt. Décrire le problème réel, le comportement attendu et les alternatives avant une modification importante.
2. **Document autonome.** Pour la tâche choisie, rendre explicites entrées, sorties, effets de bord, limites, dépendances versionnées et critère de fin. La fiche de tâche conserve critères, scénarios et preuves ; une modification majeure de contrat justifie aussi une décision locale.
3. **Revue à contexte neuf (« Goldfish »).** Fournir à un relecteur le document et ses références, sans la conversation de conception. Lui faire chercher ambiguïtés, contre-exemples, incompatibilités et preuves manquantes. Une relecture par le même agent dans le même contexte ne vaut pas revue indépendante EGM.
4. **Arbitrage explicite.** Consigner les questions retenues, écartées ou encore ouvertes ; le responsable du composant décide des compromis et des ruptures. Une proposition de roadmap ne vaut pas accord d'architecture.
5. **Réalisation et preuve.** Réaliser uniquement le périmètre autorisé, vérifier les scénarios utiles sur la version exacte, puis rattacher les résultats à la tâche.
6. **Contrôle de dérive.** À chaque évolution de contrat ou livraison, comparer code, catalogue, documentation, exemples et formats exportés. Corriger l'écart au bon endroit avant de déclarer le résultat terminé.

Cette rédaction applique la collecte de contexte et une vérification de cohérence. **Aucune revue indépendante Goldfish ni approbation humaine des choix proposés n'a été réalisée.** La revue à contexte neuf reste une étape proposée avant les changements structurants ; aucune exécution automatique ou récurrente n'est installée.

La source EGM épinglée ci-dessus a été consultée lors de la première rédaction de cette conversation. Cette révision adapte l’organisation aux règles locales ; elle ne revendique ni installation du plugin ni nouvelle exécution de son workflow.

## Reprendre un travail

Choisir une fiche depuis l’[index](../TODO.md), relire ses prérequis et leurs preuves, puis relever les SHA/versions effectivement consommés. Les valeurs « à produire » des fiches sont des livrables futurs identifiés, pas une preuve de disponibilité. Conserver les décisions fournisseur dans son [registre](../decisions/README.md) et les recettes d’adoption dans le dépôt consommateur.
