# PROJECT_STATUS.md

> This file is the handoff checkpoint between ChatGPT, Antigravity, Codex, and human development sessions.

Last update:
- Milestone 3.1 completed and verified in Unity Play Mode.

---

# Current Project State

## Status

**COMPLETED — Phase 3: Enemy Foundation (Milestone 3.1: Zombie Enemy)**
**NEXT UP — Phase 4: Wave System (Milestone 4.1: Spawner / Wave Manager)**

Milestones 1.1 (Movement), 1.2 (Camera and Aim), 1.3 (Player Health), 2.1 (Damage Architecture), 2.2 (Basic Sword Combat), and 3.1 (Basic Zombie Enemy) are completed and verified.

---

# Current Phase

## PHASE 3 — Enemy Foundation (Completed)

All milestones in Phase 3 are complete.

---

# Completed

- Core game concept defined.
- Three character concepts defined: Warrior, Archer, Gunner.
- Initial dungeon/wave progression defined.
- Temporary XP upgrade concept defined.
- Permanent skill tree concept defined.
- AI handoff workflow defined.
- Documentation created: AGENTS.md, GAME_DESIGN.md, ARCHITECTURE.md, ROADMAP.md, PROJECT_STATUS.md.
- Git repository initialized on branch `main`.
- Standard Unity `.gitignore` created and verified.
- Initial project files committed and pushed to `https://github.com/AbdullahTurgut/DungeonRoguelite`.
- **Milestone 1.1 — Player Movement**:
  - Implemented `PlayerMovement.cs` component using `CharacterController` on the X/Z horizontal plane.
  - Bound to existing `InputSystem_Actions.inputactions` using `Player/Move`.
  - Configurable movement speed (`moveSpeed = 6f`) and grounding gravity (`gravity = 20f`) in Inspector.
  - Diagonal input normalized to ensure diagonal movement speed matches cardinal movement speed.
  - Single-responsibility architecture: no combat, aiming, or health logic in movement component.
  - Created placeholder Capsule player prefab at `Assets/Prefabs/Characters/Player.prefab`.
  - Created prototype scene at `Assets/Scenes/Dungeons/Dungeon_Prototype.unity` with floor, walls, obstacles, and static top-down camera.
  - Automated in-engine verification completed with 0 errors in Play Mode.
- **Milestone 1.2 — Camera and Aim**:
  - Implemented `CameraFollow.cs` component using `Vector3.SmoothDamp` in `LateUpdate()`.
  - Maintained fixed angled top-down camera orientation (`55°` pitch, `0°` yaw/roll) without jitter.
  - Implemented `PlayerAim.cs` component converting pointer screen coordinates to world positions via horizontal mathematical `Plane` intersection at player height.
  - Player rotation strictly constrained to the Y-axis (`Euler(0, yaw, 0)`).
  - Movement and aiming operate completely independently and simultaneously (e.g. strafing and kiting).
  - Added visual non-colliding `FacingIndicator` child on `Player.prefab` to clearly display facing direction.
  - Configured `Main Camera` in `Dungeon_Prototype.unity` with `CameraFollow` targeting the player.
  - Automated Play Mode verification suite ran and passed all 7 test cases with 0 errors.
- **Milestone 1.3 — Player Health**:
  - Implemented `PlayerHealth.cs` component managing current/max health and death state.
  - Clamped health strictly between 0 and `maxHealth`, with `maxHealth >= 1`.
  - Damage reception via public `TakeDamage(float amount)` ignoring zero or negative values.
  - Event-based notification via `OnHealthChanged(float current, float max)` and `OnDied()`.
  - Guaranteed single-execution death logic; damage after death does not re-trigger death.
  - Read-only properties `MaxHealth`, `CurrentHealth`, `IsDead`, `HealthNormalized`.
  - Attached to `Player.prefab` with default `maxHealth = 100`.
  - Configured prototype scene with automated verification suite under `Assets/Tests/Verification/Milestone1_3_Verifier.cs`.
  - All 16 verification checks passed in Play Mode with 0 errors and 0 exceptions.
