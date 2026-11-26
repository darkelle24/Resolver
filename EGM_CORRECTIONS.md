# Corrections de l’audit et vérification EGM

Date : 26 novembre 2026. Audit initial : [AUDIT_REPO.md](AUDIT_REPO.md).

## Résultat

Les six constats A1 à A6 ont été traités. Deux revues indépendantes EGM ont été
effectuées en contexte neuf, en lecture seule : comportement Godot et contrats,
puis distribution et documentation. Une régression de permissions signalée par
la première revue a été corrigée puis relue. Aucun défaut avéré ne reste ouvert
dans ces périmètres. Il ne s’agit pas d’une certification complète de sécurité
ni du cycle de revue d’un nouveau document de conception.

## Corrections

| Constat | Traitement |
|---|---|
| A1 : méthodes Godot | Noms natifs provenant des constantes `MethodName` du SDK ; permissions inscrites sur les types qui fournissent les méthodes ; suppression de noms inexistants |
| A2 : tailles et budgets | Contrat du README limité aux opérations et constantes contrôlées par la VM ; responsabilité des valeurs injectées et des retours hôtes explicitée |
| A3 : capacités des objets | Section modèle de confiance : propriétés, références transitives, destruction, registre global, callbacks et threads |
| A4 : publication | Licence MIT choisie explicitement par le propriétaire, fichier LICENSE, métadonnée MIT et URL du dépôt dans les deux paquets |
| A5 : versions | Version de développement lue depuis Directory.Build.props par le packaging et la CI ; paramètres et tags restent prioritaires |
| A6 : cache local | Instructions utilisant un cache unique à chaque vérification puis rétablissement de la variable précédente |

Un sommaire et une [carte d’architecture pour contribuer](docs/ARCHITECTURE.md)
ont été ajoutés. Les plafonds de compilation et les trois mécanismes d’extension
sont documentés. La CI compare désormais le catalogue versionné avec un export frais.

Le contrat des budgets a été clarifié sans imposer une validation universelle de
toutes les valeurs hôtes. Les permissions de propriétés et de destruction restent
celles de l’adaptateur existant, désormais décrites explicitement.

## Revue indépendante après corrections

| Revue | Statut | Résultat |
|---|---|---|
| Code et contrats, persona EGM codebase_details | Terminée | Régression P2 repérée : perte de transformations de Control et Node3D lors du déplacement des autorisations héritées de Node |
| Relecture ciblée de cette correction | Terminée | Permissions rétablies ; contrôles positifs ajoutés ; finding corrigé, aucun nouveau défaut avéré |
| Livraison et documentation, persona EGM codebase_details | Terminée | Aucun finding avéré sur licence, URL, version, caches, catalogue CI et carte de contribution |

Les lecteurs ont lu leur persona, l’audit initial, les cibles et les sources
pertinentes. Le lecteur code a notamment examiné le registre, l’adaptateur, les
budgets et les nouveaux tests ; le lecteur livraison a examiné le README, la
carte, la licence, les propriétés de construction, le script et le workflow.
Leurs validations étaient statiques : ils n’ont ni compilé ni exécuté les tests.
Les résultats d’exécution suivants ont été obtenus séparément par l’agent principal.

## Validations exécutées

Environnement : Windows, .NET SDK 10.0.301, Godot .NET 4.7.2.

- Suite cœur depuis les sources : **64/64 contrôles réussis**.
- Compilation directe de l’adaptateur : **0 avertissement, 0 erreur**.
- Construction des deux paquets avec version par défaut centralisée : **réussie**.
- Même suite depuis le paquet dans un cache neuf : **64/64 contrôles réussis**.
- Compilation du consommateur du paquet Godot : **0 avertissement, 0 erreur**.
- Import et smoke Godot : **PASS**. Après la correction issue d’EGM, nouveau
  packaging, nouveau cache consommateur, compilation et smoke : **PASS**.
- Le smoke appelle réellement `is_playing`, `start` et `apply_impulse`. Il contrôle
  aussi toutes les permissions par défaut des types testés contre leurs méthodes
  natives et les permissions de transformation attendues sur Node2D, Node3D,
  Control et leurs descendants testés. Une méthode absente de la liste demeure refusée.
- Catalogue exporté comparé au fichier versionné avec la même normalisation que
  le nouveau contrôle CI : **identique**.
- Inspection des deux nuspec : **licence MIT et URL du dépôt présentes**.
- `graphify update .` : **réussi**, 1 560 nœuds et 3 677 arêtes. Extraction de code
  uniquement, sans appel à un modèle ; coût API de cette mise à jour : **0**.

La restauration Godot a utilisé le flux des nouveaux paquets et les dépendances
SDK déjà installées, copiées dans un cache neuf. Une installation entièrement
téléchargée depuis Internet n’a pas été répétée. La CI Linux n’a pas été lancée
à distance et aucun paquet n’a été publié.

## Améliorations différées

La liste des sources propres à Godot reste répétée dans les trois fichiers MSBuild,
comme documenté dans la carte de contribution. Son regroupement était proposé
pour une prochaine évolution de cette frontière, pas comme un défaut actuel.
Le lanceur de tests personnalisé, les versions d’outils flottantes et les limites
de l’audit initial ne changent pas. Aucun benchmark ni audit exhaustif de sécurité
ou de concurrence n’a été réalisé.
