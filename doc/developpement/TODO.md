# Roadmap — Resolver

Découpage révisé le 22 novembre 2026, base `ba2a9588d4098bd498b8ffb556cdeca5dacbabaa`, branche `docs/roadmap-resolver`. **Une roadmap de tâches directes, sans mini-projets ni hiérarchie artificielle.** Les identifiants historiques `resolver:R01` à `resolver:R36` sont conservés ; `resolver:R37` distingue la publication de sa préparation. La refonte précédente et sa base restent décrites dans le [contexte](travaux/CONTEXTE.md) et la [revue](travaux/REVUE_ROADMAP.md).

## But et portée

Qualifier puis faire évoluer un compilateur de fragments JSON et une VM de modding avec cœur C# autonome, diagnostics explicables, budgets et adaptateur Godot distribué séparément. Partir des capacités déjà présentes ; ne pas réécrire sans écart démontré.

Les [contraintes locales](PROJET.md) définissent les frontières. Fusion/chargement des mods, règles produit, UI et preuve d’adoption restent chez les consommateurs. Le [contexte versionné](travaux/CONTEXTE.md) rassemble capacités présentes, versions constatées, limites des preuves et adaptation d’Elephant Goldfish. La [revue de cette refonte](travaux/REVUE_ROADMAP.md) détaille les écarts corrigés et ses propres contrôles documentaires.

## Lecture et suivi

- Ce document est l’index ; **chaque fiche liée fait autorité sur ses critères, prérequis, scénarios, réalisation et preuves** selon le [guide de roadmap](ROADMAP.md) et l’[organisation](ORGANISATION.md). Le résumé de statut ici se met à jour avec la fiche, sans copier ses journaux.
- Les 37 fiches contiennent **151 mini-tâches**, identifiées de `resolver:R01-T01` à `resolver:R37-T04` selon leur fiche. Chaque ligne décrit une action, son résultat vérifiable, ses dépendances internes et son statut. Les prérequis de la fiche restent applicables ; les numéros seuls n’imposent aucun ordre. Le détail et les preuves restent dans la fiche, sans nouvelle milestone ni fichier par mini-tâche.
- La rédaction/révision est demandée. Les choix techniques et priorités restent `PROPOSEE` ; aucune tâche de code, migration, fusion ou publication n’est engagée par sa seule présence.
- Statuts : `A_FAIRE`, `EN_COURS`, `BLOQUEE`, `TERMINEE`. Toutes les tâches sont `A_FAIRE` faute de réalisation attribuable documentée ; une fiche créée n’est pas un résultat réalisé. Une dépendance future non produite n’impose pas `BLOQUEE` avant le démarrage d’un travail demandé.
- P0 : référence, risques et critères de la candidate initiale. P1 : stabilité et exploitation supplémentaires. P2 : étude conditionnelle, sans engagement de développer les idées. Une garantie P1 annoncée pour une candidate devient un critère obligatoire de cette candidate.
- Les liens du tableau sont les prérequis fermes ; les prérequis conditionnels et composants externes sont précisés dans les fiches. Chaque fiche indique aussi les tâches qu’elle débloque. Les versions/SHA des livrables futurs restent à relever après réalisation ; un numéro de tâche n’est pas une preuve de disponibilité.
- Au démarrage : attribuer responsable, vérifier contrats et versions réellement consommés, consigner branche/PR applicables. À la clôture : conserver commandes, dates, environnement, SHA réels, résultats et limites ; revue IA et validation humaine distinctes. Suivre la [convention de commits](COMMITS.md), sans renuméroter les travaux ni inventer un SHA.

## Tâches

### Contrats et référence

