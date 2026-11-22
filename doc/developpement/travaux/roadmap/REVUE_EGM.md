# Revue EGM de la roadmap reconstruite

Date : 22 novembre 2026. [Cible](../../TODO.md), [bilan et filiation](BILAN.md), [standard de revue](STANDARD_REVUE.md).

La demande autorise la reconstruction, la revue par des Goldfish en contexte neuf, puis la révision de la proposition. L’Elephant garde la synthèse du code et arbitre les corrections documentaires ; aucun choix technique n’est déclaré adopté par un avis IA.

## Méthode et version relue

Plugin EGM installé `0.1.1` : intégration Codex, personas et standard Zoom Out lus depuis le bundle local. Revue ciblée de roadmap selon [le standard local](STANDARD_REVUE.md), avec dispatch direct de trois relecteurs natifs, chacun lancé avec **`fork_turns: "none"`**. Ils ont reçu les chemins absolus de leur persona, de la cible et des sources sélectionnées, sans la conversation de conception. Ils ont travaillé en lecture seule et confirmé leurs lectures. Aucun rapport n’a été simulé dans le contexte du rédacteur.

Les contraintes communes du dépôt et le système de fichiers sont partagés ; un contexte de conversation neuf n’établit pas l’exclusion des instructions du dépôt. Cette revue ciblée ne revendique pas l’exécution des cinq étapes d’un design EGM complet ni une convergence de boucle sur plusieurs tours.

Version examinée : HEAD `c26cc3075abfc39e60ebdb9656cd6bdbfb14ffa3`, branche `docs/roadmap-resolver`, avec proposition documentaire locale. TODO, lignes 1–64 : SHA-256 **`89436214f00e4726714b26b890d983dc13926b4cd0cd93cd1a282ab51d4c2357`**. Les quinze fiches actives et le bilan faisaient partie de la cible liée ; aucun de ces fichiers n’a été modifié avant les retours des trois agents. Leurs références de lignes correspondent à cette version.

## Rapports reçus

| Relecteur / agent natif | État réel | Constats | Rapport |
|---|---|---|---|
| Codebase Details — `goldfish_codebase_r1` | completed | 0 Blocker, 0 Major, 0 Nit | [Rapport et lectures](revues/01-codebase.md) |
| Zoom Out — `goldfish_zoom_r1` | completed | 0 Blocker, 0 Major, 0 Nit | [Rapport et lectures](revues/01-zoom.md) |
| Implementation Readiness — `goldfish_readiness_r1` | completed | 0 Blocker, 0 Major, 1 Nit | [Rapport et lectures](revues/01-readiness.md) |

Les constats et conclusions sont conservés ; la liste de lectures du rapport Codebase est regroupée par fichier plutôt que de répéter ses nombreux liens absolus. Aucun relecteur absent, en échec ou interrompu n’est compté comme avis sans constat. Les tests et recettes n’ont pas été réexécutés par ces relecteurs ; ils ont examiné sources, portée des assertions et artefacts disponibles.

## Constats et résolution par l’Elephant

### readiness-1-1 — Nit — Statut réel de R09 après démarrage

