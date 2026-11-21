# Graphify, Obsidian et Hermes en Docker

La stack optionnelle utilise le checkout courant monté dans les conteneurs. Elle est indépendante des conteneurs de développement et ne clone, ne synchronise ni ne change de branche. Avant utilisation, vérifier le chemin, la branche et les changements avec Git. Ne pas modifier les mêmes fichiers simultanément dans plusieurs assistants.

| Service | Rôle | Persistance |
|---|---|---|
| graphify | Extraire le code localement, exporter le second coffre et interroger le graphe | `graphify-out/` dans ce clone |
| obsidian | Afficher Obsidian dans le navigateur, sans installation native obligatoire | Préférences dans un volume Docker ; notes dans le dépôt |
| hermes | Session interactive dans `/workspace`, avec le modèle local | Profil, mémoire et sessions dans `.assistant-local/hermes/` |
| ollama | Exécuter le modèle local pour Hermes | Poids dans un volume Docker propre à cette stack |

Hermes utilise l'image officielle NousResearch ; Graphify-Labs/graphify est installé via `graphifyy[mcp]==0.9.113`. Obsidian est distribué par **LinuxServer.io**, un fournisseur tiers, avec une interface HTTPS Selkies. Le [Dockerfile](../../tools/assistant-stack/Dockerfile) installe les outils sans incorporer le code du projet : son contexte de construction n'autorise que les fichiers nécessaires aux images.

## Prérequis

Docker Desktop ou Engine avec conteneurs Linux et Compose v2, Python 3.11+ et Git sur l'hôte. Pour un nouveau poste, utiliser une version maintenue de Docker/Compose. Les images et le premier modèle demandent du réseau et plusieurs Go de stockage. Le mode CPU fonctionne sans NVIDIA mais peut être lent ; une fenêtre de 64 000 tokens demande aussi de la RAM/VRAM pour le cache.

Le checkout doit avoir un HEAD Git réel pour la fraîcheur Graphify. Un worktree dont le fichier `.git` pointe hors du dossier monté nécessite une adaptation des montages ; commencer avec un clone autonome. Ne pas transférer le venv Windows dans Linux : les images utilisent `/opt/graphify` via `GRAPHIFY_ENV`.

## Premier setup depuis la racine du dépôt

```powershell
python tools/assistant-stack/stack.py setup --graph
python tools/assistant-stack/stack.py config
python tools/assistant-stack/stack.py build
python tools/assistant-stack/stack.py up
python tools/assistant-stack/stack.py pull-model
python tools/assistant-stack/stack.py graph-build
python tools/assistant-stack/stack.py doctor
python tools/assistant-stack/stack.py hermes
```

`setup` prépare les fichiers locaux et complète les exclusions Git ; il ne lance rien et ne télécharge aucun modèle. Il conserve les configurations déjà présentes, y compris celles d'Obsidian. `--graph` ajoute le MCP Graphify uniquement lors de la création du profil Hermes ; construire le graphe avant d'ouvrir Hermes. Pour utiliser Hermes sans MCP, omettre cette option au premier setup. Ajouter ensuite un serveur dans le profil existant est une modification manuelle, pas un écrasement par le setup.

`build` construit les images Graphify et Hermes. `up` démarre seulement Ollama et Obsidian ; Hermes est une session interactive lancée à la demande et Graphify une commande ponctuelle. `pull-model` télécharge `qwen3.5:4b`, puis crée l'alias `repository-local-64k` avec `num_ctx 64000`. `doctor` vérifie la présence de cet alias et la configuration Hermes ; il ne prouve pas un appel d'outil réussi par le modèle.

Pour choisir un autre modèle au **premier** setup : `setup --model NOM --graph`. Dans un profil existant, adapter ensemble `.assistant-local/settings.json` et `.assistant-local/Modelfile`, puis relancer `pull-model` ; le nom d'alias Hermes reste le même. Le modèle doit supporter les outils et la fenêtre demandée. Ce setup utilise un Ollama conteneurisé distinct d'une éventuelle installation hôte : les poids déjà téléchargés sur Windows ne sont pas réutilisés automatiquement.

## NVIDIA et réglages locaux

Avec un moteur Docker compatible GPU NVIDIA, placer `--gpu` **avant l'action** :

```powershell
python tools/assistant-stack/stack.py --gpu up
python tools/assistant-stack/stack.py --gpu pull-model
python tools/assistant-stack/stack.py --gpu doctor
python tools/assistant-stack/stack.py --gpu hermes
```

