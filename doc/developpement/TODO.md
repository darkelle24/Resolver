# Roadmap — Resolver

Version de planification : 21 novembre 2026. Cette roadmap concerne les capacités autonomes de `resolver`, leur qualité et leur maintenance. Les besoins produit et l'adoption d'une version restent suivis chez chaque consommateur.

**Format demandé : une seule roadmap, des tâches directes, aucun mini-projet.** Les axes ci-dessous servent à la lecture ; ils ne créent ni sous-projet, ni arborescence milestone/tâche/objectif, ni collection de fiches à remplir. Une tâche se suit directement ici sous un identifiant stable `resolver:RNN`. Une décision de contrat importante conserve sa place dans le [registre local](decisions/README.md).

Ce format applique la demande explicite de cette session en adaptant le [guide général de roadmap](ROADMAP.md) : les critères et preuves restent dans ce document plutôt que dans une fiche distincte par tâche.

La demande autorise la préparation de cette roadmap. Les priorités, seuils futurs et évolutions restent des propositions ; elle n'autorise pas leur réalisation globale, une rupture de contrat ou une publication.

## 1. Résultat visé et périmètre

Faire de Resolver une bibliothèque de modding dont un hôte C# ou Godot peut expliquer les résultats, borner les coûts, comprendre les effets de bord et adopter les versions sans surprise.

Le composant doit permettre de :

- Compiler une expression JSON ou un fragment sélectionné, puis réutiliser son handle sans reparsing pendant l'exécution.
- Exécuter les opérations et extensions autorisées avec des règles de valeurs explicites, des diagnostics source et des budgets cohérents.
- Produire une explication fidèle, limitée en volume et exploitable par un outil externe sans réexécuter les callbacks.
- Distribuer séparément le cœur autonome et l'adaptateur Godot, puis vérifier le comportement des paquets réellement consommés.
- Faire évoluer ses contrats, ses dépendances et ses versions avec des preuves et une migration lorsque nécessaire.

Les [contraintes locales](PROJET.md) restent applicables. Le chargement/fusion des mods, les règles de combat/économie, la sélection des événements à compiler, l'interface d'un jeu et son adoption NuGet appartiennent aux consommateurs. Un visualiseur ou éditeur externe possède sa propre roadmap ; Resolver fournit ses contrats d'échange.

## 2. État de départ : ce qui existe et ce qui reste à prouver

