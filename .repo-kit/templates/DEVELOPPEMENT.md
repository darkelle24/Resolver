# Développer $name

$purpose

- [Contraintes, points d'entrée et commandes](PROJET.md).
- [Roadmap locale](TODO.md) : travaux et preuves du composant.
- [Organiser la roadmap](ROADMAP.md) : découpage, prérequis, branches et preuves.
- [Commits Gitmoji et VS Code](COMMITS.md) : format des messages et génération par défaut.
- [Organisation commune](ORGANISATION.md) : statuts, réalisation et revues.
- [Dépendances et consommateurs](DEPENDANCES.md) : contrats entre dépôts.
- [Décisions](decisions/README.md) et modèles de [travail](modeles/TRAVAIL.md) / [décision](modeles/DECISION.md).
- [Graphify, Obsidian et Hermes en Docker](ASSISTANTS_DOCKER.md) : setup local optionnel et modèle Ollama.
- [Routage IA](../ai/README.md) et [documentation technique existante](../../README.md).

Python 3.11+ et Git suffisent pour le kit ; les outils du composant restent requis pour ses propres suites.

```text
python .repo-kit/kit.py check --target .
python .repo-kit/kit.py validate --target . --suite Docs
```

Pour une suite de code déclarée, commencer par `validate --target . --suite NOM --list`, puis retirer `--list` pour l'exécuter. Le lanceur s'arrête au premier échec ; délai de 300 secondes par commande, réglable avec `--timeout` (1..3600). Les sorties et le bilan sont affichés dans le terminal. Sauvegarder une preuve datée dans la fiche du travail si nécessaire.

Le kit ne crée ni dépôt distant, ni secret, ni publication. La CI existante reste la référence ; un nouveau projet doit définir ses suites et son environnement avant d'annoncer une validation de code.
