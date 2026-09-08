# ROADMAP.md

# Development Roadmap

The game should be developed in small verified milestones.

Do not skip ahead unless a dependency genuinely requires it.

---

# PHASE 0 — Project Foundation

## Milestone 0.1

- Create Unity project
- Initialize Git
- Add `.gitignore`
- Add project documentation
- Confirm project opens without Console errors
- Configure Unity Input System if used

Exit criteria:

```text
Project opens
Git works
Documentation exists
No initial Unity errors
```

---

# PHASE 1 — Player Foundation

## Milestone 1.1 — Movement

Implement:

- Player prefab
- CharacterController
- WASD movement
- Configurable movement speed

Exit criteria:
- Player moves reliably.
- No Console errors.

---

## Milestone 1.2 — Camera and Aim

Implement:

- Top-down camera
- Camera follow
- Mouse world aiming
- Player rotation toward cursor

Exit criteria:
- Camera follows player.
- Player aims correctly.
- Movement and aim can work simultaneously.

---

## Milestone 1.3 — Player Health

Implement:

- Player health component
- Damage reception
- Death event
- Temporary debug health display if required

Exit criteria:
- Player can receive damage.
- Player dies at zero health.

---

# PHASE 2 — Core Combat

## Milestone 2.1 — Damage Architecture

Implement:

- Common damage interface
- Enemy health
- Damage testing

---

## Milestone 2.2 — Sword

Implement:

- Weapon base structure
- Melee attack
- Sword hit detection
- Attack cooldown/rate
- Damage values from configuration

Exit criteria:
- Player can damage a target using sword.

---

# PHASE 3 — First Enemy

## Milestone 3.1 — Zombie

Implement:

- Zombie prefab
- Follow player
- Attack range
- Attack cooldown
- Player damage
- Enemy death

Exit criteria:

```text
Player can kill Zombie
Zombie can damage Player
Both health systems work
```

---

# PHASE 4 — Wave System

## Milestone 4.1

Implement reusable:

- WaveDefinition
- Spawn points
- WaveManager
- Enemy alive tracking
- Wave completion

Prototype dungeon:

```text
Wave 1: 10 Zombies
Wave 2: 15 Zombies
Wave 3: 20 Zombies
```

Exit criteria:
- Next wave starts only after previous enemies are dead.
- Dungeon clear event triggers after Wave 3.

---

# PHASE 5 — Experience

## Milestone 5.1

Implement:

- XP reward
- XP pickup
- PlayerExperience
- Level thresholds
- XP UI

Exit criteria:
- Killing enemies produces XP.
- Player can level up.

---

# PHASE 6 — Temporary Upgrades

## Milestone 6.1

Implement:

- Upgrade definitions
- Level-up pause
- Three upgrade choices
- Upgrade selection
- Resume gameplay

Initial upgrades:

```text
Damage +20%
Attack Speed +15%
Movement Speed +10%
```

Exit criteria:
- Upgrades visibly affect gameplay.
- Upgrades remain active until dungeon ends.

---

# PHASE 7 — Dungeon Completion

## Milestone 7.1

Implement:

- Dungeon clear state
- Stop combat
- Result UI
- Kill count
- Completion time
- XP earned
- Return/continue button

At this milestone the first vertical slice is considered complete.

---

# PHASE 8 — Character System

Only begin after the vertical slice works reliably.

## Milestone 8.1 — CharacterDefinition

Implement:
- Data-driven playable character definitions

## Milestone 8.2 — Character Selection

Implement:
- Character select UI
- Character loading/spawn

## Milestone 8.3 — Archer

Implement:
- Bow
- Projectile system

## Milestone 8.4 — Gunner

Implement:
- Rifle
- Fire rate
- Projectile or hitscan system

---

# PHASE 9 — Dungeon Progression (COMPLETED)

Implemented:

- World Map scene & dungeon selection UI
- Data-driven `DungeonDefinition` & `DungeonCatalog`
- Progression persistence & prerequisite unlocking (`DungeonProgression`)
- Dungeon completion & player defeat lifecycle flows
- Campaign flow: Dungeon 1 (`Dungeon_Prototype`) -> Unlock Dungeon 2 -> Dungeon 2 (`Dungeon_02`)

---

# PHASE 10 — Campaign Run Progression & Scaling (COMPLETED)

Implemented:

