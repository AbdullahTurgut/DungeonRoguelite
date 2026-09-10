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

# PHASE 11 — Combat & Enemy Variety (COMPLETE)

- Gate 11.1: Enemy attack contract, hit feedback, corpse cleanup — automated and user manual QA GREEN.
- Gate 11.2: Runner — automated GREEN; 41 focused checks and 170 focused regression checks.
- Gate 11.3: Tank — automated GREEN; 273 focused checks and 211 focused regression checks.
- Gate 11.4: Ranged enemy and locally owned projectiles — automated GREEN, 106 checks; local checkpoint `1a0846b`.
- Gate 11.5: Approved mixed waves and campaign integration — automated GREEN, 1485 focused checks; full 13-suite regression GREEN (2117 checks). User manual gameplay QA ALL GREEN.
- Archetypes implemented: Zombie, Runner, Tank, Ranged.
- Wave compositions & totals: D1 (38 enemies / 410 XP), D2 (35 enemies / 430 XP), D3 (42 enemies / 540 XP), Campaign (115 enemies / 1380 XP).
- Phase 11 signed off.

---

# PHASE 12 — Permanent Progression / Skill Tree (COMPLETE)

- Gate 12.1: Permanent progression foundation, data model, PlayerPrefs persistence, and stat multipliers — automated GREEN (87 checks).
- Gate 12.2: First-clear reward economy (D1=2, D2=2, D3=3; 7 pts max per character) and authoritative dungeon completion integration — automated GREEN (99 checks).
- Gate 12.3: Warrior permanent skill tree (`SkillTree_Warrior.asset`, 3 branches x 3 tiers = 9 nodes) and spawn-time modifier binding with fresh health scaling (135/135 HP) — automated GREEN (200 checks).
- Gate 12.4: Archer and Gunner permanent skill trees (`SkillTree_Archer.asset`, `SkillTree_Gunner.asset`), real combat binding, and upgrade layering — automated GREEN (226 checks).
- Gate 12.5: World Map permanent Skill Tree purchase UI ("YETENEKLER" overlay with 4 deterministic node states) and final integration — automated GREEN (127 checks).
- Full 15-suite regression matrix: 2432/2432 checks PASSED. User manual gameplay QA ALL GREEN.
- Phase 12 signed off.

---

# PHASE 13 — Dungeon 4 + Encounter Design (COMPLETE)

- Dungeon 4, The Colonnade / Bölüm 4: Sütunlu Salon, is complete: a 36m x 28m four-pillar arena with six spawn points, four production waves, 45 enemies, and 705 XP.
- D4 uses 1.3x enemy health and 1.2x enemy damage, unlocks after D3, awards 3 permanent points per character only on first clear, and carries the campaign to Level 7 late in D4.
- Automated gates, the 2129-check final matrix, D4 Wave 1 -> Wave 2 runtime regression, and manual gameplay QA are GREEN.

---

# PHASE 14 — Dungeon 5 + First Boss (COMPLETE)

- Dungeon 5, The Ashen Sanctum / Kül Mabedi, is `dungeon_5`: 1.4x enemy HP, 1.3x enemy damage, three waves, 18 normal enemies plus The Ash Warden / Kül Muhafızı, 19 enemies, and 1,200 XP total.
- The Ash Warden has 1,200 base HP / 1,680 runtime HP, two phases, no adds, no persistent hazards, and 960 boss XP. D5 reaches Level 8 at 68/1709 from the Level 7, 7/1139 entry state; campaign XP is 3,285. D5 first-clear permanent reward was 0 at Phase 14 sign-off; Phase 15 changes it to 2.
- The World Map now uses a generic three-card carousel with newest-unlocked focus, locked browsing, Enter gating, bounded button and keyboard navigation, Skill Tree modal safety, and D6+ window support.
- Final presentation includes the Warrior's 0.12-second visual-only melee slash and a centered boss name with one red RectTransform health fill over a dark empty track; no Phase II label is presented.
- User manual QA is GREEN. Focused evidence: Boss Health Bar 9/9, Warrior Slash Feedback 8/8, World Map Carousel 525 checks, Phase 10 World Map QA 10/10, and Gate 14.3 PASSED.
- Final reruns of Phase 12.5 Skill Tree UI, Gate 14.1, and D5 boss lifecycle were blocked before verifier startup by `Connection to channel LicenseClient-Alcor refused`. This is an ENVIRONMENT BLOCKER, not a production result; those suites are not recorded as passed.

