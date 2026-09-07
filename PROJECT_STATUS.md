# PROJECT_STATUS.md

> This file is the handoff checkpoint between ChatGPT, Antigravity, Codex, and human development sessions.

Last update:
- Milestone 8.4 completed and verified in Unity Play Mode.
- Gunner Combat Prototype (IPrimaryAttack, RifleWeapon, Gunner.prefab, Character_Gunner.asset) established.

---

# Current Project State

## Status

**COMPLETED — Phase 8: Character System (Milestone 8.4: Gunner Combat Prototype)**  
**NEXT UP — Phase 8: Character System (Milestone 8.5: Character Selection UI & Integration)**

Milestones 1.1 (Movement), 1.2 (Camera and Aim), 1.3 (Player Health), 2.1 (Damage Architecture), 2.2 (Basic Sword Combat), 3.1 (Basic Zombie Enemy), 4.1 (Spawner and Wave System), 5.1 (Experience System), 6.1 (Temporary Upgrades), 7.1 (Dungeon Completion), 8.1 (Character Architecture), 8.2 (Runtime Player Spawning & Explicit Binding), 8.3 (Archer Combat Prototype), and 8.4 (Gunner Combat Prototype) are completed and verified.

---

# Current Phase

## PHASE 8 — Character System (In Progress)

- Milestone 8.1: Character Definition & Architecture (Completed)
- Milestone 8.2: Runtime Player Spawning & Explicit Binding (Completed)
- Milestone 8.3: Archer (Completed)
- Milestone 8.4: Gunner (Completed)
- Milestone 8.5: Character Selection (Next Up)

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
- **Milestone 4.1 — Spawner and Wave System**:
  - Implemented data-driven `WaveDefinition.cs` ScriptableObject defining wave composition (`EnemySpawnEntry[]`) and pacing (`spawnInterval = 0.5f`).
  - Created prototype wave assets `Wave_01.asset` (10 Zombies), `Wave_02.asset` (15 Zombies), `Wave_03.asset` (20 Zombies) under `Assets/ScriptableObjects/Waves/`.
  - Implemented `WaveManager.cs` component in `Assets/Scripts/Waves/` maintaining single responsibility over wave sequencing, player targeting, and round-robin perimeter spawn points.
  - Configured 4 scene spawn points (`SpawnPoints` hierarchy) around the arena perimeter.
  - Implemented authoritative living enemy tracking using `HashSet<EnemyHealth> activeEnemies` with `LivingEnemyCount => activeEnemies.Count`. Zero frame-by-frame polling.
  - Per-enemy event subscription to `EnemyHealth.OnDied` with clean delegate unsubscription upon death and component cleanup. Idempotent death processing.
  - Enforced strict wave progression gating: next wave begins only after all enemies have finished spawning AND every living enemy is dead.
  - Emits clean `OnDungeonCompleted` event exactly once when Wave 3 is cleared.
  - Preserved non-obstructing corpse retention without impeding wave progression.
  - Verified in Unity Play Mode: all 26 automated verification checks passed with 0 compiler errors and 0 runtime exceptions.
- **Milestone 5.1 — Experience System**:
  - Implemented `PlayerExperience.cs` component in `Assets/Scripts/Experience/` managing level, current XP, and exponential threshold scaling (`RoundToInt(baseRequiredXP * Pow(xpGrowthMultiplier, level - 1))`).
  - Configured Level 1 starting values: `currentLevel = 1`, `currentXP = 0`, `baseRequiredXP = 100`, `xpGrowthMultiplier = 1.5f` (Level 1: 100 XP, Level 2: 150 XP, Level 3: 225 XP).
  - Handled cleanly in `GainExperience(int amount)`: non-positive amounts safely ignored, carry-over XP preserved across levels, multi-level jumps supported in single calls.
  - Clean decoupled event notifications: `OnExperienceChanged(int currentXP, int xpToNextLevel)` and `OnLevelUp(int newLevel)`. Zero UI or stat modification logic in progression class.
  - Implemented `ExperiencePickup.cs` physical collectible orb component with `SphereCollider` (`isTrigger = true`), kinematic `Rigidbody`, double-award guard, player trigger detection, and cleanup upon collection.
  - Implemented `ExperienceReward.cs` decoupled enemy component listening to `EnemyHealth.OnDied`, spawning `ExperiencePickup` at death position with single-fire guard.
  - Implemented `PlayerExperienceUI.cs` event-driven HUD component in `Assets/Scripts/UI/` updating Slider and `TextMeshProUGUI` with zero polling in `Update()`.
  - Created `ExperiencePickup.prefab` at `Assets/Prefabs/Pickups/ExperiencePickup.prefab` with cyan visual material.
  - Updated `Player.prefab` with `PlayerExperience.cs` component.
  - Updated `Zombie.prefab` with `ExperienceReward.cs` component configured with `xpAmount = 10` and `pickupPrefab`.
  - Configured `Dungeon_Prototype.unity` with Canvas, EventSystem, ExperienceHUD (XPSlider + LevelText), and `Milestone5_1_Verifier.cs`.
  - Automated Play Mode verification suite ran and passed all 32 checks with 0 errors and 0 runtime exceptions.
- **Milestone 6.1 — Temporary Upgrades (Upgrade Selection & Stat Modifiers)**:
  - Implemented `PlayerStats.cs` component on `Player.prefab` owning runtime temporary stat multipliers (`DamageMultiplier`, `AttackSpeedMultiplier`, `MovementSpeedMultiplier`), initialized neutrally at 1.0.
  - Implemented additive percentage stacking logic (`AddDamageBonus`, `AddAttackSpeedBonus`, `AddMovementSpeedBonus`) with zero dependencies on `UpgradeDefinition` or UI. (e.g. Damage +20% once = 1.20, twice = 1.40; Attack Speed +15% twice = 1.30; Movement Speed +10% twice = 1.20).
  - Defined `UpgradeType.cs` enum (`Damage`, `AttackSpeed`, `MovementSpeed`).
  - Created data-driven `UpgradeDefinition.cs` ScriptableObject data structure with `id`, `displayName`, `description`, `upgradeType`, and `magnitude`.
  - Created 3 prototype ScriptableObject upgrade assets under `Assets/ScriptableObjects/Upgrades/`: `Upgrade_Damage.asset` (Damage +20%, magnitude 0.20), `Upgrade_AttackSpeed.asset` (Attack Speed +15%, magnitude 0.15), and `Upgrade_MovementSpeed.asset` (Movement Speed +10%, magnitude 0.10).
  - Integrated `PlayerStats` into `MeleeWeapon.cs` (`EffectiveDamage = damage * DamageMultiplier`, `EffectiveAttackCooldown = attackCooldown / AttackSpeedMultiplier`) and `PlayerMovement.cs` (`EffectiveMoveSpeed = moveSpeed * MovementSpeedMultiplier`).
  - Implemented strict pause guards (`Time.timeScale <= 0f`) across `PlayerMovement`, `PlayerAttack`, `EnemyMovement`, and `EnemyAttack`, guaranteeing zero displacement or combat execution while paused.
  - Implemented `UpgradeManager.cs` orchestrating level-up queue (`pendingChoicesCount`), pausing gameplay (`Time.timeScale = 0f`), emitting choices, guarding against rapid double-clicks, applying selected upgrades to `PlayerStats`, and resuming gameplay (`Time.timeScale = 1f`) when the queue empties.
  - Implemented `UpgradeSelectionUI.cs` modal view controller attached to `Canvas`, listening to `UpgradeManager` events to show/hide `UpgradeSelectionPanel` with 3 dynamic `UpgradeChoiceButton` cards displaying formatted name and description.
  - Enforced clean decoupled architecture: UI components contain zero stat calculation or player stats modification logic; `PlayerStats` contains zero upgrade or UI logic.
  - Configured `Dungeon_Prototype.unity` scene with `UpgradeManager` and `Canvas/UpgradeSelectionPanel`.
  - Automated Play Mode verification suite (`Milestone6_1_Verifier.cs`) ran and passed all 36 checks with 0 errors and 0 runtime exceptions.
