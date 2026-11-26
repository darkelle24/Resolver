# Resolver : compilation de fragments JSON et VM de modding

Le chargeur fusionne les données des mods puis sélectionne les expressions à
compiler. Il passe directement chaque fragment JSON et ses métadonnées au
resolver. Le jeu conserve le CompiledScript retourné et l'exécute avec ses
variables. Les autres champs de l'item ne sont ni compilés ni modifiés.


## Navigation

- [Architecture et contribution](docs/ARCHITECTURE.md)
- [Distribution NuGet](#distribution-nuget)
- [Catalogue des nœuds](#catalogue-json-des-nœuds-disponibles)
- [Modèle de confiance](#modèle-de-confiance)
- [Vérification](#vérification)

## Distribution NuGet

Ce dossier est un projet autonome à pousser dans son propre dépôt GitHub. Il ne contient
pas l'historique Git du jeu. Les noms proposés sont `Darkelle.Resolver.Core` et
`Darkelle.Resolver.Godot`, version initiale `0.1.0` ; vérifier leur disponibilité sur
nuget.org avant la première publication. Aucun paquet n'a encore été publié.

- Le cœur .NET 10 est livré comme DLL dans le cache NuGet.
- Le paquet Godot dépend du cœur et restaure ses scripts dans `addons/resolver/`,
  un dossier généré à ignorer par Git. Ne pas modifier ces copies.
- Le générateur Roslyn reste un outil de compilation du cœur ; il n'est pas une
  dépendance requise dans les applications consommatrices.

### Installation dans un jeu Godot 4.7.2 .NET

Ajouter une référence `PackageReference` à `Darkelle.Resolver.Godot` avec
`Version="[0.1.0]"` au projet du jeu, puis exécuter `dotnet build` avant le premier
import Godot. Pour seulement préparer les ressources :

```powershell
dotnet build 'MonJeu.csproj' --target:RestoreResolverGodotSources
```

Ignorer `/addons/resolver/` dans le dépôt du jeu. Les scripts se chargent depuis
`res://addons/resolver/src/godot/`. Une bibliothèque sans Godot référence seulement
`Darkelle.Resolver.Core`. La version exacte choisie reste enregistrée par le jeu.

### Préparation locale avant la première publication

```powershell
./tools/Pack.ps1
dotnet run --project tests/Resolver.Tests.csproj
# Conserver cette valeur pour la restaurer après les vérifications.
$previousPackageCache = $env:NUGET_PACKAGES
# Un cache neuf à chaque passage évite de tester un ancien paquet de même version.
$env:NUGET_PACKAGES = Join-Path ([IO.Path]::GetTempPath()) ("resolver-check-" + [guid]::NewGuid())
dotnet restore tests/Resolver.Tests.csproj -p:ResolverUsePackage=true --source ./artifacts/packages --source https://api.nuget.org/v3/index.json
dotnet run --project tests/Resolver.Tests.csproj -p:ResolverUsePackage=true --no-restore
dotnet restore tests/godot-project/Resolver.Godot.Smoke.csproj --source ./artifacts/packages --source https://api.nuget.org/v3/index.json
dotnet build tests/godot-project/Resolver.Godot.Smoke.csproj --no-restore
$env:NUGET_PACKAGES = $previousPackageCache
```

La source locale est passée à la commande ; elle ne remplace pas la source de
production et ne doit pas être ajoutée à une configuration utilisateur globale.

### Publier depuis GitHub Actions

1. Créer le dépôt GitHub vide puis y pousser le contenu de ce dossier (branche `main`).
2. Créer un compte nuget.org et une clé API autorisée à publier les deux paquets.
3. Dans GitHub, créer l'environnement `nuget` et son secret `NUGET_API_KEY`.
4. Choisir les permissions/protections de cet environnement selon le projet.
5. Créer et pousser un tag `v0.1.0` pour publier la première version.

Commandes depuis ce dossier, après création d'un dépôt GitHub vide (remplacer
`URL_DU_DEPOT` par son URL réelle) :

```powershell
git init -b main
git add .
git commit -m "🎉 Extraire le resolver et préparer sa distribution NuGet"
git remote add origin URL_DU_DEPOT
git push -u origin main
# Après configuration du secret NUGET_API_KEY :
git tag v0.1.0
git push origin v0.1.0
```

Le workflow `.github/workflows/nuget.yml` teste les sources, construit les paquets,
teste le cœur empaqueté et l'adaptateur dans un projet Godot consommateur, puis
publie uniquement pour un tag `v...`. Les PR, push sur main et lancements manuels
valident et produisent des artefacts sans publier. La version provient du tag.
Les secrets ne sont pas nécessaires pour les validations. Les publications
existantes sont ignorées pour permettre de reprendre une publication partielle.

Pour une nouvelle version, mettre à jour la version de développement dans
`Directory.Build.props`, puis pousser le tag correspondant. Le script de packaging
et la CI hors tag lisent cette même valeur. `Pack.ps1 -Version` et les tags
permettent de sélectionner explicitement une autre version.
Le jeu adopte ensuite explicitement cette version et renouvelle ses validations.
La publication effective et la disponibilité des noms restent à vérifier dans
ton compte nuget.org. Le projet est distribué sous [licence MIT](LICENSE).

## Projets et points d'entrée

- [Resolver.Core](./core/Resolver.Core.csproj) : bibliothèque .NET 10 sans Godot.
- [VmEngine](./src/central/runtime/VmEngine.cs) : API C# indépendante du moteur.
- [VmEngineNode](./src/godot/VmEngineNode.cs) : API Godot/GDScript.
- [VmCompilationMetadata](./src/central/compiler/VmCompilationMetadata.cs) :
  identifiant, chemin du fragment, provenance et étiquettes de l'application.
- [Resolver.Godot.props](./Resolver.Godot.props) : inclut l'adaptateur dans l'assembly
  principal Godot et référence le cœur. Les autres sources restent exclues de cet
  assembly, notamment le générateur et les tests.
- [Resolver.csproj](./Resolver.csproj) : compilation autonome de l'adaptateur.

L'import MSBuild garantit que les chemins des scripts C# exposés à GDScript sont
générés relativement au projet Godot du jeu, tandis que le cœur reste une DLL
indépendante.

## Compiler seulement onAttack depuis GDScript

Le dictionnaire item est déjà produit par votre chargeur/fusionneur.
[Exemple complet d'item](./examples/sword.json) : name et price sont des données ;
onAttack est une expression.

Dans votre fonction de chargement :

~~~gdscript
const EngineScript = preload("res://addons/resolver/src/godot/VmEngineNode.cs")
const MetadataScript = preload("res://addons/resolver/src/godot/VmCompilationMetadataObject.cs")
const OptionsScript = preload("res://addons/resolver/src/godot/VmExecutionOptionsObject.cs")

var engine = EngineScript.new()
var item = {
    "name": "Excalibur",
    "price": "10",
    "onAttack": {
        "operation": "multiply",
        "inputs": {
            "left": {"operation": "read_variable", "name": "hp_max_target"},
            "right": 10
        }
    }
}

var metadata = MetadataScript.new()
metadata.ScriptId = "items/sword:onAttack"
metadata.SourceFile = "<merged>"
metadata.RootPath = "/onAttack"
metadata.Tags["item_id"] = "sword"

# Une fois après la fusion. Seul ce sous-arbre est transmis au compilateur.
var compilation = engine.CompileFragment(item["onAttack"], metadata)
if not compilation.Success:
    print(compilation.Error.ToJson())
    return

var attack_script = compilation.Script

# Pendant le jeu : aucun accès fichier et aucun parsing JSON.
var result = engine.Execute(attack_script, {"hp_max_target": 50})
if result.Success:
    print(result.Value) # 500
else:
    print(result.Error.ToJson())

var options = OptionsScript.new()
options.Explain = true
options.MaxTraceSteps = 10000
options.MaxInstructions = 1000000

var detailed = engine.Execute(attack_script, {"hp_max_target": 100}, options)
print(detailed.GetExplanationJson())
# Racine : /onAttack ; enfant gauche : /onAttack/inputs/left.
if detailed.Explanation and detailed.Explanation.Tree:
    var left = detailed.Explanation.Tree.Inputs["left"].Node
    print(left.Path, " ", left.Output.Value)

# name et price restent accessibles normalement :
print(item["name"], " ", item["price"])

engine.free()
~~~

Si le document fusionné contient une collection d'items, RootPath peut être
"/items/sword/onAttack". Ce chemin est fourni par l'appelant ; le compilateur
ne cherche pas lui-même des champs onAttack dans tout le document.

CompileFragment accepte un dictionnaire/tableau/scalaire JSON déjà parsé.
Une chaîne est une constante de script. Si vous disposez du texte d'un fragment,
utilisez CompileFragmentJson(json_text, metadata).

CompileJson(json_text, source_file, script_id) et LoadAndCompile(path) restent
disponibles pour un document contenant uniquement un script ; ils ne sont pas
nécessaires dans le pipeline de fusion.

Les méthodes sont appelées avec leur casse C# : Execute, ToJson, etc.
Godot ne transmet pas les valeurs par défaut des paramètres C# à GDScript :
la façade déclare de vraies surcharges par nombre d'arguments.

Les erreurs sont journalisées automatiquement dans Godot. Pour gérer soi-même
l'affichage : engine.LogErrors = false. Le diagnostic retourné reste disponible.

## Utilisation depuis C# sans Godot

~~~csharp
using System.Text.Json.Nodes;
using VMRuntime;
using VMRuntime.Diagnostics;
using VMRuntime.Logging;

var engine = new VmEngine(new TextWriterVmLogger(Console.Error));

// Dans le jeu, ce JsonNode vient déjà du fusionneur.
var item = JsonNode.Parse("""
{
  "name": "Excalibur",
  "price": "10",
  "onAttack": {
    "operation": "multiply",
    "inputs": {
      "left": { "variable": "hp_max_target" },
      "right": 10
    }
  }
}
""")!;

var metadata = new VmCompilationMetadata
{
    ScriptId = "items/sword:onAttack",
    SourceFile = "<merged>",
    RootPath = "/onAttack",
    Tags = new Dictionary<string, string> { ["item_id"] = "sword" }
};

var compilation = engine.CompileFragment(item["onAttack"], metadata);
if (!compilation.Success)
{
    Console.WriteLine(compilation.Error!.ToJson());
    return;
}

var result = engine.Execute(
    compilation.Script!,
    new Dictionary<string, DynValue> { ["hp_max_target"] = DynValue.FromInt(50) },
    new VmExecutionOptions { Explain = true }
);
if (result.Success)
    Console.WriteLine(result.Value.AsInt()); // 500
else if (result.Error is VmVariableNotFoundError missing)
    Console.WriteLine(missing.VariableName);

Console.WriteLine(result.ToJson());
~~~

CompileFragment accepte aussi un JsonElement sélectionné dans un JsonDocument.
Le handle ne conserve pas de référence au document ; il reste valide après sa
libération. Compiler un fragment ne modifie jamais son objet parent.

Le constructeur prépare le moteur ; aucune initialisation globale n'est
nécessaire dans le cœur. Chaque invocation possède sa pile et sa table de
variables. Les collections et objets fournis en variables restent des références :
leurs mutations sont intentionnellement visibles pour l'hôte.

## Métadonnées et provenance après fusion

Les métadonnées sont une classe typée, pas des champs à ajouter au langage JSON :

| Propriété | Signification |
|---|---|
| ScriptId | Identifiant du script ; par défaut SourceFile suivi de RootPath. |
| SourceFile | Identifiant/fichier de repli ; le compilateur ne l'ouvre pas. |
| RootPath | JSON Pointer de l'expression dans le document fusionné. |
| SourceMap | Dictionnaire de positions d'origine, indexé par pointeurs relatifs au fragment. |
| Tags | Étiquettes de l'application, par exemple item_id, mod_id ou category. |

SourceMap est facultatif. Pour une expression qui vient d'un fichier identifié,
SourceFile peut contenir ce chemin. Pour un mélange dont l'origine précise n'est
pas connue, conserver "<merged>" évite de désigner arbitrairement le dernier mod.

Le fusionneur peut fournir une origine différente pour chaque entrée :

~~~gdscript
const OriginScript = preload("res://addons/resolver/src/godot/VmSourceOriginObject.cs")

var origin = OriginScript.new()
origin.File = "mods/base/items.json"
origin.Path = "/sword/onAttack/inputs/left"
origin.Line = 42
origin.ByteColumn = 9

metadata.SourceMap["/inputs/left"] = origin
# La clé "" désigne l'origine de la racine du fragment.
~~~

Une entrée SourceMap exacte conserve fichier, chemin, ligne et colonne d'origine.
Une origine d'ancêtre donne le fichier et un préfixe de chemin aux descendants,
mais ne permet pas d'inventer leur ligne : leurs positions restent inconnues.

Après parsing/fusion, les positions de caractères originales ne peuvent pas être
retrouvées par une simple sérialisation. Les positions non fournies valent 0 et
HasPosition est false. Le fusionneur doit enregistrer une provenance par nœud
s'il souhaite des lignes exactes. L'historique file_entries d'un item ne suffit
pas à lui seul à attribuer chaque champ à un fichier et une ligne.

Deux chemins sont distingués :

- Path d'un nœud d'explication, et Error.JsonInstruction.Path : position dans le
  document fusionné, avec RootPath comme préfixe.
- Source.Path : position dans le fichier original indiqué par Source.File.

Le handle conserve une copie indépendante de ces métadonnées. Modifier ensuite
un objet Origin ou le dictionnaire de métadonnées ne change pas un script compilé.

## Catalogue JSON des nœuds disponibles

Le catalogue est produit par le registre qui sert à compiler les scripts.
Il contient les 41 nœuds intégrés et les nœuds du projet enregistrés sur cette
instance de moteur. La définition d'un nœud fournit aussi sa documentation.

Depuis GDScript :

~~~gdscript
var catalog_json = engine.GetNodeCatalogJson()
print(catalog_json)

var catalog = engine.GetNodeCatalog()
for node in catalog.Nodes:
    print(node.Operation, " ", node.Description, " ", node.Output.Types)
~~~

Depuis C# :

~~~csharp
VmNodeCatalog catalog = engine.GetNodeCatalog();
string json = engine.GetNodeCatalogJson();
~~~

[Catalogue JSON intégré prêt à consommer](./docs/node_catalog.json).
Ce fichier est un export du registre par défaut ; l'API du moteur exporte aussi
les extensions effectivement enregistrées dans votre jeu.

### Contrat du catalogue, format_version = 1

| Champ racine | Contenu |
|---|---|
| format_version | Version du contrat JSON du catalogue. |
| registry_version | Révision du registre dans cette instance du moteur. |
| registry_hash | Empreinte de ses définitions, types littéraux et conventions de syntaxe. |
| syntax | Champs operation/type, conteneur inputs, placement des paramètres, ordre d'évaluation et formes abrégées. |
| value_types | Identifiants des types VM acceptés dans les contrats. |
| literal_types | Tags $type disponibles et définition de leurs composantes. |
| nodes | Liste triée des définitions disponibles. |

Chaque entrée nodes est une VmNodeDefinition :

| Champ | Contenu |
|---|---|
| operation | Identifiant canonique, par exemple multiply ou project:deal_damage. |
| title, category, description | Texte affiché et regroupement pour l'éditeur. |
| version | Version du contrat propre à ce nœud. |
| aliases | Autres noms reconnus par le compilateur. |
| inputs | Expressions enfants, avec nom, description, accepted_types, required, variadic, min_items, null_as_empty, has_default et default_value. |
| parameters | Valeurs littérales de compilation : nom, description, accepted_types, required, min_length, enum_values, has_default et default_value. |
| output | Types possibles, description et règle d'inférence. |
| examples | Exemples JSON de la forme attendue. |
| evaluation | eager, literal, variable, conditional, short_circuit, loop, sequence ou custom. |
| deprecated | Indication destinée à l'éditeur. |
| origin | builtin ou project. |

Les types sont les identifiants en minuscules de la VM, ou any. Une entrée
variadic représente un tableau d'expressions, ordonné comme dans le JSON.
L'ordre des entrées scalaires est celui de la définition du nœud.

Dans le JSON d'un script, les paramètres littéraux sont normalement placés
directement sur l'opération ; les expressions sont placées dans inputs.
Le compilateur accepte aussi les entrées directement et les paramètres sous
inputs. Il rejette un même champ fourni aux deux endroits.

Un tableau JSON littéral est une constante. Pour évaluer ses éléments comme
des expressions, utiliser le nœud array ou une entrée variadic.

has_default distingue une absence de défaut d'un défaut JSON explicitement null.
Une entrée scalaire facultative omise produit Null ; une entrée variadique
facultative omise produit une liste vide. Fournir un défaut compatible avec les
types acceptés si la valeur Null ne convient pas.

Les types des callbacks et compilateurs de projet sont vérifiés à l'exécution.
Les opérateurs intégrés conservent les règles de promotion de la VM ; certaines
combinaisons de types restent des erreurs même lorsque chaque type isolé est
listé comme entrée possible.

array_contains accepte une valeur simple ou un tableau de valeurs à rechercher.
Son paramètre littéral mode vaut "any" par défaut, ou "all" pour exiger toutes les valeurs.

Pour régénérer le fichier des nœuds intégrés :

~~~text
dotnet run --project tests/Resolver.Tests.csproj -- --export-node-catalog docs/node_catalog.json
~~~

Le jeu peut sauvegarder GetNodeCatalogJson() après ses enregistrements pour
produire un catalogue contenant ses nœuds spécifiques. Le transport HTTP vers
le futur éditeur peut utiliser directement cette chaîne JSON.

Un handle compilé expose aussi GetNodeCatalogJson() : il conserve le catalogue
exact de sa compilation, même après une mise à jour du registre. Ce snapshot
correspond aux champs registry_version/registry_hash de son explication.

## Ajouter un nœud en C#

Un nœud courant est une classe implémentant IVmJsonNode. La même définition
décrit ses entrées et produit la documentation exportée :

~~~csharp
using System.Text.Json;
using VMRuntime;

public sealed class AddPercentNode : IVmJsonNode
{
    public VmNodeDefinition Definition => new()
    {
        Operation = "add_percent",
        Title = "Ajouter un pourcentage",
        Category = "project/math",
        Description = "Ajoute un pourcentage à une valeur.",
        Aliases = ["boost"],
        Inputs =
        [
            new() { Name = "value", Description = "Valeur initiale.",
                    AcceptedTypes = ["int", "float"] },
            new() { Name = "percent", Description = "Pourcentage à ajouter.",
                    AcceptedTypes = ["int", "float"] }
        ],
        Output = new() { Types = ["float"], Description = "Valeur ajustée." },
        Examples =
        [
            JsonSerializer.SerializeToElement(new
            {
                operation = "add_percent",
                inputs = new { value = 100, percent = 25 }
            })
        ]
    };

    public DynValue Execute(VmNodeArguments args)
    {
        static float Number(DynValue value) =>
            value.Type == VMRuntime.ValueType.Int
                ? value.AsInt()
                : value.AsFloat();

        return DynValue.FromFloat(
            Number(args.GetInput("value")) *
            (1f + Number(args.GetInput("percent")) / 100f)
        );
    }
}
~~~

Enregistrer l'instance avant de compiler les scripts :

~~~csharp
engine.RegisterNode(new AddPercentNode());
~~~

Le nœud devient compilable immédiatement et apparaît dans GetNodeCatalogJson().
Un callback direct est aussi possible avec RegisterNode(definition, implementation).

Dans un contrôleur C# utilisant VmEngineNode, CoreEngine donne accès à ce même
moteur pour enregistrer les classes C#. Cette propriété est réservée à C#.

VmNodeArguments expose GetInput(name), GetInputs(name) pour les entrées
variadiques, et GetParameter(name) pour les paramètres littéraux. Les paramètres
de type collection sont copiés par invocation ; les entrées conservent les
références fournies au moteur.

## Ajouter un nœud depuis GDScript

Les définitions sont aussi des classes Godot éditables :

~~~gdscript
var definition = VmNodeDefinitionObject.new()
definition.Operation = "add_percent"
definition.Title = "Ajouter un pourcentage"
definition.Category = "project/math"
definition.Description = "Ajoute un pourcentage à une valeur."
definition.Output.Types = ["float"]
definition.Output.Description = "Valeur ajustée."
definition.ExamplesJson = '[{"operation":"add_percent","value":100,"percent":25}]'

for input_name in ["value", "percent"]:
    var input = VmNodeInputDefinitionObject.new()
    input.Name = input_name
    input.Description = "Entrée numérique : " + input_name
    input.AcceptedTypes = ["int", "float"]
    definition.Inputs.append(input)

var registration = engine.RegisterNode(
    definition,
    func(inputs: Dictionary, parameters: Dictionary):
        return float(inputs["value"]) * (1.0 + float(inputs["percent"]) / 100.0)
)

if not registration.Success:
    print(registration.Error.ToJson())
~~~

Le callable reçoit deux dictionnaires natifs : inputs et parameters.
Une entrée variadique est accessible comme un tableau sous son nom déclaré.
Les paramètres se déclarent avec VmNodeParameterDefinitionObject ; les défauts
utilisent DefaultValueJson, par exemple '"none"', '0' ou 'null'. ExamplesJson
contient un tableau d'exemples JSON.

Les valeurs natives sont converties selon les règles de DynValueConverter ;
les objets Godot restent des références, les tableaux/dictionnaires sont convertis.

La définition est copiée lors de l'enregistrement. Le nœud conserve son nom
canonique dans l'explication, même si le script utilise un alias.

## Ajouter un nœud de contrôle de flux en C#

Un callback ordinaire est eager : toutes ses entrées sont évaluées avant son
appel. Pour un nœud à branches ou une autre stratégie d'évaluation, implémenter
IVmJsonNodeCompiler et appeler RegisterCompilerNode(instance).

La définition décrit les entrées, les paramètres et le champ Evaluation.
La méthode Compile reçoit un VmNodeCompilationContext :

- CompileInput(name) compile une entrée et retourne le nombre de valeurs empilées.
- CompileAllInputs() compile toutes les entrées en ordre de définition.
- PushConstant(value) ajoute et charge une constante.
- AddConstant(value) retourne un index de constante pour les opérandes immédiats.
- Emit(opcode, a, b, c) émet une instruction associée au nœud JSON courant.
- Position et PatchJump permettent de construire des branches.
- Fail(message, field) retourne un diagnostic de compilation localisé.

Les sauts conditionnels immédiats utilisent b: 1, comme les opcodes existants.
Le cadre du nœud et la correspondance source sont ajoutés automatiquement.
Chaque chemin doit laisser exactement une valeur de sortie sur la pile.
Les types déclarés des entrées compilées et de la sortie sont vérifiés.

[L'exemple WhenNode et ses tests](./tests/Program.cs) montrent une branche qui
n'évalue que value ou fallback. Les instructions de cadre et de liaison des
callbacks sont réservées au compilateur. Le contexte est fermé à la fin de Compile.

## Registre, cache et explication

Les identifiants intégrés sont réservés. Une collision de nom ou d'alias est
une erreur d'enregistrement. Un nœud du projet peut être remplacé explicitement
avec replace: true en C#, ou le troisième argument true depuis GDScript.

Chaque modification du registre incrémente registry_version. Une nouvelle
compilation tient compte de cette révision, même lorsque le JSON est identique.
Les handles déjà compilés gardent leur implémentation liée et leur définition.

registry_hash décrit les contrats exportés ; registry_version distingue aussi
le remplacement d'un callback ayant une définition inchangée. La version du
registre et son empreinte sont conservées dans le programme et l'explication.

Les entrées du callback sont évaluées une seule fois. La trace contient le
nœud du projet, ses entrées imbriquées, ses paramètres et son résultat.
Ses erreurs utilisent la hiérarchie existante, avec l'instruction InvokeNode,
les entrées bytecode et la position source. Les valeurs d'entrée d'un callback
sont capturées avant ses éventuelles mutations.

## JSON accepté

Une expression peut être une primitive JSON, un tableau constant, une constante
explicite, une variable ou une opération. Les opérations acceptent leurs entrées
directement ou sous un champ inputs, mais pas aux deux endroits simultanément.

~~~json
{
  "operation": "multiply",
  "inputs": {
    "left": { "operation": "read_variable", "name": "population" },
    "right": { "variable": "tax_rate" }
  }
}
~~~

Les formes type: "Const", type: "Add", type: "Mul", etc. restent des alias pour
les opérations intégrées. Une constante dictionnaire s'écrit explicitement :

~~~json
{ "operation": "constant", "value": { "country": "FRA", "active": true } }
~~~

| Opérations | Entrées |
|---|---|
| add, subtract, multiply, divide, modulo | left, right |
| equal, not_equal, greater_than, less_than, greater_or_equal, less_or_equal | left, right |
| min, max | left, right |
| not, negate | value |
| clamp, in_range | value, min, max |
| if | condition, then, else (facultatif, null par défaut) |
| and, or | left, right ; évaluation avec court-circuit |
| while | condition, body ; retourne la dernière valeur du corps ou null |
| sequence | steps : tableau d'expressions, retourne la dernière |
| array | items : tableau d'expressions |
| array_get, array_remove_at | array, index |
| array_set | array, index, value |
| array_push, array_contains | array, value |
| array_length | array |
| array_concat | left, right |
| call | function : nom fixe ; args : expressions en ordre source |
| object_get | target, property |
| object_set | target, property, value |
| object_is_valid | target |
| object_has_property | target, property |
| object_has_method | target, method |
| object_get_property_list, object_get_method_list | target |
| object_call | target ; method : nom fixe ; args : expressions |

Les champs function, method pour object_call, name et value pour constant sont
des paramètres fixes, disponibles aussi dans les métadonnées de compilation.

Les shims acceptent des littéraux explicitement typés :

~~~json
{ "$type": "vector2", "x": 10, "y": 20 }
~~~

Types : vector2, vector2i, vector3, vector3i, color. Les couleurs utilisent r/g/b/a ;
a vaut 1 s'il est absent. Les composantes ne sont pas bornées à [0, 1].

Les nombres entiers doivent entrer dans Int32. Les littéraux avec point ou
exposant deviennent Float32 finis ; les dépassements sont des erreurs de
compilation. La division entière par zéro échoue ; les opérations flottantes
suivent les règles IEEE 754. AsInt/AsFloat/etc. sont stricts ; les opérateurs
utilisent la table de conversion existante.

## Cache et changement du JSON

CompileJson et LoadAndCompile réutilisent le même handle si le SHA-256 des octets
UTF-8, le nom source et l'identifiant sont inchangés. Un nouvel appel de chargement
après modification recompile la source. Il n'y a pas de surveillance de fichiers
en arrière-plan : le jeu choisit le moment du rechargement des mods.

CompileFragment ne calcule son empreinte que sur le fragment sélectionné :
changer name, price ou un autre événement de l'item ne le recompile pas.
Le cache distingue les chemins et identifiants de fragments. Il tient aussi
compte d'une MetadataHash : une modification de provenance ou de Tags produit
un nouveau handle avec les bons diagnostics, même si le calcul est identique.
L'ordre d'insertion des entrées SourceMap et Tags n'invalide pas le cache.

Un ancien handle reste valide ; le jeu remplace explicitement son handle par le
nouveau résultat de compilation réussi. Execute ne fait aucune vérification de
fichier. ClearCache n'invalide pas les handles déjà conservés.

## Erreurs typées

[VmError](./src/central/diagnostics/VmErrors.cs) est la racine commune :

- VmCompilationError : syntaxe, champ manquant, type JSON, nombre hors plage,
  opération inconnue, fichier inaccessible.
- VmExecutionError : contexte JSON et bytecode toujours disponibles.
- Spécialisations : VmJsonSyntaxError, VmMissingFieldError,
  VmVariableNotFoundError, VmTypeMismatchError, VmDivisionByZeroError, VmHostCallError.

Les exceptions de cast internes contiennent l'accesseur, les types attendu/réel
et un aperçu borné. La frontière d'exécution les convertit en diagnostics.
Les erreurs de script sont retournées ; les contrats C# invalides, tels qu'un
handle null ou une option négative, restent des erreurs d'argument. La façade
Godot les retourne sous forme INVALID_ARGUMENT.

Même sans Explain :

~~~json
{
  "kind": "execution",
  "code": "DIVISION_BY_ZERO",
  "message": "An integer divisor was zero.",
  "source": {
    "file": "mods/tax.json",
    "path": "",
    "line": 1,
    "byte_column": 1
  },
  "json_instruction": {
    "operation": "divide",
    "inputs": {
      "left": { "type": "int", "value": 1000, "truncated": false },
      "right": { "type": "int", "value": 0, "truncated": false }
    }
  },
  "bytecode_instruction": {
    "instruction_index": 7,
    "opcode": "Div",
    "operands": { "a": 0, "b": 0, "c": 0 },
    "inputs": {
      "left": { "type": "int", "value": 1000, "truncated": false },
      "right": { "type": "int", "value": 0, "truncated": false }
    }
  }
}
~~~

L'index d'instruction commence à 0 ; les positions connues commencent à 1.
Après fusion, ligne et colonne valent 0 si elles ne sont pas fournies ; HasPosition est false.
La colonne est en octets UTF-8. path est un JSON Pointer RFC 6901 :
la racine est "", les caractères ~ et / dans une clé sont échappés.

Une entrée absente/non évaluée porte type: "unavailable", distinct de Null.
Une clé JSON absente est signalée à l'objet parent et son nom figure dans l'erreur.

Les entrées bytecode sont capturées avant le dépilement. Une instruction JSON
peut correspondre à plusieurs instructions bytecode et avoir d'autres entrées.

Les erreurs de conversion à la frontière Godot ont instruction_index: -1 et
opcode: "Unavailable", puisqu'aucune instruction bytecode n'a échoué à cet endroit.
Un retour non convertible, notamment une collection cyclique, devient
RESULT_CONVERSION et reste visible à la racine de l'explication.

## Explication et futur visualiseur web

L'explication contient format_version, script, source_file, source_hash, success,
truncated et tree. Chaque nœud contient son path dans le document fusionné, son operation,
ses inputs imbriqués, son output typé ou null, et son error éventuelle.

- Seules les expressions réellement visitées apparaissent.
- if conserve la condition et la branche choisie, avec selected_branch.
- and/or omettent l'entrée court-circuitée.
- Une erreur est attachée au nœud fautif et aussi au résultat global.
- Les parents interrompus ont status: "aborted" et aucune sortie calculée.
- Une entrée répétée dans une boucle devient un tableau de nœuds, en ordre
  d'exécution. node_id identifie le nœud compilé ; execution_id identifie le passage.
- Les paramètres fixes sont directement sérialisés, par exemple inputs.name.
  Les valeurs dynamiques sont représentées par les nœuds qui les ont produites.

Côté classes, Inputs contient VmTraceInput : VmTraceLiteral, VmTraceChild ou
VmTraceOccurrences. Les convertisseurs JSON retirent ces enveloppes afin de
produire la forme imbriquée attendue. La façade expose Literal, Node ou Occurrences.

MaxTraceSteps limite l'historique enregistré, sans arrêter le calcul.
Le chemin actif vers une erreur est préservé même après cette limite.
MaxInstructions arrête réellement une exécution trop longue.

MaxAllocatedValues borne les unités d'allocation produites par les opérations intégrées
(copies de collections, ajouts, concaténations et arguments). MaxCollectionItems et
MaxStringLength limitent les valeurs sur les chemins contrôlés par la VM :
constantes copiées et opérations intégrées. Les variables injectées et les retours
de callbacks ne sont pas systématiquement validés en taille : une valeur déjà
trop grande peut traverser une expression de lecture. L’hôte reste responsable
de la taille de ses entrées et sorties. Les valeurs par défaut sont
respectivement 1 000 000 unités, 16 384 éléments et 65 536 caractères. Les erreurs
ALLOCATION_LIMIT, COLLECTION_LIMIT et STRING_LIMIT conservent le contexte source.
Les callbacks natifs restent du code de confiance : ces budgets ne constituent pas
une limite de mémoire pour leurs allocations internes.

Les valeurs sont des instantanés détachés : profondeur maximale 8, 128 éléments
par collection, 4096 valeurs par instantané, 4096 caractères par chaîne. Leur
truncated indique une omission. Les objets hôtes sont décrits par leur type CLR,
sans appeler leur ToString ni parcourir leurs propriétés.

L'explication expose aussi root_path, is_fragment et metadata_hash.
Le site peut associer les nœuds à l'item fusionné via path, et afficher leur
provenance originale via source. source_hash porte sur le texte du fragment
compilé, pas sur name/price ni sur tout le document fusionné. GetSourceJson
fournit le snapshot exact à associer à cette empreinte ; reformatter ou
resérialiser autrement ce snapshot peut changer son empreinte.
Une absence dans une trace tronquée ne prouve pas que l'expression n'a pas été exécutée.

## Export du programme et correspondance source

CompiledScript.OriginalJson / GetSourceJson restituent le texte compilé :
le texte original pour une entrée textuelle, ou un snapshot du seul fragment
pour une entrée déjà parsée.
ExportProgramJson exporte les instructions avec leur node_id, les constantes
et les métadonnées des nœuds : source, operation, paramètres, parent_node_id,
parent_input et noms des entrées. RootPath, IsFragment, Metadata et MetadataHash
conservent le contexte de sélection et de fusion. RegistryVersion et RegistryHash
identifient le catalogue utilisé ; les liaisons de callbacks sont exportées comme
métadonnées, sans sérialiser leurs délégués.

Cet export est destiné à l'inspection ; il n'est pas un format de chargement
de bytecode depuis une source externe. Les constantes exportées utilisent les
mêmes instantanés bornés que les diagnostics. Le texte original est conservé
pour permettre une recompilation sans perte.

## Modèle de confiance

Les appels `object_call` sont filtrés par ExposedMethodRegistry. Ce registre est
global au processus : autorisations et exclusions affectent toutes les instances.
Les exclusions sont prioritaires dans la hiérarchie des types.

Les propriétés suivent un autre contrat : `object_get` et `object_set` acceptent
les propriétés présentes dans GetPropertyList sans liste d’autorisation dédiée.
Une propriété peut retourner une référence utilisable comme nouvelle cible.
`object_free` détruit directement la cible valide sans consulter le registre des
méthodes. Injecter un objet du jeu accorde donc ces capacités au script.
Pour des mods exigeant moins de pouvoirs, fournir des objets façades ou un
IVmHostObjects restreint. Le registre des méthodes seul ne constitue pas un
confinement complet.

Les callbacks et compilateurs natifs sont du code de confiance. MaxInstructions
compte les instructions VM et ne peut pas interrompre un callback bloquant.
Les accès Godot doivent respecter les threads autorisés par le moteur.
La compilation limite le JSON à 16 MiB, une profondeur de 128 et 100 000 opérations.
Ces plafonds sont distincts des budgets d’exécution.

## Objets Godot et fonctions C#/Lua

IVmHostObjects est l'unique accès aux objets du jeu dans le cœur.
GodotHostObjects vérifie la validité des objets et utilise ExposedMethodRegistry
pour les appels de méthodes. Les noms des méthodes natives Godot sont en
snake_case, par exemple get_name. Les méthodes C# personnalisées conservent
leur nom réellement exposé. Les exclusions du registre sont prioritaires.

Les conversions explicites ToScript()/ToGodot() conservent une seule DLL Core
identique dans les tests et le jeu. Des opérateurs implicites entre les types
Godot et les shims ne peuvent pas être ajoutés par un troisième assembly.
Godot Int64 vers VM Int32 est vérifié ; StringName devient String.

RegisterFunction expose explicitement un callback C# :

~~~csharp
engine.RegisterFunction("double", args =>
    DynValue.FromInt(args[0].AsInt() * 2));
~~~

Depuis GDScript, RegisterFunction accepte un Callable. Un appel de fonction est
effectué une seule fois ; Explain n'entraîne aucune réexécution. La trace montre
les arguments et le résultat de l'appel. Les calculs internes C#/Lua nécessitent
une instrumentation propre à ce langage.

Aucun interpréteur Lua n'est embarqué : un hôte peut raccorder sa bibliothèque
Lua via RegisterFunction. Les accès Godot doivent être effectués sur un thread
autorisé par Godot.

## Vérification

Depuis ce dossier avec un SDK .NET 10 (sélectionné par global.json) :

~~~text
dotnet run --project tests/Resolver.Tests.csproj
dotnet build Resolver.csproj
~~~

Depuis ce dépôt, après restauration et compilation de `tests/godot-project/Resolver.Godot.Smoke.csproj` comme indiqué plus haut :

~~~text
godot --headless --editor --path tests/godot-project --import
godot --headless --path tests/godot-project --script res://addons/resolver/tests/godot/smoke.gd
~~~

Le test GDScript désactive les logs automatiques et vérifie volontairement
plusieurs erreurs. Il se termine avec un code différent de zéro en cas d'échec.

La primitive d'assemblage historique reste disponible. RuleNodeProcessor.ProcessJson
préserve les compilateurs RuleNode explicitement enregistrés lorsque la racine
utilise leur champ type. Pour les expressions intégrées, il passe par le nouveau
compilateur et relocalise le programme ajouté. Les compilateurs personnalisés
historiques gardent leurs propres règles d'émission et leurs diagnostics existants.
Ils ne sont pas automatiquement intégrés à VmEngine ni à sa trace détaillée ;
les extensions de cette nouvelle API utilisent RegisterFunction, RegisterNode
ou RegisterCompilerNode selon leur rôle.
