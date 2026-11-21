"""Install an autonomous repository workflow, without third-party Python packages."""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import stat
import subprocess
import sys
from string import Template

VERSION = "1.2.0"
HOME = Path(__file__).resolve().parent
MANIFEST = ".repo-kit/manifest.json"
CONFIG = ".repo-kit/project.json"
MANAGED = {
    "AGENTS.md": "AGENTS.md",
    "doc/ai/README.md": "AI.md",
    "doc/developpement/ORGANISATION.md": "ORGANISATION.md",
    "doc/developpement/COMMITS.md": "COMMITS.md",
    "doc/developpement/ROADMAP.md": "ROADMAP.md",
    "doc/developpement/ASSISTANTS_DOCKER.md": "ASSISTANTS_DOCKER.md",
    "doc/developpement/modeles/TRAVAIL.md": "TRAVAIL.md",
    "doc/developpement/modeles/DECISION.md": "DECISION.md",
}
LOCAL = {
    ".graphifyignore": "GRAPHIFYIGNORE",
    ".vscode/settings.json": "VSCODE.json",
    "README.md": "README.md",
    "doc/developpement/README.md": "DEVELOPPEMENT.md",
    "doc/developpement/PROJET.md": "PROJET.md",
    "doc/developpement/TODO.md": "TODO.md",
    "doc/developpement/DEPENDANCES.md": "DEPENDANCES.md",
    "doc/developpement/decisions/README.md": "DECISIONS.md",
}
PAYLOADS = (
    "tools/assistant-stack/stack.py",
    "tools/assistant-stack/compose.yaml",
    "tools/assistant-stack/compose.gpu.yaml",
    "tools/assistant-stack/Dockerfile",
    "tools/assistant-stack/Dockerfile.dockerignore",
    "tools/knowledge/knowledge.py",
    "tools/knowledge/requirements.txt",
)


class KitError(ValueError):
    pass


def read_json(path):
    try:
        return json.loads(path.read_text(encoding="utf-8-sig"))
    except (OSError, ValueError) as exc:
        raise KitError(f"JSON illisible : {path}: {exc}") from exc


def encoded(value):
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8")


def digest(data):
    # Git may convert line endings on Windows. Only textual files are managed.
    return hashlib.sha256(data.replace(b"\r\n", b"\n")).hexdigest()


def linked(path):
    try:
        return path.is_symlink() or bool(getattr(path.lstat(), "st_file_attributes", 0) & getattr(stat, "FILE_ATTRIBUTE_REPARSE_POINT", 0))
    except FileNotFoundError:
        return False


def safe_path(root, name):
    if not isinstance(name, str) or not name or "\\" in name:
        raise KitError(f"Chemin relatif attendu avec / : {name}")
    path = Path(name)
    if path.is_absolute() or ".." in path.parts or ":" in name:
        raise KitError(f"Chemin hors dépôt refusé : {name}")
    candidate = root / path
    # Reject links/junctions even when they point inside the repository.
    for part in [candidate, *candidate.parents]:
        if part == root:
            break
        if linked(part):
            raise KitError(f"Lien de système de fichiers refusé : {part}")
        if part.exists() and part != candidate and not part.is_dir():
            raise KitError(f"Un parent n'est pas un dossier : {part}")
    if not candidate.resolve().is_relative_to(root):
        raise KitError(f"Chemin hors dépôt refusé : {name}")
    return candidate


def git(root, *args):
    result = subprocess.run(["git", "-C", str(root), *args], capture_output=True, text=True)
    if result.returncode:
        raise KitError(result.stderr.strip() or "Git indisponible")
    return result.stdout.strip()


def repository(target, init_git=False):
    root = Path(os.path.abspath(target))
    if linked(root) or root.resolve() != root:
        raise KitError("La cible doit être un chemin canonique sans lien.")
    if not root.exists():
        if init_git:
            return root
        raise KitError("Cible absente : cloner le dépôt ou fournir --init-git.")
    if not root.is_dir():
        raise KitError("La cible doit être un dossier.")
    if init_git and not any(root.iterdir()):
        return root
    actual = Path(git(root, "rev-parse", "--show-toplevel")).resolve()
    if actual != root:
        raise KitError("Viser la racine exacte du dépôt, pas un sous-dossier.")
    return root