- **Milestone 2.1 — Damage Architecture**:
  - Defined minimal `IDamageable` interface in `Assets/Scripts/Combat/IDamageable.cs` with `void TakeDamage(float amount);`.
  - Updated `PlayerHealth.cs` to implement `IDamageable` natively with zero modifications to clamping, death guards, or event logic.
  - Evaluated and deferred `DamageInfo` struct, avoiding premature combat complexity and unused struct allocations.
  - Preserved polymorphic decoupling allowing future weapons to apply damage without needing to know concrete entity types.
  - Created automated Play Mode verification suite at `Assets/Tests/Verification/Milestone2_1_Verifier.cs`.
  - All 13 verification checks passed in Play Mode with 0 errors and 0 exceptions.
- **Milestone 2.2 — Basic Sword Combat**:
  - Implemented `PlayerAttack.cs` component binding to `Player/Attack` input (`<Mouse>/leftButton`, Gamepad Attack).
  - Implemented `MeleeWeapon.cs` component in `Assets/Scripts/Weapons/` with configurable `damage` (25), `attackCooldown` (0.5s), `range` (2.0m), and `arcAngle` (120°).
  - Instantaneous `Physics.OverlapSphere` query filtered by owner forward arc cone, strictly protecting against hits from behind or outside the arc.
  - Multi-collider de-duplication per swing via `HashSet<IDamageable>`.
  - Self-damage immunity preventing weapon owner from damaging itself.
  - Added non-colliding `WeaponAnchor` and placeholder `SwordVisual` cube to `Player.prefab`.
  - Created test dummy `TestDamageableTarget.cs` in `Assets/Tests/Verification/`.
  - Automated Play Mode verification suite (`Milestone2_2_Verifier.cs`) passed all 17 checks with 0 errors.
- **Milestone 3.1 — Basic Zombie Enemy**:
  - Implemented `EnemyHealth.cs` implementing `IDamageable` with configurable `maxHealth` (50), health clamping, and single-fire `OnDied` event. Strictly owns health state without directly disabling other components.
  - Implemented `EnemyMovement.cs` with direct pursuit using `CharacterController` on the X/Z plane with configurable `moveSpeed` (3) and `stoppingDistance` (1.3). Subscribes cleanly to `EnemyHealth.OnDied` to disable movement and its `CharacterController`.
  - Implemented `EnemyAttack.cs` executing attacks within `attackRange` (1.5) dealing `damage` (10) with `attackCooldown` (1.0s) via `IDamageable.TakeDamage()`. Subscribes cleanly to `EnemyHealth.OnDied` to halt attacks.
  - Created reusable `Zombie.prefab` at `Assets/Prefabs/Enemies/Zombie.prefab` with placeholder capsule and facing indicator.
  - Automated Play Mode verification suite (`Milestone3_1_Verifier.cs`) passed all 27 checks with 0 errors.

---

# Next Task

**Milestone 4.1 — Spawner / Wave Manager**

Tasks for Milestone 4.1:
1. Create `WaveDefinition` ScriptableObject or wave configuration data structure.
2. Create `WaveManager.cs` or `EnemySpawner.cs` component in `Assets/Scripts/Waves/`.
3. Implement sequential wave spawning (e.g. 3 waves for vertical slice).
4. Track active enemies and trigger wave completion when all enemies in a wave are defeated.
5. Provide clean events for wave start, wave clear, and dungeon completion.

Do NOT start:
- Temporary level-up upgrades / XP bar (Milestones 5.1 / 5.2)
- Boss logic (Milestone 6.1)
- UI / Game complete screen (Milestones 7.1 / 7.2)

---

# Current Architectural Decisions

