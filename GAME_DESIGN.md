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

# World Map

The world map is a later milestone.

Initial concept:

```text
Dungeon 1
   ↓
Dungeon 2
   ↓
Dungeon 3
   ↓
Boss Dungeon
```

Locked dungeons must clearly show progression requirements.

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