- **Milestone 7.1 — Dungeon Completion (Result Screen & Dungeon Clear State)**:
  - Created `DungeonRunSummary.cs` immutable value structure in `Assets/Scripts/Dungeons/` holding `CompletionTime`, `EnemiesDefeated`, `FinalLevel`, `TotalXPEarned`, and formatted time (`MM:SS`). Pure data container without UI or gameplay dependencies.
  - Implemented `DungeonRunStats.cs` component on `RunControllers` GameObject tracking unpaused active run elapsed time (`Time.time - startTime`), tracking enemies defeated via `WaveManager.OnEnemyDefeated`, and building final `DungeonRunSummary`. Final level and total XP are sourced directly from `PlayerExperience` without internal duplication.
  - Updated `WaveManager.cs` to publish authoritative `OnEnemyDefeated(EnemyHealth enemy)` event fired exactly once when an actively tracked wave enemy dies, cleanly decoupling kill tracking to `DungeonRunStats`.
  - Updated `PlayerExperience.cs` to track cumulative `TotalXPEarned` incremented once per valid `GainExperience` call. Preserved production API purity with zero test-only reset methods.
  - Updated `ExperiencePickup.cs` with public `TryCollect(PlayerExperience playerExperience)` API reusing existing `isCollected` duplicate guard and immediate destruction flow.
  - Implemented `DungeonCompletionController.cs` orchestration layer on `RunControllers`:
    - Subscribes to `WaveManager.OnDungeonCompleted` and `UpgradeManager.OnUpgradeSelectionClosed`.
    - Owns completion state with strict single-fire execution guard (`hasCompleted = true`).
    - Final-kill XP resolution: automatically collects any remaining active `ExperiencePickup` instances in the scene for the player upon dungeon completion, preventing final enemy XP loss.
    - Deterministic Option B collision priority: if final XP triggers a level-up or pending upgrade choices exist, `UpgradeManager` resolves its queue first while owning pause; once closed (`OnUpgradeSelectionClosed`), `DungeonCompletionController` takes pause ownership (`Time.timeScale = 0f`) and presents results.
    - Zero UI collision: `UpgradeSelectionPanel` and `DungeonCompletePanel` are never visible simultaneously.
    - Single-mechanism pause: avoided redundant component disabling; relies cleanly on unified `Time.timeScale <= 0f` guards across player movement, attack, and enemy AI.
    - Implemented `RestartDungeon()`: resets `Time.timeScale = 1f` immediately before reloading active scene via `SceneManager.LoadScene`.
    - Failsafe cleanup: `OnDestroy` guarantees `Time.timeScale` is not left at 0 if destroyed unexpectedly.
  - Implemented `DungeonCompleteUI.cs` presentation layer on `Canvas`:
    - Modal `DungeonCompletePanel` displaying formatted stats (`Time: MM:SS`, `Enemies Defeated: X`, `Level Reached: X`, `XP Earned: X`).
    - Interactive `Restart Dungeon` button wired to `controller.RestartDungeon()`.
    - Pure presentation layer with zero calculation, combat, or wave logic.
  - Configured `Dungeon_Prototype.unity` with `RunControllers` and `Canvas/DungeonCompletePanel`. Clean scene verified with zero verifiers attached.
  - Automated Play Mode verification suite (`Milestone7_1_Verifier.cs`) ran and passed all 28 checks with 0 errors and 0 runtime exceptions.
- **Milestone 8.1 — Character Definition & Architecture**:
  - Implemented data-driven `CharacterDefinition.cs` ScriptableObject in `Assets/Scripts/Characters/` with minimal identity fields (`id`, `displayName`, `description`, `characterPrefab`). Strictly decoupled from weapon-specific math and runtime stat stacking.
  - Created prototype asset `Assets/ScriptableObjects/Characters/Character_Warrior.asset` referencing `Warrior.prefab`. Deferred Archer/Gunner definitions and `CharacterRoster` catalog to avoid incomplete/dead assets.
  - Implemented lightweight `PlayableCharacter.cs` root component in `Assets/Scripts/Characters/`:
    - Functions as authoritative playable character identity anchor and exposes its `CharacterDefinition`.
    - Maintained strictly minimal: zero combat calculations, movement logic, health tracking, XP math, upgrade logic, or monolithic manager responsibilities.
  - Prefab Migration (`Player.prefab` -> `Warrior.prefab`):
    - Cleanly renamed `Assets/Prefabs/Characters/Player.prefab` to `Warrior.prefab` while strictly preserving the Unity asset GUID (`6f312a6b5127de248b5f102e81070d89`).
    - Renamed prefab root GameObject to `Warrior`.
    - Retained `Tag = "Player"` as generic runtime identity used for enemy targeting and camera tracking across future character classes.
    - Attached `PlayableCharacter` component to `Warrior.prefab` root, bound to `Character_Warrior.asset`.
  - Base Stat & Combat Ownership:
    - Authoritative gameplay base values remain directly on character prefab components (`PlayerMovement.moveSpeed = 6`, `PlayerHealth.maxHealth = 100`, `MeleeWeapon.damage = 25`, `MeleeWeapon.attackCooldown = 0.5`), preventing weapon-specific parameters from polluting generic character definitions.
    - Preserved existing Warrior sword combat (`PlayerAttack` -> `MeleeWeapon`) without introducing premature universal weapon abstractions.
    - Confirmed universal compatibility of `PlayerStats.cs` runtime modifiers (`DamageMultiplier`, `AttackSpeedMultiplier`, `MovementSpeedMultiplier`) and `UpgradeManager` for future Archer/Gunner ranged combat without character-type branching.
  - Scene Reference & Future Spawning Architecture:
    - Retained pre-placed `Warrior.prefab` instance in `Assets/Scenes/Dungeons/Dungeon_Prototype.unity` with direct scene wiring.
    - **CRITICAL ARCHITECTURAL DECISION**: Automatic discovery (`GameObject.FindWithTag("Player")` / `FindFirstObjectByType<T>()`) is an intermediate fallback for the pre-placed Warrior and is **NOT** the final runtime-spawn architecture.
    - Milestone 8.2 will introduce `PlayerSpawner`, which will instantiate the selected `CharacterDefinition.characterPrefab` and perform **explicit runtime binding** of player-dependent scene systems (`CameraFollow`, `WaveManager`, `UpgradeManager`, `PlayerExperienceUI`, `DungeonCompletionController`, `DungeonRunStats`) to avoid lifecycle/race-condition pitfalls.
  - Repaired stale path references across all 6 legacy setup utilities (`Milestone1_1_Setup.cs` through `Milestone6_1_Setup.cs`) to point to `Warrior.prefab`.
  - Automated Play Mode verification suite (`Milestone8_1_Verifier.cs`) ran and passed all 41 checks with 0 errors and 0 runtime exceptions.