- **Portée relue :** R09, lignes 36, 42 et 48 ; empreinte avant correction `34ea268b429502dc719b4c8f126ede3c4930138f924e54f6b21184bcb1d2aca0`.
- **Constat reçu :** la fiche permettait de produire constat/proposition avant accord mais prescrivait de garder A_FAIRE en son absence, ce qui pouvait masquer un travail réellement commencé.
- **Catégorie : PROPOSE FIX**, appliquée dans le périmètre de révision documentaire demandé. Motif vérifiable : [ROADMAP.md](../../ROADMAP.md#statuts-branches-et-revues) distingue tâche définie, travail commencé et blocage réel.
- **Modification appliquée :** approche et critère C3 indiquent désormais EN_COURS après démarrage, ou BLOQUEE si l’arbitrage empêche effectivement la suite ; preuves partielles conservées. L’accord reste nécessaire à la clôture et aucun durcissement n’est déclaré accepté. Le statut initial de la fiche reste A_FAIRE, puisque la tâche technique n’a pas démarré.
- **Aucun constat contesté ou escaladé.** Aucun accord technique, avis humain ou progrès d’implémentation n’est inféré des trois revues.

Les autres choix restent dans les [arbitrages ouverts](../../TODO.md#arbitrages-encore-ouverts). La correction de ce Nit ne les tranche pas.

### Vérification ciblée après correction

Un **quatrième contexte neuf**, `goldfish_r09_verification`, a relu R09 en lecture seule, avec la persona Implementation Readiness, le standard local et les règles de statuts du dépôt. Il n’a reçu ni la conversation ni les rapports précédents. Cible : R09, lignes 1–62, SHA-256 **`0c8ae884292db94c56a8bc96a4f8aa82072f44bb0e49624c2b1f32aa0276b9c2`**.

Résultat effectivement retourné : **completed, 0 Blocker, 0 Major, 0 Nit**. [Rapport complet](revues/02-r09.md). La remarque de suivi est corrigée et contre-relue ; aucune approbation technique n’en est déduite.

### Compatibilité des liens historiques

Le contrôle final du rédacteur a aussi étendu la vérification aux documents historiques préservés. Leur ancienne revue renvoie à `TODO.md#tâches`. Le titre de cette section est donc conservé sous « Tâches », au lieu de « Travaux proposés », pour garder ce lien utilisable. Cette correction de titre intervient après les trois revues générales ; elle ne change ni une tâche, ni un critère, ni une dépendance. Les empreintes ci-dessus identifient honnêtement les versions effectivement relues.

## Contrôles après révision

Les preuves Core/Build/Pack de départ sont dans le [bilan](BILAN.md#vérifications-réellement-exécutées) et ne sont pas des réalisations des tâches planifiées. Les contrôles documentaires ci-dessous sont séparés des quatre avis IA et de toute validation humaine.

| Contrôle exécuté depuis la racine | Résultat et portée |
|---|---|
| `python .repo-kit/kit.py validate --target . --suite Docs` | REUSSIE, kit 1.2.0 : structure, empreintes du socle et liens locaux parcourus par le kit ; ne prouve pas les recettes de code |
| `python artifacts/roadmap-20261122-e11334/check_roadmap.py` | REUSSIE : 67 documents, 1 509 liens/ancres locaux, 15 fiches actives, 42 scénarios NON_EXECUTEE, 37 dispositions de parents et 151 identifiants enfants conservés ; graphe déclaré sans cycle et liens retour cohérents |
| Contrôle des empreintes initiales et comparaison de la copie historique | REUSSIE : toutes les sources, tests, outils, configurations et anciennes fiches conservés ; seuls TODO/PROJET sont modifiés parmi les fichiers déjà suivis ; ancien index copié à l’identique |
| `git diff --check`, inspection des fichiers modifiés, de l’index et de HEAD | REUSSIE : aucune erreur d’espacement ; modifications documentaires uniquement, index Git inchangé ; HEAD toujours c26cc3075abfc39e60ebdb9656cd6bdbfb14ffa3. Git signale sa conversion LF/CRLF habituelle |
| Avis humains, recettes moteur/visuelles, CI distante, NuGet public et adoption consommateur | NON_EXECUTEE dans cette reconstruction ; aucun résultat ou accord déduit |

La vérification locale complémentaire est conservée comme outil de session dans `artifacts/roadmap-20261122-e11334/check_roadmap.py`, sans l’ajouter comme suite du composant. Elle contrôle liens/ancres, tableaux, critères présents, correspondance index/fiches, dépendances déclarées et liens retour, absence de cycles, dispositions historiques et préservation des fichiers. Elle n’exécute aucun scénario futur.

## État livré et suite

Proposition documentaire révisée et revue IA terminées. Les quinze tâches techniques restent A_FAIRE ; les arbitrages Q1–Q6 restent ouverts. Aucun commit, ajout à l’index, tag, push, fusion, publication ou adoption consommateur effectué par cette intervention. Les modifications se limitent à TODO/PROJET et aux nouveaux documents d’historique, fiches actives, bilan et rapports. Les anciennes fiches, les sources et le socle sont conservés.
