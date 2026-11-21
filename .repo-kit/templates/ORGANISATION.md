# Organisation commune — socle repository-kit/1

Chaque dépôt possède son code, ses contrats, décisions, tests et roadmap. Le consommateur possède le besoin produit, la version adoptée et la preuve d'intégration. Un composant publié n'est pas automatiquement intégré dans un jeu.

## Préparer un travail

Lire la [roadmap locale](TODO.md) et les [contraintes du projet](PROJET.md). Réutiliser un travail existant si son périmètre convient. Créer une fiche avec le [modèle](modeles/TRAVAIL.md) pour un résultat attribuable et vérifiable ; une petite bibliothèque peut rester au niveau des tâches.

Pour un ensemble plus grand, utiliser une milestone `MNN`, des mini-tâches `MNN-TNN` et des objectifs `MNN-TNN-ONN`. Conserver leurs identifiants, liens parents/enfants et critères propres. Ne pas créer ces trois niveaux pour une correction courte. Les références inter-dépôts prennent la forme `depot:identifiant` avec URL réelle.

Les statuts sont A_FAIRE, EN_COURS, BLOQUEE, TERMINEE. Un résultat coché requiert ses preuves ; un simple déplacement de code ou une fiche créée ne termine pas le travail. Nommer chaque prérequis avec son livrable, sa version, sa disponibilité et sa preuve, sans déduire l'ordre des numéros.

## Réaliser et vérifier

Identifier clone, branche et modifications. Créer une branche de travail adaptée à la tâche, sans mélanger les changements étrangers. Vérifier les contrats réellement consommés. Ajouter des scénarios utiles de comportement ou de régression ; ne pas ajouter de test qui recopie uniquement l'implémentation.

Exécuter les suites proportionnées définies dans le projet. `Docs` vérifie seulement le socle et ses liens. Un outil absent, une erreur, un timeout ou une commande non lancée n'est pas une réussite. Distinguer REUSSIE, ECHEC, NON_EXECUTEE et NON_APPLICABLE justifié. Les recettes Godot visuelles, exports et essais consommateurs restent distincts des tests de bibliothèque.

Conserver dans le travail commande exacte, date, environnement, version et résultat. Les commits suivent Gitmoji et décrivent un changement cohérent. Ne pas créer de commit, tag, publication ou approbation fictifs. Les revues IA et humaine sont distinctes ; une fusion ou publication exige une autorisation applicable et les contrôles requis sur la bonne version.

## Décisions et contrats

Conserver les décisions locales dans le [registre](decisions/README.md), avec besoin, options, raisons, statut et source d'accord. PROPOSEE ne vaut pas RETENUE ; une décision retenue ne prouve pas sa réalisation. Utiliser le [modèle](modeles/DECISION.md). Un changement transversal référence les travaux des dépôts affectés.

Le dépôt fournisseur teste sa capacité ; chaque consommateur teste son intégration avec une version explicite. Éviter les dépendances circulaires. Publier un changement incompatible avec sa migration et son versionnement approprié. Ne pas renuméroter les anciens travaux lors d'une extraction : garder leur historique et pointer vers les nouveaux travaux locaux.

## Mise à jour du socle

Le manifeste `.repo-kit/manifest.json` conserve version et empreintes des fichiers communs. Le code du kit et ses modèles sont copiés localement pour fonctionner sans le dépôt d'origine. La configuration `.repo-kit/project.json`, les contraintes, roadmap, décisions et fiches appartiennent au projet.

Une nouvelle version du kit est appliquée depuis une copie revue avec `update`, d'abord sans `--apply`. Un fichier commun modifié localement provoque un conflit et aucune écriture ; décider explicitement comment reporter l'adaptation. Les documents locaux existants ne sont pas remplacés. Aucun téléchargement ou synchronisation automatique n'est effectué.
