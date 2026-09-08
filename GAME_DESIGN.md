# GAME_DESIGN.md

## Working Concept

Genre:

**Top-down action roguelite / dungeon crawler**

Primary gameplay loop:

```text
Main Menu
    ↓
Character Selection
    ↓
World Map
    ↓
Select Dungeon
    ↓
Fight Enemy Waves
    ↓
Gain Experience
    ↓
Level Up
    ↓
Choose Temporary Upgrade
    ↓
Final Wave / Boss
    ↓
Dungeon Complete
    ↓
Receive Permanent Rewards
    ↓
Unlock Next Dungeon
    ↓
Return to World Map
```

### Roguelite Campaign Run Semantics

Campaign progression across dungeons (D1 -> D2 -> D3) follows strict roguelite run semantics:

- **Victory**: The active run continues into subsequent dungeons, preserving character Level, accumulated XP, and collected temporary upgrades.
- **Defeat -> Retry**: The player retries the current dungeon, rolling back exclusively to the dungeon-entry checkpoint (restoring the Level, XP, and temporary upgrades that the player had when entering that specific dungeon attempt).
- **Defeat -> Return to Map**: The active run ends immediately. Level, XP, and temporary upgrades reset to baseline (Level 1, 0 XP, neutral modifiers).
- **Persistent Progression**: Completed and unlocked dungeons in the campaign (`DungeonProgression`) remain permanently saved across runs and defeats.

---

# Playable Characters

## Warrior

Role:
- Close-range melee fighter
- High survivability
- Strong area damage

Suggested base identity:

```text
Health: High
Damage: High
Range: Low
Defense: High
Movement: Medium
```

Starting weapon:

**Sword**
- Base Damage: 25
- Attack Range: 2.5m (manually tuned and accepted)
- Cleave Arc: 120°
- Attack Cooldown: 0.5s

Possible future skills:

- Whirlwind
- Shield Bash
- Battle Rage
- Heavy Strike
- Bleeding Slash

---

## Archer

Role:
- Long-range precision fighter
- Mobile
- Strong projectile upgrades

Suggested base identity:

```text
Health: Medium
Damage: High
Range: Very High
Defense: Low
Movement: High
```

Starting weapon:

**Bow**

Possible future skills:

- Piercing Arrow
- Multi Shot
- Rain of Arrows
- Critical Focus
- Quick Step

---

## Gunner

Role:
- Modern ranged fighter
- High fire rate
- Strong sustained damage

Suggested base identity:

```text
Health: Medium
Damage: Medium
Range: High
Defense: Medium
Fire Rate: Very High
```

Starting weapon:

**Rifle**

Possible future skills:

- Rapid Fire
- Explosive Shot
- Critical Burst
- Armor Piercing
- Reload Mastery

---

# Controls

Initial PC controls:

```text
WASD        Move
Mouse       Aim
Left Click  Primary Attack
Space       Dash - future milestone
1 / 2 / 3   Active Skills - future milestone
ESC         Pause/Menu
```

---

# Combat

Initial combat should be readable and simple.

Player:
- Has health.
- Can receive damage.
- Can attack.
- Dies when health reaches zero.

Enemy:
- Detects/follows player.
- Attacks when close enough.
- Has health.
- Dies at zero health.
- Grants or drops experience.

---

# Dungeon Structure

Each dungeon should be data-driven.

Possible dungeon data:

```text
DungeonDefinition
- ID
- Display Name
- Chapter
- Scene
- Waves
- Boss
- Rewards
- Required Dungeon
```

Example:

## Dungeon 1 — Forgotten Graveyard

Wave 1:
- 10 Zombies

Wave 2:
- 15 Zombies

Wave 3:
- 20 Zombies

Future boss:
- Gravekeeper

Clear reward example:
- Gold
- Skill Point
- Dungeon 2 unlock

---

# Experience System

Enemies grant experience.

Possible initial behavior:

```text
Enemy dies
    ↓
Experience Pickup appears
    ↓
Player collects it
    ↓
XP bar increases
    ↓
Level threshold reached
    ↓
Gameplay pauses
    ↓
Three upgrades appear
    ↓
Player chooses one
    ↓
Gameplay resumes
```

---

# Temporary Upgrades

Temporary upgrades apply only during the current dungeon run.

Initial upgrade pool:

- Damage +20%
- Attack Speed +15%
- Movement Speed +10%

Future examples:

- Max Health +20%
- Critical Chance +5%
- Critical Damage +25%
- Cooldown Reduction +10%
- +1 Projectile
- Area Size +15%
- Knockback +20%

Temporary upgrades reset when the run ends.

---

# Permanent Progression

Permanent progression is separate from run XP.

Possible permanent resources:

- Skill Points
- Gold
- Essence / Souls

Character skill trees should be small initially.

Target:
- 5–8 nodes per character in the first complete version.

Example Warrior tree:

```text
Sword Mastery
     |
Damage +10%
   /     \
Bleed   Heavy Strike
  |         |
Crit      Stun
   \       /
   Whirlwind+
```

---

# World Map & Dungeon Flow