- **Milestone 8.2 — Runtime Player Spawning & Explicit Binding**:
  - Implemented data-driven, single-responsibility `PlayerSpawner.cs` component in `Assets/Scripts/Characters/`:
    - Instantiates selected `CharacterDefinition.characterPrefab` at runtime at an explicit `PlayerSpawnPoint` Transform.
    - Exposes authoritative `PlayableCharacter ActiveCharacter` and decoupled `event Action<PlayableCharacter> OnPlayerSpawned`.
    - Strict spawn failure handling: logs explicit errors and refuses to spawn if `CharacterDefinition`, character prefab, or `PlayerSpawnPoint` is missing (guaranteed zero silent Vector3.zero fallbacks or broken player instances).
    - Single-spawn guard: rejects duplicate spawn attempts without creating redundant player instances.
    - Zero combat, wave, UI, or camera logic in spawner.
  - Implemented **Explicit Runtime Binding** architecture across all player-dependent scene systems:
    - Robust catch-up subscription pattern supporting both lifecycle orders:
      - Consumer subscribes before player spawns: binds via `OnPlayerSpawned` event callback.
      - Player spawns before consumer enables: binds immediately via `ActiveCharacter` check in `OnEnable()`.
      - Clean unsubscription in `OnDisable()` preventing memory leaks.
      - Repeated binding calls safely tolerate duplicate invocations without duplicating event delegates.
    - `CameraFollow.cs`: exposes `BindTarget(Transform newTarget)` and `SetPlayerSpawner(PlayerSpawner spawner)`. Snaps and follows runtime Warrior.
    - `WaveManager.cs`: exposes `BindPlayer(Transform target)` and `SetPlayerSpawner(PlayerSpawner spawner)`. Decoupled reference binding from wave initiation (`BindPlayer()` does NOT start waves).
    - `UpgradeManager.cs`: exposes `BindPlayer(PlayerExperience exp, PlayerStats stats)` and `SetPlayerSpawner(PlayerSpawner spawner)`. Safely rebinds to runtime components without character-type branching.
    - `PlayerExperienceUI.cs`: exposes `Bind(PlayerExperience exp)` and `SetPlayerSpawner(PlayerSpawner spawner)`. Rebinds and immediately refreshes Level 1 (0 / 100 XP) display.
    - `DungeonCompletionController.cs`: exposes `BindPlayer(PlayerExperience exp)` and `SetPlayerSpawner(PlayerSpawner spawner)`. Programmatically resolves pending pickups and final stats from runtime player.
    - `DungeonRunStats.cs`: decoupled from `PlayerSpawner`. Tracks unpaused elapsed gameplay time strictly starting from `WaveManager.OnDungeonStarted`, and kill tracking from `WaveManager.OnEnemyDefeated`.
    - `UpgradeSelectionUI` and `DungeonCompleteUI`: remain strictly player-independent.
  - Lifecycle Architecture Separation (Spawn != Bind != BeginDungeon):
    - Scene Awake: `PlayerSpawner` instantiates `Warrior.prefab` at `PlayerSpawnPoint`, sets `ActiveCharacter`, emits `OnPlayerSpawned`.
    - OnEnable/Awake: Consumers bind references.
    - Scene Start: `WaveManager` validates player target and triggers `BeginDungeon()`, firing `OnDungeonStarted` exactly once, beginning Wave 1.
    - Binding callbacks (`HandlePlayerSpawned`, `BindPlayer`) have zero side effects and never start gameplay.
  - Production Scene Migration:
    - Updated `Assets/Scenes/Dungeons/Dungeon_Prototype.unity` to contain zero pre-placed `PlayableCharacter` or `Warrior.prefab` instances on disk.
    - Added `PlayerSpawnPoint` at (0, 0, 0) and `PlayerSpawner` configured with `Character_Warrior.asset`.
    - Configured all scene-level managers with `PlayerSpawner` references.
    - Preserved restart flow: reloading scene naturally spawns a fresh Warrior at Level 1 with 0 XP and neutral stats.
    - Verified scene cleanliness: zero verifiers or test-only components saved on disk.
  - Automated Play Mode verification suite (`Milestone8_2_Verifier.cs`) ran and passed all 45 checks with 0 errors and 0 runtime exceptions.