| Tâche et fiche faisant autorité | Priorité proposée | Statut | Prérequis fermes locaux |
|---|---|---|---|
| [R01 — Établir la référence de validation](travaux/R01.md) | P0 | A_FAIRE | Aucun travail antérieur |
| [R02 — Fixer la matrice de support](travaux/R02.md) | P0 | A_FAIRE | [R01](travaux/R01.md) |
| [R03 — Rendre le langage et l'API normatifs](travaux/R03.md) | P0 | A_FAIRE | [R01](travaux/R01.md) |
| [R04 — Délimiter le déterminisme et les règles numériques](travaux/R04.md) | P0 | A_FAIRE | [R02](travaux/R02.md), [R03](travaux/R03.md) |
| [R05 — Stabiliser les formats d'échange](travaux/R05.md) | P0 | A_FAIRE | [R03](travaux/R03.md) |
| [R06 — Assurer la cohérence du catalogue](travaux/R06.md) | P0 | A_FAIRE | [R03](travaux/R03.md), [R05](travaux/R05.md) |

### Limites et cycle de vie

| Tâche et fiche faisant autorité | Priorité proposée | Statut | Prérequis fermes locaux |
|---|---|---|---|
| [R07 — Vérifier les limites de compilation](travaux/R07.md) | P0 | A_FAIRE | [R03](travaux/R03.md) |
| [R08 — Vérifier les budgets d'exécution](travaux/R08.md) | P0 | A_FAIRE | [R03](travaux/R03.md), [R04](travaux/R04.md) |
| [R09 — Définir les permissions sur les objets hôtes](travaux/R09.md) | P0 | A_FAIRE | [R03](travaux/R03.md) |
| [R10 — Encadrer les callbacks et leurs erreurs](travaux/R10.md) | P0 | A_FAIRE | [R03](travaux/R03.md), [R08](travaux/R08.md), [R09](travaux/R09.md) |
| [R11 — Clarifier les mutations et l'échec partiel](travaux/R11.md) | P0 | A_FAIRE | [R03](travaux/R03.md), [R10](travaux/R10.md) |
| [R12 — Définir le contrat de concurrence](travaux/R12.md) | P1 | A_FAIRE | [R02](travaux/R02.md), [R10](travaux/R10.md), [R11](travaux/R11.md) |
| [R13 — Maîtriser la durée de vie du cache et des handles](travaux/R13.md) | P1 | A_FAIRE | [R06](travaux/R06.md), [R11](travaux/R11.md) |
| [R14 — Fiabiliser le rechargement côté bibliothèque](travaux/R14.md) | P0 | A_FAIRE | [R06](travaux/R06.md), [R11](travaux/R11.md) |

### Extensions, diagnostics et tests

| Tâche et fiche faisant autorité | Priorité proposée | Statut | Prérequis fermes locaux |
|---|---|---|---|
| [R15 — Garantir les contrats des nœuds personnalisés](travaux/R15.md) | P0 | A_FAIRE | [R03](travaux/R03.md), [R06](travaux/R06.md), [R10](travaux/R10.md), [R11](travaux/R11.md) |
| [R16 — Sécuriser le générateur et le chemin historique](travaux/R16.md) | P1 | A_FAIRE | [R02](travaux/R02.md), [R03](travaux/R03.md) |
| [R17 — Rendre l'explication fidèle et bornée](travaux/R17.md) | P0 | A_FAIRE | [R05](travaux/R05.md), [R08](travaux/R08.md), [R10](travaux/R10.md), [R11](travaux/R11.md) |
| [R18 — Fournir des contrats utilisables par les outils externes](travaux/R18.md) | P1 | A_FAIRE | [R05](travaux/R05.md), [R06](travaux/R06.md), [R17](travaux/R17.md) |
| [R19 — Renforcer les tests par des contre-exemples](travaux/R19.md) | P0 | A_FAIRE | [R01](travaux/R01.md), [R03](travaux/R03.md) |
| [R20 — Définir l'observabilité utile](travaux/R20.md) | P1 | A_FAIRE | [R05](travaux/R05.md), [R17](travaux/R17.md) |

### Godot et distribution

| Tâche et fiche faisant autorité | Priorité proposée | Statut | Prérequis fermes locaux |
|---|---|---|---|
| [R21 — Vérifier la parité C# / Godot / GDScript](travaux/R21.md) | P0 | A_FAIRE | [R02](travaux/R02.md), [R03](travaux/R03.md), [R04](travaux/R04.md), [R05](travaux/R05.md), [R09](travaux/R09.md), [R10](travaux/R10.md), [R11](travaux/R11.md), [R15](travaux/R15.md) |
| [R22 — Qualifier import et smoke moteur](travaux/R22.md) | P0 | A_FAIRE | [R02](travaux/R02.md), [R21](travaux/R21.md), [R25](travaux/R25.md) |
| [R23 — Réaliser la recette visuelle Godot](travaux/R23.md) | P0 | A_FAIRE | [R22](travaux/R22.md) |
| [R24 — Vérifier les exports annoncés comme supportés](travaux/R24.md) | P1 | A_FAIRE | [R02](travaux/R02.md), [R21](travaux/R21.md), [R22](travaux/R22.md), [R25](travaux/R25.md) |
| [R25 — Qualifier les deux paquets et leurs mises à niveau](travaux/R25.md) | P0 | A_FAIRE | [R02](travaux/R02.md) |
| [R26 — Rendre la chaîne de construction maîtrisable](travaux/R26.md) | P0 | A_FAIRE | [R02](travaux/R02.md), [R25](travaux/R25.md) |
| [R27 — Faire de la CI une preuve consultable](travaux/R27.md) | P0 | A_FAIRE | [R01](travaux/R01.md), [R02](travaux/R02.md), [R06](travaux/R06.md), [R19](travaux/R19.md), [R22](travaux/R22.md), [R25](travaux/R25.md) |

### Usage, stabilité et maintenance

| Tâche et fiche faisant autorité | Priorité proposée | Statut | Prérequis fermes locaux |
|---|---|---|---|
| [R28 — Établir une référence de performances](travaux/R28.md) | P1 | A_FAIRE | [R02](travaux/R02.md), [R03](travaux/R03.md), [R05](travaux/R05.md) |
| [R29 — Optimiser à partir des mesures](travaux/R29.md) | P1 | A_FAIRE | [R19](travaux/R19.md), [R28](travaux/R28.md) |
| [R30 — Rendre la documentation praticable](travaux/R30.md) | P0 | A_FAIRE | [R02](travaux/R02.md), [R03](travaux/R03.md), [R05](travaux/R05.md), [R06](travaux/R06.md), [R09](travaux/R09.md), [R10](travaux/R10.md), [R11](travaux/R11.md), [R14](travaux/R14.md), [R21](travaux/R21.md), [R22](travaux/R22.md), [R23](travaux/R23.md), [R25](travaux/R25.md) |
| [R31 — Préparer les règles de distribution et de maintenance](travaux/R31.md) | P0 | A_FAIRE | [R02](travaux/R02.md), [R05](travaux/R05.md), [R26](travaux/R26.md) |
| [R32 — Préparer une première version adoptable](travaux/R32.md) | P0 | A_FAIRE | [R01](travaux/R01.md), [R02](travaux/R02.md), [R03](travaux/R03.md), [R04](travaux/R04.md), [R05](travaux/R05.md), [R06](travaux/R06.md), [R07](travaux/R07.md), [R08](travaux/R08.md), [R09](travaux/R09.md), [R10](travaux/R10.md), [R11](travaux/R11.md), [R14](travaux/R14.md), [R15](travaux/R15.md), [R17](travaux/R17.md), [R19](travaux/R19.md), [R21](travaux/R21.md), [R22](travaux/R22.md), [R23](travaux/R23.md), [R25](travaux/R25.md), [R26](travaux/R26.md), [R27](travaux/R27.md), [R30](travaux/R30.md), [R31](travaux/R31.md) |
| [R33 — Transmettre le contrat aux consommateurs](travaux/R33.md) | P0 | A_FAIRE | [R25](travaux/R25.md), [R30](travaux/R30.md), [R32](travaux/R32.md) |
| [R34 — Définir le passage à une API stable](travaux/R34.md) | P1 | A_FAIRE | [R12](travaux/R12.md), [R13](travaux/R13.md), [R16](travaux/R16.md), [R18](travaux/R18.md), [R20](travaux/R20.md), [R28](travaux/R28.md), [R31](travaux/R31.md), [R32](travaux/R32.md), [R33](travaux/R33.md) |
| [R35 — Installer une routine de maintenance documentée](travaux/R35.md) | P1 | A_FAIRE | [R26](travaux/R26.md), [R27](travaux/R27.md), [R31](travaux/R31.md) |
| [R36 — Arbitrer les extensions futures sur besoin réel](travaux/R36.md) | P2 | A_FAIRE | [R03](travaux/R03.md), [R05](travaux/R05.md), [R33](travaux/R33.md) |

### Publication distincte

| Tâche et fiche faisant autorité | Priorité proposée | Statut | Prérequis fermes locaux |
|---|---|---|---|
| [R37 — Publier une version après autorisation explicite](travaux/R37.md) | P0 | A_FAIRE | [R32](travaux/R32.md) |

## Ordre conseillé et conditions de passage

Ces étapes sont des repères de lecture, sans branche, statut ou sous-projet propre. Les dépendances réelles des fiches priment sur leur ordre d’affichage.

| Repère | Travail utile | Condition de passage |
|---|---|---|
| Reprendre | [R01](travaux/R01.md), puis [R02](travaux/R02.md) et [R03](travaux/R03.md) | Bilan et périmètre de support explicites, contrôles manquants nommés |
| Réduire les risques | Contrats, budgets, permissions, effets de bord, traces et extensions nécessaires à l’API exposée | Preuves de comportement sur le snapshot retenu ; dépendances P0 satisfaites |
| Qualifier la distribution | [R25](travaux/R25.md) dès R02, puis parité, import, smoke, recette interactive, provenance et CI | Paquets exacts et scénarios applicables prouvés ; aucune assimilation build/recette |
| Préparer la candidate | [R30](travaux/R30.md), [R31](travaux/R31.md), [R32](travaux/R32.md) | Garanties reliées à leurs preuves ; limites, licence et migrations lisibles |
| Transmettre / publier | [R33](travaux/R33.md) peut transmettre une candidate ; [R37](travaux/R37.md) exige un accord de publication distinct | Fourniture, publication et adoption restent des résultats séparés |
| Stabiliser et maintenir | P1 selon les garanties visées, [R34](travaux/R34.md), [R35](travaux/R35.md) | Retours, mesures et compatibilité vérifiés ; maintenance future toujours suivie |

**Prochaine mini-tâche disponible : `resolver:R01-T01` — relever le snapshot et les outils**, dans le [découpage de R01](travaux/R01.md#mini-tâches). Rédiger un bilan honnête reste possible même si un moteur, le réseau ou une preuve consommateur manque. Les corrections et nouvelles capacités demandent ensuite leur propre périmètre de réalisation autorisé.

## À étudier — aucune fonctionnalité engagée

La tâche [R36](travaux/R36.md) conserve l’identifiant historique de l’étude. Les idées suivantes ne sont ni des engagements ni des tâches d’implémentation :

| Idée | Condition avant une éventuelle décision |
|---|---|
| CLI de validation hors jeu, schéma/autocomplétion | Besoin d’auteur de mods confirmé et alternative avec les API existantes examinée |
| Nouveaux nœuds génériques | Plusieurs usages justifiant une capacité fournisseur, plutôt qu’une règle propre à un jeu |
| Debug pas-à-pas, annulation coopérative | Contrat d’état/effets, coût, compatibilité et limites vis-à-vis des callbacks définis |
| Autres runtimes, cibles, Lua fourni par l’hôte | Consommateur identifié, matrice de support et coût de maintenance acceptés |
| Visualiseur ou éditeur externe | Projet et autorité distincts ; Resolver fournit les contrats, pas sa roadmap produit |

Un interpréteur Lua embarqué, JIT, bytecode externe chargeable ou réécriture VM n’est pas retenu par défaut. Pour toute idée acceptée ultérieurement, enregistrer l’accord réel puis créer une tâche directe avec résultat observable, prérequis et preuve attendue, sans réutiliser un identifiant existant.

## Arbitrages et risques à suivre

| Sujet | Autorité / tâche | Condition à éclaircir |
|---|---|---|
| Plateformes et portée du déterminisme | [R02](travaux/R02.md), [R04](travaux/R04.md) | Support proposé puis accepté sur preuves ; aucune promesse universelle |
| Permissions hôtes et coûts hors budgets | [R07](travaux/R07.md) à [R11](travaux/R11.md) | Prétraitement, propriétés/libération, callbacks privilégiés et échec partiel |
| Cache, remplacement et concurrence | [R12](travaux/R12.md) à [R14](travaux/R14.md) | Rétention mémoire, anciens handles, état partagé et responsabilité de l’hôte |
| Contrats et documentation qui divergent | [R05](travaux/R05.md), [R06](travaux/R06.md), [R17](travaux/R17.md), [R18](travaux/R18.md), [R30](travaux/R30.md) | Une trace tronquée n’est pas une preuve de non-exécution ; formats et sources exacts |
| Sources générées et distribution | [R21](travaux/R21.md) à [R27](travaux/R27.md) | Parité, UID/chemins, upgrade/downgrade, téléchargements et CI réellement exécutée |
| Licence, version, publication partielle | [R31](travaux/R31.md), [R32](travaux/R32.md), [R37](travaux/R37.md) | Choix du détenteur des droits, disponibilité des deux paquets, accord sur la version exacte |
| Publication confondue avec adoption | [R33](travaux/R33.md), [dépendances](DEPENDANCES.md) | Version et recette produit chez chaque consommateur ; aucune clôture croisée supposée |

Le responsable du composant arbitre les décisions et engagements ; le réalisateur conserve les preuves ; le relecteur donne un avis distinct ; le mainteneur consommateur choisit et qualifie sa version. Ces personnes sont à nommer au démarrage des travaux concernés.

La routine et ses journaux appartiennent à [R35](travaux/R35.md) : triage après incident, dérive code/docs après changement, qualification de candidate, suivi des dépendances et retours d’adoption à intervalle convenu. Aucun service récurrent, modèle local, installation de plugin ou publication n’est lancé par cette roadmap.