Progression between dungeons follows a sequential map structure:

```text
Dungeon 1 (Bölüm 1: Giriş)
   ↓
Dungeon 2 (Bölüm 2: Karanlık Koridor)
   ↓
Dungeon 3 (Bölüm 3: Mahzenin Derinlikleri)
   ↓
Boss Dungeon (Future Phase)
```

Locked dungeons clearly display unlocking prerequisites. Completed dungeons can be revisited or continued in sequence.

---

# Campaign Run Progression & Checkpoints

A campaign run spans across connected dungeons with transient progression persistence:

1. **Commit vs. Checkpoint Semantics**:
   - **Committed Run State**: Progression (Level, current XP, total XP, chosen upgrades) is committed into durable session memory *only* upon achieving Dungeon Victory.
   - **Dungeon Entry Checkpoint**: Whenever a dungeon loads, an entry checkpoint is created.
   - **Defeat Retry**: If defeated and the player chooses **YENİDEN DENE** (Retry), progression reverts to the dungeon entry checkpoint. Any XP or upgrades gained during the failed attempt are discarded, preventing death-farming exploits.
   - **Defeat Return to Map**: If defeated and the player chooses **HARİTAYA DÖN** (Return to Map), the campaign run is terminated. Run progression resets completely (Level 1, 0 XP, 0 upgrades).
   - **Full Health Reset**: Player health is never stored across scenes or retries. The player always starts every dungeon and every retry at 100% full health.

2. **Session Architecture Decoupling**:
   - `CharacterSelectionSession`: Exclusively owns the active hero selection (`CharacterDefinition`).
   - `DungeonRunSession`: Exclusively owns the active dungeon selection (`DungeonDefinition`).
   - `RunProgressionSession`: Exclusively owns active run progression data. Stores `ownerCharacterId` purely to validate whether the progression matches the selected hero. If mismatched, it resets cleanly.

---

# Dungeon Scaling & Difficulty Curves

Enemy health and damage scale per dungeon via `DungeonDefinition` multipliers without modifying base prefab assets:

| Dungeon | Display Name | Waves | Enemies | Total XP | HP Multiplier | Zombie HP | Damage Multiplier | Zombie Damage |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Dungeon 1** | Bölüm 1: Giriş | 3 | 45 | 450 XP | 1.0x | 50 HP | 1.0x | 10 Dmg |
| **Dungeon 2** | Bölüm 2: Karanlık Koridor | 4 | 40 | 400 XP | 1.1x | 55 HP | 1.0x | 10 Dmg |
| **Dungeon 3** | Bölüm 3: Mahzenin Derinlikleri | 4 | 50 | 500 XP | 1.2x | 60 HP | 1.1x | 11 Dmg |

### Combat Breakpoint Philosophy
- Player power growth via level-up upgrades (+20% damage, +15% attack speed) outpaces the subtle enemy HP scaling (10-20%), creating a sense of increasing mastery rather than a bullet-sponge grind.
- Weapon breakpoints against 50/55/60 HP zombies remain smooth:
  - **Warrior** (25 base dmg): 2 hits (D1) -> 3 hits (D2) -> drops back to 2 hits with one Damage upgrade.
  - **Archer** (20 base dmg): 3 hits (D1/D2/D3) -> drops to 2 hits with upgrades.
  - **Gunner** (10 base dmg): 5 hits (D1) -> 6 hits (D2/D3) -> sustained DPS melts groups as fire rate increases.

---

# Enemy Types

Initial:

## Zombie

Behavior:
- Follow player.
- Attack at melee range.
- Moderate health.
- Slow movement.

Future enemy examples:

- Runner Zombie
- Brute
- Ranged Cultist
- Shield Enemy
- Exploding Enemy
- Elite variants

---

# Boss Design

Bosses are not required for the first combat prototype.

Future boss guidelines:

- Distinct silhouette
- Telegraph attacks
- Multiple attack patterns
- Clear damage windows
- Reward dungeon completion

---

# Art Direction

Initial development art:

Use primitives and placeholder assets.

Examples:

```text
Warrior -> Capsule
Zombie -> Capsule
Sword -> Cube
Dungeon -> Plane + Walls
```

Priority:

```text
Gameplay first
Art second
Polish third
```

Possible final style:

**Stylized low-poly dark fantasy with selected modern elements for Gunner.**

Avoid pursuing realistic graphics in the first production phase.

---

# Camera

Recommended:

**Top-down / angled third-person camera**

The camera should:
- Follow player smoothly.
- Keep combat space visible.
- Avoid excessive rotation.
- Support mouse-based aiming.

---

# First Vertical Slice

The first playable milestone is complete when the player can:

1. Enter one dungeon.
2. Control the Warrior.
3. Move with WASD.
4. Aim using the mouse.
5. Attack using a sword.
6. Kill Zombies.
7. Survive enemy attacks.
8. Complete three waves.
9. Collect XP.
10. Level up.
11. Choose temporary upgrades.
12. Clear the dungeon.
13. See a Dungeon Complete screen.

No character selection, world map, permanent skill tree, Archer, or Gunner is required before this vertical slice works reliably.