def project(data):
    fields = {"schema", "id", "name", "purpose", "repository", "boundaries", "entry_points", "prerequisites", "commands", "consumers"}
    if not isinstance(data, dict) or set(data) != fields or data.get("schema") != "repository-project/1":
        raise KitError("Configuration repository-project/1 attendue ; champs inconnus ou absents.")
    for key in ("id", "name", "purpose", "repository"):
        if not isinstance(data[key], str) or "\n" in data[key] or "\r" in data[key]:
            raise KitError(f"Champ texte invalide : {key}")
    if not re.fullmatch(r"[a-z][a-z0-9-]*", data["id"]) or not data["name"] or not data["purpose"]:
        raise KitError("Identifiant, nom et but non vides requis.")
    if data["repository"] and not re.fullmatch(r"https://[^\s<>]+", data["repository"]):
        raise KitError("repository doit être une URL HTTPS ou vide pour un dépôt local.")
    for key in ("boundaries", "entry_points", "prerequisites", "consumers"):
        if not isinstance(data[key], list) or any(not isinstance(item, str) or not item.strip() or "\n" in item for item in data[key]):
            raise KitError(f"Liste de textes attendue : {key}")
    for name in data["entry_points"]:
        safe_path(Path.cwd().resolve(), name)
    if not isinstance(data["commands"], dict):
        raise KitError("Dictionnaire de suites attendu.")
    for suite, commands in data["commands"].items():
        if not re.fullmatch(r"[A-Za-z][A-Za-z0-9-]*", suite) or suite == "Docs" or not isinstance(commands, list) or not commands:
            raise KitError(f"Suite invalide ou réservée : {suite}")
        if any(not isinstance(argv, list) or not argv or any(not isinstance(arg, str) or not arg or "\x00" in arg for arg in argv) for argv in commands):
            raise KitError(f"Commandes attendues comme listes d'arguments : {suite}")
    return data


def rendered(data):
    bullets = lambda items: "\n".join(f"- {item}" for item in items) or "- Aucun élément déclaré."
    variables = {
        "name": data["name"], "id": data["id"], "purpose": data["purpose"],
        "repository": data["repository"] or "Dépôt local, URL non déclarée.",
        "boundaries": bullets(data["boundaries"]),
        "entry_points": bullets([f"[{name}](../../{name})" for name in data["entry_points"]]),
        "prerequisites": bullets(data["prerequisites"]), "consumers": bullets(data["consumers"]),
        "commands": "\n\n".join(f"### {suite}\n\n```text\n" + "\n".join(json.dumps(argv, ensure_ascii=False) for argv in commands) + "\n```" for suite, commands in data["commands"].items()) or "Aucune suite de code configurée. Le contrôle Docs ne valide aucune fonctionnalité.",
    }
    files = {}
    for target, source in {**MANAGED, **LOCAL}.items():
        files[target] = Template((HOME / "templates" / source).read_text(encoding="utf-8")).substitute(variables).encode("utf-8")
    files[".repo-kit/kit.py"] = Path(__file__).read_bytes()
    for path in sorted((HOME / "templates").glob("*")):
        files[f".repo-kit/templates/{path.name}"] = path.read_bytes()
    for name in PAYLOADS:
        # Installed kits carry their own snapshot; development uses canonical sources.
        source = HOME / "payloads" / name
        if not source.is_file():
            source = HOME.parents[1] / name
        contents = source.read_bytes()
        files[name] = contents
        files[f".repo-kit/payloads/{name}"] = contents
    return files


def load_manifest(root):
    path = safe_path(root, MANIFEST)
    if not path.exists():
        return None
    data = read_json(path)
    if not isinstance(data, dict) or data.get("schema") != "repository-kit/1" or not isinstance(data.get("files"), dict):
        raise KitError("Manifeste du kit invalide.")
    for name, value in data["files"].items():
        safe_path(root, name)
        if not isinstance(value, str) or not re.fullmatch(r"[0-9a-f]{64}", value):
            raise KitError("Empreinte de fichier invalide.")
    return data


