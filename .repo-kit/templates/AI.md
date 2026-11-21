# Démarrage IA — $name

Le dépôt contient son contexte de travail autonome. Il n'est pas nécessaire de charger la documentation d'un jeu consommateur pour modifier une capacité générique.

| Action | Lectures utiles |
|---|---|
| Comprendre le composant | [README technique](../../README.md), puis [contraintes et entrées](../developpement/PROJET.md) |
| Planifier | [Guide de roadmap](../developpement/ROADMAP.md), [roadmap locale](../developpement/TODO.md), [organisation](../developpement/ORGANISATION.md), un modèle de travail |
| Réaliser | Fiche du travail demandé, contraintes locales et fichiers affectés ; élargir la lecture si le périmètre change |
| Revoir | Version exacte, diff, critères et preuves ; une demande de revue seule ne corrige ni ne publie |
| Préparer Graphify, Obsidian ou Hermes | [Stack Docker optionnelle](../developpement/ASSISTANTS_DOCKER.md), puis setup local explicite |
| Valider | `python .repo-kit/kit.py validate --target . --suite Docs`, puis suite de code applicable définie dans les contraintes locales |
| Rédiger un commit | [Convention Gitmoji et génération VS Code](../developpement/COMMITS.md), diff Git réellement préparé |
| Modifier un contrat | [Décisions](../developpement/decisions/README.md), consommateurs et compatibilité concernés |

Les liens distants servent à consulter les autres roadmaps si le réseau et les droits le permettent. Un dépôt public n'est pas chargé automatiquement par tous les clients IA. Vérifier la source et sa version ; signaler un accès manquant. Une dépendance indisponible empêche de valider le critère qui la consomme, sans arrêter les travaux indépendants.

Pour transmettre un travail : fournir dépôt, branche/SHA, fiche locale, résultat attendu, contraintes et preuves. Les identifiants de travaux sont qualifiés par le dépôt pour éviter les collisions.

La [configuration locale](../../.repo-kit/project.json) et le [manifeste du socle](../../.repo-kit/manifest.json) identifient les commandes et la version installée. Aucun appel de modèle, service récurrent ou lecture distante n'est installé par le kit.
