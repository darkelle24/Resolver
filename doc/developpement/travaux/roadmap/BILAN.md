# Bilan de reconstruction de la roadmap

Analyse du **22 novembre 2026** dans Resolver. [Roadmap révisée](../../TODO.md), [revue EGM](REVUE_EGM.md).

## Version et limites de l’analyse

Racine identifiée par Git : `C:/Users/$KE1100-DQLH02RHTTI7/Documents/GitHub/Resolver` ; branche `docs/roadmap-resolver`, réutilisée. Au démarrage, HEAD était `ba2a9588d4098bd498b8ffb556cdeca5dacbabaa` avec 40 fichiers documentaires modifiés : PROJET, TODO, R01–R37 et REVUE_ROADMAP. Ils ont été lus comme sources de planification, pas annulés.

Pendant l’analyse, HEAD est devenu `c26cc3075abfc39e60ebdb9656cd6bdbfb14ffa3` (« Ajouter des mini-tâches pour chaque fiche de la roadmap »). **Ce commit n’a pas été créé par cette reconstruction.** Les empreintes des fichiers suivis sauvegardées avant les nouvelles écritures correspondent au contenu de ce HEAD. La comparaison Git entre les deux SHA ne montre aucun changement dans core, src, tests, generator, packaging, tools/Pack.ps1, global.json, Directory.Build.props ou le workflow NuGet. Les observations de code ci-dessous s’appliquent donc au contenu de `c26cc30` ; la présente reconstruction reste locale, sans commit de réalisation.

Les 37 anciennes fiches, leurs 151 mini-tâches et les traces précédentes sont préservées. L’ancien TODO est copié sans changement dans [TODO_HISTORIQUE_2026-11-22.md](../../TODO_HISTORIQUE_2026-11-22.md). Les contraintes locales de format sont adaptées à la demande actuelle, sans modifier le socle partagé.

Lecture ciblée des entrées publiques, compilation, VM, valeurs/conversions, diagnostics, registres, façade Godot, générateur, tests, packaging et workflow. Ce travail n’est ni un audit exhaustif de sécurité ni une validation du produit consommateur. Aucun état distant NuGet, GitHub Actions ou HexGameGodot n’a été consulté ; les liens externes du dépôt ne sont pas des preuves nouvelles.

## Prérequis constatés

| Élément | Contrat déclaré | Constat effectif / limite |
|---|---|---|
| .NET | [global.json](../../../../global.json) : 10.0.100, latestFeature, sans prerelease ; projets runtime net10.0 | SDK 10.0.403, runtime 10.0.14, Windows 10 x64 ; Core et Build réussis |
| Godot | [Resolver.csproj](../../../../Resolver.csproj) : Godot.NET.Sdk/4.7.2 ; workflow : moteur .NET 4.7.2 Linux | SDK de compilation utilisable ; `Get-Command godot` ne résout aucun exécutable et HEX_GODOT_PATH n’est pas défini. Aucun import ou smoke moteur exécuté |
| Générateur | [Generator.csproj](../../../../generator/Generator.csproj) : netstandard2.0, CodeAnalysis.CSharp 4.8.0, Analyzers 3.3.4 | Consommé lors des builds locaux ; aucun test Roslyn isolé ou contrat multi-SDK nouveau |
| Paquets | [Directory.Build.props](../../../../Directory.Build.props), [Pack.ps1](../../../../tools/Pack.ps1) : 0.1.0 par défaut | Deux paquets locaux construits ; aucune disponibilité publique déduite |
| Documentation | Python 3.11+ et Git ; kit installé 1.2.0 | Python 3.15.2 observé ; contrôles documentaires propres à cette révision dans la revue |
| Formats | Catalogue/explication/programme version 1 ; diagnostics JSON en snake_case | Format de sortie présent ; aucune promesse de chargement de bytecode exporté |

## Capacités déjà présentes

