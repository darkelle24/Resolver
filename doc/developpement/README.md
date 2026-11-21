# Développer Resolver

Compiler et exécuter des fragments JSON avec diagnostics et budgets, en fournissant un cœur C# autonome et un adaptateur Godot distribué séparément.

- [Contraintes, points d'entrée et commandes](PROJET.md).
- [Roadmap locale](TODO.md) : travaux et preuves du composant.
- [Organisation commune](ORGANISATION.md) : statuts, réalisation et revues.
- [Dépendances et consommateurs](DEPENDANCES.md) : contrats entre dépôts.
- [Décisions](decisions/README.md) et modèles de [travail](modeles/TRAVAIL.md) / [décision](modeles/DECISION.md).
- [Routage IA](../ai/README.md) et [documentation technique existante](../../README.md).

Python 3.11+ et Git suffisent pour le kit ; les outils du composant restent requis pour ses propres suites.

```text
python .repo-kit/kit.py check --target .
python .repo-kit/kit.py validate --target . --suite Docs
```

Pour une suite de code déclarée, commencer par `validate --target . --suite NOM --list`, puis retirer `--list` pour l'exécuter. Le lanceur s'arrête au premier échec ; délai de 300 secondes par commande, réglable avec `--timeout` (1..3600). Les sorties et le bilan sont affichés dans le terminal. Sauvegarder une preuve datée dans la fiche du travail si nécessaire.

Le kit ne crée ni dépôt distant, ni secret, ni publication. La CI existante reste la référence ; un nouveau projet doit définir ses suites et son environnement avant d'annoncer une validation de code.

## Essai local du 21 novembre 2026

Hôte Windows, Python 3.11, SDK .NET 10.0.403 et Windows PowerShell 5.1. Kit 1.0.0 en modifications locales dans HexGameGodot, base `ce224a5ecd1625f556212524f29d834b3df1eeab`. Cible : clone Resolver, branche main, base `ead75c3dd8bc20ce04a616d8cdaad5d69f66a0ff`, initialement propre. Les changements d'organisation des deux dépôts restent non commités ; ces SHA identifient les bases et non une livraison du kit.

| Contrôle réellement exécuté | Résultat |
|---|---|
| `python -m unittest discover -s tools/repository-kit/tests -v` dans le jeu | 12 tests réussis : installation, autonomie, conservation, mises à jour, conflits, frontières et erreurs de validation |
| Installation avec le profil Resolver après aperçu | Socle et documents locaux créés ; sources, packaging et workflow existants préservés |
| `python .repo-kit/kit.py check --target .` dans Resolver | REUSSIE : structure, empreintes et destinations locales des liens |
| `python .repo-kit/kit.py validate --target . --suite Core` dans Resolver | REUSSIE : 64/64 tests C# |
| `python .repo-kit/kit.py validate --target . --suite Build` dans Resolver | REUSSIE : adaptateur compilé, zéro erreur et avertissement |
| `python .repo-kit/kit.py validate --target . --suite Package` dans Resolver | REUSSIE : deux paquets locaux 0.1.0 construits, sans publication |
| Aperçu `update` vers Resolver après installation | Aucun fichier commun à changer ; README local conservé |
| Tests Python du moteur de contexte du jeu | 21 tests réussis ; catalogue régénéré et contrôlé |

Le README technique de Resolver reçoit seulement des liens de navigation vers son guide, sa roadmap et son contexte IA. Les sorties de compilation et paquets restent dans ses dossiers ignorés. Import moteur, recette visuelle Godot, tests consommateurs des paquets, CI distante et publication : NON_EXECUTEE pendant cet essai. Aucun commit, push, tag ni dépôt distant créé.
