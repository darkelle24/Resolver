# Convention des commits — Gitmoji

Les nouveaux commits utilisent **un emoji Gitmoji**, un périmètre utile et un titre en français à l’infinitif. Cette convention s’applique aux prochains commits ; ne pas réécrire les anciens.

## Format

~~~text
<emoji> <périmètre>[!]: <Action et résultat concret>

<Pourquoi ce changement est nécessaire et compromis utiles, si besoin.>

Changements principaux:
<emoji> <périmètre>[!]: <Action et résultat concret>
<emoji> <périmètre>[!]: <Autre changement majeur du commit>

Objectif: MNN-TNN-ONN
Validation: <contrôle réellement exécuté et résultat, ou non exécutée>
~~~

Le titre vise au plus 72 caractères, emoji compris, sans point final. Les crochets indiquent que `!` est facultatif : ne pas les écrire dans le message. Le périmètre est une zone technique courte et stable, en minuscules : `doc`, `outils`, `ui`, `réseau`, `tests`, `configuration`, ou un composant réel du dépôt. Éviter les listes de fichiers et les identifiants d'objectifs dans le titre ; ils appartiennent au corps pour garder l'historique lisible par domaine. Les anciens titres avec identifiant restent valides historiquement.

Commencer le sujet par un verbe à l'infinitif et décrire le résultat observable : « Empêcher une double application » plutôt que « Modifier le code ». Choisir un changement cohérent par commit ; séparer des intentions indépendantes quand elles peuvent être relues et annulées séparément.

Après le titre et l’explication facultative du pourquoi, ajouter un mini changelog sous `Changements principaux:`. Chaque ligne reprend le format `<emoji> <périmètre>[!]: <Action et résultat concret>`, avec un verbe à l’infinitif, un emoji adapté à son intention et son propre périmètre. Résumer seulement les principaux changements réellement présents dans le commit, sans inventaire par fichier ni répétition du titre. Une seule ligne suffit pour un changement simple. Le mini changelog ne justifie pas de regrouper des intentions indépendantes.

L’explication du problème résolu et des compromis reste facultative, sans recopier le diff. Ajouter ensuite `Objectif:` avec le véritable identifiant, ou `Objectifs:` pour plusieurs travaux réellement concernés. Ne pas inventer un identifiant ou un test pour compléter le message. Une ligne `Validation:` ne rapporte que des contrôles connus ; indiquer « non exécutée » si l'information est utile, sinon l'omettre.

Pour une rupture de contrat public nécessitant une adaptation chez les consommateurs, ajouter `!` après le périmètre et un paragraphe `Rupture:` expliquant le contrat changé et sa migration. Un refactoring interne n'est pas une rupture. Ce format Gitmoji ne prétend pas être Conventional Commits ; un outil de publication doit être configuré explicitement avant d'en déduire une version.

## Repères

| Gitmoji | Intention |
|---|---|
| ✨ | Ajouter une fonctionnalité |
| 🐛 | Corriger un défaut |
| ♻️ | Refactoriser sans changement fonctionnel prévu |
| ✅ | Ajouter ou adapter des tests |
| 📝 | Ajouter ou actualiser la documentation |
| 🔧 | Modifier la configuration |
| 🔨 | Ajouter ou modifier un outil de développement |
| 👷 | Ajouter ou modifier la CI |
| 🚚 | Déplacer ou renommer des ressources |
| ⚡️ | Améliorer une performance mesurée |
| 🔒️ | Corriger un problème de sécurité |
| ⏪️ | Annuler un changement |

Pour le titre, choisir l’intention principale du diff. Dans le mini changelog, choisir l’emoji de chaque changement majeur, pas un emoji par fichier. ✅ désigne un changement de tests ; il ne certifie pas que tous les contrôles passent. Une simple case cochée ne justifie pas un message de livraison. La liste complète appartient au [référentiel Gitmoji](https://gitmoji.dev/).

## Exemples fictifs

~~~text
🐛 simulation: Empêcher une double mutation lors d’un retry

Réutiliser l’accusé de la commande pour conserver une seule application.

Changements principaux:
🐛 simulation: Renvoyer l’accusé existant lors d’un retry
✅ tests: Couvrir la répétition d’une commande déjà appliquée

Objectif: M34-T01-O02
Validation: non exécutée
~~~

~~~text
📝 doc: Décrire le cycle de revue des milestones

Changements principaux:
📝 doc: Préciser les preuves attendues pour chaque revue
~~~

~~~text
✨ sauvegarde!: Versionner le contrat des instantanés

Changements principaux:
✨ sauvegarde!: Exiger une version explicite des instantanés

Rupture: Exiger une version explicite à la lecture des instantanés.
Migration: Convertir les anciens instantanés avant leur chargement.
Validation: non exécutée
~~~

Ces exemples ne prouvent aucune réalisation. Un commit ne doit inclure que des changements cohérents et autorisés. Préserver les modifications étrangères ; ne pas amender, forcer le push, supprimer une signature ou contourner des hooks sans autorisation adaptée.

## Rédaction par une IA

Lire le diff préparé avec Git et les consignes du dépôt. Décrire le résultat réel, sa raison et les limites connues. Choisir un emoji du tableau et un périmètre vérifié. Produire **uniquement le message de commit**, sans bloc de code, préambule ni titre de réponse, lorsque la demande consiste à générer ce message. Si le diff est vide, ne pas inventer de message.

Un message généré ne crée pas un commit et n'autorise pas un push. Lire le diff de l'index Git ; si aucun changement n'est préparé, signaler la situation sans ajouter de fichier arbitrairement.

## Génération dans VS Code

Le kit crée les [réglages VS Code](../../.vscode/settings.json) s’ils sont absents et associe ce fichier à `github.copilot.chat.commitMessageGeneration.instructions`. Préparer les changements puis utiliser le bouton de génération du message dans le panneau de contrôle de code source. Relire le résultat avant de valider.

Ce réglage concerne la génération Copilot de VS Code lorsqu’elle est disponible et activée ; il ne garantit pas le comportement de tous les autres générateurs d’extensions. Fournir le présent guide aux autres IA. La configuration suit le [mécanisme officiel des instructions VS Code](https://code.visualstudio.com/docs/agent-customization/custom-instructions#specify-instructions-for-generated-content).

Si des réglages existaient avant l'installation, le kit les conserve intégralement. Ajouter ou compléter manuellement cette propriété dans `.vscode/settings.json`, en conservant les autres propriétés et les instructions déjà utiles :

```json
{
  "github.copilot.chat.commitMessageGeneration.instructions": [
    { "file": "doc/developpement/COMMITS.md" }
  ]
}
```

Ouvrir la racine du dépôt comme dossier VS Code pour que ce chemin soit résolu dans le bon projet. La présence du réglage ne prouve pas qu'un fournisseur IA l'a appliqué ; vérifier un message généré dans l'interface avec un diff réel.

## Trace des objectifs

Après le commit de réalisation, reporter son SHA réel et son apport dans la fiche d’objectif, puis enregistrer cette trace dans un commit documentaire suivant. Ne pas tenter de référencer dans un commit son propre SHA. Voir les [règles communes](ORGANISATION.md) et le [guide de roadmap](ROADMAP.md).

Retour aux [guides de développement](./README.md).