- **Milestone 8.3 — Archer Combat Prototype (Ranged Combat & Polymorphic Attacks)**:
  - Primary Attack Abstraction:
    - Defined minimal `IPrimaryAttack.cs` interface in `Assets/Scripts/Weapons/` with `bool TryAttack();` contract.
    - Implemented `IPrimaryAttack` natively across both `MeleeWeapon.cs` (Warrior) and `BowWeapon.cs` (Archer).
    - Preserved `MeleeWeapon.BaseDamage => damage` and backward compatibility.
  - PlayerAttack Input-Forwarding Architecture:
    - Refactored `PlayerAttack.cs` to coordinate primary attacks polymorphically through `IPrimaryAttack`.
    - Preserved `Warrior.prefab` serialization via `[FormerlySerializedAs("equippedWeapon")] [SerializeField] private MonoBehaviour primaryWeapon;`.
    - Pure input forwarding: delegates `TryAttack()` directly to `primaryAttack.TryAttack()`, with zero character branching, zero weapon inventories, zero weapon switching, and zero damage math.
    - Retained backward-compatible `EquippedWeapon` getter and setters `SetPrimaryWeapon(IPrimaryAttack)` / `SetEquippedWeapon(MeleeWeapon)` for legacy tooling.
  - BowWeapon Implementation:
    - Implemented `BowWeapon.cs` component in `Assets/Scripts/Weapons/` implementing `IPrimaryAttack`.
    - Prototype weapon values: `damage = 20f`, `attackCooldown = 0.6f`, `projectileSpeed = 18f`, `projectileLifetime = 2.0f`.
    - Integrated with `PlayerStats` multipliers (`EffectiveDamage = damage * DamageMultiplier`, `EffectiveAttackCooldown = attackCooldown / AttackSpeedMultiplier`).
    - Enforced cooldown timing and pause guards (`Time.timeScale <= 0f`).
    - Single-fire instantiation: instantiates exactly one `ArrowProjectile` at child `ProjectileSpawnPoint` aligned with character facing direction. Emits `OnAttack` event.
  - ArrowProjectile Single Authoritative Collision Architecture:
    - Implemented `ArrowProjectile.cs` component in `Assets/Scripts/Weapons/`.
    - Strictly **one authoritative hit-resolution path** via `FixedUpdate()` physics sweep using `Physics.SphereCastAll(..., sweepRadius, travelDirection, travelDistance, collisionLayers, QueryTriggerInteraction.Ignore)`.
    - Trigger-ignore behavior: `QueryTriggerInteraction.Ignore` guarantees trigger volumes (e.g. `ExperiencePickup` collectibles) do not block, explode, or take damage from arrows.
    - Hierarchy-safe owner immunity: ignores hits against `ownerRoot` and any of its children (`hitTransform == ownerRoot || hitTransform.IsChildOf(ownerRoot)`), guaranteeing complete self-damage immunity regardless of collider configuration.
    - Single-hit guard: `hitResolved = true` flag ensures a single arrow impacts exactly one `IDamageable` target or solid wall without multi-hit penetrations.
    - Obstacle destruction: solid walls (`BoxCollider` / `MeshCollider` with `isTrigger = false`) trigger immediate arrow destruction.
    - Pause behavior: when `Time.timeScale <= 0f`, projectile displacement is frozen, lifetime timer does not advance, and attacks cannot be initiated.
    - Auto-destruction: destroys itself cleanly when elapsed scaled lifetime exceeds `lifetime = 2.0s`.
    - Purity: zero competing `OnTriggerEnter` or `OnCollisionEnter` damage paths; zero test-only APIs.
  - Archer Assets & Prefab:
    - Created `Character_Archer.asset` under `Assets/ScriptableObjects/Characters/` (`id = "archer"`, `displayName = "Archer"`, description, referencing `Archer.prefab`).
    - Created `Arrow.prefab` at `Assets/Prefabs/Weapons/Arrow.prefab` configured with `ArrowProjectile` (`sweepRadius = 0.15f`), trigger `CapsuleCollider`, kinematic `Rigidbody` (`isKinematic = true`, `useGravity = false`), and placeholder elongated cyan cylinder visual.
    - Created `Archer.prefab` at `Assets/Prefabs/Characters/Archer.prefab`:
      - Shares identical player systems: `Tag = "Player"`, `CharacterController` (height 2.0, radius 0.5), `PlayerMovement` (moveSpeed = 6.5), `PlayerAim`, `PlayerHealth` (maxHealth = 100), `PlayerStats`, `PlayerExperience` (baseRequiredXP = 100), `PlayerAttack`, and `PlayableCharacter` referencing `Character_Archer.asset`.
      - Configured with `BowWeapon` (`damage = 20`, `attackCooldown = 0.6s`, `projectileSpeed = 18m/s`, `projectileLifetime = 2.0s`, `arrowPrefab = Arrow.prefab`).
      - Visual hierarchy: placeholder capsule body, `FacingIndicator`, `WeaponAnchor`, `BowVisual` placeholder, and `ProjectileSpawnPoint` at `(0.35, 1.0, 0.55)`.
      - Does NOT contain `MeleeWeapon`.
  - Universal Stat Scaling:
    - Confirmed universal `PlayerStats` compatibility without character branching:
      - Damage +20%: Bow effective damage scales 20 -> 24.
      - Attack Speed +15%: Bow effective cooldown scales 0.6s -> 0.5217s.
      - Movement Speed +10%: Archer movement speed scales 6.5 -> 7.15.
  - Spawner & Explicit Scene Binding Compatibility:
    - `PlayerSpawner.Spawn(Character_Archer)` cleanly instantiates Archer at `PlayerSpawnPoint`.
    - All scene systems (`CameraFollow`, `WaveManager`, `UpgradeManager`, `PlayerExperienceUI`, `DungeonCompletionController`) bind explicitly and dynamically without modification.
    - Zombie pursue and damage Archer via `IDamageable.TakeDamage()`.
    - Archer fires arrows, damages Zombie (`50 -> 30 HP`), and kills award XP drops.
  - Production Scene Cleanliness & Non-Regression:
    - `Dungeon_Prototype.unity` on disk remains 100% untouched and defaulted to `Character_Warrior.asset`, with zero pre-placed players and zero attached verifiers.
    - Warrior vertical slice remains regression-free: spawns and attacks with `MeleeWeapon` (Check 68 passed).
  - Check 33 Verifier Timing Diagnosis & Resolution:
    - Check 33 initially failed because the verifier only yielded 2 FixedUpdate frames (0.04s) for a 30 m/s arrow starting at z=0, leaving the arrow at z=1.2m when asserting z > 2.2m.
    - Diagnosed and proved to be an assertion timing bug in the verifier, NOT an active production bug (production `ArrowProjectile` was alive and correctly ignoring the trigger).
    - Isolated deterministic test implemented: validates trigger passthrough, zero damage applied to trigger (`HitCount == 0`, `TotalDamage == 0`), subsequent solid wall collision, and immediate arrow destruction. Confirmed passing in targeted run and full suite.
  - Verification Results:
    - Automated Play Mode verification suite (`Milestone8_3_Verifier.cs`) ran and passed all 68 checks with 0 errors and 0 runtime exceptions.
  - Safe Manual Playtesting Helper:
    - Created editor-only `Milestone8_3_ManualPlayHelper.cs` under `Assets/Editor/` with menu items to playtest Archer or Warrior in-memory, automatically restoring `Character_Warrior` default upon exiting Play Mode without dirtying the scene on disk.

- **Milestone 8.4 — Gunner Combat Prototype (Hitscan Firearm & Occlusion Ordering)**:
  - RifleWeapon Implementation:
    - Implemented `RifleWeapon.cs` component in `Assets/Scripts/Weapons/` implementing `IPrimaryAttack`.
    - Single authoritative hitscan sweep path using `Physics.SphereCastAll` (radius `0.1m`, range `25m`, `QueryTriggerInteraction.Ignore`).
    - Deterministic ascending distance ordering: hits sorted via `System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance))`.
    - Strict occlusion ordering: owner root and descendants safely ignored, trigger volumes ignored, first valid solid impact resolves.
    - Solid wall blocks shot; targets behind wall receive zero damage; near target absorbs shot preventing penetration to far target (strictly zero bullet penetration).
    - Prototype weapon baseline: `damage = 10f`, `attackCooldown = 0.18f`, `range = 25f`, `castRadius = 0.1f`.
    - Integrated with `PlayerStats` multipliers (`EffectiveDamage = damage * DamageMultiplier`, `EffectiveAttackCooldown = attackCooldown / AttackSpeedMultiplier`).
    - Enforced cooldown timing and pause guards (`Time.timeScale <= 0f`).
    - Emits clean `OnAttack` event.
  - Gunner Assets & Prefab:
    - Created `Character_Gunner.asset` under `Assets/ScriptableObjects/Characters/` (`id = "gunner"`, `displayName = "Gunner"`, description, referencing `Gunner.prefab`).
    - Created `Gunner.prefab` at `Assets/Prefabs/Characters/Gunner.prefab`:
      - Shares identical player systems: `Tag = "Player"`, `CharacterController` (height 2.0, radius 0.5), `PlayerMovement` (moveSpeed = 6.0), `PlayerAim`, `PlayerHealth` (maxHealth = 100), `PlayerStats`, `PlayerExperience` (baseRequiredXP = 100), `PlayerAttack`, and `PlayableCharacter` referencing `Character_Gunner.asset`.
      - Configured with `RifleWeapon` (`damage = 10`, `attackCooldown = 0.18s`, `range = 25m`, `castRadius = 0.1m`, `targetLayers = ~0`).
      - Visual hierarchy: placeholder capsule body, `FacingIndicator`, `WeaponAnchor`, `RifleVisual` placeholder, and `MuzzlePoint` child at `(0, 0, 0.45)`.
      - Does NOT contain `MeleeWeapon` or `BowWeapon`.
  - Universal Stat Scaling:
    - Universal `PlayerStats` compatibility verified on Gunner:
      - Damage +20%: Rifle effective damage scales 10 -> 12.
      - Attack Speed +15%: Rifle effective cooldown scales 0.18s -> 0.1565s.
      - Movement Speed +10%: Gunner movement speed scales 6.0 -> 6.6.
  - Spawner & Explicit Scene Binding:
    - `PlayerSpawner.Spawn(Character_Gunner)` cleanly instantiates Gunner at `PlayerSpawnPoint`.
    - All scene systems (`CameraFollow`, `WaveManager`, `UpgradeManager`, `PlayerExperienceUI`, `DungeonCompletionController`) bind explicitly and dynamically without modification.
    - Zombie pursues and damages Gunner via `IDamageable.TakeDamage()`.
    - Gunner fires rifle shots, damages Zombie (`50 -> 40 -> 30 -> 20 -> 10 -> 0 HP`), and kills drop XP.
  - Safe Manual Playtesting Helper:
    - Created dedicated editor-only `Milestone8_4_ManualPlayHelper.cs` under `Assets/Editor/` with menu item to playtest Gunner in-memory, automatically restoring `Character_Warrior` default upon exiting Play Mode without dirtying the scene on disk.
  - Production Scene Cleanliness & Non-Regression:
    - `Dungeon_Prototype.unity` on disk remains 100% untouched and defaulted to `Character_Warrior.asset`, with zero pre-placed players and zero attached verifiers.
    - Warrior vertical slice remains regression-free: spawns and attacks with `MeleeWeapon` (Check 41 passed).
    - Archer ranged combat remains regression-free: spawns and fires arrows with `BowWeapon` (Check 42 passed; full 68-check regression suite passed).
  - Verification Results:
    - Automated Play Mode verification suite (`Milestone8_4_Verifier.cs`) ran and passed all 44 checks with 0 errors and 0 runtime exceptions.