def install(root, data, apply=False, init_git=False, update=False):
    old = load_manifest(root)
    if update and not old:
        raise KitError("Aucun kit installé ; utiliser init.")
    if old and old.get("project") != data["id"]:
        raise KitError("Le kit installé appartient à un autre projet.")
    desired = rendered(data)
    managed_names = set(MANAGED) | set(PAYLOADS) | {name for name in desired if name.startswith(".repo-kit/")}
    previous = old["files"] if old else {}
    obsolete = set(previous) - managed_names
    if obsolete:
        raise KitError(f"Migration explicite requise pour les anciens fichiers : {sorted(obsolete)}")
    plan, conflicts = [], []
    for name, contents in desired.items():
        path = safe_path(root, name)
        if path.exists() and not path.is_file():
            conflicts.append(name)
            continue
        if name in previous and path.exists() and digest(path.read_bytes()) != previous[name]:
            conflicts.append(name)
        elif path.exists() and digest(path.read_bytes()) == digest(contents):
            plan.append({"path": name, "action": "IDENTIQUE"})
        elif name in previous:
            plan.append({"path": name, "action": "METTRE_A_JOUR"})
        elif path.exists():
            if name in managed_names:
                conflicts.append(name)
            else:
                plan.append({"path": name, "action": "CONSERVER_LOCAL"})
        else:
            plan.append({"path": name, "action": "CREER"})
    config_path = safe_path(root, CONFIG)
    if config_path.exists() and project(read_json(config_path)) != data:
        conflicts.append(CONFIG)
    if conflicts:
        raise KitError("Conflit ; aucune écriture effectuée : " + ", ".join(conflicts))
    for name in data["entry_points"]:
        if not safe_path(root, name).exists():
            raise KitError(f"Point d'entrée annoncé absent : {name}")
    manifest = {"schema": "repository-kit/1", "version": VERSION,
                "project": data["id"], "source": "HexGameGodot/tools/repository-kit",
                "files": {name: digest(desired[name]) for name in sorted(managed_names)}}
    result = {"target": str(root), "version": VERSION, "applied": apply, "files": plan}
    if apply:
        if init_git and not (root / ".git").exists():
            root.mkdir(parents=True, exist_ok=True)
            git(root, "init")
            git(root, "symbolic-ref", "HEAD", "refs/heads/main")
        # All conflicts and path boundaries are checked before the first write.
        for item in plan:
            if item["action"] in {"CREER", "METTRE_A_JOUR"}:
                path = safe_path(root, item["path"])
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_bytes(desired[item["path"]])
        config_path.parent.mkdir(parents=True, exist_ok=True)
        if not config_path.exists():
            config_path.write_bytes(encoded(data))
        safe_path(root, MANIFEST).write_bytes(encoded(manifest))
    return result


