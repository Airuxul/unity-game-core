# Documentation Governance

**Last Updated:** 2026-06-02 · **Scope:** this package repository

## Layout

| Track | Paths |
|-------|--------|
| User | `README.md` (English), `README.zh-CN.md` (Chinese) |
| Agent | `docs/*.md` (English) |

## Bilingual README rule

- User-visible install paths, dependency versions, and capability lists must stay aligned across both READMEs.
- `README.zh-CN.md` may summarize long API tables and code samples; link to [README.md](../README.md) for full examples.
- Agent-only detail belongs in `docs/` (English), not duplicated into both READMEs.

## Update workflow

1. In the **meta repository** ([AirUnityPackage](https://github.com/Airuxul/AirUnityPackage)), run skill `doc-read-index` (read-only inventory).
2. Apply changes with meta skill `doc-generate-update` (skills live only at meta `.cursor/skills/`, not in this package).
3. Keep `README.md` and `README.zh-CN.md` in sync for user-visible changes.
4. Append non-trivial agent edits to `docs/CHANGELOG_AGENT.md`.

## Cross-repo rules

Layering, dependency direction, and C# folder templates are defined in the meta repo:

| Topic | Document |
|-------|----------|
| Layers L0–L2, what belongs in this package | [ARCHITECTURE](https://github.com/Airuxul/AirUnityPackage/blob/main/.cursor/rules/PACKAGE_ARCHITECTURE.md) |
| Hard constraints (no UI/CLI here) | [CONSTRAINTS](https://github.com/Airuxul/AirUnityPackage/blob/main/.cursor/rules/PACKAGE_CONSTRAINTS.md) |
| `Runtime/` / `Editor/` layout for `com.air.unity-game-core` | [C_SHARP_STANDARDS](https://github.com/Airuxul/AirUnityPackage/blob/main/.cursor/rules/C_SHARP_STANDARDS.md) |

Pre-commit doc validation (`tools/validate-docs.ps1`) runs on the **meta** clone when both READMEs and `docs/` are committed together.