| Capacité | Sources et preuves actuelles | Limite de la conclusion |
|---|---|---|
| Fragments, cache et provenance | [VmEngine](../../../../src/central/runtime/VmEngine.cs), [VmCompilationMetadata](../../../../src/central/compiler/VmCompilationMetadata.cs) ; tests de sélection onAttack, source distincte, ancien handle, JsonElement après Dispose, ordre des métadonnées | Compilation déjà disponible ; pas de chargeur/fusionneur de mods ni remplacement collectif de handles fourni au jeu |
| Langage et extensions | [VmJsonCompiler](../../../../src/central/compiler/VmJsonCompiler.cs), [VmBuiltinNodes](../../../../src/central/nodes/VmBuiltinNodes.cs), [VmNodeRegistry](../../../../src/central/nodes/VmNodeRegistry.cs) ; tests des alias, paramètres, types, variadiques, compilateurs et liaisons | 41 nœuds, 5 littéraux ; pas de justification pour réécrire le registre ou ajouter un nouvel interpréteur |
| Exécution et limites | [VM](../../../../src/central/runtime/VM.cs), [options](../../../../src/central/runtime/VmContracts.cs) ; tests d’instructions, tableaux, troncature et arrêt de boucle | Compteurs existants ; pas une borne de mémoire totale, de temps natif ou d’allocations de callback |
| Diagnostics et explication | [VmJson](../../../../src/central/diagnostics/VmJson.cs), [VmValueSnapshot](../../../../src/central/diagnostics/VmValueSnapshot.cs), [VmExplanation](../../../../src/central/diagnostics/VmExplanation.cs) ; tests UTF-8/JSON Pointer, branches, occurrences, capture avant mutation et cycles | Export d’inspection et instantanés bornés ; aucun visualiseur web présent ni adoption externe qualifiée |
| Isolation du cœur | [Projet Core](../../../../core/Resolver.Core.csproj), test d’absence de références Godot, valeurs vectorielles autonomes | Test C# de la DLL ; ne prouve pas tous les chemins Godot ni le comportement des hôtes partagés |
| Adaptateur et distribution | [VmEngineNode](../../../../src/godot/VmEngineNode.cs), [DynValueConverter](../../../../src/central/value/DynValueConverter.cs), [cibles NuGet](../../../../packaging/build/Darkelle.Resolver.Godot.targets), [workflow](../../../../.github/workflows/nuget.yml) | Build et Pack locaux réussis ; smoke fourni mais non exécuté, montée/descente de version non essayée |
| Chemin historique | [RuleNodeProcessor](../../../../src/central/rule_node_system/RuleNodeProcessor.cs), [Assembler](../../../../src/central/opcode/Assembler.cs), [générateur](../../../../generator/ExecuteInstructionGenerator.cs) ; tests de relocalisation des sauts/liaisons et RuleNode enregistré | Capacité conservée ; aucun consommateur ni défaut démontré justifiant une migration ou dépréciation générale |

Les [64 tests](../../../../tests/Program.cs) ont été exécutés. Cette preuve porte sur leurs assertions, pas sur tous les comportements mentionnés par les anciennes fiches.

## Écarts démontrés et observations utiles

| Référence | Nature et constat | Source / reproduction | Conséquence proposée |
|---|---|---|---|
| E01 | Écart au déterminisme annoncé : un même script de concaténation de vecteur retourne `1.5` sous en-US et `1,5` sous fr-FR | Script `{"operation":"add","left":"v=","right":{"$type":"vector2","x":1.5,"y":2.5}}`, même handle ; ScriptMath utilise les record structs, DynValue délègue leur ToString à la conversion | Correction limitée [R38](R38.md) ; la politique numérique générale reste une question distincte |
| E02 | Rejet tardif : un texte JSON de 16 777 218 caractères est refusé avec JSON_SIZE_LIMIT après **33 555 504 octets** alloués sur le thread, entrée construite avant mesure | `JsonSerializer.Serialize(new string('x',16*1024*1024))`, GC.GetAllocatedBytesForCurrentThread autour de CompileJson ; [VmEngine](../../../../src/central/runtime/VmEngine.cs) encode/hache avant [LocatedJson](../../../../src/central/compiler/LocatedJson.cs), qui réencode avant contrôle | [R07](R07.md). Mesure ponctuelle sur cet hôte, pas un benchmark ni un objectif mémoire contractuel |
| E03 | Comportement présent : constante chaîne abcdef avec MaxStringLength=5 → STRING_LIMIT | Sonde Core sur `{"operation":"constant","value":"abcdef"}` | Réutiliser ce cas et compléter les frontières [R08](R08.md), sans déclarer les budgets défaillants en général |
| E04 | Comportement présent : array_push sur la liste a puis variable absente → VARIABLE_NOT_FOUND ; la liste hôte contient toujours l’ajout | Sonde sequence avec a fourni par référence, `[array_push(a,1), read_variable(missing)]` | Contrat d’échec partiel [R40](R40.md), sans inventer un rollback attendu |
| E05 | Comportement présent : un handle call compilé avec fonction retournant 1 retourne 2 après remplacement RegisterFunction ; les nœuds enregistrés conservent au contraire leur liaison selon les tests existants | Sonde `{"operation":"call","function":"value","args":[]}` ; dictionnaire de fonctions copié par Execute | Décrire la différence [R40](R40.md), sans la qualifier de défaut de cache |
| E06 | Comportement présent : un champ inconnu rgiht ajouté à add est ignoré, résultat 3 | Sonde `{"operation":"add","left":1,"right":2,"rgiht":999}` ; VmNodeSyntax.AllowUnknownFields=true | Inclure la tolérance dans [R39](R39.md) ; aucun mode strict décidé |
| E07 | Décalage documentaire : README décrit encore initialisation/push du dépôt et affirme l’absence de publication | README confronté au dépôt Git existant ; état distant non consulté | Correction du parcours [R30](R30.md), sans annoncer que NuGet est vide |
| E08 | Paquets construits mais métadonnées incomplètes : pas d’URL repository ni de licence ; dépendance Core du paquet Godot écrite version="0.1.0" | Inspection des deux nuspec dans les nupkg produits ; aucune plage exacte [0.1.0] écrite dans ce nuspec | Provenance/résolution [R25](R25.md), choix humain de licence [R31](R31.md) ; la plage compatible reste à décider |
| E09 | Différence de frontière : les permissions des méthodes ne sont pas celles des propriétés ; Free est direct ; registre de méthodes statique | [GodotHostObjects](../../../../src/godot/GodotHostObjects.cs), [ExposedMethodRegistry](../../../../src/central/security/ExposedMethodRegistry.cs) | Constat/accord [R09](R09.md), preuve moteur [R21](R21.md). Lecture de code, pas démonstration d’exploitation ni besoin acquis de refonte |