Base Git examinée : `ead75c3dd8bc20ce04a616d8cdaad5d69f66a0ff`, clone [Resolver](https://github.com/darkelle24/Resolver), branche initiale `main`. Le kit et sa documentation étaient déjà préparés dans l'index, sans commit. Cette roadmap est rédigée sur `docs/roadmap-resolver` et ne constitue pas une version livrée.

| Domaine | Constat dans les sources et documents | Conséquence pour la roadmap |
|---|---|---|
| Compilation et cache | [VmEngine](../../src/central/runtime/VmEngine.cs) expose compilation textuelle, fragments parsés, cache et handles réutilisables | Consolider les contrats et limites, sans présenter ces API comme une fonctionnalité à créer |
| Langage et extensions | [Catalogue exporté](../../docs/node_catalog.json), registre de nœuds, callbacks et compilateurs avancés présents ; le smoke Godot attend 41 nœuds et 5 types littéraux | Contrôler cohérence, évolution, compatibilité et qualité des extensions |
| Exécution | [Options](../../src/central/runtime/VmContracts.cs) : limites d'instructions, trace, allocations, collections et chaînes | Vérifier les frontières et interactions, plutôt que promettre une sandbox complète |
| Entrée JSON | [Lecteur](../../src/central/compiler/LocatedJson.cs) : limite 16 Mio, profondeur 128 et refus des clés dupliquées | Mesurer aussi les allocations et traitements avant/après ces contrôles |
| Diagnostics | Erreurs typées, JSON Pointer, provenance, export du programme et explications présents | Stabiliser les formats et leur consommation externe |
| Objets hôtes | [Registre](../../src/central/security/ExposedMethodRegistry.cs) pour les méthodes ; [adaptateur Godot](../../src/godot/GodotHostObjects.cs) pour propriétés, appels et libération | Examiner séparément chaque capacité : le filtrage des méthodes ne définit pas celui des propriétés |
| Distribution | [Packaging](../../tools/Pack.ps1), sources Godot générées et [workflow NuGet](../../.github/workflows/nuget.yml) présents | Vérifier les paquets, mises à niveau et exécutions réelles avant livraison |
| Tests | [Programme de tests C#](../../tests/Program.cs) et [smoke GDScript](../../tests/godot/smoke.gd) présents | Présence de scénarios distincte de leur exécution sur une version donnée |
| Historique de validation | Le [guide de développement](README.md) consigne un essai local du 21 novembre : 64/64 tests C#, build et packaging réussis, avec ses limites | Preuve historique déclarée dans ce guide, pas nouvelle exécution ni recette moteur de cette roadmap |
| Publication et adoption | Version de développement `0.1.0` ; le README indique qu'aucun paquet n'a encore été publié ; aucune vérification distante NuGet faite ici | Disponibilité des noms, version publiée, CI distante et version adoptée restent à constater |

Cette lecture ciblée n'est pas un audit exhaustif du code. Une tâche de consolidation peut aboutir à « contrat déjà satisfait » avec une preuve ; elle n'impose pas une réécriture. Aucun pourcentage d'avancement n'est déduit de la présence du code.

### Versions et prérequis constatés

| Élément consommé | Version ou contrat constaté | Disponibilité / limite de preuve |
|---|---|---|
| SDK .NET | [global.json](../../global.json) : `10.0.100`, `latestFeature`, sans prerelease ; projets runtime `net10.0` | `dotnet --version` retourne `10.0.403` sur cet hôte |
| Godot | [Projet adaptateur](../../Resolver.csproj) : `Godot.NET.Sdk/4.7.2` ; workflow : moteur .NET `4.7.2` | Configuration lue ; installation moteur, téléchargement et recette non vérifiés ici |
| Générateur | [Projet Roslyn](../../generator/Generator.csproj) : `netstandard2.0`, CodeAnalysis.CSharp `4.8.0`, Analyzers `3.3.4` | Références lues ; compatibilité effective à vérifier avec les SDK supportés |
| Paquets Resolver | [Version centrale](../../Directory.Build.props), script Pack et workflow : `0.1.0` par défaut ; références consommateur exactes | Valeur déclarée, sans preuve de publication ; vérifier leur cohérence à chaque livraison |
| Contrats exportés | Catalogue JSON `format_version: 1`, versions/empreintes de registre et nœuds ; explication versionnée | Ne pas confondre version de paquet, de format, de nœud, version du registre et empreintes |
| Kit de documentation | Python `3.11+`, Git et fichiers locaux ; PowerShell 5.1 Windows ou 7 pour Pack | Python `3.11.5` constaté ; contrôle documentaire de cette modification consigné en section 12 |
| Consommateur connu | [HexGameGodot et guide d'intégration](DEPENDANCES.md) | Version adoptée et preuves côté jeu non consultées ; ne pas les supposer |

## 3. Méthode inspirée d'Elephant Goldfish

Source consultée le 21 novembre 2026 : [README EGM](https://github.com/google/elephant-goldfish-model/blob/7d1b721e51448518cc9b0336465041230200dd10/README.md), révision affichée `7d1b721e51448518cc9b0336465041230200dd10`. Le modèle distingue la conception avec contexte, la revue dans un contexte neuf et l'arbitrage humain.

Adaptation proposée pour Resolver, sans installer le plugin :

1. **Contexte durable (« Elephant »).** Partir des sources, contrats, contraintes, décisions et preuves du dépôt. Décrire le problème réel, le comportement attendu et les alternatives avant une modification importante.
2. **Document autonome.** Pour la tâche choisie, rendre explicites entrées, sorties, effets de bord, limites, dépendances versionnées et critère de fin. Une note dans cette roadmap suffit tant que le changement reste simple ; une modification majeure de contrat justifie une décision locale.
3. **Revue à contexte neuf (« Goldfish »).** Fournir à un relecteur le document et ses références, sans la conversation de conception. Lui faire chercher ambiguïtés, contre-exemples, incompatibilités et preuves manquantes. Une relecture par le même agent dans le même contexte ne vaut pas revue indépendante EGM.
4. **Arbitrage explicite.** Consigner les questions retenues, écartées ou encore ouvertes ; le responsable du composant décide des compromis et des ruptures. Une proposition de roadmap ne vaut pas accord d'architecture.
5. **Réalisation et preuve.** Réaliser uniquement le périmètre autorisé, vérifier les scénarios utiles sur la version exacte, puis rattacher les résultats à la tâche.
6. **Contrôle de dérive.** À chaque évolution de contrat ou livraison, comparer code, catalogue, documentation, exemples et formats exportés. Corriger l'écart au bon endroit avant de déclarer le résultat terminé.

Cette rédaction applique la collecte de contexte et une vérification de cohérence. **Aucune revue indépendante Goldfish ni approbation humaine des choix proposés n'a été réalisée.** La revue à contexte neuf reste une étape proposée avant les changements structurants ; aucune exécution automatique ou récurrente n'est installée.

## 4. Comment suivre les tâches

- **P0** : lever une incertitude fondamentale ou un risque avant une première livraison destinée à être adoptée.
- **P1** : fiabilité, exploitation et compatibilité à consolider avant d'annoncer une stabilité générale.
- **P2** : évolution conditionnelle, à engager seulement pour un besoin confirmé.
- Statuts d'exécution : `A_FAIRE`, `EN_COURS`, `BLOQUEE`, `TERMINEE`. Toutes les tâches ci-dessous commencent `A_FAIRE` ; les priorités restent proposées.
- Recommandation : une seule tâche `EN_COURS` par personne ; faire passer une dépendance `BLOQUEE` avec cause et condition de déblocage, sans arrêter les tâches indépendantes.
- Les dépendances `RNN` désignent le livrable accepté de cette tâche, avec sa version et sa preuve, et non un simple numéro ou une fiche créée. Elles n'interdisent pas d'examiner les sujets en parallèle.
- Une dépendance prime sur l'étiquette de priorité : R13–R15 et R26, bien que P1, alimentent des critères P0 de rechargement/documentation, parité Godot ou distribution. Les traiter avant la candidate lorsque ces critères les consomment ; « P1 » ne signifie pas facultatif.
- Lorsqu'une tâche démarre, ajouter ici responsable, branche, version/SHA du code et des prérequis. Lorsqu'elle se termine, ajouter commande/recette, date, environnement, résultat et preuve. Ne pas inventer de date de livraison avant mesure de la charge et disponibilité du responsable.

Les sections 5 à 9 détaillent **une liste plate de 36 tâches**. Les axes n'ont ni statut ni livraison séparée. Une évolution P2 non retenue se consigne comme proposition écartée, sans la marquer `TERMINEE`.

## 5. Définir et protéger les contrats

### R01 — Établir la référence de validation · P0 · A_FAIRE

- **À faire :** identifier le snapshot à valider, relancer les contrôles existants utiles, inventorier capacités testées, scénarios manquants et validations externes. Réconcilier les indications du README avec la réalité du dépôt déjà créé.
- **Prérequis :** sources identifiées, outils du tableau des versions et accès réseau lorsque requis ; aucun autre travail RNN.
- **Fin / preuve :** bilan daté distinguant tests sources, build, paquets, Godot, CI et consommateur ; chaque contrôle est `REUSSIE`, `ECHEC`, `NON_EXECUTEE` ou `NON_APPLICABLE` justifié. Un échec externe est attribué au prérequis concerné.

### R02 — Fixer la matrice de support · P0 · A_FAIRE

- **À faire :** définir SDK .NET, Godot .NET, systèmes et cibles d'export réellement supportés ; vérifier disponibilité des versions/actions/outils, restaurations et compatibilité Roslyn. Distinguer versions testées et versions seulement envisagées.
- **Prérequis :** inventaire R01 ; besoins des consommateurs avec leur source et version lorsqu'ils sont disponibles.
- **Fin / preuve :** matrice acceptée et commandes reproductibles sur les combinaisons retenues. Pas de promesse mobile, Web, AOT ou multiplateforme à partir d'un seul build Windows.

### R03 — Rendre le langage et l'API normatifs · P0 · A_FAIRE

- **À faire :** définir grammaire JSON, alias historiques, entrées directes/`inputs`, paramètres fixes, variables, ordre d'évaluation, court-circuit, `null`, champs inconnus et erreurs d'argument. Décrire aussi l'API C# et sa façade GDScript.
- **Prérequis :** R01 ; catalogue et API du snapshot choisi.
- **Fin / preuve :** chaque opération publique a un contrat d'entrées/sorties et un exemple valide/invalide vérifiable ; les usages actuellement compatibles sont conservés ou une migration est explicitement décidée.

### R04 — Délimiter le déterminisme et les règles numériques · P0 · A_FAIRE

- **À faire :** préciser conversions, débordements Int32, Float32, NaN/infini produits à l'exécution, division/modulo, comparaisons, égalité des collections et ordre des dictionnaires. Définir l'influence de la culture, de la plateforme et des callbacks hôtes.
- **Prérequis :** R02 pour la portée multiplateforme et R03 pour les opérations.
- **Fin / preuve :** mêmes entrées contrôlées et mêmes versions donnent les résultats promis dans la matrice retenue ; tests aux limites et cultures distinctes. Ne pas promettre une identité flottante universelle ni le déterminisme d'un callback arbitraire.

### R05 — Stabiliser les formats d'échange · P0 · A_FAIRE

- **À faire :** documenter formats/versionnement des résultats, erreurs, traces, catalogue, provenance et export d'inspection ; fixer le sens des champs absents, `null`, `unavailable`, `truncated`, indices, colonnes UTF-8 et JSON Pointer.
- **Prérequis :** R03 ; exemples exportés depuis le code identifié.
- **Fin / preuve :** fixtures représentatives et politique d'évolution additive/incompatible acceptée ; un lecteur peut comprendre une trace tronquée, un fragment fusionné et une erreur sans position. L'export de programme reste un format d'inspection, sans promesse de chargement de bytecode externe.

### R06 — Assurer la cohérence du catalogue · P0 · A_FAIRE

- **À faire :** contrôler que les nœuds et littéraux exportés correspondent au registre, aux exemples et aux types effectifs ; préciser version de nœud, empreinte, remplacement de callback et invalidation du cache. Prévoir un contrôle de dérive du catalogue versionné.
- **Prérequis :** R03 et R05 ; commande d'export existante dans les tests.
- **Fin / preuve :** exporter vers un fichier temporaire, comparer sémantiquement au catalogue suivi et compiler tous les exemples ; une modification de contrat et un remplacement d'implémentation sont distingués par les identifiants prévus.

## 6. Borner les coûts, les accès et le cycle de vie

### R07 — Vérifier les limites de compilation · P0 · A_FAIRE

- **À faire :** examiner texte, fichier, JsonNode/JsonElement et conversion Godot : taille, profondeur, largeur, nombre de nœuds/instructions/constantes et coût des métadonnées. Mesurer les copies, encodages et hash effectués avant rejet ; ne pas se limiter au contrôle du lecteur JSON.
- **Prérequis :** R03 ; limites existantes à préserver, décisions explicites pour en ajouter ou modifier.
- **Fin / preuve :** corpus trop volumineux, trop profond et très large rejeté de manière contrôlée avec diagnostic utile ; charge de compilation observée et limites documentées. Toute nouvelle option publique a un comportement par défaut et une compatibilité définis.

### R08 — Vérifier les budgets d'exécution · P0 · A_FAIRE

- **À faire :** couvrir chaque opération intégrée, copies, concaténations, arguments, collections imbriquées et toutes les frontières des budgets ; préciser les valeurs injectées, retours de callbacks et coûts de sérialisation qui sont inclus ou exclus.
- **Prérequis :** R03–R04 ; options actuelles et règles d'allocation du snapshot.
- **Fin / preuve :** scénarios juste avant/à/après les limites et boucles infinies ; erreur source cohérente, pas de croissance non bornée dans le périmètre annoncé, même avec trace active. Les budgets VM ne sont pas présentés comme une limite mémoire/temps du processus ou du code natif.

### R09 — Définir les permissions sur les objets hôtes · P0 · A_FAIRE

- **À faire :** inventorier lecture/écriture de propriétés, appels, introspection et libération ; examiner héritage, exclusions, `AllowAll`, portée statique du registre et isolation entre hôtes. Arbitrer une éventuelle politique de capacités plus restrictive.
- **Prérequis :** R03 ; décision locale avant toute restriction incompatible des usages actuels.
- **Fin / preuve :** tableau des accès permis/refusés et scénarios avec objets invalides/libérés, méthodes exclues et propriétés sensibles ; un refus ne produit pas l'effet interdit. Distinguer politique fournie par Resolver et configuration obligatoire de l'hôte.

### R10 — Encadrer les callbacks et leurs erreurs · P0 · A_FAIRE

- **À faire :** préciser code de confiance, validation des arguments/retours, exceptions, mutations, réentrance, remplacement et durée de vie ; définir ce qui se passe si un callback bloque ou appelle une opération externe.
- **Prérequis :** R03, R08–R09 ; contrat de RegisterFunction et des nœuds projet.
- **Fin / preuve :** un callback n'est exécuté qu'une fois par appel prévu, y compris avec Explain ; exceptions et retours invalides deviennent les diagnostics annoncés. L'arrêt des instructions VM n'est pas vendu comme une interruption d'un callback bloqué ; une isolation supplémentaire reste une décision motivée par le besoin.

### R11 — Clarifier les mutations et l'échec partiel · P0 · A_FAIRE

- **À faire :** documenter propriété et partage des collections/objets, copies des constantes, variables fournies, instantanés des entrées et effets déjà produits lorsqu'une instruction échoue. Définir si une transaction est un besoin de l'hôte, sans supposer de rollback VM.
- **Prérequis :** R03 et R10.
- **Fin / preuve :** scénarios d'alias, mutations après capture de trace, appels répétés et échec après écriture ; aucune garantie d'atomicité ou d'absence de mutation n'est annoncée sans mécanisme et preuve correspondants.

### R12 — Définir le contrat de concurrence · P1 · A_FAIRE

- **À faire :** distinguer exécution simultanée d'un handle, compilation, modification du registre, callbacks, logger et objets partagés ; préciser ce que les verrous internes garantissent et ce que l'hôte doit synchroniser.
- **Prérequis :** R10–R11 et matrice R02.
- **Fin / preuve :** stress ciblé et scénarios de réentrance sur les usages supportés ; documentation des usages interdits, notamment accès Godot hors thread autorisé. Des verrous présents ne suffisent pas à déclarer toute l'API thread-safe.

### R13 — Maîtriser la durée de vie du cache et des handles · P1 · A_FAIRE

- **À faire :** mesurer rétention mémoire avec beaucoup de scripts/mods, échecs de recompilation et changements de registre ; choisir si taille maximale, éviction ou suppression ciblée est nécessaire. Préserver les contrats de ClearCache et des anciens handles.
- **Prérequis :** R06, R11 ; corpus de charge décrit et versionné.
- **Fin / preuve :** charge/rechargements répétés mesurés ; source, métadonnées et registre invalident comme prévu ; ancien handle encore utilisable selon son contrat. Une stratégie d'éviction éventuelle ne détruit pas les handles possédés par le consommateur.

### R14 — Fiabiliser le rechargement côté bibliothèque · P1 · A_FAIRE

- **À faire :** spécifier compilation réussie avant remplacement, échec conservant le dernier handle valide, snapshot source exact et provenance à jour. Expliquer le cas d'un ensemble de scripts à remplacer ensemble et la responsabilité d'orchestration de l'hôte.
- **Prérequis :** R06, R11 et R13.
- **Fin / preuve :** bon script → modification invalide → correction, avec exécutions sur ancien/nouveau handle et attribution correcte des sources. Aucun watcher automatique, fusionneur ou mécanisme d'adoption propre au jeu ajouté implicitement.

## 7. Consolider les extensions, diagnostics et preuves de comportement

### R15 — Garantir les contrats des nœuds personnalisés · P1 · A_FAIRE

- **À faire :** couvrir noms/collisions, paramètres requis/enum, types d'entrée/sortie, variadiques, ordre, snapshot des définitions, remplacement et compatibilité d'anciens handles. Vérifier les compilateurs avancés et leur contrat de pile.
- **Prérequis :** R03, R06 et R10–R11.
- **Fin / preuve :** une extension valide se documente et s'exécute sans modification centrale ; une définition invalide est refusée sans enregistrement partiel ; les traces et diagnostics restent exploitables.

### R16 — Sécuriser le générateur et le chemin historique · P1 · A_FAIRE

- **À faire :** inventorier les consommateurs de RuleNode/Assembler et du générateur Roslyn ; vérifier émission, relocalisation des sauts/liaisons et erreurs de génération. Choisir maintien ou dépréciation sur un usage réel.
- **Prérequis :** R02–R03 et R15 ; toute dépréciation demande une décision et une migration.
- **Fin / preuve :** scénarios d'assemblage concaténé, contrôle de flux et compilation d'une extension historique ; message de génération/action de migration compréhensible. Aucun nettoyage supprimant une API par simple absence d'usage dans les tests locaux.

### R17 — Rendre l'explication fidèle et bornée · P0 · A_FAIRE

- **À faire :** vérifier branches choisies, court-circuit, répétitions de boucle, identifiants de passage, parents interrompus, erreur racine, capture avant mutation et chemin d'erreur après troncature. Examiner le volume et la profondeur de sérialisation de l'arbre final.
- **Prérequis :** R05, R08 et R10–R11.
- **Fin / preuve :** mêmes résultats et mêmes effets avec Explain activé/désactivé ; erreurs encore localisables après limite de trace ; collections cycliques et sorties volumineuses n'entraînent ni parcours illimité ni appel à ToString sur les objets hôtes.

### R18 — Fournir des contrats utilisables par les outils externes · P1 · A_FAIRE

- **À faire :** fournir des fixtures et, selon utilité, schémas JSON pour catalogue, erreur, source et explication ; préciser liens entre node_id, execution_id, path, source_hash, metadata_hash et snapshot exact. Identifier les données potentiellement sensibles à filtrer avant partage.
- **Prérequis :** R05–R06 et R17.
- **Fin / preuve :** lecture indépendante de fixtures : fragment fusionné, boucle, erreur de conversion Godot, trace tronquée et nœud custom ; aucun outil ne doit déduire « non exécuté » d'une simple absence dans une trace tronquée.

### R19 — Renforcer les tests par des contre-exemples · P0 · A_FAIRE

- **À faire :** compléter les tests selon les risques R03–R11 et R17 : JSON malformé/ambigu, Unicode, limites numériques, collections cycliques, graphes larges, effets de bord et permissions. Ajouter des cas générés reproductibles et comparaisons différentielles seulement lorsqu'une référence sémantique fiable existe.
- **Prérequis :** R01 et contrats des tâches testées ; ne pas attendre toutes les tâches pour commencer.
- **Fin / preuve :** chaque défaut trouvé possède une reproduction minimale ; graines/corpus conservés pour les tests générés ; résultats attendus issus du contrat, pas d'une copie de l'implémentation. Aucune obligation d'installer un framework pour obtenir un chiffre de couverture.

### R20 — Définir l'observabilité utile · P1 · A_FAIRE

- **À faire :** préciser codes stables, journalisation unique, identifiants de script/source/registre et mesures compilation/exécution/cache ; vérifier coût lorsque désactivée, comportement du logger et confidentialité des variables/chemins exposés.
- **Prérequis :** R05, R12–R13 et R17.
- **Fin / preuve :** diagnostiquer un échec et un cache obsolète à partir des éléments annoncés, sans doublons ni réexécution ; traces détaillées activables au choix de l'hôte. Les mécanismes génériques restent locaux et ne créent pas de télémétrie distante automatique.

## 8. Qualifier Godot et les paquets réellement distribués

### R21 — Vérifier la parité C# / Godot / GDScript · P0 · A_FAIRE

- **À faire :** inventorier API publiques, surcharges par nombre d'arguments, casse, conversions Variant, Int64→Int32, Float32, StringName, vecteurs/couleurs, null, tableaux/dictionnaires, cycles et objets libérés. Vérifier erreurs de frontière et wrapper des nœuds projet.
- **Prérequis :** R02–R05, R09–R11 et R15.
- **Fin / preuve :** scénarios comparables exécutés dans le cœur et GDScript ; mêmes valeurs/diagnostics dans le périmètre commun, écarts spécifiques documentés, aucun lien Godot dans l'assembly Core.

### R22 — Qualifier import et smoke moteur · P0 · A_FAIRE

- **À faire :** restaurer les paquets locaux dans le projet Godot dédié, compiler avant import, vérifier chemins `res://`, UID et chargement des ressources ; exécuter le smoke sur le moteur exact supporté.
- **Prérequis :** R02 et R21 ; paquets exacts produits par R25, moteur disponible avec preuve de version.
- **Fin / preuve :** import puis smoke headless terminés avec code attendu et marqueur `GDScript smoke checks: PASS`, sources chargées depuis le paquet ; logs archivés avec version/OS. Un build .NET seul ne termine pas cette tâche.

### R23 — Réaliser la recette visuelle Godot · P0 · A_FAIRE

- **À faire :** ouvrir le projet de qualification dans l'éditeur, vérifier reconnaissance des scripts C#, utilisation GDScript, affichage d'erreur/provenance et consultation d'une explication normale/tronquée. Décrire une recette manuelle minimale réutilisable.
- **Prérequis :** R22 ; environnement Godot interactif de la matrice R02.
- **Fin / preuve :** observations et validation humaine datées sur les paquets exacts, avec captures utiles et anomalies restantes. La recette qualifie l'adaptateur, sans prétendre valider l'interface ou le gameplay d'un consommateur.

### R24 — Vérifier les exports annoncés comme supportés · P1 · A_FAIRE

- **À faire :** exécuter un export puis le binaire avec une expression, un callback, une erreur et une trace ; examiner chargement, réflexion/générateur, trimming et restrictions de plateforme lorsqu'applicables.
- **Prérequis :** R02, R21–R22 et R25 ; templates d'export/licences/outils disponibles pour chaque cible retenue.
- **Fin / preuve :** résultat réel par cible et version de paquet. Cible non essayée = non qualifiée ; la preuve d'export et de fonctionnement du jeu reste dans son dépôt.

### R25 — Qualifier les deux paquets et leurs mises à niveau · P0 · A_FAIRE

- **À faire :** construire Core/Godot avec une version cohérente, vérifier contenu et dépendance exacte, restaurer dans un cache vide et tester sans référence aux sources. Tester installation propre, upgrade/downgrade et suppression des fichiers Godot devenus obsolètes uniquement dans le dossier généré.
- **Prérequis :** R02 et R06 ; scripts/targets existants, version de qualification explicitement choisie.
- **Fin / preuve :** tests Core empaqueté réussis et projet Godot consommant les deux paquets ; aucun doublon de types, référence locale cachée ou générateur requis à l'exécution. Vérifier absence de suppression hors `addons/resolver/` ; ne pas supprimer un cache utilisateur global pour tester.

### R26 — Rendre la chaîne de construction maîtrisable · P1 · A_FAIRE

- **À faire :** inventorier dépendances directes/transitives, versions d'actions CI, SDK et téléchargements ; contrôler provenance, empreintes disponibles, licences, vulnérabilités pertinentes et reproductibilité attendue. Vérifier les métadonnées des paquets et leur lien vers les sources.
- **Prérequis :** R02 et R25 ; sources officielles et données d'audit réellement accessibles.
- **Fin / preuve :** build depuis un clone/cache propres selon procédure documentée, inventaire versionné et anomalies traitées/acceptées explicitement. Un réglage `Deterministic=true` seul ne prouve pas la reproductibilité du paquet complet.

### R27 — Faire de la CI une preuve consultable · P0 · A_FAIRE

- **À faire :** conserver les validations sources, paquets et smoke existantes ; intégrer les contrôles Docs/catalogue/contrats pertinents, rendre visibles versions, échecs, artefacts et durées. Vérifier réellement disponibilité des actions, URL et empreinte Godot déclarées.
- **Prérequis :** R01–R02, R06, R19, R22 et R25 ; contrôles ajoutés progressivement dès disponibilité.
- **Fin / preuve :** exécution distante sur le SHA concerné, lien de run et résultats de chaque étape ; publication toujours limitée au chemin autorisé par tag et environnement. Les contrôles requis/protections de branche sont une configuration à constater, pas une propriété déduite du YAML.

## 9. Préparer l'usage, la livraison et la maintenance

### R28 — Établir une référence de performances · P1 · A_FAIRE

- **À faire :** mesurer compilation froide/cache chaud, Execute, Explain, sérialisation, appels hôtes et conversion Godot avec petits scripts et charges représentatives plus lourdes ; mesurer allocations, rétention du cache et latences répétées.
- **Prérequis :** contrats R03–R05, corpus décrit et environnement R02 ; résultats corrects avant mesure.
- **Fin / preuve :** protocole, matériel, configuration, version, répétitions et dispersion conservés ; budgets acceptables fixés à partir d'un besoin réel. Aucun objectif arbitraire en microsecondes ni comparaison non reproductible.

### R29 — Optimiser à partir des mesures · P1 · A_FAIRE

- **À faire :** choisir les coûts dominants mesurés : snapshots, sérialisation, conversions, allocations, synchronisation ou lookup ; comparer une évolution ciblée à la référence et chiffrer ses compromis.
- **Prérequis :** R28 et régressions de comportement appropriées R19 ; R12 si la synchronisation change.
- **Fin / preuve :** bénéfice mesuré dans le scénario visé, contrats/cache/budgets/trace préservés, cas dégradés documentés. Si aucune optimisation n'est justifiée, consigner le constat et les mesures comme résultat de cette investigation.

### R30 — Rendre la documentation praticable · P0 · A_FAIRE

- **À faire :** organiser parcours Core et Godot, installation propre, compilation de fragment, variables, extensions, diagnostics, budgets, permissions et rechargement ; relier référence du langage/catalogue et limites connues. Corriger les étapes devenues historiques, notamment création initiale du dépôt et état de publication.
- **Prérequis :** R02–R06, R09–R11, R14 et R21–R25 pour les parties concernées ; mise à jour progressive.
- **Fin / preuve :** un lecteur sans historique de conversation suit les commandes sur un environnement supporté et sait traiter une erreur ; exemples vérifiés et liens valides. Documentation autonome du fournisseur, guide d'adoption du jeu chez le consommateur.

### R31 — Préparer les règles de distribution et de maintenance · P0 · A_FAIRE

- **À faire :** faire choisir explicitement une licence et vérifier compatibilité des dépendances ; définir versionnement, changelog, compatibilité avant/après 1.0, dépréciations, signalement de défauts et canal de signalement de vulnérabilités. Identifier responsable de publication et politique des secrets.
- **Prérequis :** R02, R05 et inventaire R26 pour les éléments de distribution ; choix de licence par le détenteur des droits.
- **Fin / preuve :** décisions et politique publiables dans le dépôt, métadonnées cohérentes, aucun secret versionné ; interlocuteur et procédure d'incident identifiés. La roadmap ne choisit pas une licence et ne configure pas un compte externe à la place du responsable.

### R32 — Préparer une première version adoptable · P0 · A_FAIRE

- **À faire :** choisir le numéro de version après vérification de l'existant ; contrôler disponibilité/propriété des noms NuGet, notes de version, paquets exacts, licence, limites et migration. Vérifier environnement `nuget`, droits et secret via les outils autorisés sans exposer sa valeur.
- **Prérequis :** bilan des P0 R01–R11, R17, R19, R21–R23, R25, R27, R30–R31 ; éléments P1 exigés par le périmètre effectivement annoncé. Aucun prérequis non exécuté ne devient implicitement réussi.
- **Fin / preuve :** dossier de version prêt à relire, tous les critères annoncés justifiés et accord explicite de publication sur la version exacte. La création/poussée du tag et la publication ne s'exécutent qu'après cette autorisation ; après exécution, constater disponibilité effective et reprise d'une publication partielle si nécessaire.

### R33 — Transmettre le contrat aux consommateurs · P0 · A_FAIRE

- **À faire :** fournir version/SHA/empreintes, API requises, dépendances, compatibilité, changements et procédure de migration/retour à une version précédente ; préciser la séparation des données générées et des sources.
- **Prérequis :** R25, R30 et version candidate ou publiée identifiée par R32. Une qualification locale sur candidate est distinguée d'une adoption d'un paquet publié.
- **Fin / preuve fournisseur :** livrables et limites vérifiables, références des travaux consommateurs lorsqu'elles existent. **L'adoption n'est terminée que chez chaque consommateur**, avec sa version exacte et ses preuves de chargement/fusion, exécution, UI, export et gameplay selon son besoin ; son indisponibilité ne bloque pas les capacités indépendantes du fournisseur.

### R34 — Définir le passage à une API stable · P1 · A_FAIRE

- **À faire :** recueillir retours d'intégration sur les versions de qualification, traiter défauts bloquants, vérifier migrations et dépréciations ; définir les surfaces promises stables et leur durée de support avant de proposer 1.0.
- **Prérequis :** R12–R16, R18, R20, R24, R26, R28–R29 selon support annoncé, R31–R33 ; preuves consommateurs nécessaires aux garanties d'intégration concernées.
- **Fin / preuve :** critères de stabilité acceptés et vérifiés, défauts connus classés, formats/API gelés au niveau promis et procédure de migration répétable. Une version 1.0 n'est ni une date automatique ni la conséquence du seul passage des tests Core.

### R35 — Installer une routine de maintenance documentée · P1 · A_FAIRE

- **À faire :** définir triage des anomalies, politique de correctifs, revue des dépendances, suivi des performances, dérive documentaire et versions supportées ; organiser conservation des preuves et dépréciation des anciens contrats.
- **Prérequis :** R26–R28 et R31 ; les premières règles peuvent être écrites avant publication.
- **Fin / preuve :** responsabilités et checklist de maintenance définies ; premier passage documenté. Les activités récurrentes se consignent ensuite dans le journal ci-dessous ; terminer cette tâche signifie établir la routine, pas prétendre terminer toute maintenance future. Aucun ordonnanceur créé sans demande.

### R36 — Arbitrer les extensions futures sur besoin réel · P2 · A_FAIRE

- **À faire :** collecter et comparer les besoins : CLI de validation hors jeu, schéma/autocomplétion, nouveaux nœuds génériques, debug pas-à-pas, annulation coopérative, autres runtimes/cibles ou intégration Lua fournie par l'hôte.
- **Prérequis :** besoin sourcé, R03–R05 et retours R33 ; R07–R12 pour toute extension de l'exécution. Évaluer coût de maintenance et alternative côté hôte avant ajout.
- **Fin / preuve :** pour chaque besoin retenu, décision de périmètre et tâche directe avec contrat, dépendances et critère mesurable ; rejet/report motivé des autres. Ni interpréteur Lua embarqué, bytecode externe chargeable, JIT, nouvel éditeur web ou réécriture VM engagés par défaut.

## 10. Ordre conseillé et conditions de passage

Il s'agit d'un ordre de réduction des incertitudes, pas d'un calendrier ni de mini-projets. Une tâche peut commencer sur ses propres prérequis ; par exemple la qualification des paquets n'attend pas les optimisations.

| Moment | Tâches à privilégier | Condition pour élargir le travail |
|---|---|---|
| Reprendre sur une base fiable | R01–R02, puis R03–R06 et vérification de la chaîne R25/R27 | Snapshot et versions identifiés, contrats et contrôles de départ lisibles, blocages nommés |
| Fermer les risques fondamentaux | R07–R11, R13–R15, R17, R19, R21 ; R25 en parallèle dès prérequis disponibles | Comportements aux limites et permissions prouvés ; effets de bord et limites de confiance explicités ; extensions requises par Godot qualifiées |
| Qualifier une version adoptable | R22–R23, R27, R30–R33 ; R26 alimentant la distribution | Paquets exacts, smoke, recette humaine et critères de distribution satisfaits ; publication seulement sur accord applicable |
| Consolider la stabilité | Compléments R12–R16 et R26 non déjà requis par la candidate ; R18, R20, R24, R28–R29, R34 | Garanties de concurrence, cycle de vie, performance et cibles annoncées vérifiées ; retours d'adoption disponibles |
| Maintenir et décider des ajouts | R35, puis R36 selon besoin | Toute nouvelle capacité a une utilité, un coût, un propriétaire et une preuve attendue |

**Première action recommandée : R01.** Produire le bilan réel, puis vérifier R02 et la restauration/qualification des paquets ; cela révélera les blocages de versions ou d'environnement avant de planifier des développements plus coûteux. Ne pas lancer R36 tant qu'aucun besoin ne justifie ses ajouts.

### Arbitrages encore ouverts

| Question | Tâche qui doit la résoudre | État |
|---|---|---|
| Quelles plateformes, versions Godot/.NET et cibles d'export garantir ? | R02 / R24 | PROPOSEE, décision à obtenir |
| Quelle portée exacte pour « déterministe » ? | R04 | PROPOSEE, garanties à fixer et mesurer |
| Faut-il restreindre davantage propriétés/libération et isoler les registres entre hôtes ? | R09 | PROPOSEE, compatibilité à examiner |
| Quels plafonds de compilation et quelle politique mémoire du cache ? | R07 / R13 | PROPOSEE, mesures et besoin requis |
| Quels formats/API doivent être stables et comment les migrer ? | R05 / R31 / R34 | PROPOSEE, engagement à définir |
| Quelle licence, quels mainteneurs et quelle politique de support ? | R31 / R35 | PROPOSEE, choix humain requis |
| Quelle première version publier et quel consommateur qualifier en premier ? | R32 / R33 | PROPOSEE, état NuGet et adoption à constater |

## 11. Ce qu'il faut gérer dans la durée

### Risques et réponses attendues

| Risque concret | Réponse / tâche | Signal à surveiller |
|---|---|---|
| Prétraitement ou trace beaucoup plus coûteux que le calcul | R07–R08, R17, R28 | Pic mémoire/temps avant rejet, sérialisation très profonde |
| Callback natif bloqué ou privilégié malgré les budgets VM | R09–R10 | Dépassement temps hôte, opération non autorisée, confusion sur la sandbox |
| Échec après mutation d'une collection ou d'un objet de jeu | R11 / R14 | État partiellement appliqué, tentative de rechargement sans ancien handle valide |
| Ancien handle, registre ou provenance mal associés | R06 / R13–R15 | Résultat correct mais mauvaise source, cache ne reflétant pas le contrat attendu |
| Différence Core/GDScript ou scripts NuGet obsolètes | R21–R25 | Type différent, ressource introuvable, doublon après mise à niveau |
| Catalogue, documentation et code divergent | R06 / R18 / R27 / R30 | Exemple non compilable, champ exporté non documenté |
| Version/action/download indisponible ou incompatible | R02 / R26–R27 | Restauration en échec, différence local/CI, empreinte non vérifiée |
| Paquet publié mais non adopté, ou publication de deux paquets incomplète | R32–R33 | Un seul paquet disponible, consommateur encore sur ancienne version |
| Hausse continue du coût ou de la mémoire après plusieurs rechargements | R12–R14 / R28–R29 | Rétention de handles/collections, contention, latences qui augmentent |

### Responsabilités

- **Responsable de Resolver, à nommer :** arbitrages, compatibilité, priorités, acceptation des preuves et autorisation de livraison.
- **Réalisateur d'une tâche, à nommer au démarrage :** modification cohérente, scénarios, mise à jour du contrat et preuve sur la bonne version.
- **Relecteur à contexte neuf, à désigner si cette revue est engagée :** challenger le document autonome et ses références ; son avis reste distinct d'un test et d'un accord humain.
- **Mainteneur consommateur :** choix de version, orchestration du chargement/rechargement, permissions hôtes, recette produit et retour de migration dans son propre dépôt.

### Routine proposée, sans automatisation installée

| Déclencheur | Vérifications et résultat à conserver |
|---|---|
| Chaque changement de comportement ou d'API | Contrat mis à jour, scénarios de régression utiles, décision/migration si incompatible, contrôle de dérive |
| Chaque changement Godot/packaging | Paquets propres, upgrade/downgrade, import/smoke ; recette interactive et export selon impact |
| Chaque version candidate | Suites applicables, notes de version, compatibilité, dépendances/licences, artefacts exacts et limites connues |
| Chaque livraison autorisée | Run distant et tag/version constatés, disponibilité des deux paquets, suivi de reprise si publication partielle, transmission consommateurs |
| Après adoption et à intervalle convenu | Triage retours, dépendances, performances, cache, versions à déprécier et synchronisation documentaire |
| Chaque incident | Reproduction, versions touchées, effet sur état/données, correctif ou contournement, tests ciblés et note de migration |

Pour un signalement, demander au minimum : version de paquet et hôte, fragment JSON réduit, variables nécessaires expurgées, options/budgets, erreur/trace, comportement attendu/réel et fréquence. Les secrets et données privées ne font pas partie d'une reproduction publique.

### Journal d'exécution à compléter au fil du travail

Les lignes de tâche ci-dessus portent leur statut courant. Ce journal conserve les versions et preuves successives ; il ne duplique pas le suivi des consommateurs.

| Tâche | Date / responsable | Version, branche / PR | Prérequis effectivement consommés | Réalisation, commande / recette et preuve | Revue IA / validation humaine | Prochaine action / blocage |
|---|---|---|---|---|---|---|

Aucune tâche R01–R36 n'est déclarée réalisée par la rédaction de ce document. Aucun commit, tag, push, publication ou accord d'architecture n'est créé par la roadmap.

## 12. Validation de cette modification documentaire

Périmètre de ce travail : enrichissement de cette roadmap locale uniquement ; aucune modification du code, de l'API, du workflow ou du socle commun par sa rédaction. Des changements supplémentaires du kit, dont le passage du manifeste de `1.0.0` à `1.1.0`, ont été constatés pendant la session et conservés sans les réécrire. Le dernier contrôle Docs utilise le socle `1.1.0` présent à ce moment.

Contrôles exécutés le 21 novembre 2026 sur l'hôte Windows, avec Python `3.11.5`, puis renouvelés après les derniers ajustements documentaires :

| Contrôle | Résultat de cette session |
|---|---|
| `python .repo-kit/kit.py validate --target . --suite Docs` | REUSSIE : structure, empreintes du socle et destinations des liens locaux ; ce contrôle ne valide pas le code ni les liens distants |
| `git diff --check` | REUSSIE : aucune erreur d'espacement ; Git signale seulement sa conversion habituelle LF/CRLF |
| Contrôle ponctuel Python des identifiants, références et liens | REUSSIE : 36 identifiants R01–R36 uniques, références de tâches connues et destinations locales existantes |
| Contrôle ponctuel Python du graphe des prérequis déclarés | REUSSIE : 36 tâches, aucun cycle dans les dépendances RNN des lignes « Prérequis » ; les approbations et prérequis externes restent à vérifier au démarrage |
| Relecture des tâches, priorités, périmètre et limites de preuve | REUSSIE par l'agent rédacteur ; ce constat ne vaut pas revue indépendante ni acceptation humaine |
| Conservation du travail préparé avant la roadmap | REUSSIE : empreinte du diff indexé inchangée avant/après (`1f7e92f0cb38dde01a8c17c47945a4469ed04989`, empreinte de comparaison et non SHA de commit) ; aucun ajout à l'index par ce travail, changements supplémentaires du kit conservés |
| Suites Core / Build / Package, moteur, exports et consommateurs | NON_EXECUTEE pour cette modification documentaire ; elles restent à exécuter selon R01 et le travail de code concerné |
| Revue indépendante EGM / acceptation humaine de la roadmap | NON_EXECUTEE ; aucune approbation déduite |

Références de reprise : [routage IA](../ai/README.md), [contraintes](PROJET.md), [organisation](ORGANISATION.md), [dépendances et consommateurs](DEPENDANCES.md), [décisions](decisions/README.md) et [documentation technique](../../README.md).
