"""Portable, local Graphify entry point. Python 3.11.5+, no shell expansion."""
from __future__ import annotations

import argparse
from datetime import datetime, timezone
import hashlib
import importlib.metadata
import json
import os
from pathlib import Path
import subprocess
import sys
import venv

ROOT = Path(__file__).resolve().parents[2]
# Containers install their Linux environment in the image, never in a host venv.
ENV = Path(os.environ.get("GRAPHIFY_ENV", str(ROOT / ".graphify" / "venv"))).resolve()
PYTHON = ENV / ("Scripts/python.exe" if os.name == "nt" else "bin/python")
OUT = ROOT / "graphify-out"
GRAPH = OUT / "graph.json"
STATE = OUT / "hexgame-state.json"
VERSION = "0.9.113"
# Include Godot sources and documents even when the AST extractor skips them.
TEXT_SUFFIXES = {
    ".cs", ".gd", ".ps1", ".psm1", ".psd1", ".py", ".sh", ".js", ".ts",
    ".md", ".json", ".yaml", ".yml", ".toml", ".txt", ".csproj", ".props",
    ".targets", ".sln", ".godot", ".tscn", ".tres", ".cfg",
}


def git(*args: str) -> bytes:
    return subprocess.check_output(["git", "-C", str(ROOT), *args])


def snapshot() -> dict:
    """Use bytes, not mtimes: checkout, edits, additions and deletions count."""
    paths = set(git("ls-files", "--cached", "--others", "--exclude-standard", "-z").split(b"\0"))
    digest = hashlib.sha256()
    for raw in sorted(paths):
        if not raw:
            continue
        name = raw.decode("utf-8", errors="surrogateescape")
        path = ROOT / name
        if name.startswith((".graphify/", "graphify-out/", "doc/.obsidian/")):
            continue
        if path.suffix.lower() not in TEXT_SUFFIXES and name not in {".gitignore", ".graphifyignore", ".graphifyrc"}:
            continue
        digest.update(raw + b"\0")
        digest.update(hashlib.sha256(path.read_bytes()).digest() if path.is_file() else b"DELETED")
    return {
        "root": str(ROOT),
        "head": git("rev-parse", "HEAD").decode().strip(),
        "branch": git("rev-parse", "--abbrev-ref", "HEAD").decode().strip(),
        "sources_sha256": digest.hexdigest(),
        "graphify_version": VERSION,
    }


def freshness() -> tuple[bool, str]:
    if not GRAPH.is_file() or not STATE.is_file():
        return False, "ABSENT ou construction incomplete"
    try:
        state = json.loads(STATE.read_text(encoding="utf-8"))
        if state["source"] != snapshot():
            return False, "PERIME : clone, branche, HEAD ou fichiers modifies"
        if state["graph_sha256"] != hashlib.sha256(GRAPH.read_bytes()).hexdigest():
            return False, "PERIME : graphe modifie hors du lanceur"
        return True, f"A_JOUR : {state['built_utc']} ({state['nodes']} noeuds, {state['edges']} relations)"
    except (ValueError, KeyError, OSError):
        return False, "INVALIDE : etat local illisible"


def local_environment() -> dict[str, str]:
    return {
        **os.environ,
        "GRAPHIFY_BACKEND": "none",
        "GRAPHIFY_OUT": str(OUT),
        "GRAPHIFY_QUERY_LOG_DISABLE": "1",
        "GRAPHIFY_NO_TIPS": "1",
        "PYTHONIOENCODING": "utf-8",
        "PYTHONUTF8": "1",
    }


def run_graphify(*args: str) -> None:
    subprocess.run([str(PYTHON), "-m", "graphify", *args], cwd=ROOT,
                   env=local_environment(), check=True)


def setup() -> None:
    if not (3, 11, 5) <= sys.version_info[:3] < (3, 15):
        raise RuntimeError("Utiliser Python >= 3.11.5 et < 3.15 (3.12 conseille).")
    if not PYTHON.is_file():
        venv.EnvBuilder(with_pip=True).create(ENV)
    subprocess.run([str(PYTHON), "-m", "pip", "install", "--disable-pip-version-check",
                    "-r", str(Path(__file__).with_name("requirements.txt"))], check=True, cwd=ROOT)
    print("Graphify installe dans .graphify/venv. Prochaine commande : build.")


