# PROJECT_STATUS.md

> This file is the handoff checkpoint between ChatGPT, Antigravity, Codex, and human development sessions.

Last update:
- Initial project planning stage

---

# Current Project State

## Status

**NOT STARTED — Unity project setup pending**

No gameplay implementation should be assumed complete until confirmed in the repository.

---

# Current Phase

## PHASE 0 — Project Foundation

Current milestone:

**Milestone 0.1 — Initialize the Unity project and development workflow**

---

# Completed

- Core game concept defined.
- Three character concepts defined:
  - Warrior
  - Archer
  - Gunner
- Initial dungeon/wave progression defined.
- Temporary XP upgrade concept defined.
- Permanent skill tree concept defined.
- AI handoff workflow defined.
- AGENTS.md created.
- GAME_DESIGN.md created.
- ARCHITECTURE.md created.
- ROADMAP.md created.
- PROJECT_STATUS.md created.

---

# Next Task

Once the Unity project exists:

1. Inspect Unity project version and structure.
2. Confirm Unity opens without Console errors.
3. Initialize/verify Git repository.
4. Add proper Unity `.gitignore`.
5. Configure the Unity Input System if not already configured.
6. Create the first prototype dungeon scene.
7. Begin **Milestone 1.1 — Player Movement**.

Do NOT start:
- Archer
- Gunner
- Character selection
- World map
- Permanent skill trees
- Bosses

before the first Warrior vertical slice is working.

---

# First Implementation Task

Use this prompt with Antigravity or Codex:

```text
Read AGENTS.md, PROJECT_STATUS.md, ARCHITECTURE.md, GAME_DESIGN.md and ROADMAP.md.

Inspect the current Unity project and git history before changing anything.

Implement Milestone 1.1 only:

- Create the basic player prefab/framework.
- Use CharacterController.
- Add WASD movement.
- Movement speed must be configurable.
- Keep movement logic separate from health, aiming and combat.
- Do not implement enemies, weapons, XP, waves or UI yet.

After implementation:
- verify Unity compilation
- check Console errors
- test player movement
- update PROJECT_STATUS.md
- summarize changed files
```

---

# Current Architectural Decisions

- Engine: Unity 6 LTS
- Language: C#
- Game type: Top-down 3D action roguelite / dungeon crawler
- Initial playable character: Warrior
- Movement: CharacterController
- Input: Unity Input System preferred
- Data configuration: ScriptableObjects where appropriate
- Development visuals: primitives/placeholders
- Git used as implementation history and checkpoint system
- AI context stored in repository documentation

---

# Known Issues

None yet.

Project implementation has not started.

---

# Testing Status

Not yet applicable.

No gameplay code has been created.

---

# Recent Git Checkpoint

No game-development commit has been recorded in this document yet.

Once Git is initialized, record the latest relevant commit here:

```text
Latest verified commit:
<commit hash> <commit message>
```

---

# Handoff Notes

When switching from Antigravity to Codex or from Codex to Antigravity:

1. Stop the first agent.
2. Save all file changes.
3. Check `git status`.
4. Commit stable work when appropriate.
5. Update this file.
6. Start the next agent.
7. Tell it to inspect Git history and all project documentation.
8. Never let both agents modify the same working tree simultaneously.

---

# Session End Checklist

Before an AI coding session ends, update:

## Completed This Session

```text
- ...
```

## Changed Files

```text
- ...
```

## Tested

```text
- ...
```

## Known Issues

```text
- ...
```

## Next Task

```text
- ...
```

## Latest Verified Commit

```text
<hash> <message>
```

---

# Rule

`PROJECT_STATUS.md` must always describe the real repository state.

Do not mark a feature complete merely because code was generated.

A feature is complete only when its implementation exists and has been verified as far as the available tools allow.