---

# Next Task

**Milestone 8.5 — Character System (Character Selection UI & Integration)**

Tasks for Milestone 8.5:
1. Implement Character Selection UI / pre-run character picker.
2. Implement character roster catalog ScriptableObject.
3. Integrate selected character into PlayerSpawner / dungeon entry flow.
4. Verify transition between Character Selection and Dungeon_Prototype.

Deferred Work (Post-Milestone 8):
- Full-auto / held-fire input semantics — evaluate after Gunner manual gameplay test.
- Generic projectile base class abstraction (evaluated and confirmed unjustified; Gunner uses hitscan, so only 1 projectile weapon exists in project).
- 3D models, character animations, audio, and particle VFX
- Corpse cleanup / fading system
- World Map & multi-dungeon progression (Phase 9)
- Permanent skill trees & save/load (Phase 10)

Do NOT start:
- Character Selection UI until instructed
- World Map / multi-dungeon progression (Phase 9)
- Permanent skill trees (Phase 10)

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
- Weapon & Attack Architecture: Polymorphic primary attack via minimal `IPrimaryAttack.cs` (`bool TryAttack()`). `PlayerAttack.cs` coordinates input forwarding with zero weapon management, inventory, or character branching. Concrete implementations: `MeleeWeapon.cs` (Warrior forward arc overlap query), `BowWeapon.cs` (Archer arrow projectile sweep), and `RifleWeapon.cs` (Gunner hitscan distance-sorted query).
- Projectile Architecture: Single authoritative collision sweep via `ArrowProjectile.cs` using `FixedUpdate()` `Physics.SphereCastAll(..., QueryTriggerInteraction.Ignore)`. Enforces trigger immunity, hierarchy-safe owner self-damage immunity, single-hit guard, solid obstacle destruction, and scaled-time pause freezing. Zero competing `OnTriggerEnter` or `OnCollisionEnter` damage logic.
- Hitscan Firearm Architecture: Single authoritative hitscan sweep via `RifleWeapon.cs` using `Physics.SphereCastAll(..., castRadius, forward, range, targetLayers, QueryTriggerInteraction.Ignore)`. Hits sorted deterministically by ascending distance (`hit.distance`). First valid solid non-owner hit resolves. Hierarchy-safe owner immunity (`hitTransform == ownerRoot || hitTransform.IsChildOf(ownerRoot)`), trigger volumes ignored. Obstacle occlusion: solid wall stops shot; near target absorbs shot preventing penetration to far target (strictly zero bullet penetration). Single attack event per shot.
- Health Architecture: `PlayerHealth.cs` and `EnemyHealth.cs` both implement `IDamageable` independently with clamped health and single-fire death events
- Enemy Architecture: Modular components (`EnemyHealth`, `EnemyMovement`, `EnemyAttack`) communicating via clean C# events without monolithic controllers
- Wave Architecture: Data-driven `WaveDefinition` ScriptableObjects sequenced by `WaveManager.cs` using round-robin perimeter spawn points
- Enemy Tracking: Authoritative `HashSet<EnemyHealth>` with clean event lifecycle management and zero scene-wide polling
- Character Spawning & Runtime Binding Architecture:
  - `PlayerSpawner.cs` is the authoritative runtime character factory in dungeon scenes.
  - Spawns configured `CharacterDefinition.characterPrefab` at runtime at `PlayerSpawnPoint` Transform during scene `Awake()`.
  - Exposes `PlayableCharacter ActiveCharacter` and emits `OnPlayerSpawned`.
  - Strict lifecycle separation: Spawn != Bind != BeginDungeon. Spawning and binding have zero side-effects on gameplay initiation.
  - Explicit consumer binding (`CameraFollow`, `WaveManager`, `UpgradeManager`, `PlayerExperienceUI`, `DungeonCompletionController`) uses robust catch-up subscription pattern supporting both pre-spawn subscription and post-spawn catch-up.
  - `WaveManager.BeginDungeon()` requires valid runtime player target and begins waves strictly during `Start()` phase, emitting `OnDungeonStarted` exactly once.
  - `DungeonRunStats.cs` timing begins from `WaveManager.OnDungeonStarted`, and kill tracking from `WaveManager.OnEnemyDefeated`.
  - Zero pre-placed player characters in dungeon scenes on disk.
- Experience Architecture:
  - `PlayerExperience` owns XP accumulation and level progression without UI or stat modifications; tracks cumulative `TotalXPEarned`
  - `ExperienceReward` on enemy prefabs listens to `EnemyHealth.OnDied` and drops `ExperiencePickup` without coupling `EnemyHealth` or `WaveManager` to XP logic
  - `ExperiencePickup` physical collectible with trigger volume and kinematic `Rigidbody`; exposes `TryCollect(PlayerExperience)` for safe trigger or programmatic collection
  - `PlayerExperienceUI` strictly event-driven; updates Slider and TextMeshProUGUI on `OnExperienceChanged` and `OnLevelUp`
- Stat & Upgrade Architecture:
  - `PlayerStats.cs` component on `Player.prefab` owns runtime temporary stat multipliers (`DamageMultiplier`, `AttackSpeedMultiplier`, `MovementSpeedMultiplier`) with additive percentage stacking rules (`1.0 + sum(bonus)`). Decoupled from `UpgradeDefinition` and UI.
  - Data-driven `UpgradeDefinition` ScriptableObjects mapped by `UpgradeType` enum to dedicated `PlayerStats` methods.
  - `UpgradeManager` listens to `PlayerExperience.OnLevelUp`, queues pending selections (`pendingChoicesCount`), pauses time (`Time.timeScale = 0f`), and only unpauses when the queue is fully resolved.
- Dungeon Completion Architecture:
  - `DungeonRunStats.cs` tracks active unpaused run time and listens to `WaveManager.OnEnemyDefeated` for kill counts.
  - `DungeonRunSummary` immutable value structure encapsulating run metrics.
  - `DungeonCompletionController.cs` orchestrates completion lifecycle: resolves remaining XP pickups, waits for any pending upgrade selections (Option B), establishes pause ownership (`Time.timeScale = 0f`), and fires `OnDungeonCompleted(summary)`.
  - Pause Invariants: Unified single-mechanism pause via `Time.timeScale <= 0f`, respected by PlayerMovement, PlayerAttack, MeleeWeapon, EnemyMovement, and EnemyAttack.
  - Restart Behavior: `RestartDungeon()` resets `Time.timeScale = 1f` immediately prior to reloading active scene via `SceneManager.LoadScene`.
  - `DungeonCompleteUI.cs` strictly presentation layer displaying stats and handling `Restart Dungeon` button click.
- Input: Unity Input System (`com.unity.inputsystem` 1.20.0) with `InputSystem_Actions.inputactions`
- Single Responsibility: Separate components across Player, Combat, Weapons, Enemies, Waves, Experience, Upgrades, Dungeons, and UI
- Visuals: Primitives/placeholders (Capsules with FacingIndicators, SwordVisual, and Cyan Pickup diamond)
- Git used as checkpoint and handoff system