---
# PHASE 15 — Permanent Progression Rebalance (COMPLETE)

- Existing nine-node trees cost 1/2/2 per branch: 15 points per character.
- First-clear rewards D1–D10: 1/1/1/1/2/1/1/2/2/3. Replays never farm points.
- Safe v1 -> v2 migration grandfathers purchases/points/history, with one future legacy D5 claim. Focused migration/economy smoke passed.

# PHASE 16 — Hub, Armorer and Tier I Weapons (COMPLETE)

- Small 3D Hub_Armory, three character displays, deliberate Blacksmith interaction, free D5 milestone weapons and selected-character equipment presentation.
- D5 once-only Blacksmith introduction automatically claims/equips the active hero's Tier I weapon. Other heroes retain free claims.
- Additive v2 -> v3 equipment persistence; authoritative stat layering preserves permanent versus temporary state. Focused smoke and user manual QA GREEN.

# PHASE 17 — Dungeons 6 and 7 (COMPLETE)

- D6 / Kırık Geçit: causeway, five waves, 55 enemies / 900 XP, exit L8 968/1709, 1 point.
- D7 / Çökmüş Sarnıç: central-basin ring, five waves, 70 enemies / 1,100 XP, exit L9 359/2563, 1 point.
- Production lifecycle smoke and manual campaign QA GREEN.

# PHASE 18 — Dungeons 8 and 9 (COMPLETE)

- D8 / Közlü Avlu: four-obelisk courtyard, five waves, 80 enemies / 1,300 XP, exit L9 1659/2563, 2 points.
- D9 / Gölge Hisarı: two-lane fortress, six waves, 90 enemies / 1,600 XP, exit L10 696/3844, 2 points.
- Production lifecycle smoke and manual campaign QA GREEN.

# PHASE 19 — Dungeon 10 and Hollow Castellan (COMPLETE)

- D10 / Yıldızsız Taht: four-pillar throne arena, three warm-up waves followed by Hollow Castellan; 81 enemies / 3,200 XP, exit L11 52/5767, 3 first-clear points.
- Boss: 2,000 base / 3,800 runtime HP, 2,000 XP, cleave and line dash; Phase II adds faster cadence and a three-projectile shard fan. No adds or persistent hazards. Narrow shared boss HUD contract.
- Tier II milestone claim/equip, persistent campaign completion, and modest Hub acknowledgement are implemented.
- Once-only D10 Blacksmith reward sequence follows completed XP/upgrades/victory and the Haritaya Dön action. Active hero's Tier II is revealed and auto-equipped with persistent, duplicate-safe ownership and missed Tier I recovery.
- D10 gameplay, Hollow Castellan, Tier II and Blacksmith reward flow: user manual QA GREEN. Focused boss and reward-flow smokes passed.
- D5 Warrior spacing adjustment is accepted for now (2.9m Strike/stop, 2.1s cooldown, 0.85s recovery); further melee/boss tuning is deferred to Phase 20.

# PHASE 20 — First Campaign Stabilization (PLANNED, NOT STARTED)

- Begin only on explicit user instruction after this sign-off.
- Address evidence-driven campaign feel, readability, balance, UI/Hub usability, persistence or performance issues from manual play.
- Do not automatically implement every polish category. No D11–D15, Tier III, loot/inventory/crafting systems, final art pipeline or generic frameworks.

# Current Priority

Phases 0–19 are implemented and accepted. The Phase 15–19 campaign milestone is manually GREEN, including the D10 Blacksmith Tier II reward flow. Stop before Phase 20.

Historical automation caveat: Phase 14 final reruns of Phase 12.5 Skill Tree UI, Gate 14.1 and D5 boss lifecycle remain environment-blocked before execution by Unity licensing IPC. They are not recorded as passed; Gate 14.3 later passed. See PROJECT_STATUS.md for focused evidence and checkpoint history.