- Milestone 10.1: Campaign Run Progression Checkpoints
  - `RunProgressionSession` static state carrier with commit/checkpoint semantics
  - Transient run progression (currentLevel, currentXP, totalXPEarned, appliedUpgrades)
  - Progression committed strictly on victory; entry checkpoint restored on defeat retry; run ended on map return
  - Full health restored on entry/retry (health is never part of run progression)
  - Character ownership validation (`ownerCharacterId`) decoupling character selection from run session
- Milestone 10.2: Data-Driven Enemy Scaling & Warrior Melee Reach Tuning
  - `DungeonDefinition` scaling multipliers (`EnemyHealthMultiplier`, `EnemyDamageMultiplier`)
  - Runtime enemy scaling in `EnemyHealth`, `EnemyAttack`, and `WaveManager`
  - Tuned dungeon difficulty: D1 (1.0x HP, 1.0x Dmg), D2 (1.1x HP, 1.0x Dmg), D3 (1.2x HP, 1.1x Dmg)
  - Tuned Warrior melee attack reach from 2.0m to 2.5m (preserving 25 damage, 120° arc, 0.5s cooldown)
- Milestone 10.3: Run Lifecycle & End-Run Semantics
  - Zero-exploit rollback on defeat retry (discards XP/upgrades earned in failed attempt)
  - Run termination and reset on defeat return to map
  - Explicit Turkish explanatory subtitles on defeat UI (`[YENİDEN DENE]`, `[HARİTAYA DÖN]`)
- Milestone 10.4: Playable Dungeon 3 & Multi-Dungeon Continuity
  - Created Dungeon 3 (`Dungeon_03.unity`, `Dungeon_03.asset`) with 4 waves (50 enemies, 500 XP)
  - Deep crypt visual ambiance and enemy scaling (60 HP zombies, 11 damage)
  - Full 5-scene build sequence: `[0: CharacterSelection, 1: WorldMap, 2: Dungeon_Prototype, 3: Dungeon_02, 4: Dungeon_03]`
  - Verified multi-transition run continuity across D1 -> D2 -> D3
- Milestone 10.5: Progression Foundations & Architecture Invariants
  - Layered `PlayerStats` architecture: Effective Multiplier = Base * Permanent * Temporary
  - Architecture boundaries verified: `CharacterSelectionSession` owns hero, `DungeonRunSession` owns dungeon, `RunProgressionSession` owns campaign progression
  - Chapter/Boss classification foundation defined (`DungeonType.Normal`, `DungeonType.Boss`)
  - Comprehensive automated Play Mode test suite (11/11 checks) and full sequential regressions passed (47/47 checks)

---

# PHASE 11 — Permanent Progression & Meta Trees

Implement:

- Meta currency / Skill points earned from runs
- Character-specific permanent skill tree
- 5–8 initial nodes per character
- Permanent upgrades save/load
- Hooking permanent multipliers into `PlayerStats` foundations

---

# PHASE 12 — Enemy Expansion

Add:

- Runner
- Brute
- Ranged enemy
- Elite enemy

Then balance spawn compositions.

---

# PHASE 13 — Bosses

Implement:

- Boss framework
- Boss health UI
- Attack telegraphs
- First dungeon boss

---

# PHASE 14 — Content and Polish

Add:

- Real character models
- Environment assets
- Animations
- VFX
- Audio
- Music
- Screen shake where appropriate
- Better UI
- Feedback
- Balancing

---

# PHASE 15 — Production

Implement:

- Settings
- Resolution
- Audio sliders
- Save validation
- Build configuration
- Performance checks
- Steam-ready PC build if desired

---

# Current Priority

Unless `PROJECT_STATUS.md` says otherwise:

```text
Phase 0–7 (Core Vertical Slice)              -> COMPLETED
    ↓
Phase 8 (Character System)                    -> COMPLETED
    ↓
Phase 9 (Dungeon Progression)                 -> COMPLETED
    ↓
Phase 10 (Campaign Run Progression & Scaling) -> COMPLETED (Manual QA GREEN)
    ↓
Phase 11 (Permanent Progression & Meta Trees) -> NEXT UP
```

Milestones 0 through 10 are fully implemented, automated-verified, and manual-QA-verified.
Phase 10 is complete and signed off.
Next phase is Phase 11 (Permanent Progression & Meta Trees).