---

# Known Issues

None.

*(Resolved: During test harness setup, `AddComponent<ExperiencePickup>()` on a naked GameObject threw an error due to `[RequireComponent(typeof(Collider))]` requiring a concrete collider. Resolved by adding `SphereCollider` prior to `ExperiencePickup` in the test setup.)*

---

# Future Maintenance & Technical Debt

- **World Map & Main Menu Navigation Deferred**: Completion currently provides a `Restart Dungeon` button reloading the prototype scene. Navigation to World Map / Main Menu is deferred to Phase 9.
- **Randomized Upgrade Pool & Rarity Deferred**: The prototype presents a fixed pool of 3 upgrades (Damage, Attack Speed, Movement Speed). Weighted random rolling, upgrade rarity, rerolls, and bans are deferred to future upgrade polish milestones.
- **Permanent Meta-Progression Deferred**: Skill tree and permanent stat upgrades are deferred to Phase 10.
- **DungeonDefinition Deferred**: `DungeonDefinition` remains deferred until Phase 9 / multi-dungeon progression actually requires dungeon-level metadata.
- **Corpse Cleanup and Object Pooling Deferred**: Defeated enemy corpses remain in scene as non-obstructing entities. Object pooling and corpse cleanup will be introduced in later milestones.
- **Direct Pursuit vs Pathfinding**: Current `EnemyMovement` uses direct `CharacterController` pursuit. This is sufficient for the prototype but does not provide full pathfinding around complex dungeon geometry. Re-evaluate NavMesh when dungeon layouts require real obstacle navigation.
- **Pickup Magnetism Deferred**: Physical touch collection via trigger volume is implemented; magnetic pickup attraction radius is deferred to future polish/upgrade milestones.
- **Verifier Script Reorganization**: Legacy verifier scripts currently located under production script folders (`Assets/Scripts/Player/Milestone1_1_Verifier.cs` and `Assets/Scripts/Player/Milestone1_2_Verifier.cs`) should later be moved into the dedicated test/verification structure (`Assets/Tests/Verification/`).

---

# Testing Status

## Milestone 8.1 Verification

Automated Play Mode verification suite ran and passed all 41 checks in Unity (`playmode_m8_1.log`):
- **Check 1**: CharacterDefinition is a ScriptableObject (PASSED).
- **Check 2**: Character_Warrior.asset exists and loaded successfully (PASSED).
- **Check 3**: Warrior definition valid: ID='warrior', DisplayName='Warrior' (PASSED).
- **Check 4**: Warrior definition points to the Warrior character prefab (PASSED).
- **Check 5**: Warrior.prefab exists at Assets/Prefabs/Characters/Warrior.prefab (PASSED).
- **Check 6**: Old Player.prefab no longer exists; cleanly migrated to Warrior.prefab (PASSED).
- **Check 7**: Warrior.prefab preserved the existing Unity GUID (`6f312a6b5127de248b5f102e81070d89`) (PASSED).
- **Check 8**: Warrior GameObject root is named 'Warrior' (PASSED).
- **Check 9**: Warrior root retains Tag = 'Player' (PASSED).
- **Check 10**: Warrior GameObject has PlayableCharacter component attached (PASSED).
- **Check 11**: Dungeon_Prototype contains exactly one PlayableCharacter (Warrior) (PASSED).
- **Check 12**: Scene cleanliness architecture verified; cleanup routine decouples test harness and no legacy verifiers exist (PASSED).
- **Check 13**: Warrior base MoveSpeed remains 6.0 (current: 6) (PASSED).
- **Check 14**: Warrior MaxHealth remains 100.0 (current: 100) (PASSED).
- **Check 15**: Warrior sword base Damage remains 25.0 (current: 25) (PASSED).
- **Check 16**: Warrior sword base AttackCooldown remains 0.5s (current: 0.5) (PASSED).
- **Check 17**: PlayerMovement steps without error and respects physics bounds (PASSED).
- **Check 18**: PlayerAim updates yaw rotation while strictly preserving horizontal constraints (PASSED).
- **Check 19**: PlayerHealth receives damage, clamps value, and raises health change events (PASSED).
- **Check 20**: MeleeWeapon attack executed and OnAttack event dispatched successfully (PASSED).
- **Check 21**: Enemy health and death pipeline operates correctly (PASSED).
- **Check 22**: ExperiencePickup TryCollect correctly awards XP and destroys itself (PASSED).
- **Check 23**: PlayerExperience advanced level (1 -> 2) and fired OnLevelUp (PASSED).
- **Check 24**: PlayerExperienceUI elements configured and bound (PASSED).
- **Check 25**: UpgradeManager responsive to level up; pending queue supported (PASSED).
- **Check 26**: Damage bonus applied (+20%): multiplier is 1.20 (PASSED).
- **Check 27**: Attack speed bonus applied (+15%): multiplier is 1.15 (PASSED).
- **Check 28**: Movement speed bonus applied (+10%): EffectiveMoveSpeed is 6.6 (PASSED).
- **Check 29**: WaveManager playerTarget points directly to the active Warrior (PASSED).
- **Check 30**: CameraFollow target points directly to the active Warrior (PASSED).
- **Check 31**: DungeonCompletionController present and orchestrates completion lifecycle (PASSED).
- **Check 32**: DungeonRunStats packages summary accurately: Time='00:00', Enemies=0 (PASSED).
- **Check 33**: RestartDungeon method is exposed and bound to completion controller (PASSED).
- **Check 34**: UpgradeManager is completely character-agnostic with zero character-type branching (PASSED).
- **Check 35**: UpgradeSelectionUI is completely character-agnostic with zero character-type branching (PASSED).
- **Check 36**: CharacterDefinition contains no weapon-specific combat calculation fields (PASSED).
- **Check 37**: CharacterDefinition does not duplicate gameplay stat ownership (moveSpeed/maxHealth) (PASSED).
- **Check 38**: PlayableCharacter is a lean identity root component with zero calculation bloat (PASSED).
- **Check 39**: Zero stale Player.prefab path references remain across repository scripts (PASSED).
- **Check 40**: Unity compiled with 0 errors (PASSED).
- **Check 41**: Play Mode produced 0 runtime exceptions (PASSED).

## Milestone 8.2 Verification