## Manques de preuve, sans défaut inventé

Installation des paquets dans un cache vide, upgrade/downgrade, import et smoke Godot, recette visuelle, run CI distant, export de jeu, état NuGet et adoption consommateur : **NON_EXECUTEE** pendant cette reconstruction. Leur absence ici ne signifie pas qu’ils n’ont jamais réussi ailleurs. Aucune preuve d’un autre dépôt n’est recopiée ou supposée.

Les budgets hors construction des tableaux ont une couverture à compléter. Un grand nombre de scénarios d’extensions et traces existent déjà. Aucun besoin mesuré n’est fourni pour une optimisation, une éviction de cache, une nouvelle couche d’observabilité, une API stable 1.0 ou une extension produit. Ces sujets restent dans les [questions ouvertes](../../TODO.md#arbitrages-encore-ouverts).

## Vérifications réellement exécutées

Commandes depuis la racine du dépôt, le 22 novembre 2026, Windows/PowerShell, SDK .NET 10.0.403 et Python 3.15.2. Le dossier `artifacts/roadmap-20261122-e11334/` est ignoré par Git ; il conserve les sondes, logs, paquets et empreintes de sauvegarde de cette session. Les résultats utiles sont transcrits ici pour ne pas dépendre de ce dossier temporaire.

| Commande / contrôle | Résultat réel et portée |
|---|---|
| `git rev-parse --show-toplevel`, `git branch --show-current`, `git status --short`, `git rev-parse HEAD` | Racine/branche et évolution des bases constatées ; voir version ci-dessus |
| `dotnet --info`, `python --version`, résolution `Get-Command godot` et HEX_GODOT_PATH | Versions ci-dessus ; aucun moteur résolu dans cet environnement |
| `dotnet run --project tests/Resolver.Tests.csproj -c Release --no-restore` | **REUSSIE**, sortie 0 : **64/64 checks passed** |
| `dotnet build Resolver.csproj -c Release --no-restore` | **REUSSIE**, sortie 0 : **0 Warning(s), 0 Error(s)** |
| `./tools/Pack.ps1 -OutputDirectory artifacts/roadmap-20261122-e11334/packages` | **REUSSIE**, sortie 0 : Core et Godot 0.1.0 produits ; aucune publication |
| `dotnet run --project tests/Resolver.Tests.csproj -c Release --no-build --no-restore -- --export-node-catalog artifacts/roadmap-20261122-e11334/node_catalog.json` puis comparaison JSON par Python | **REUSSIE**, sortie 0 : équivalence sémantique avec docs/node_catalog.json ; 41 nœuds et 5 types littéraux |
| `dotnet run --project artifacts/roadmap-20261122-e11334/Probe.csproj -c Release` | **REUSSIE** comme outil d’observation, sortie 0 ; E01–E06 observés. Ce n’est pas une suite de régression ajoutée au projet ni une preuve que les corrections futures sont réalisées |
| Inspection Python zipfile des nuspec et calcul SHA-256 des paquets locaux | **REUSSIE**, E08 ; empreintes ci-dessous |
| Comparaison Git ba2a958… → c26cc30… sur sources/manifests/tests/packaging/workflow et comparaison des empreintes des fichiers suivis avant rédaction | Aucun changement de contenu du code examiné ; le commit documentaire intervenu pendant l’analyse n’est pas attribué au rédacteur |
| Docs, liens Markdown, cohérence des tâches et diff final | Résultats après révision consignés dans [REVUE_EGM.md](REVUE_EGM.md), sans les anticiper ici |

Paquets de cette observation, **pas une candidate approuvée** :

- `Darkelle.Resolver.Core.0.1.0.nupkg` : SHA-256 `9efa483168fa8cc01ebb1d82422259ac92b103fb3dcbee46cd8592f457f957ea`.
- `Darkelle.Resolver.Godot.0.1.0.nupkg` : SHA-256 `0fbbd447bd0ff2761ebfc4c4284503c38e1537794bd36e72aa04797a7ac8f558`.

Ces empreintes identifient les paquets de la session, pas ceux d’une CI ou d’un serveur NuGet.

## Traçabilité des anciens identifiants

« Abandonnée » signifie retirée de la **liste active des travaux autonomes**, pas suppression d’une capacité ou clôture d’un travail réalisé. Une fusion ne prouve pas tous les critères de l’ancienne fiche. Les liens historiques pointent vers les fiches préservées ; les destinations vers la proposition active. Chaque identifiant enfant RNN-TNN reste dans sa fiche historique et hérite de la disposition du parent, sans renumérotation ni statut terminé.

| Ancien travail | Disposition dans la proposition | Destination et raison |
|---|---|---|
| [R01](../R01.md) — référence de validation | Remplacée | Le présent bilan établit la référence réellement exécutée ; manques spécifiques vers R25/R21/R27. Aucun chantier générique à recommencer, aucune clôture de l’ancienne fiche déduite |
| [R02](../R02.md) — matrice de support | Conservée | [R02](R02.md), configurations testées distinctes des déclarations, sans nouvelle cible imposée |
| [R03](../R03.md) — langage/API normatifs | Fusionnée | [R39](R39.md), contrats actuels et fixtures ; parcours dans R30. Pas de nouvelle spécification incompatible imposée |
| [R04](../R04.md) — déterminisme/numérique | Remplacée | [R38](R38.md) traite E01 ; politique numérique étendue dans Q1 tant qu’un besoin/accord manque |
| [R05](../R05.md) — formats d’échange | Fusionnée | [R39](R39.md), référence/fixtures et versionnement des sorties existantes |
| [R06](../R06.md) — cohérence catalogue | Fusionnée | [R39](R39.md) et [R27](R27.md), contrôle automatisé ; comparaison actuelle déjà réussie, registre non recréé |
| [R07](../R07.md) — compilation bornée | Conservée | [R07](R07.md), coût avant rejet E02 et voies d’entrée existantes |
| [R08](../R08.md) — budgets d’exécution | Conservée | [R08](R08.md), frontières manquantes des compteurs, pas nouvelle sandbox |
| [R09](../R09.md) — permissions hôtes | Conservée | [R09](R09.md), contrat explicite et accord réel avant tout durcissement |
| [R10](../R10.md) — callbacks | Fusionnée | [R40](R40.md) et [R21](R21.md), confiance, erreurs, remplacements, différence de façade |
| [R11](../R11.md) — mutations/échec partiel | Fusionnée | [R40](R40.md) et R21, E04 et référence/copie ; pas de rollback décidé |
| [R12](../R12.md) — concurrence | Fusionnée | [R40](R40.md), périmètre Core indépendant et restrictions de l’hôte ; Godot dans R21. Pas de support multithread général promis |
| [R13](../R13.md) — cache/handles | Abandonnée | Éviction et politique mémoire sans charge représentative ; Q4. ClearCache et validité des anciens handles restent dans R40 |
| [R14](../R14.md) — rechargement | Fusionnée | [R40](R40.md), bonne source/échec/correction et remplacement par l’hôte ; pas de chargeur de mods |
| [R15](../R15.md) — nœuds personnalisés | Fusionnée | [R40](R40.md) et R21 pour restes de cycle de vie/façade ; nombreux contrats déjà couverts dans les 64 tests, aucun chantier de réimplémentation |
| [R16](../R16.md) — générateur/historique | Abandonnée | Tests d’usage/relocation existants et builds réussis ; aucun défaut/consommateur démontré pour une migration générale. Réouvrir sur un cas de génération ou compatibilité précis |
| [R17](../R17.md) — explication | Fusionnée | [R08](R08.md) pour bornes, [R39](R39.md) pour fixtures ; nombreux scénarios déjà réussis |
| [R18](../R18.md) — outils externes | Fusionnée | [R39](R39.md), contrat et exemples partageables ; aucun serveur ni visualiseur commandé |
| [R19](../R19.md) — contre-exemples | Fusionnée | Régressions dans chaque tâche concernée, notamment R38/R07/R08 ; pas de corpus généré ou fuzzer obligatoire sans risque ciblé |
| [R20](../R20.md) — observabilité | Fusionnée | [R40](R40.md) pour responsabilité du logger, R39 pour identifiants/export ; nouvelles métriques renvoyées à Q4 |
| [R21](../R21.md) — parité des façades | Conservée | [R21](R21.md) intègre le smoke moteur R22 et garde les différences Core/GDScript explicites |
| [R22](../R22.md) — import/smoke | Fusionnée | [R21](R21.md), une qualification moteur sur paquets exacts au lieu de deux chaînes séparées |
| [R23](../R23.md) — recette visuelle | Conservée | [R23](R23.md), observations d’éditeur et avis humain, distincts du smoke |
| [R24](../R24.md) — exports | Abandonnée | Aucune cible d’export demandée/qualifiée ; Q2. Un futur support annoncé devra avoir sa tâche d’export et de lancement du binaire |
| [R25](../R25.md) — paquets/mises à niveau | Conservée | [R25](R25.md), installation/changements de version et provenance réellement contrôlés |
| [R26](../R26.md) — chaîne de construction | Fusionnée | [R25](R25.md) pour provenance, R27 pour outils CI, R31 pour licences. Audit général/reproductibilité binaire non imposés faute de besoin défini |
| [R27](../R27.md) — CI consultable | Conservée | [R27](R27.md), étendre la chaîne existante et constater le run exact autorisé |
| [R28](../R28.md) — référence de performances | Abandonnée | Aucun workload/objectif représentatif fourni ; Q4 avant une campagne de mesures |
| [R29](../R29.md) — optimisation | Abandonnée | Aucun coût dominant mesuré ; pas d’optimisation prescrite |
| [R30](../R30.md) — documentation | Conservée | [R30](R30.md), corriger le parcours obsolète et relier preuves/références, sans tout réécrire |
| [R31](../R31.md) — distribution/maintenance | Conservée | [R31](R31.md), licence et règles de distribution minimales ; pas de durée de support inventée |
| [R32](../R32.md) — candidate | Conservée | [R32](R32.md), dossier soumis à revue ; périmètre/version à choisir, pas première publication supposée |
| [R33](../R33.md) — transmission | Fusionnée | [R32](R32.md), même dossier fournisseur ; adoption et recette chez le consommateur |
| [R34](../R34.md) — API stable | Abandonnée | Pas de retour d’intégration ni garanties 1.0 définies ; Q5, sans suppression de la compatibilité existante |
| [R35](../R35.md) — routine maintenance | Abandonnée | Processus récurrent sans résultat ponctuel démontré ; règles minimales R31 et contrôles R27 suffisent au besoin actuel, sans automatisation nouvelle |
| [R36](../R36.md) — extensions futures | Abandonnée | Étude générique remplacée par Q6 et conditions d’ouverture d’un besoin concret ; aucune extension engagée |
| [R37](../R37.md) — publication autorisée | Conservée | [R37](R37.md), accord sur version exacte et preuve distincte, jamais déclenchée par la roadmap |

Les 151 mini-tâches historiques ne sont pas redistribuées artificiellement en une nouvelle grille. Leurs identifiants et leur texte restent intacts dans les anciens fichiers ; le lecteur remonte au parent de ce tableau pour trouver la destination ou la raison du retrait. Les critères abandonnés ou laissés à clarifier ne sont pas annoncés satisfaits.
