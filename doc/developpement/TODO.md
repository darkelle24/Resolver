# Roadmap reconstruite — Resolver

Proposition du **22 novembre 2026**, issue du code, des tests et des contraintes du dépôt. Base observée : `c26cc3075abfc39e60ebdb9656cd6bdbfb14ffa3`, branche `docs/roadmap-resolver`. Les travaux ci-dessous restent **A_FAIRE** ; leurs choix et leur ordre sont **PROPOSEE**. L’autorisation porte sur cette reconstruction et sa revue, pas sur la réalisation du code, une fusion ou une publication.

Le [bilan sourcé](travaux/roadmap/BILAN.md) distingue capacités présentes, écarts reproduits, manques de preuve et besoins à clarifier. La [revue Goldfish](travaux/roadmap/REVUE_EGM.md) conserve les avis et leur traitement. Les [contraintes locales](PROJET.md) et les [règles de travail](ORGANISATION.md) s’appliquent.

## Ce qui existe déjà

Resolver compile des fragments JSON, conserve provenance et catalogue, met en cache des handles, exécute avec des budgets, expose des diagnostics/explications et accepte des extensions. Le cœur reste indépendant de Godot ; l’adaptateur et les deux paquets existent. **64/64 tests Core**, le **build de l’adaptateur** et la **construction des deux paquets locaux** ont réussi pendant cette analyse ; le catalogue suivi correspond sémantiquement à l’export actuel, avec **41 nœuds et 5 types littéraux**. Commandes et limites dans le [bilan](travaux/roadmap/BILAN.md#vérifications-réellement-exécutées).

Cela ne prouve pas un import moteur, une recette visuelle, une CI distante, une publication ou une adoption dans un jeu. Aucune tâche n’est ajoutée pour recréer ces capacités déjà présentes.

## Tâches

Une ligne correspond à un résultat de travail, de taille adaptée à son périmètre. Les fiches liées font autorité pour les critères, scénarios et preuves. Aucun nombre de sous-tâches, milestone ou niveau intermédiaire n’est imposé. Le tableau indique les **livrables requis pour démarrer la partie dépendante**, pas un ordre tiré des numéros ; les conditions de clôture et besoins externes figurent dans chaque fiche.

| Identifiant et résultat | Pourquoi ce travail est utile maintenant | Prérequis locaux nécessaires |
|---|---|---|
| [R38 — Conversions en chaîne indépendantes de la culture](travaux/roadmap/R38.md) | Même script, résultat différent sous en-US/fr-FR : contre-exemple reproduit | Aucun |
| [R07 — Rejet précoce des sources excessives](travaux/roadmap/R07.md) | Copie/hachage avant la limite de 16 Mio ; coût reproduit avant rejet | Aucun ; preuve Variant à rattacher à R21 |
| [R08 — Budgets restants et effets après arrêt](travaux/roadmap/R08.md) | Compléter les frontières manquantes, sans confondre compteurs VM et mémoire totale | Aucun |
| [R40 — Contrat de remplacement, partage et échec des hôtes](travaux/roadmap/R40.md) | Liaisons call/nœud différentes, références partagées, effets partiels observés | Aucun ; différences Godot qualifiées dans R21 |
| [R09 — Permissions réelles des objets hôtes](travaux/roadmap/R09.md) | Le registre des méthodes ne filtre pas les propriétés ; accord de politique encore absent | Aucun pour le constat/Core ; recette moteur R21 et accord nécessaires à la clôture |
| [R39 — Contrats d’échange et fixtures vérifiables](travaux/roadmap/R39.md) | Rendre les formats actuels consommables et contrôler la dérive du catalogue | Aucun |
| [R02 — Environnements qualifiés et cibles seulement déclarées](travaux/roadmap/R02.md) | Distinguer versions configurées, SDK exécuté et moteur non essayé | Aucun pour l’inventaire et les prérequis |
| [R25 — Paquets, provenance et changements de version](travaux/roadmap/R25.md) | Pack réussi ; installation propre et upgrade/downgrade non prouvés, URL/licence absentes des nuspec | Aucun pour les essais ; choix R31 pour les métadonnées finales |
| [R21 — Façade Godot sur paquets exacts](travaux/roadmap/R21.md) | Smoke fourni mais non exécuté ici ; conversions et droits à éprouver dans le moteur | R02 : moteur disponible et identifié ; R25 : paquets restaurables/projet compilable |
| [R23 — Recette visuelle Godot](travaux/roadmap/R23.md) | L’éditeur et la lecture humaine des diagnostics ont une preuve propre | R21 : projet importé et smoke réussi ; avis humain nécessaire |
| [R27 — Contrôles de contrat et CI consultables](travaux/roadmap/R27.md) | Workflow existant sans contrôle Docs/catalogue ; aucun run distant constaté ici | R39 : commandes de contrôle ; R02 : outils du workflow disponibles |
| [R30 — Parcours d’installation actualisé](travaux/roadmap/R30.md) | Le README demande encore de créer le dépôt et affirme un état de publication non vérifié | Aucun pour corriger le parcours ; lier ensuite les preuves R02/R21/R25/R39 |
| [R31 — Licence et règles minimales de distribution](travaux/roadmap/R31.md) | Licence absente des fichiers suivis et paquets observés ; choix du responsable requis | Aucun travail technique préalable ; accord humain nécessaire |
| [R32 — Dossier de candidate et transmission fournisseur](travaux/roadmap/R32.md) | Rendre une éventuelle livraison concrète et relisible avant décision | R25 : paquets qualifiés ; R31 : licence/règles approuvées ; R30 : parcours correspondant |
| [R37 — Publication après autorisation explicite](travaux/roadmap/R37.md) | Exécuter et constater une publication seulement lorsqu’une version exacte est autorisée | R32 : dossier exact sans réserve de publication non arbitrée ; autorisation spécifique |

Tous ces travaux sont A_FAIRE : les observations de départ et la rédaction des fiches ne constituent pas leur réalisation. Aucun responsable, date de livraison, estimation ou accord technique n’est inventé.

## Ordre utile et dépendances conditionnelles

Commencer par R38 et R07 pour les deux écarts reproduits, et R30 pour le parcours obsolète. R08, R40, R09 et R39 peuvent être préparées indépendamment, à partir des comportements actuels. R02 et les essais techniques de R25 préparent la qualification moteur R21. R31 peut être arbitré sans attendre ces corrections. Cet ordre est une proposition motivée, pas une affectation de travail.

R23 consomme la preuve moteur de R21 ; R27 consomme les contrôles de R39 et les outils vérifiés par R02. Les changements ultérieurs d’une surface déjà qualifiée imposent de renouveler les seules preuves affectées sur les paquets exacts. **Aucune boucle de dépendance n’est requise** : R21 teste les droits actuels sans attendre une nouvelle politique R09 ; R25 peut faire ses essais avant le choix de licence R31 ; R02 inventorie les outils avant les recettes aval.

R32 assemble le dossier choisi. Il doit joindre R21/R23/R27 s’il annonce ces qualifications. Les autres garanties doivent elles aussi avoir une preuve correspondant à la candidate ; un défaut connu reste corrigé ou explicitement soumis à arbitrage, sans faux accord. Préparer ce dossier ne signifie ni le déclarer approuvé, ni publier, ni faire adopter la version par un consommateur.

## Arbitrages encore ouverts

| Question | Ce qui est établi et ce qui manque | Effet sur la suite |
|---|---|---|
| Q1 — Jusqu’où promettre le déterminisme ? | La dépendance à la culture est reproduite. Débordements, NaN/infini calculés, égalités mixtes et différences de plateforme n’ont pas un contrat exhaustif accepté | R38 propose la correction limitée. Une politique numérique nouvelle exige cas attendus, compatibilité et accord ; aucun gel général n’est présupposé |
| Q2 — Quelles configurations et quels exports annoncer ? | .NET/Godot déclarés ; Core/build locaux réussis. Aucun moteur, export ou plateforme supplémentaire qualifié dans cette analyse | R02/R21 qualifient les configurations retenues. Créer un travail d’export seulement pour une cible demandée, avec templates et recette du binaire |
| Q3 — Quel périmètre de confiance pour l’hôte ? | Méthodes filtrées, propriétés accessibles selon leur existence, registre statique ; allocations natives et effets de bord hors isolation complète | R09/R40 documentent et éprouvent le contrat ; durcissement, isolation inter-hôtes, transaction ou préemption ne sont pas décidés |
| Q4 — Quels volumes et coûts posent réellement problème ? | Aucun corpus consommateur représentatif, objectif de latence ou pression mémoire du cache mesurée fourni | Ne pas programmer éviction, benchmark général, métriques nouvelles ou optimisation ; demander un scénario chiffré avant d’ouvrir un travail ciblé |
| Q5 — Quelle licence, compatibilité et trajectoire de livraison ? | Licence non choisie, dépendance Core du paquet Godot non écrite comme version exacte, état distant NuGet non vérifié | R25/R31 doivent faire expliciter ces choix. R32 n’impose ni version 0.1.0 ni date ni passage 1.0 ; R37 garde son autorisation propre |
| Q6 — Quelles extensions produit sont nécessaires ? | Aucun besoin concret n’est fourni pour Lua embarqué, asynchronisme, bytecode importable, éditeur web ou nouveaux nœuds métier | À étudier à partir d’un usage, d’une alternative côté hôte et d’un critère de fin ; aucune tâche de développement engagée |

Ces questions restent dans le dépôt fournisseur pour ses contrats génériques. Les besoins produit, la version adoptée et les preuves d’intégration de chaque consommateur restent dans son propre dépôt.

## Historique et traçabilité

L’[ancien index est conservé à l’identique](TODO_HISTORIQUE_2026-11-22.md). Les anciennes fiches R01–R37, leur contexte, leur revue et leurs 151 mini-tâches restent intacts sous `travaux/` ; ils décrivent la proposition antérieure, pas une seconde liste active. **Les fiches actives sont uniquement celles liées dans le tableau ci-dessus, sous `travaux/roadmap/`.** Les résultats conservés gardent leur identifiant ; les nouveaux périmètres reçoivent R38–R40 sans réemploi d’un identifiant abandonné.

Le [tableau de filiation complet](travaux/roadmap/BILAN.md#traçabilité-des-anciens-identifiants) indique pour chaque ancien R01–R37 : conservée, remplacée, fusionnée ou abandonnée comme tâche autonome, avec raison et destination. Les identifiants RNN-TNN restent consultables dans leurs fiches historiques et suivent cette filiation de parent ; aucune équivalence de tous leurs critères ni clôture de ces mini-tâches n’est prétendue.

La [revue EGM](travaux/roadmap/REVUE_EGM.md) porte sur cette proposition documentaire. La réalisation de chaque travail demandera ses propres contrôles, décisions et revues sur la version concernée.