Automated Play Mode verification suite ran and passed all 45 checks in Unity (`playmode_m8_2.log`):
- **Check 1**: Dungeon_Prototype.unity contains zero pre-placed PlayableCharacter instances on disk (PASSED).
- **Check 2**: PlayerSpawner exists in Dungeon_Prototype scene (PASSED).
- **Check 3**: PlayerSpawnPoint Transform exists at (0.00, 0.00, 0.00) (PASSED).
- **Check 4**: PlayerSpawner.DefaultCharacter correctly configured: 'Warrior' (id: warrior) (PASSED).
- **Check 5**: Exactly one playable character exists at runtime: Warrior(Clone) (PASSED).
- **Check 6**: Spawned instance possesses PlayableCharacter component (PASSED).
- **Check 7**: Spawned PlayableCharacter references Warrior definition: Warrior (PASSED).
- **Check 8**: Spawn position matches spawn point (horizDist: 0.0000, vertDist: 0.0800 grounded) (PASSED).
- **Check 9**: Spawn rotation matches spawn point (angle: 0.0000) (PASSED).
- **Check 10**: Duplicate Spawn() call safely rejected; returned existing ActiveCharacter without spawning a second player (PASSED).
- **Check 11**: Missing spawn point produces NO player instance (PASSED).
- **Check 12**: Missing spawn point does NOT silently fall back to Vector3.zero (PASSED).
- **Check 13**: Null CharacterDefinition explicitly rejected without spawning (PASSED).
- **Check 14**: CharacterDefinition with null prefab explicitly rejected without spawning (PASSED).
- **Check 15**: Prefab without PlayableCharacter component is rejected and cleaned up (PASSED).
- **Check 16**: CameraFollow.Target explicitly bound to runtime Warrior transform (PASSED).
- **Check 17**: CameraFollow tracks runtime Warrior translation in world space (PASSED).
- **Check 18**: WaveManager.PlayerTarget explicitly bound to runtime Warrior transform (PASSED).
- **Check 19**: WaveManager.HasDungeonStarted is true; dungeon run successfully started (PASSED).
- **Check 20**: WaveManager.BeginDungeon() does not begin waves if player target is null (PASSED).
- **Check 21**: WaveManager.BeginDungeon() is single-fire; duplicate calls are safely ignored (PASSED).
- **Check 22**: Spawned Zombie 'Zombie(Clone)' pursues runtime Warrior target (PASSED).
- **Check 23**: PlayerHealth receives damage and clamps value (hp: 100 -> 90) (PASSED).
- **Check 24**: Runtime Warrior sword attack damages EnemyHealth (50 -> 25) and MeleeWeapon is functional (PASSED).
- **Check 25**: UpgradeManager explicitly bound to runtime PlayerExperience and PlayerStats (PASSED).
- **Check 26**: PlayerExperienceUI explicitly bound to runtime PlayerExperience (PASSED).
- **Check 27**: PlayerExperienceUI displays initial Level 1 display: 'Level 1 (0 / 100 XP)' (PASSED).
- **Check 28**: ExperiencePickup successfully collected by runtime Warrior (0 -> 20 XP) (PASSED).
- **Check 29**: Level-up triggered (Level 2) and UpgradeManager opened choice panel (PASSED).
- **Check 30**: Damage upgrade applied additively (+20%): 1.20 (PASSED).
- **Check 31**: Attack Speed upgrade applied additively (+15%): 1.15 (PASSED).
- **Check 32**: Movement Speed upgrade applied additively (+10%): 1.10 (PASSED).
- **Check 33**: DungeonRunStats is tracking elapsed gameplay time (0.15s) from dungeon start (PASSED).
- **Check 34**: DungeonCompletionController explicitly bound to runtime PlayerExperience (PASSED).
- **Check 35**: Dungeon completion deterministically resolved remaining active XP pickups (PASSED).
- **Check 36**: DungeonComplete state reached; FinalSummary generated: DungeonRunSummary (PASSED).
- **Check 37**: Catch-up subscription pattern: consumer subscribing AFTER spawn binds immediately via ActiveCharacter (PASSED).
- **Check 38**: Pre-spawn subscription pattern: consumer subscribing BEFORE spawn binds when OnPlayerSpawned fires (PASSED).
- **Check 39**: Repeated BindPlayer() calls do not duplicate event subscriptions (choices added: 1) (PASSED).
- **Check 40**: All scene systems bind dynamically via explicit runtime binding without pre-placed dependencies (PASSED).
- **Check 41**: No global GameManager singleton or service locator introduced (PASSED).
- **Check 42**: CharacterDefinition contains zero runtime mutable state or selection flags (PASSED).
- **Check 43**: Dungeon_Prototype.unity contains zero verifiers on disk (PASSED).
- **Check 44**: Unity compiled with 0 errors (PASSED).
- **Check 45**: Play Mode produced 0 runtime exceptions (PASSED).

## Milestone 8.3 Verification

Automated Play Mode verification suite ran and passed all 68 checks in Unity (`playmode_m8_3.log`):
- IPrimaryAttack interface contract validated.
- MeleeWeapon and BowWeapon polymorphic implementations verified.
- PlayerAttack input-forwarding architecture verified.
- BowWeapon cooldown (0.6s), pause blocking, arrow instantiation, and event emission verified.
- ArrowProjectile travel, pause freeze, lifetime auto-destruction, owner immunity, trigger passthrough, IDamageable hit detection, wall destruction, and single-hit guard verified.
- Character_Archer.asset and Archer.prefab hierarchy, stats, and components verified.
- Spawner instantiation, explicit dynamic binding of all scene systems, universal stat upgrades (+20% dmg, +15% atk spd, +10% move speed), zombie combat, XP drops, and dungeon completion verified.
- Warrior vertical slice non-regression verified (68/68 passed).

## Milestone 8.4 Verification

Automated Play Mode verification suite ran and passed all 44 checks in Unity (`playmode_m8_4.log`):
- **Check 1**: IPrimaryAttack interface exists with bool TryAttack() contract (PASSED).
- **Check 2**: Warrior prefab uses MeleeWeapon implementing IPrimaryAttack (PASSED).
- **Check 3**: Archer prefab uses BowWeapon implementing IPrimaryAttack (PASSED).
- **Check 4**: RifleWeapon implements IPrimaryAttack (PASSED).
- **Check 5**: RifleWeapon.BaseDamage returns 10 (PASSED).
- **Check 6**: RifleWeapon.AttackCooldown returns 0.18s (PASSED).
- **Check 7**: RifleWeapon.Range returns 25m (PASSED).
- **Check 8**: Rifle EffectiveDamage correctly scaled +20% (10 -> 12) (PASSED).
- **Check 9**: Rifle EffectiveAttackCooldown correctly scaled +15% (0.18 / 1.15 ≈ 0.1565s) (PASSED).
- **Check 10**: PlayerMovement EffectiveMoveSpeed correctly scaled +10% (6.0 -> 6.6) (PASSED).
- **Check 11**: Pause strictly blocks Rifle firing (Time.timeScale <= 0f) (PASSED).
- **Check 12**: Attack cooldown strictly enforced (rapid shot blocked, shot after 0.18s succeeds) (PASSED).
- **Check 13**: Exactly one OnAttack event fired per valid attack (PASSED).
- **Check 14**: Aim direction respected: aligned target hit, off-axis target untouched (PASSED).
- **Check 15**: Owner hierarchy safely ignored even with overlapping collider; target ahead receives damage (PASSED).
- **Check 16**: Trigger volumes strictly ignored; target beyond trigger receives hit (PASSED).
- **Check 17**: Occlusion Case A verified: Owner ignored, Trigger ignored, Zombie damaged (10), Wall behind irrelevant (PASSED).
- **Check 18**: Occlusion Case B verified: Wall stops shot, Zombie behind wall receives ZERO damage (PASSED).
- **Check 19**: Occlusion Case C verified: Near Zombie damaged exactly once (10), Far Zombie receives ZERO damage (strictly no penetration) (PASSED).
- **Check 20**: Range limit enforced: target beyond 25m receives ZERO damage (PASSED).
- **Check 21**: Gunner.prefab exists at Assets/Prefabs/Characters/Gunner.prefab (PASSED).
- **Check 22**: Gunner.prefab Tag is 'Player' (PASSED).
- **Check 23**: Gunner.prefab contains all required components with baseline values (moveSpeed=6.0, hp=100, dmg=10, cd=0.18s) (PASSED).
- **Check 24**: Gunner.prefab does NOT contain MeleeWeapon or BowWeapon (PASSED).
- **Check 25**: Gunner.prefab hierarchy complete: Visual, FacingIndicator, WeaponAnchor, RifleVisual, MuzzlePoint (PASSED).
- **Check 26**: Character_Gunner.asset configured correctly and links to Gunner.prefab (PASSED).
- **Check 27**: CharacterDefinition schema contains zero combat calculation or stat modifier fields (PASSED).
- **Check 28**: PlayerSpawner.Spawn(Character_Gunner) successfully instantiated Gunner (PASSED).
- **Check 29**: CameraFollow target explicitly bound to runtime Gunner transform (PASSED).
- **Check 30**: WaveManager playerTarget explicitly bound to runtime Gunner transform (PASSED).
- **Check 31**: UpgradeManager explicitly bound to runtime Gunner PlayerExperience and PlayerStats (PASSED).
- **Check 32**: PlayerExperienceUI explicitly bound to runtime Gunner PlayerExperience (PASSED).
- **Check 33**: DungeonCompletionController explicitly bound to runtime Gunner PlayerExperience (PASSED).
- **Check 34**: Gunner took damage via IDamageable (100 -> 90) (PASSED).
- **Check 35**: Gunner rifle attacks damaged and killed Zombie (HP reduced to 0) (PASSED).
- **Check 36**: Zombie death spawned ExperiencePickup (PASSED).
- **Check 37**: Gunner collected ExperiencePickup and gained XP (PASSED).
- **Check 38**: Level-up triggered (Level 1 -> 2) and UpgradeManager presented 3 choices (PASSED).
- **Check 39**: Damage upgrade applied to runtime Gunner: EffectiveDamage scaled from 10 to 12 (PASSED).
- **Check 40**: Dungeon completion flow executed and generated valid DungeonRunSummary with Gunner (PASSED).
- **Check 41**: Warrior vertical slice non-regression verified: spawned and executed MeleeWeapon attack (PASSED).
- **Check 42**: Archer combat non-regression verified: spawned and fired BowWeapon arrow (PASSED).
- **Check 43**: Dungeon_Prototype.unity defaultCharacter is Character_Warrior on disk (PASSED).
- **Check 44**: Zero permanent verifier objects saved on disk (PASSED).