Conserver ce choix pour les commandes qui démarrent les services. L'overlay [GPU](../../tools/assistant-stack/compose.gpu.yaml) réserve une carte pour Ollama ; Obsidian utilise le rendu CPU. Sous Linux, préparer le pilote et NVIDIA Container Toolkit ; sous Docker Desktop, activer le backend et la prise en charge GPU compatibles avec le poste. Une carte détectée sur Windows ne prouve pas son accès depuis Docker.

Le fichier ignoré `.assistant-local/compose.env` reçoit les surcharges `ASSISTANT_UID`, `ASSISTANT_GID`, `ASSISTANT_TZ`, `ASSISTANT_OBSIDIAN_PORT` et les images `ASSISTANT_*_IMAGE` déclarées dans le [Compose](../../tools/assistant-stack/compose.yaml). Sous Linux, aligner UID/GID sur `id -u` et `id -g` avant le premier lancement ; les valeurs initiales sont 1000/1000. La stack utilise un nom de projet dérivé du chemin absolu du clone pour isoler les volumes entre dépôts. Déplacer le clone change ce nom et demande de récupérer les anciens volumes explicitement.

Hermes et Ollama ont des tags de version ; Obsidian suit `latest` tant qu'un digest n'est pas renseigné. Pour une installation reproductible, épingler les images par digest après recette. Les dépendances Python transitives ne sont pas intégralement verrouillées. Aucun téléchargement n'est une preuve de compatibilité fonctionnelle.

## Ouvrir Obsidian et utiliser le graphe

Après `up`, ouvrir **https://localhost:3001**. Accepter le certificat autosigné local, puis « Ouvrir un dossier comme coffre » :

- `/vaults/documentation` correspond au dossier `doc/` du checkout. Les modifications des notes écrivent dans le dépôt.
- `/vaults/code` correspond à `graphify-out/obsidian/`, après `graph-build`. Ses notes sont générées ; conserver les décisions durables dans la documentation.

Le port est lié à `127.0.0.1`, sans exposition LAN par défaut. L'interface inclut les fonctionnalités du bureau LinuxServer ; garder cet accès local. Aucun compte Obsidian, plugin communautaire ou serveur MCP Obsidian n'est requis : Hermes lit les mêmes Markdown depuis `/workspace/doc/`.

```powershell
python tools/assistant-stack/stack.py graph-status
python tools/assistant-stack/stack.py query NomDeSymbole
python tools/assistant-stack/stack.py status
python tools/assistant-stack/stack.py down
```

Graphify utilise le lanceur de fraîcheur du dépôt : après modification de fichiers, HEAD ou branche, reconstruire et redémarrer la session MCP. L'export hôte et l'export Docker portent des chemins de clone différents ; reconstruire en changeant d'environnement. Le service Graphify n'a pas de réseau au runtime et force le mode code-only. Il n'analyse pas sémantiquement les documents et ne garantit pas la couverture de tous les langages, notamment GDScript.

Dans Hermes, vérifier `pwd`, `git status --short --branch`, puis demander la lecture de `AGENTS.md` et du routage IA. Faire une requête Graphify ciblée et vérifier le chemin source retourné. Choisir les outils et la confiance des skills selon le besoin ; le setup n'accorde pas automatiquement confiance aux skills et ne configure aucune messagerie, automatisation ni route cloud. Le profil natif Hermes de l'hôte reste distinct.

`down` conserve modèles et préférences. Ne pas ajouter `--volumes` pour un simple arrêt. Les profils, sessions, caches et modèles ne suivent pas Git ; sur un autre poste, reprendre les documents versionnés et refaire le setup local.

## Template des dépôts auxiliaires et contrôles

Le kit 1.2.0 copie ce guide, les scripts de stack, le lanceur Graphify et ses dépendances dans chaque dépôt équipé. Il transporte aussi leur instantané dans `.repo-kit/payloads/` pour rester autonome. Les fichiers communs modifiés localement provoquent un conflit lors d'une mise à jour. Le setup local demeure une étape explicite ; installer le template ne lance ni Docker ni le modèle.

Les contrôles à exécuter sont : tests du kit et du setup, tests du lanceur Graphify, `config` en mode CPU et GPU, construction des deux cibles, extraction réelle, ouverture des deux coffres et un appel d'outil depuis Hermes. Noter les résultats réels : une configuration Compose valide ne prouve pas que le moteur, le GPU, le modèle ou l'interface fonctionnent.

Sources du setup : [Docker Hermes officiel](https://hermes-agent.nousresearch.com/docs/user-guide/docker), [Obsidian LinuxServer](https://docs.linuxserver.io/images/docker-obsidian/), [Graphify](https://github.com/Graphify-Labs/graphify), [GPU Compose](https://docs.docker.com/compose/how-tos/gpu-support/). Les parcours natifs existants restent utilisables séparément.
