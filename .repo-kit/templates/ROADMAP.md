# Organiser la roadmap du composant

La [roadmap locale](TODO.md) indexe les capacités autonomes du composant. Le détail et les preuves restent dans les fiches ; les besoins produit et la recette d'adoption restent chez les consommateurs. Les [contraintes locales](PROJET.md) définissent le périmètre.

## Du besoin au travail prêt à démarrer

1. Décrire un résultat observable, ses bénéficiaires et ses limites. Vérifier la roadmap existante pour éviter les doublons. Garder les idées non retenues dans une section « À étudier », sans les présenter comme des engagements.
2. Choisir la taille utile : une fiche pour une correction courte ; une milestone `MNN` pour une capacité intégrée, des mini-tâches `MNN-TNN` pour ses livrables et des objectifs `MNN-TNN-ONN` si elles nécessitent plusieurs résultats distincts. Ne pas imposer trois niveaux à tous les travaux.
3. Copier le [modèle de travail](modeles/TRAVAIL.md), attribuer un identifiant libre et stable, puis corriger ses liens relatifs. Ne pas renuméroter l'historique ni réutiliser l'identifiant d'un travail abandonné.
4. Définir les critères vérifiables avant réalisation : scénario, résultat attendu et méthode de contrôle. Une milestone possède aussi ses critères d'intégration ; la somme des cases ne les prouve pas.
5. Nommer chaque prérequis réellement consommé : livrable, dépôt, identifiant, version/SHA, lien et preuve de disponibilité. Indiquer aussi ce que le résultat débloque. Vérifier les liens dans les deux sens et éviter les cycles ; l'ordre numérique n'est pas une dépendance.
6. Ajouter la fiche à TODO avec son statut réel. Prioriser selon les besoins et les prérequis disponibles ; un travail indépendant peut avancer pendant un blocage ailleurs.

## Arborescence à adapter

Pour une roadmap importante, ranger les fiches sous `doc/developpement/travaux/`, par exemple une fiche `M01.md`, une fiche `M01-T01.md` et une fiche `M01-T01-O01.md`. Ces chemins sont des exemples, pas des travaux installés. Chaque fiche lie son parent et ses enfants ; TODO conserve une vue synthétique avec des liens vers les fiches.

Pour une petite bibliothèque, des fiches de tâches directement liées depuis TODO suffisent. Ne pas recopier les mêmes critères et journaux dans plusieurs niveaux : le parent suit le résultat intégré et renvoie aux preuves de ses enfants.

## Statuts, branches et revues

| Statut | Condition |
|---|---|
| A_FAIRE | Travail défini ; aucune réalisation déduite de sa création |
| EN_COURS | Travail démarré, état et prochaine étape renseignés |
| BLOQUEE | Prérequis manquant, cause et condition de sortie explicites |
| TERMINEE | Résultat et critères satisfaits avec les preuves applicables et les références de réalisation |

Créer une branche par milestone, par exemple `milestone/m01-nom-court`, depuis une base vérifiée ; ses tâches et objectifs partagent cette branche. Une petite tâche autonome peut utiliser `fix/nom-court` ou `docs/nom-court`. Réutiliser une branche existante après examen de son état. La création de fiches seule ne crée pas de branche ou de PR.

Une PR vers la branche d'intégration du projet peut commencer en brouillon. Consigner séparément revue IA et validation humaine, avec date, version/SHA examiné et demandes restantes. Après correction, renouveler les contrôles et revues affectés. La fusion exige les validations requises sur la bonne version et l'autorisation applicable. Avant fusion, écrire « prête, fusion en attente » si l'intégration fait partie du résultat ; ne pas annoncer une intégration anticipée.

## Mettre à jour sans inventer l'avancement

Après réalisation, consigner le résultat, les décisions, les commandes et résultats réels, l'environnement, la version examinée et les limites dans la fiche. Une validation non lancée reste NON_EXECUTEE ; un avis IA ne remplace pas un accord humain. Un pourcentage n'a de sens qu'avec un périmètre et un mode de calcul explicites.

Les [commits](COMMITS.md) gardent un périmètre technique dans le titre et les identifiants de travaux dans le corps. Reporter ensuite leurs SHA réels et leur apport dans les fiches, dans un commit documentaire suivant. Des modifications locales restent signalées comme non commitées. Conserver les traces des travaux remplacés ou abandonnés, avec la raison et le lien vers la suite.

Pour une capacité fournie à un autre dépôt, lier la version disponible et sa preuve dans les [dépendances](DEPENDANCES.md). Le consommateur garde sa propre fiche et sa recette d'intégration ; publier une bibliothèque ne clôt pas son adoption.
