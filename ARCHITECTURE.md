# ARCHITECTURE.md

## Architecture Goal

Create a modular Unity project that can grow from a simple Warrior-vs-Zombie prototype into a multi-character dungeon roguelite.

The architecture should allow:
- Additional playable characters
- Additional weapons
- Additional enemies
- Additional upgrades
- Additional dungeons
- Bosses
- Save/load
- Permanent skill trees

without rewriting core gameplay systems.

---

# Recommended Project Structure

```text
Assets/
│
├── Art/
│
├── Audio/
│
├── Materials/
│
├── Prefabs/
│   ├── Characters/
│   ├── Enemies/
│   ├── Weapons/
│   ├── Pickups/
│   └── Environment/
│
├── Scenes/
│   ├── Bootstrap/
│   ├── MainMenu/
│   ├── CharacterSelect/
│   ├── WorldMap/
│   └── Dungeons/
│
├── Scripts/
│   ├── Core/
│   ├── Player/
│   ├── Combat/
│   ├── Enemies/
│   ├── Weapons/
│   ├── Experience/
│   ├── Upgrades/
│   ├── Skills/
│   ├── Dungeons/
│   ├── Waves/
│   ├── Save/
│   └── UI/
│
└── ScriptableObjects/
    ├── Characters/
    ├── Weapons/
    ├── Enemies/
    ├── Upgrades/
    ├── Skills/
    ├── Waves/
    └── Dungeons/
```

Do not create empty folders for systems that are not yet being implemented unless useful for clarity.

---

# Core Principles

## 1. Composition Over Monolithic Controllers

Player gameplay should be composed from components.

Example:

```text
Player
├── PlayerMovement
├── PlayerAim
├── PlayerHealth
├── PlayerExperience
├── WeaponController
└── PlayerStats
```

Enemy:

```text
Zombie
├── EnemyMovement
├── EnemyAttack
├── EnemyHealth
├── ExperienceReward
├── EnemyVisualFeedback
└── CorpseCleanup
```

---

# Player

## PlayerMovement

Responsibilities:
- Read movement input.
- Move the character.
- Respect movement speed.
- Avoid combat logic.

Initial implementation:
- `CharacterController`
- WASD movement

---

## PlayerAim

Responsibilities:
- Read mouse position.
- Convert cursor position to world direction.
- Rotate player toward aim target.

Avoid placing aiming logic inside `PlayerMovement`.

---

## PlayerHealth

Responsibilities:
- Store current/max health.
- Receive damage.
- Trigger death.

Should support common damage interfaces.

---

## PlayerStats

Possible responsibilities:
- Base damage multiplier
- Attack speed multiplier
- Movement speed multiplier
- Max health modifiers
- Critical stats

Temporary upgrades can modify runtime stats here or through a dedicated modifier system.

Do not over-engineer stat stacking in the first milestone.

---

# Combat

## IDamageable

Conceptual contract:

```csharp
public interface IDamageable
{
    void TakeDamage(float amount);
}
```

Exact signature may evolve.

Purpose:
- Weapons do not depend on specific enemy classes.
- Player and enemies can share common damage flow.

---

## DamageInfo

Future-safe optional structure:

```text
DamageInfo
- Amount
- Source
- Damage Type
- Critical
- Knockback
```

Do not implement before needed.

---

# Weapons

Recommended hierarchy:

```text
Weapon
├── MeleeWeapon
└── ProjectileWeapon
```

Alternative composition is acceptable if cleaner.

## WeaponDefinition

ScriptableObject configuration may contain:

```text
Display Name
Damage
Attack Rate
Range
Prefab
Icon
```

Later:
- Projectile prefab
- Projectile speed
- Critical chance
- Upgrade tags

---

# Enemy Architecture

Avoid one giant Enemy class.

Recommended:

## EnemyMovement
- Target player.
- Move toward target.
- Stop within attack range.

## EnemyAttack
- Attack cooldown.
- Attack range.
- Damage.
- Implements `IEnemyAttack`: `InitializeAttack(float damageMultiplier)` and `SetTarget(Transform target)`.
- `WaveManager` resolves this interface once per spawn for difficulty scaling and explicit target binding. Health and movement retain their existing components; no enemy-type branching is required.