def check(root):
    manifest = load_manifest(root)
    if not manifest:
        raise KitError("Kit absent.")
    expected = set(MANAGED) | set(PAYLOADS) | {f".repo-kit/payloads/{name}" for name in PAYLOADS} | {".repo-kit/kit.py"} | {
        f".repo-kit/templates/{path.name}" for path in (HOME / "templates").glob("*")}
    if set(manifest["files"]) != expected or manifest.get("version") != VERSION:
        raise KitError("Inventaire ou version du manifeste incohérent avec le kit installé.")
    data = project(read_json(safe_path(root, CONFIG)))
    if manifest.get("project") != data["id"]:
        raise KitError("Identifiant du manifeste incohérent.")
    problems = []
    for name, expected in manifest["files"].items():
        path = safe_path(root, name)
        if not path.is_file() or digest(path.read_bytes()) != expected:
            problems.append(f"Fichier commun modifié ou absent : {name}")
    for name in set(LOCAL) | {CONFIG} | set(data["entry_points"]):
        if not safe_path(root, name).is_file():
            problems.append(f"Document ou entrée absent : {name}")
    # Check local Markdown destinations, excluding fenced examples and remote URLs.
    for name in {**MANAGED, **LOCAL}:
        path = safe_path(root, name)
        if path.suffix != ".md" or not path.is_file():
            continue
        fence = False
        for number, line in enumerate(path.read_text(encoding="utf-8-sig").splitlines(), 1):
            if re.match(r"^\s*(```|~~~)", line):
                fence = not fence
                continue
            if fence:
                continue
            for link in re.findall(r"\[[^\]]*\]\(([^)]+)\)", line):
                if re.match(r"^[a-zA-Z][a-zA-Z0-9+.-]*:", link) or link.startswith("#"):
                    continue
                destination = (path.parent / link.split("#")[0]).resolve()
                if not destination.is_relative_to(root) or not destination.exists():
                    problems.append(f"Lien absent ou hors dépôt : {name}:{number}: {link}")
    if problems:
        raise KitError("\n".join(problems))
    return {"status": "REUSSIE", "scope": "Structure, fichiers communs et liens locaux du kit uniquement",
            "version": manifest["version"], "project": data["id"], "suites": list(data["commands"])}


def validate(root, suite, list_only=False, timeout=300):
    check(root)
    data = project(read_json(safe_path(root, CONFIG)))
    if suite == "Docs":
        return check(root)
    if suite not in data["commands"]:
        raise KitError(f"Suite absente : {suite}")
    commands = data["commands"][suite]
    if list_only:
        return {"suite": suite, "status": "NON_EXECUTEE", "commands": commands}
    results = []
    failed = False
    for original in commands:
        argv = list(original)
        if failed:
            results.append({"command": original, "status": "NON_EXECUTEE"})
            continue
        if argv[0] == "$POWERSHELL":
            argv[0] = shutil.which("pwsh") or shutil.which("powershell") or "pwsh"
        elif argv[0] == "$GODOT":
            argv[0] = os.environ.get("HEX_GODOT_PATH") or "godot"
        print("Exécution : " + json.dumps(argv, ensure_ascii=False), file=sys.stderr, flush=True)
        try:
            result = subprocess.run(argv, cwd=root, timeout=timeout, check=False)
            failed = result.returncode != 0
            results.append({"command": original, "status": "ECHEC" if failed else "REUSSIE", "exit_code": result.returncode})
        except (OSError, subprocess.TimeoutExpired) as exc:
            failed = True
            results.append({"command": original, "status": "ECHEC", "reason": str(exc)})
    return {"suite": suite, "status": "ECHEC" if failed else "REUSSIE", "steps": results}


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    sub = parser.add_subparsers(dest="action", required=True)
    for action in ("init", "update", "check", "validate"):
        command = sub.add_parser(action)
        command.add_argument("--target", type=Path, required=True)
        if action in {"init", "update"}:
            command.add_argument("--apply", action="store_true", help="Sans ce drapeau : aperçu sans écriture.")
        if action == "init":
            command.add_argument("--config", type=Path, required=True)
            command.add_argument("--init-git", action="store_true", help="Créer seulement un dépôt local vide.")
        if action == "validate":
            command.add_argument("--suite", default="Docs")
            command.add_argument("--list", action="store_true")
            command.add_argument("--timeout", type=int, default=300)
    args = parser.parse_args(argv)
    try:
        root = repository(args.target, getattr(args, "init_git", False))
        if args.action in {"init", "update"}:
            config = args.config if args.action == "init" else safe_path(root, CONFIG)
            result = install(root, project(read_json(config)), args.apply, getattr(args, "init_git", False), args.action == "update")
        elif args.action == "check":
            result = check(root)
        else:
            if args.timeout < 1 or args.timeout > 3600:
                raise KitError("Timeout attendu entre 1 et 3600 secondes.")
            result = validate(root, args.suite, args.list, args.timeout)
        print(json.dumps(result, ensure_ascii=False, indent=2))
        return 1 if result.get("status") == "ECHEC" else 0
    except (KitError, OSError) as exc:
        print(f"Kit : {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