def prepare_export() -> None:
    """Remove owned notes before re-export (case-only renames on Windows).

    Preserve foreign notes and Obsidian preferences. Validate every target before
    removing anything; the exporter manifest may never escape its generated vault.
    """
    vault = OUT / "obsidian"
    manifest = vault / ".graphify_obsidian_manifest.json"
    if not manifest.is_file():
        return
    owned = json.loads(manifest.read_text(encoding="utf-8"))["files"]
    targets = []
    for name in owned:
        if not isinstance(name, str):
            raise RuntimeError("Manifest Obsidian invalide.")
        relative = Path(name)
        if relative.is_absolute() or ".." in relative.parts:
            raise RuntimeError("Manifest Obsidian hors du coffre genere.")
        target = vault / relative
        if relative.suffix == ".md":
            if target.is_symlink() or target.resolve().parent != vault.resolve():
                raise RuntimeError("Note generee hors du coffre attendu.")
            targets.append(target)
    for target in targets:
        target.unlink(missing_ok=True)


def build() -> None:
    OUT.mkdir(exist_ok=True)
    (OUT / ".gdignore").touch()  # Godot must not import hundreds of generated notes.
    STATE.unlink(missing_ok=True)  # A failed build must never retain a success stamp.
    before = snapshot()
    # Graphify --force preserves some previous layers. Start from a new graph so
    # excluded/removed sources cannot survive a rebuild; AST caches may be reused.
    GRAPH.unlink(missing_ok=True)
    run_graphify("extract", ".", "--code-only", "--force", "--out", str(OUT))
    graph = json.loads(GRAPH.read_text(encoding="utf-8"))
    if not graph.get("nodes"):
        raise RuntimeError("Graphe vide : examiner les diagnostics Graphify.")
    prepare_export()
    run_graphify("export", "obsidian", "--graph", str(GRAPH), "--dir", str(OUT / "obsidian"))
    if before != snapshot():
        raise RuntimeError("Les sources ont change pendant la construction : relancer build.")
    state = {
        "source": before,
        "built_utc": datetime.now(timezone.utc).isoformat(),
        "graph_sha256": hashlib.sha256(GRAPH.read_bytes()).hexdigest(),
        "nodes": len(graph["nodes"]),
        "edges": len(graph.get("links", graph.get("edges", []))),
        "mode": "local-code-only; documents lus directement dans doc/",
    }
    STATE.write_text(json.dumps(state, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(freshness()[1])


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["setup", "build", "status", "query", "explain", "path", "mcp"])
    parser.add_argument("terms", nargs="*")
    args = parser.parse_args()
    expected = {"query": 1, "explain": 1, "path": 2}.get(args.action, 0)
    if len(args.terms) != expected:
        parser.error(f"{args.action} attend {expected} argument(s) texte.")
    if args.action == "setup":
        setup()
        return 0
    if args.action == "status":
        ok, message = freshness()
        print(message)
        return 0 if ok else 2
    if not PYTHON.is_file():
        raise RuntimeError("Graphify absent : executer setup d'abord.")
    # Always use this clone's environment, independent of PATH or current directory.
    if Path(sys.prefix).resolve() != ENV.resolve():
        return subprocess.call([str(PYTHON), str(Path(__file__).resolve()), *sys.argv[1:]],
                               cwd=ROOT, env=local_environment())
    if importlib.metadata.version("graphifyy") != VERSION:
        raise RuntimeError("Version Graphify inattendue : relancer setup.")
    if args.action == "build":
        build()
        return 0
    ok, message = freshness()
    if not ok:
        print(message + "; executer build, ou utiliser rg et les sources.", file=sys.stderr)
        return 2
    if args.action == "mcp":
        # No informational stdout before the JSON-RPC stdio transport.
        return subprocess.call([str(PYTHON), "-m", "graphify.serve", "--graph", str(GRAPH)],
                               cwd=ROOT, env=local_environment())
    extra = ["--budget", "1600", "--depth", "2"] if args.action == "query" else []
    run_graphify(args.action, *args.terms, "--graph", str(GRAPH), *extra)
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except subprocess.CalledProcessError as exc:
        sys.exit(exc.returncode)
    except (OSError, RuntimeError, ValueError, importlib.metadata.PackageNotFoundError) as exc:
        print(f"Connaissance locale : {exc}", file=sys.stderr)
        sys.exit(1)
