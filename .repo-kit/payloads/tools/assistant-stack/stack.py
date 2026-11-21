"""Prepare and run the optional local assistant stack. Python 3.11+, no dependencies."""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
LOCAL = ROOT / ".assistant-local"
ALIAS = "repository-local-64k"


def checked(path):
    for part in (path, *path.parents):
        if part == ROOT:
            break
        if part.is_symlink() or part.resolve() != part.absolute():
            raise RuntimeError(f"Chemin avec lien/jonction refuse : {part}")
    if not path.resolve().is_relative_to(ROOT):
        raise RuntimeError(f"Chemin hors depot : {path}")
    return path


def prepare(model="qwen3.5:4b", graph=False):
    if not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9._:/-]*", model):
        raise RuntimeError("Nom de modele Ollama invalide.")
    config = {
        "model": {"provider": "custom", "default": ALIAS,
                  "base_url": "http://ollama:11434/v1", "context_length": 64000},
        "terminal": {"backend": "local", "timeout": 180, "cwd": "/workspace"},
        "agent": {"max_turns": 12, "api_max_retries": 1, "auto_recovery_cycles": 0},
        "compression": {"enabled": True, "threshold": 0.5},
        "file_read_max_chars": 16000,
        "tool_output": {"max_bytes": 12000, "max_lines": 250},
    }
    if graph:
        config["mcp_servers"] = {"repository_graphify": {
            "command": "/opt/graphify/bin/python",
            "args": ["/workspace/tools/knowledge/knowledge.py", "mcp"],
        }}
    files = {
        LOCAL / "hermes/config.yaml": json.dumps(config, ensure_ascii=False, indent=2) + "\n",
        LOCAL / "settings.json": json.dumps({"model": model}, indent=2) + "\n",
        LOCAL / "Modelfile": f"FROM {model}\nPARAMETER num_ctx 64000\n",
        LOCAL / "compose.env": "# Surcharges locales facultatives des variables ASSISTANT_*\n",
        LOCAL / ".gdignore": "",
        ROOT / "graphify-out/.gdignore": "",
        ROOT / "doc/.obsidian/app.json": json.dumps({
            "useMarkdownLinks": True, "newLinkFormat": "relative", "alwaysUpdateLinks": False}, indent=2) + "\n",
        ROOT / "doc/.obsidian/core-plugins.json": json.dumps([
            "file-explorer", "global-search", "switcher", "graph", "backlink", "outgoing-link", "outline", "canvas"], indent=2) + "\n",
    }
    ignore = checked(ROOT / ".gitignore")
    for path in files:
        checked(path)
        if path.exists() and not path.is_file():
            raise RuntimeError(f"Fichier attendu : {path}")
    checked(ROOT / "graphify-out/obsidian")
    if ignore.exists() and not ignore.is_file():
        raise RuntimeError(".gitignore doit etre un fichier.")
    # Validate all destinations before writing. Existing profiles are never replaced.
    for path, contents in files.items():
        if not path.exists():
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(contents, encoding="utf-8")
    (ROOT / "graphify-out/obsidian").mkdir(parents=True, exist_ok=True)
    rules = ["/.assistant-local/", "/.graphify/", "/graphify-out/", "/doc/.obsidian/workspace*.json"]
    previous = ignore.read_text(encoding="utf-8-sig") if ignore.exists() else ""
    missing = [rule for rule in rules if rule not in previous.splitlines()]
    if missing:
        with ignore.open("a", encoding="utf-8") as stream:
            stream.write(("\n" if previous and not previous.endswith("\n") else "")
                         + "\n# Local assistant stack\n" + "\n".join(missing) + "\n")
    print("Setup local prepare ; profils existants conserves. Aucun telechargement ni service lance.")


def compose_args(gpu=False):
    name = "assistant-" + hashlib.sha256(str(ROOT).encode()).hexdigest()[:12]
    result = ["docker", "compose", "--project-directory", str(ROOT), "--project-name", name,
              "--env-file", str(LOCAL / "compose.env"),
              "-f", str(ROOT / "tools/assistant-stack/compose.yaml")]
    if gpu:
        result += ["-f", str(ROOT / "tools/assistant-stack/compose.gpu.yaml")]
    return result


def run(gpu, *args):
    subprocess.run([*compose_args(gpu), *args], cwd=ROOT, check=True)


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--gpu", action="store_true", help="Activer la reservation NVIDIA pour Ollama.")
    sub = parser.add_subparsers(dest="action", required=True)
    setup = sub.add_parser("setup")
    setup.add_argument("--model", default="qwen3.5:4b")
    setup.add_argument("--graph", action="store_true", help="Ajouter le MCP Graphify au nouveau profil Hermes.")
    for action in ("config", "build", "up", "down", "status", "doctor", "pull-model", "graph-build", "graph-status", "hermes"):
        sub.add_parser(action)
    query = sub.add_parser("query")
    query.add_argument("symbol")
    args = parser.parse_args(argv)
    try:
        if args.action == "setup":
            prepare(args.model, args.graph)
            return 0
        if not (LOCAL / "compose.env").is_file():
            raise RuntimeError("Executer setup avant cette commande.")
        if args.action == "config":
            run(args.gpu, "--profile", "tools", "config", "--quiet")
        elif args.action == "build":
            run(args.gpu, "--profile", "tools", "build", "graphify", "hermes")
        elif args.action == "up":
            run(args.gpu, "up", "-d", "ollama", "obsidian")
        elif args.action == "down":
            run(args.gpu, "--profile", "tools", "down")  # No --volumes: retain models and preferences.
        elif args.action == "status":
            run(args.gpu, "--profile", "tools", "ps", "--all")
        elif args.action == "pull-model":
            model = json.loads((LOCAL / "settings.json").read_text(encoding="utf-8"))["model"]
            run(args.gpu, "exec", "-T", "ollama", "ollama", "pull", model)
            run(args.gpu, "exec", "-T", "ollama", "ollama", "create", ALIAS, "-f", "/setup/Modelfile")
        elif args.action in {"graph-build", "graph-status", "query"}:
            action = {"graph-build": "build", "graph-status": "status", "query": "query"}[args.action]
            tail = [args.symbol] if args.action == "query" else []
            run(args.gpu, "run", "--rm", "-T", "--no-deps", "graphify", action, *tail)
        elif args.action == "doctor":
            run(args.gpu, "exec", "-T", "ollama", "ollama", "show", ALIAS)
            run(args.gpu, "run", "--rm", "-T", "hermes", "config", "check")
        elif args.action == "hermes":
            # Interactive use; no automatic prompt, task, trust, or remote fallback.
            run(args.gpu, "run", "--rm", "hermes", "chat", "--provider", "custom",
                "--base-url", "http://ollama:11434/v1", "--model", ALIAS, "--max-turns", "12")
        return 0
    except subprocess.CalledProcessError as exc:
        return exc.returncode
    except (OSError, RuntimeError, ValueError, KeyError) as exc:
        print(f"Assistant stack : {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
