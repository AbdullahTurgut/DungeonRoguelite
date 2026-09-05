# AGENTS.md

## Project Purpose

This repository contains a Unity top-down action roguelite / dungeon crawler.

The game is being developed with AI-assisted coding tools such as:
- Google Antigravity
- OpenAI Codex
- ChatGPT for architecture, planning, review, and handoff

All coding agents must follow the rules in this file before modifying the project.

---

## Core Game Concept

The player selects one of three characters:

1. Warrior
2. Archer
3. Gunner

The player enters dungeons from a world map.

Each dungeon contains:
- Enemy waves
- Experience gain
- Temporary level-up upgrades
- A final boss or clear condition
- Rewards
- Unlocking of later dungeons

Dungeon progression follows:

Dungeon 1 -> Dungeon 2 -> Dungeon 3 -> ...

The game will eventually include:
- Character selection
- Multiple weapons
- Enemy variants
- Bosses
- Temporary run upgrades
- Permanent skill trees
- Save/load
- Dungeon progression
- World map
- Audio
- VFX
- Polished UI

---

## Current Development Philosophy

Do NOT attempt to build the complete game in one pass.

Work milestone by milestone.

The first target is a vertical slice containing:

- One playable Warrior
- WASD movement
- Top-down camera
- Mouse aiming
- Sword attack
- One Zombie enemy
- Enemy AI
- Player health
- Enemy health
- Enemy death
- Three waves
- Experience drops
- XP bar
- Level-up
- Three upgrade choices
- Dungeon complete screen

Do not implement systems from later milestones unless required by the current task.

---

## Mandatory Reading Order

Before starting any implementation task, read:

1. `AGENTS.md`
2. `PROJECT_STATUS.md`
3. `ARCHITECTURE.md`
4. `GAME_DESIGN.md`
5. `ROADMAP.md`

Then inspect:
- Relevant existing scripts
- Unity scenes
- Prefabs
- ScriptableObjects
- Git history

Do not rely only on documentation. Verify the actual repository state.

---

## Coding Rules

### General

- Use clean, modular C#.
- Prefer composition over large monolithic classes.
- Keep classes focused on a single responsibility.
- Avoid unnecessary abstractions.
- Avoid premature optimization.
- Do not rewrite unrelated systems.
- Do not rename files/classes without a clear reason.
- Do not delete working code unless required.
- Preserve compatibility with existing systems when possible.

### Unity

Prefer separate components such as:

- `PlayerMovement`
- `PlayerHealth`
- `PlayerAim`
- `PlayerExperience`
- `WeaponController`
- `EnemyHealth`
- `EnemyMovement`
- `EnemyAttack`
- `WaveManager`

Avoid giant classes such as:

- `PlayerManager` containing every player feature
- `GameManager` containing all game logic

`GameManager` should only exist if it has a clearly defined orchestration responsibility.

---

## Data-Driven Design

Use ScriptableObjects where configuration should be editable without changing code.

Possible data types:

- `CharacterDefinition`
- `WeaponDefinition`
- `EnemyDefinition`
- `SkillDefinition`
- `UpgradeDefinition`
- `WaveDefinition`
- `DungeonDefinition`

Do NOT hard-code character or dungeon-specific values into managers.

Bad:

```csharp
if (dungeon == 1)
{
    SpawnZombie(10);
}
```

Preferred:

```text
DungeonDefinition
    -> WaveDefinition[]
        -> Enemy spawn configuration
```

---

## Character Architecture

Do NOT create separate full gameplay implementations such as:

- `WarriorController`
- `ArcherController`
- `GunnerController`

unless character-specific behavior genuinely requires it.

Shared systems should remain shared.

Character differences should primarily come from configuration:

```text
CharacterDefinition
- Display Name
- Max Health
- Movement Speed
- Starting Weapon
- Character Prefab
- Portrait
- Base Stats
- Available Skills
```

---

## Combat Architecture

Combat should be extensible.

Recommended concepts:

```text
IDamageable
Weapon
MeleeWeapon
ProjectileWeapon
DamageInfo
HealthComponent
```

Damage receivers should not need to know which specific weapon dealt the damage.

---

## Testing Rules

After implementation:

1. Check Unity compilation.
2. Inspect Console errors.
3. Test the changed gameplay path.
4. Verify no unrelated scene or prefab was broken.
5. Report what was tested.
6. Document remaining known issues.

Never claim a feature works unless it was actually verified as far as available tooling allows.

---

## Git Rules

Work in small logical changes.

Recommended commit style:

```text
chore: initialize project architecture
feat: implement top-down player movement
feat: add player health system
feat: implement melee combat
feat: add zombie enemy AI
feat: implement dungeon wave system
feat: add experience and level-up system
fix: correct enemy attack cooldown
refactor: separate player aiming from movement
```

Do not bundle unrelated systems into one commit when avoidable.

---

## AI Handoff Rules

This repository may be modified by multiple coding agents.

Before starting work:
- Read `PROJECT_STATUS.md`.
- Inspect recent Git history.
- Confirm the current unfinished milestone.
- Continue from the existing state.
- Do not redo completed work.

Before ending a work session:
- Update `PROJECT_STATUS.md`.
- Record completed work.
- Record next task.
- Record known issues.
- Record testing performed.
- Record important architectural decisions.
- Make a clean Git commit when appropriate.

Only one coding agent should actively modify the repository at a time.

Do not run Antigravity and Codex simultaneously against the same working tree.

---

## Handoff Prompt

A new coding agent can be started with:

```text
Read AGENTS.md, PROJECT_STATUS.md, ARCHITECTURE.md, GAME_DESIGN.md and ROADMAP.md.

Inspect the current repository and recent git history.

Continue from the next unfinished task in PROJECT_STATUS.md.

Do not redo completed work.
Do not modify unrelated systems.
Follow the architecture and coding rules in AGENTS.md.

After implementation:
- verify compilation
- test the changed gameplay path
- update PROJECT_STATUS.md
- summarize changed files and known issues
```

---

## Priority Rule

If documentation conflicts with the actual implementation:

1. Identify the mismatch.
2. Preserve working code unless change is required.
3. Update documentation to reflect the final accepted architecture.
4. Never silently invent missing behavior.

The repository is the source of truth for what currently exists.
`PROJECT_STATUS.md` is the source of truth for what should happen next.
