# Contraintes locales — $name

Ce document appartient au projet et peut être adapté. La configuration exécutable des suites est [.repo-kit/project.json](../../.repo-kit/project.json) ; reporter ici toute évolution des commandes et contrats.

## But et identité

$purpose

Identifiant de dépôt pour les références de travaux : `$id`.
URL déclarée : $repository

## Frontières à préserver

$boundaries

## Entrées techniques existantes

$entry_points

## Prérequis de validation

$prerequisites

## Suites déclarées

Les commandes sont des tableaux d'arguments exécutés depuis la racine sans interpréteur shell implicite. Les seules substitutions d'exécutable sont `$$POWERSHELL` (pwsh ou Windows PowerShell) et `$$GODOT` (HEX_GODOT_PATH ou godot).

$commands

## Limites de preuve

La présence d'un workflow ne prouve pas une exécution distante. La présence d'une référence de paquet ne prouve pas sa publication. La recette visuelle et l'intégration dans chaque consommateur gardent leurs propres preuves.