## EnemyVisualFeedback
- Subscribes to `EnemyHealth.OnHealthChanged`; only decreases below maximum health trigger a hit flash, including lethal damage. Initialization and full-health restoration do not flash.
- Uses cached `MaterialPropertyBlock` instances and URP Lit `_BaseColor`, preserving shared materials and restoring pre-flash overrides. No `Renderer.material` or `Renderer.materials` access.
- White flash lasts 0.08 unscaled seconds; repeated hits restart it, and disabling the component restores the previous overrides.

## CorpseCleanup
- Subscribes independently to `EnemyHealth.OnDied` and destroys the dead enemy root, including its visual children, after 1.5 unscaled seconds.
- Does not control health, physics, attacks, XP, wave counts, or completion. Existing death listeners stop movement/attacks/collision and spawn XP immediately.
- Keeps the dead root active until cleanup, allowing hit feedback to finish even during upgrade/result pauses. No pooling or resurrection support is introduced.
- `WaveManager.CurrentWaveSpawned` is a historical list and may contain destroyed Unity references; `ActiveEnemies` remains the authoritative living collection.

## EnemyHealth
- Current health.
- Damage reception.
- Death event.

## ExperienceReward
- XP amount.
- Spawn pickup or directly award XP.

---

# Enemy Configuration

`EnemyDefinition` ScriptableObject:

```text
Display Name
Max Health
Move Speed
Damage
Attack Range
Attack Cooldown
XP Reward
Prefab
```

---

# Experience

## PlayerExperience

Responsibilities:
- Current XP
- Current level
- Required XP
- Level-up event

Should not directly build UI.

---

## ExperiencePickup

Responsibilities:
- Store XP value.
- Detect player collection.
- Award experience.
- Destroy itself.

---

# Upgrades

Initial implementation can use:

`UpgradeDefinition` ScriptableObject

Fields may include:

```text
ID
Display Name
Description
Icon
Upgrade Type
Magnitude
```

First upgrade types:
- Damage multiplier
- Attack speed multiplier
- Movement speed multiplier

The upgrade selection system should request three valid upgrade choices.

---

# Waves

## WaveDefinition

Data describing one wave.

Possible structure:

```text
WaveDefinition
- Enemy Groups
- Spawn Count
- Spawn Interval
```

Future support:
- Multiple enemy types
- Elite chance
- Spawn zones

---

## WaveManager

Responsibilities:
- Start wave.
- Spawn configured enemies.
- Track living enemies.
- Detect wave completion.
- Start next wave.
- Trigger dungeon clear when all waves are complete.

Must NOT contain hard-coded Dungeon 1 values.

---

# Dungeons

## DungeonDefinition

ScriptableObject:

```text
ID
Display Name
Scene
Waves
Boss
Rewards
Required Dungeon
```

The initial vertical slice can use only the wave portion.

---

# UI Architecture

UI listens to gameplay state where possible.

Examples:

```text
PlayerExperience
    ↓ event
XPBarUI

WaveManager
    ↓ event
WaveUI

PlayerHealth
    ↓ event
HealthBarUI
```

Avoid gameplay systems directly searching for UI objects every frame.

---

# Game Flow

Future high-level flow:

```text
Bootstrap
    ↓
Main Menu
    ↓
Character Selection
    ↓
World Map
    ↓
Dungeon Scene
    ↓
Dungeon Result
    ↓
World Map
```

Do not implement a complicated global state machine during the first milestone.

---

# Save System

Later milestone.

Expected data:

```text
Unlocked Dungeons
Character Unlocks
Permanent Skill Progress
Gold / Currency
Settings
```

Temporary run upgrades should NOT be saved as permanent progression.

---

# Scene Rules

Suggested eventual scenes:

```text
Bootstrap
MainMenu
CharacterSelect
WorldMap
Dungeon_Prototype
```

For the first vertical slice, using one dungeon scene is enough.

---

# Dependency Rules

Preferred direction:

```text
UI
↓ listens to
Gameplay Components
↓ use
Data Definitions
```

Avoid circular dependencies.

Example:

Bad:

```text
PlayerMovement -> XPBarUI
XPBarUI -> PlayerMovement
```

Preferred:

```text
PlayerExperience -> event
XPBarUI subscribes
```

---

# Initial Technical Decisions

Until deliberately changed:

- Engine: Unity 6 LTS
- Language: C#
- Perspective: Top-down 3D
- Movement: CharacterController
- Input: Unity Input System preferred
- Data: ScriptableObjects
- Development assets: primitives/placeholders
- Version control: Git
- AI coordination: AGENTS.md + PROJECT_STATUS.md + Git

If an implementation requires changing one of these decisions, document the reason in `PROJECT_STATUS.md`.
