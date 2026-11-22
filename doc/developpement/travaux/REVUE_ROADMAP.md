# Revue et refonte de la roadmap — 21 novembre 2026

## Demande, version examinée et périmètre

Demande explicite : revoir et refaire la roadmap avec les nouvelles consignes. La demande initiale de tâches directes sans mini-projets est conservée ; le détail et les preuves sont déplacés dans des fiches, conformément au [guide de roadmap](../ROADMAP.md) et au [modèle de travail](../modeles/TRAVAIL.md). L'adaptation est inscrite dans les [contraintes locales](../PROJET.md), sans modification du socle.

| Référence | Constat |
|---|---|
| Dépôt / racine | Resolver, clone local identifié par `git rev-parse --show-toplevel` |
| Branche réutilisée | `docs/roadmap-resolver` |
| SHA de départ et de la roadmap examinée | `51e47cb556f83ba8789aa43f718709f393b42ee2` |
| État initial | Index et arbre de travail propres selon `git status --short` |
| Kit présent | `1.2.0` ; sources et manifeste lus, aucune mise à jour du kit effectuée par cette refonte |
| Nature de l'accord | Revue et réécriture documentaires demandées ; aucun accord global sur l'exécution des tâches techniques ou la publication |

La comparaison de l'ancienne base de code `ead75c3dd8bc20ce04a616d8cdaad5d69f66a0ff` au SHA de départ ne montre pas de modification des sources runtime, tests, projets, packaging et workflow examinés. Les constats techniques de la première roadmap sont repris comme lecture ciblée et les contrats/versions utiles ont été relus ; aucune ancienne réussite de tests n'est présentée comme réexécutée ici.

## Constats de revue et corrections

Cette revue a été réalisée par l'agent rédacteur dans le contexte de la conversation. Elle ne constitue pas une revue indépendante Goldfish. Elle porte sur l'organisation, la cohérence et la vérifiabilité du plan, pas sur un audit exhaustif du composant.

| Constat sur l'ancienne version | Risque ou conséquence | Correction dans cette révision |
|---|---|---|
| L'index cumulait les 36 descriptions détaillées et les journaux, avec une dérogation aux fiches | Navigation et autorité des preuves différentes du nouveau guide | [TODO](../TODO.md) synthétique ; fiches R01–R37 fondées sur le modèle, sans hiérarchie de mini-projets |
| La base et le récit Git décrivaient encore `ead75c3` et du travail non commité | Contexte périmé alors que la roadmap et le kit sont désormais dans HEAD | [Contexte](CONTEXTE.md) actualisé sur le SHA de départ, avec distinction historique/révision courante |
| Prérequis indiqués surtout par des numéros, sans livrable nommé ni lien retour | Ordre de lecture pouvant être confondu avec dépendance disponible | Chaque fiche nomme ses livrables amont, versions à relever, disponibilité non supposée et tâches aval |
| Certaines dépendances étaient trop larges pour le comportement à qualifier | Cache, historique, paquets ou observabilité retardés sans nécessité | R14 ne dépend plus de la politique d'éviction R13 ; R16 s'examine sans R15 ; R25 n'attend pas une évolution du catalogue ; cas conditionnels explicités |
| Des P1 étaient indirectement nécessaires à toutes les P0 de la candidate | Priorité de lecture trompeuse | R14, R15 et R26 deviennent P0 proposés ; dépendances fermes et garanties conditionnelles distinguées ; R30 ne requiert pas une cible d'export non annoncée |
| Critères et preuves étaient réunis dans un paragraphe sans scénarios suivis séparément | Clôture difficile à vérifier et risque de prétendre des tests déjà faits | Au moins deux scénarios par fiche, résultat attendu, recette prévue, état NON_EXECUTEE et cases non cochées |
| R32 mélangeait candidate prête, accord de publication et constat après publication | Préparation impossible à clore sans opération externe ; autorisation ambiguë | [R32](R32.md) finit avec le dossier de candidate ; [R37](R37.md) exige son accord réel puis constate publication et disponibilité |
| Les idées futures figuraient au même niveau que les capacités à réaliser | Étude assimilable à commande de développement | Section « À étudier » dans l'index ; [R36](R36.md) reste une étude d'arbitrage, sans nouvelle capacité engagée |
| Les preuves Docs parlaient encore du kit 1.1.0 et de l'ancien diff indexé | Trace de validation non applicable à la refonte | Anciens résultats conservés dans l'historique Git ; nouveaux contrôles de cette révision consignés ci-dessous avec leur portée |