- Engine: Unity 6 LTS (6000.3.23f1)
- Language: C#
- Game type: Top-down 3D action roguelite / dungeon crawler
- Render Pipeline: URP (17.3.0)
- Movement: `CharacterController` driven on X/Z plane with grounded vertical velocity (`PlayerMovement.cs` and `EnemyMovement.cs`)
- Aiming: Screen-to-world raycast against horizontal mathematical `Plane` at player height; Y-axis only rotation (`PlayerAim.cs`)
- Camera: Custom lightweight `CameraFollow.cs` in `LateUpdate` with `Vector3.SmoothDamp` and fixed top-down pitch
- Combat Abstraction: Minimal `IDamageable` interface (`TakeDamage(float amount)`) in `Assets/Scripts/Combat/`
- Weapon Architecture: Decoupled `PlayerAttack.cs` (input coordination) and `MeleeWeapon.cs` (hit detection & cooldown tracking)
- Health Architecture: `PlayerHealth.cs` and `EnemyHealth.cs` both implement `IDamageable` independently with clamped health and single-fire death events
- Enemy Architecture: Modular components (`EnemyHealth`, `EnemyMovement`, `EnemyAttack`) communicating via clean C# events without monolithic controllers
- Input: Unity Input System (`com.unity.inputsystem` 1.20.0) with `InputSystem_Actions.inputactions`
- Single Responsibility: Separate components across Player, Combat, Weapons, and Enemies
- Visuals: Primitives/placeholders (Capsules with FacingIndicators and SwordVisual)
- Git used as checkpoint and handoff system

---

# Known Issues

None.

---

# Future Maintenance & Technical Debt

- **Direct Pursuit vs Pathfinding**: Current `EnemyMovement` uses direct `CharacterController` pursuit. This is sufficient for the prototype but does not provide full pathfinding around complex dungeon geometry. Re-evaluate NavMesh when dungeon layouts require real obstacle navigation.
- **Verifier Script Reorganization**: Legacy verifier scripts currently located under production script folders (`Assets/Scripts/Player/Milestone1_1_Verifier.cs` and `Assets/Scripts/Player/Milestone1_2_Verifier.cs`) should later be moved into the dedicated test/verification structure (`Assets/Tests/Verification/`).

---

# Testing Status

## Milestone 3.1 Verification

Automated Play Mode verification suite ran and passed in Unity (`playmode_m3_1.log`):
- **Test 1 & 2 - Initial Health**: Zombie starts with `MaxHealth == 50`, `CurrentHealth == 50`, `IsDead == false` (PASSED)
- **Test 3 & 4 - Follow & Stopping Distance**: Zombie pursues player from 4m and halts at `CurrentDist = 1.30m` near `stoppingDistance = 1.3m` (PASSED)
- **Test 5 - Range Guard**: Zombie does not attack when outside range at 4m (PlayerHealth remains 100) (PASSED)
- **Test 6 & 7 - Inside Range Attack**: Zombie strikes inside 1.5m range applying exactly 10 damage (PlayerHealth drops 100 $\rightarrow$ 90) (PASSED)
- **Test 8 - Attack Cooldown Guard**: Attack cooldown prevents immediate repeat attack (PlayerHealth remains 90) (PASSED)
- **Test 9 - Attack Recovery**: Second attack executes after 1.0s cooldown expires (PlayerHealth drops 90 $\rightarrow$ 80) (PASSED)
- **Test 10 - Pursuit Resumption**: Zombie immediately resumes pursuit when player moves away, closing distance to 1.30m (PASSED)
- **Test 11 & 12 - First Sword Hit**: Player sword resolves `IDamageable` on Zombie, dealing 25 damage (Zombie health 50 $\rightarrow$ 25) (PASSED)
- **Test 13-16 - Second Sword Hit & Death**: Second hit reduces health 25 $\rightarrow$ 0, clamps to 0, sets `IsDead == true`, and fires `OnDied` exactly once (PASSED)
- **Test 17 & 18 - Component Disabling**: Dead Zombie sets `EnemyMovement.enabled == false` and `EnemyAttack.enabled == false` (PASSED)
- **Test 19 - Dead Zombie Harmless**: Dead Zombie cannot damage player even when standing adjacent (PlayerHealth untouched) (PASSED)
- **Test 20 - Non-Obstruction**: Dead Zombie disables `CharacterController` so corpse does not block player (PASSED)
- **Test 21 - PlayerMovement Integrity**: Player moves freely via WASD (moveSpeed = 6) (PASSED)
- **Test 22 - PlayerAim Integrity**: Player rotates and aims toward cursor without drift (angle diff $0.00^\circ$) (PASSED)
- **Test 23 - PlayerHealth Integrity**: Player health reception and clamps intact (PASSED)
- **Test 24 - Sword Combat Integrity**: Player sword attack remains fully operational (PASSED)
- **Test 25 - Grounding & Collision**: CharacterController grounding and collisions intact (PASSED)
- **Event Subscription Idempotency**: Disabling/enabling enemy components does not produce duplicate callbacks (PASSED)
- **Test 26 - Compilation**: 0 compiler errors or warnings (PASSED)
- **Test 27 - Runtime Diagnostics**: 0 exceptions in Play Mode (PASSED)

