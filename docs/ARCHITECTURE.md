# Architecture et contribution

Le chargeur de mods sélectionne un fragment JSON. `VmJsonCompiler` le transforme
en `CompiledScript` réutilisable ; `VmEngine.Execute` crée une VM par appel avec
ses variables, puis retourne une valeur ou une erreur typée et une trace optionnelle.
La provenance fournie par le chargeur est conservée dans les diagnostics.

## Frontières

- `core/Resolver.Core.csproj` : moteur .NET sans Godot, compilateur, valeurs,
  bytecode, diagnostics et registre des nœuds sous `src/central`.
- `generator/` : générateur Roslyn du dispatch des opcodes et des méthodes de
  l’assembleur. Il sert à construire le cœur et ne doit pas devenir une dépendance
  des applications consommatrices.
- `src/godot/` et les exceptions de sources listées dans `Resolver.Godot.props` :
  façades GDScript, conversions et accès aux objets Godot. Ces sources sont
  compilées dans le projet du jeu pour respecter ses chemins `res://`.
- `packaging/` : paquet adaptateur et restauration des sources dans le dossier
  généré `addons/resolver/` du consommateur.

Le compilateur et le catalogue utilisent le même `VmNodeRegistry`. Les handles
gardent leurs définitions et callbacks photographiés après remplacement d’un nœud.
Les tables d’exécution sont séparées ; les objets hôtes et collections injectées
gardent leurs références. La politique des méthodes Godot reste globale au processus.

## Où intervenir

Utiliser `RegisterFunction` pour un callback hôte, `RegisterNode` pour un nœud
documenté à entrées évaluées immédiatement, `RegisterCompilerNode` pour un contrôle
de flux personnalisé. La compatibilité `RuleNodeProcessor` est un chemin historique
distinct, couvert par des tests ; elle n’enregistre pas automatiquement ses nœuds
dans la nouvelle API.

Un nouvel opcode nécessite son attribut, son implémentation et une vérification
de sa correspondance source. Pour une évolution de la frontière Godot, garder
alignés les exclusions du cœur, `Resolver.Godot.props` et les inclusions du paquet.
Cette liste reste répétée aujourd’hui ; son regroupement est une amélioration
possible lors d’une prochaine évolution de cette frontière.

## Valider une modification

Suivre les commandes de [vérification et packaging](../README.md#vérification),
puis tester le paquet dans un cache neuf et exécuter le smoke Godot.
Le lanceur `tests/Program.cs` renvoie un code non nul si un contrôle échoue.
Après changement des définitions de nœuds, exporter le catalogue :

```powershell
dotnet run --project tests/Resolver.Tests.csproj -- --export-node-catalog docs/node_catalog.json
```

La CI compare ce fichier avec un export frais. La version de développement vient
de `Directory.Build.props` ; les publications utilisent la version du tag.
Voir aussi le [modèle de confiance](../README.md#modèle-de-confiance).