Milestone 8.3 Archer regression suite re-run: all 68 checks PASSED (`playmode_m8_3_regression.log`).

---

# Recent Git Checkpoint

```text
Latest verified commit:
eb8f471 feat: add archer ranged combat prototype
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
- Milestone 8.4 Gunner Combat Prototype implementation and Play Mode verification.
- Assets/Scripts/Weapons/RifleWeapon.cs: implemented IPrimaryAttack, Physics.SphereCastAll hitscan query (radius 0.1m, range 25m), ascending distance hit ordering, hierarchy-safe owner immunity, trigger volume passthrough, strict obstacle occlusion (solid wall blocks targets behind it, nearest target prevents penetration), 0.18s cooldown, pause blocking, and universal PlayerStats integration.
- Assets/ScriptableObjects/Characters/Character_Gunner.asset: Gunner character archetype definition (id: "gunner", displayName: "Gunner", referencing Gunner.prefab).
- Assets/Prefabs/Characters/Gunner.prefab: Gunner playable character prefab configured with RifleWeapon, Visual, FacingIndicator, WeaponAnchor, RifleVisual, and MuzzlePoint child, sharing standard player systems (CharacterController, PlayerMovement, PlayerAim, PlayerHealth, PlayerStats, PlayerExperience, PlayerAttack, PlayableCharacter).
- Assets/Editor/Milestone8_4_Setup.cs: automated setup utility and batchmode verification runner.
- Assets/Editor/Milestone8_4_ManualPlayHelper.cs: safe in-memory manual playtest helper with automatic scene revert on exit.
- Assets/Tests/Verification/Milestone8_4_Verifier.cs: 44-check Play Mode verification suite covering contracts, stats, hitscan mechanics, occlusion ordering, prefab integrity, spawner binding, full combat loop, and non-regression.
```

## Changed Files

```text
- Assets/Editor/Milestone8_4_ManualPlayHelper.cs
- Assets/Editor/Milestone8_4_ManualPlayHelper.cs.meta
- Assets/Editor/Milestone8_4_Setup.cs
- Assets/Editor/Milestone8_4_Setup.cs.meta
- Assets/Prefabs/Characters/Gunner.prefab
- Assets/Prefabs/Characters/Gunner.prefab.meta
- Assets/ScriptableObjects/Characters/Character_Gunner.asset
- Assets/ScriptableObjects/Characters/Character_Gunner.asset.meta
- Assets/Scripts/Weapons/RifleWeapon.cs
- Assets/Scripts/Weapons/RifleWeapon.cs.meta
- Assets/Tests/Verification/Milestone8_4_Verifier.cs
- Assets/Tests/Verification/Milestone8_4_Verifier.cs.meta
- PROJECT_STATUS.md
```

## Tested

```text
- 44/44 automated Play Mode verification checks passed with 0 errors and 0 runtime exceptions (playmode_m8_4.log)
- 68/68 automated Play Mode verification checks passed for Milestone 8.3 regression suite (playmode_m8_3_regression.log)
- IPrimaryAttack contract validated on MeleeWeapon, BowWeapon, and RifleWeapon
- RifleWeapon base damage = 10, cooldown = 0.18s, range = 25m, cast radius = 0.1m
- Universal stat upgrades on Gunner: damage +20% (10 -> 12), attack speed +15% (cooldown 0.18 -> 0.1565s), move speed +10% (6.0 -> 6.6)
- Pause guard: firing strictly blocked while Time.timeScale <= 0
- Cooldown enforcement: rapid firing blocked, shot succeeds after cooldown expires
- One attack input produces exactly one OnAttack event
- Aim direction respected: aligned target hit, off-axis target untouched
- Owner hierarchy safely ignored even when cast begins inside owner collider
- Trigger volumes strictly ignored (QueryTriggerInteraction.Ignore)
- Occlusion Case A: Owner ignored, Trigger ignored, Zombie damaged (10), Wall behind irrelevant
- Occlusion Case B: Wall stops shot, Zombie behind wall receives zero damage
- Occlusion Case C: Near Zombie damaged exactly once, Far Zombie receives zero damage (strictly no penetration)
- Range limit: target beyond 25m receives zero damage
- Gunner.prefab: Tag == "Player", CharacterController, PlayerMovement (6.0), PlayerAim, PlayerHealth (100), PlayerStats, PlayerExperience (100), PlayerAttack, PlayableCharacter, RifleWeapon, no MeleeWeapon or BowWeapon
- Visual hierarchy: Visual, FacingIndicator, WeaponAnchor, RifleVisual, MuzzlePoint
- Character_Gunner.asset: metadata valid, references Gunner.prefab, zero leaked combat math fields
- Runtime PlayerSpawner instantiation of Gunner via PlayerSpawner.Spawn(Character_Gunner)
- Dynamic explicit binding of CameraFollow, WaveManager, UpgradeManager, PlayerExperienceUI, and DungeonCompletionController to Gunner
- Zombie attacks Gunner and Gunner takes damage via IDamageable (100 -> 90 HP)
- Gunner fires rifle, damages and kills Zombie (5 shots = 50 HP)
- Zombie death spawns ExperiencePickup, Gunner collects pickup and gains XP
- Gunner level-up presents 3 upgrade choices, Damage upgrade applied successfully
- Dungeon completion flow executes with Gunner and generates valid DungeonRunSummary
- Warrior vertical slice non-regression: spawns and executes MeleeWeapon attack
- Archer combat non-regression: spawns and fires BowWeapon arrow
- Production Dungeon_Prototype.unity on disk retains Character_Warrior default and zero permanent verifiers
```

## Known Issues

```text
None.
```

## Next Task

```text
Milestone 8.5 — Character System (Character Selection UI & Integration)
```

## Latest Verified Commit

```text
eb8f471 feat: add archer ranged combat prototype
```
