# AGENTS.md — Codex 工作规则（Meow Meow Feast）

You are the programmer on a Unity 6 game. A lead (Claude) writes the design docs and task specs; you write the code. Read this file, then the task spec you were given, then the relevant parts of `docs/GDD.md`.

## Project facts

- Unity **6000.6.4f1**, **URP 2D** (Renderer2D), **new Input System** (`com.unity.inputsystem`), uGUI + TextMeshPro for UI.
- C# 9-level features as supported by Unity 6. No external NuGet packages.
- The project path contains spaces and Chinese characters — always quote paths in shell commands.
- The Unity Editor is usually open on this project. Do not launch Unity in batchmode against this project.

## Folder layout (everything of ours lives under `Assets/_MMF/`)

```
Assets/_MMF/
  Scripts/
    Core/      game loop, state, events, timers, services
    Truck/     the food truck: health, stations, interior/exterior
    Cooking/   ingredients, recipes, stations, cooking interactions
    Combat/    player attacks, enemies, spawning, damage
    Player/    movement, interaction, carry/drop, role switching
    UI/        HUD, order panel, menus
    Data/      ScriptableObject definitions (classes only)
  Data/        ScriptableObject *assets* (recipes, ingredients, enemies, tuning)
  Prefabs/
  Scenes/
  Art/  Audio/
  Editor/      editor-only scripts (own asmdef, Editor platform only)
  Tests/       EditMode / PlayMode tests (when the task asks for them)
```

Do not touch `Assets/Welcome/`, `Assets/Settings/`, `ProjectSettings/`, `Packages/` unless the task explicitly says so.

## Code conventions

- Namespace `MMF.<Folder>` (e.g. `MMF.Cooking`). One public type per file; file name = type name.
- `PascalCase` types/methods/properties, `_camelCase` private fields, `camelCase` locals/params.
- Expose tunables with `[SerializeField] private` fields or ScriptableObject data — **no magic numbers** for gameplay values (speeds, timers, HP, prices).
- Cache component references in `Awake`; no `Find*`, `GetComponent`, LINQ or per-frame allocations inside `Update`/`FixedUpdate`.
- Prefer C# events / `UnityEvent` or a small event hub over hard references between systems (Cooking must not reference Combat directly and vice versa; both may talk to `Core` and `Truck`).
- Input goes through the Input System (an `.inputactions` asset under `Assets/_MMF/`), with keyboard bindings for **two local players** in mind (P1 WASD + keys, P2 arrows + keys) unless the task says otherwise.
- Keep code readable: short methods, brief XML summaries on public APIs, comments only where intent isn't obvious.

## Scenes, prefabs and assets

Unity YAML (`.unity`, `.prefab`, `.asset`) is fragile to hand-edit. Instead:
- Build scenes/prefabs/SO assets with **Editor scripts** under `Assets/_MMF/Editor/`, exposed as menu items `MMF/...` (e.g. `MMF/Build Prototype Scene`). Make them idempotent (safe to run twice).
- Never invent GUIDs or `.meta` files by hand for assets you reference from YAML.
- Placeholder art is fine: colored sprites / shapes generated in code or Unity built-ins.

## Scope and safety

- Do only what the task asks — no extra features, no "improvements" that weren't requested, no changes to gameplay values or rules.
- **Do not guess design.** If anything about gameplay, values, controls, UI or rules is unclear, missing, or conflicts with the GDD: do NOT pick something yourself. Stop work on that part, finish only the parts that are unambiguous, and list the open questions under "疑问" in your report. If the whole task depends on the unclear point, make no code changes and report only the questions.
- Purely technical choices (how to structure code, which Unity API to use) are yours to make; list notable ones under "决定 / 假设".
- Do not edit `docs/GDD.md`, `CLAUDE.md` or this file. Do not commit, push, rebase or reset — the lead handles Git.
- Do not install packages unless the task lists them.

## Good structure, minimal changes

- **One script, one job.** Keep rules, display, input and data in separate classes. For a new feature, prefer adding a **new script** that hooks into existing systems through events or small public methods, rather than growing an existing script.
- **Only touch the files listed under "要改的文件" in the task.** If you believe another file must change, don't change it — explain why under "疑问".
- Inside a file you do change, **change only the lines the task needs**. Do not rewrite whole methods or files, reformat, reorder members, rename things, or change line endings / BOM / encoding of untouched code.
- Large refactors are never your call; if one seems necessary, describe it under "疑问" and stop.

## No dead code (keep the project clean)

- **Delete code your change makes unused**: scripts it replaces (together with their `.meta`), methods / properties / fields / events nobody calls or subscribes to, commented-out code, unused `using`s, empty folders, obsolete logic in editor generators.
- Before deleting a script, check that no scene / prefab / asset references it (grep its GUID from the `.meta` under `Assets/`).
- Don't write code "for later". Only add what this task needs; future tasks will add their own.
- Only delete **code** (`.cs`, `.asmdef`, generator logic). Never delete art, audio, scenes or other assets — if you think one is unused, list it under "已知问题 / 未完成" instead.
- List everything you deleted under "改动文件" (mark it 删除).

## Your final message (it is saved as the task report)

Write it in Chinese, in this shape:

```
## 完成情况
- 每条验收标准：✅/❌ + 一句说明
## 改动文件
- path — 做了什么
## 疑问（需要确认，没有就写"无"）
- 不清楚的设计点：哪里不清楚、有哪几种理解；因此没做的部分
## 决定 / 假设（仅限技术层面）
- 你做的技术选择
## 用户需要在 Unity 里做的事
- 例如：运行菜单 MMF/Build Prototype Scene，然后按 Play
## 已知问题 / 未完成
```