Les identifiants R01–R36 restent attachés à leur résultat historique, même lorsque la priorité ou les dépendances sont clarifiées. R37 est un ajout, sans réemploi d'identifiant. Les modifications de priorités sont des propositions de planification, pas des décisions techniques retenues.

## Structure et limites du résultat

- [Index](../TODO.md) : résultats à viser, tâches et statuts synthétiques, ordre conseillé, risques et idées à étudier.
- [Fiches de tâches](../TODO.md#tâches) : détail et autorité des critères, prérequis, scénarios, décisions, preuves et reprise ; aucune réalisation technique annoncée à leur création.
- [Contexte commun](CONTEXTE.md) : versions/sources de planification et méthode inspirée d'EGM, sans dupliquer les statuts des fiches.
- [Contraintes locales](../PROJET.md) : adaptation du format sans toucher aux instructions ou modèles du socle.
- [Dépendances et consommateurs](../DEPENDANCES.md) : autorité d'intégration chez le consommateur ; version adoptée, accès distant et recette non supposés.

La stack Docker et les assistants optionnels présents dans le dépôt ne sont pas requis pour produire cette roadmap. Aucun conteneur, modèle, extraction Graphify, compte externe ou service planifié n'est lancé.

## Contrôles de cette refonte documentaire

Environnement constaté le 21 novembre 2026 : Windows, PowerShell, Python `3.11.5`, SDK effectif `.NET 10.0.403`. Cette révision n'altère ni code runtime, ni tests, ni contrat public, ni packaging, ni workflow. Les scénarios de qualification du composant restent à réaliser dans leurs tâches.

| Contrôle | État et preuve |
|---|---|
| `python .repo-kit/kit.py validate --target . --suite Docs` | REUSSIE, kit `1.2.0` : structure, empreintes du socle et destinations des liens que le kit parcourt ; ne prouve pas les scénarios de code ni les URL distantes |
| Vérification ponctuelle Python de toutes les fiches et liens locaux | REUSSIE : 41 documents, 1 080 liens/ancres locaux vérifiés, UTF-8, colonnes des tableaux et absence d'espaces de fin ; nouveaux documents inclus car Docs ne les parcourt pas tous |
| Vérification des dépendances, liens aval et cohérence index/fiches | REUSSIE : 37 fiches uniques ; 36 identifiants et titres historiques conservés ; priorités/statuts/prérequis synchronisés ; liens fermes et conditionnels réciproques ; aucun cycle, même en réunissant les dépendances conditionnelles ; tous les prérequis fermes d'une P0 sont P0 |
| Vérification des scénarios et statuts initiaux | REUSSIE : 74 lignes de scénarios NON_EXECUTEE, aucune case cochée ; 37 tâches A_FAIRE ; cette vérification structurelle n'exécute aucun scénario |
| `git diff --check` et périmètre des changements | REUSSIE : aucune erreur d'espacement ; Git signale sa conversion LF/CRLF habituelle. Index inchangé ; aucun diff dans le code, packaging, workflow, outils ou fichiers du socle protégés ; changements limités à TODO, PROJET et travaux/ |
| Core / Build / Package, moteur, CI distante, NuGet et consommateurs | NON_EXECUTEE pour cette refonte documentaire ; absence de modification de code, résultats futurs dans les fiches |
| Relecture IA de cohérence documentaire | Réalisée par le rédacteur, constats ci-dessus ; pas de revue indépendante |
| Revue indépendante Goldfish et validation humaine | NON_EXECUTEE ; aucun accord déduit |

Le contrôle Python ponctuel est un script de vérification de cette session, conservé dans son dossier temporaire et non ajouté comme suite du projet. Commande réellement utilisée depuis la racine du dépôt, à renouveler après les derniers ajustements de cette trace :

```powershell
python 'C:/Users/$KE1100-DQLH02RHTTI7/AppData/Local/Temp/vscode-agent-codex-sandbox-bA1aUU/check_resolver_roadmap.py'
```

Sa portée est structurelle : présence des sections du modèle, conservation des identifiants, cohérence index/fiches, scénarios non cochés, liens locaux/ancres, graphes des dépendances et conservation des sources protégées. Les dates, accords et résultats futurs restent à renseigner lors de la réalisation. La relecture du contenu par le rédacteur est distincte de ce contrôle automatique.

## Commits, revue et reprise

La présente refonte reste non commitée à sa rédaction. Aucun commit de cette révision, tag, push, fusion ou publication n'est effectué par la demande de roadmap. Le SHA de départ identifie la version relue, pas un futur commit de refonte.

La réalisation de chaque tâche a sa propre revue et ses contrôles. Pour démarrer le composant, choisir [R01](R01.md) et produire le bilan réel ; les autres fiches restent A_FAIRE et leurs livrables futurs ne sont pas réputés disponibles.

## Ajout des mini-tâches — 22 novembre 2026

Demande explicite : créer les mini-tâches de la roadmap. Base vérifiée `ba2a9588d4098bd498b8ffb556cdeca5dacbabaa`, branche réutilisée `docs/roadmap-resolver`, index et arbre propres au démarrage. La mention « non commitée » de la refonte précédente décrit son état historique à sa rédaction ; ce nouveau découpage reste lui-même non commité.

Les 37 fiches proposent désormais 151 mini-tâches internes `resolver:RNN-TNN`. Chaque ligne précise action, résultat vérifiable, dépendances internes et statut `A_FAIRE`. Le suivi reste dans la fiche d’origine ; ses critères, prérequis, scénarios et preuves restent applicables. L’index et l’adaptation locale du format sont actualisés. Aucun fichier du socle ni source du composant modifié.

Les configurations consommées par le plan ont été relues : SDK déclaré `10.0.100` avec `latestFeature`, Core `net10.0`, Godot SDK `4.7.2`, Roslyn `4.8.0` et formats de catalogue version 1. Il s’agit de versions déclarées ; aucune disponibilité moteur, publication ou compatibilité nouvelle n’est déduite de cette lecture.

Environnement des contrôles documentaires : Windows, PowerShell et Python `3.15.2` constaté. Les résultats ci-dessous qualifient ce découpage, pas l’exécution des travaux planifiés.

| Contrôle exécuté | Résultat et portée |
|---|---|
| `python .repo-kit/kit.py validate --target . --suite Docs` | REUSSIE, kit `1.2.0` ; structure, empreintes et liens locaux parcourus par le kit |
| Contrôle Python ponctuel transmis sur l’entrée standard (`python -`) | REUSSIE : 37 fiches, 151 identifiants uniques, statuts A_FAIRE et dépendances internes existantes sans cycle ; 74 scénarios NON_EXECUTEE conservés ; contenu antérieur des fiches inchangé hors ligne de navigation et ajout du découpage |
| Même contrôle, liens et structure | REUSSIE : 40 documents examinés, 1 098 liens/ancres locaux, tableaux et UTF-8 vérifiés ; les URL distantes ne sont pas qualifiées |
| `git diff --check` et inspection du périmètre Git | REUSSIE : aucune erreur d’espacement ; changements documentaires uniquement, index inchangé ; conversion LF/CRLF habituelle signalée par Git |
| Core, Build, Package, moteur, CI distante et consommateurs | NON_EXECUTEE : découpage documentaire, aucun changement de code ni réalisation des mini-tâches |

Relecture de cohérence par le rédacteur ; aucune revue indépendante ni validation humaine de réalisation. La demande autorise le découpage documentaire et ne vaut pas accord sur les choix techniques, fusion ou publication. Première reprise proposée : `resolver:R01-T01`, relevé du snapshot et des outils.