---

# Recent Git Checkpoint
 
```text
Latest verified commit:
<pending commit for Milestone 3.1>
```

---

# Handoff Notes

When switching between agents:
1. Confirm `git status` is clean.
2. Inspect `PROJECT_STATUS.md` for current phase and next milestone.
3. Follow single responsibility guidelines in `AGENTS.md`.

---

# Session End Checklist

## Completed This Session

```text
- Milestone 3.1 Basic Zombie Enemy implementation and verification.
- Assets/Scripts/Enemies/EnemyHealth.cs
- Assets/Scripts/Enemies/EnemyMovement.cs
- Assets/Scripts/Enemies/EnemyAttack.cs
- Assets/Prefabs/Enemies/Zombie.prefab
- Assets/Tests/Verification/Milestone3_1_Verifier.cs
- Assets/Editor/Milestone3_1_Setup.cs
- Assets/Scenes/Dungeons/Dungeon_Prototype.unity (configured Milestone3_1_Verifier)
```

## Changed Files

```text
- Assets/Scripts/Enemies/EnemyHealth.cs
- Assets/Scripts/Enemies/EnemyHealth.cs.meta
- Assets/Scripts/Enemies/EnemyMovement.cs
- Assets/Scripts/Enemies/EnemyMovement.cs.meta
- Assets/Scripts/Enemies/EnemyAttack.cs
- Assets/Scripts/Enemies/EnemyAttack.cs.meta
- Assets/Scripts/Enemies.meta
- Assets/Prefabs/Enemies/Zombie.prefab
- Assets/Prefabs/Enemies/Zombie.prefab.meta
- Assets/Prefabs/Enemies.meta
- Assets/Tests/Verification/Milestone3_1_Verifier.cs
- Assets/Tests/Verification/Milestone3_1_Verifier.cs.meta
- Assets/Editor/Milestone3_1_Setup.cs
- Assets/Editor/Milestone3_1_Setup.cs.meta
- Assets/Scenes/Dungeons/Dungeon_Prototype.unity
- PROJECT_STATUS.md
```

## Tested

```text
- In-engine Play Mode automated verification suite (all 27 checks passed)
- Zombie initialization, health state, and clamping
- Direct pursuit, speed, and stopping distance (1.3m)
- Melee attack damage (10 dmg), range check (1.5m), and attack cooldown (1.0s)
- Pursuit resumption when target moves away
- Player sword hit detection and damage application (2 hits defeat Zombie)
- Clean death handling: OnDied event, component disabling, CharacterController disabled (non-obstructing)
- Event subscription idempotency upon component toggle
- Non-interference with PlayerMovement, PlayerAim, PlayerHealth, and MeleeWeapon
- Unity compilation with 0 errors and 0 runtime exceptions
```

## Known Issues

```text
None.
```

## Next Task

```text
Milestone 4.1 — Spawner / Wave Manager
```

## Latest Verified Commit

```text
<pending commit for Milestone 3.1>
```
