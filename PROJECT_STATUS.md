# PROJECT_STATUS.md

> This file is the handoff checkpoint between ChatGPT, Antigravity, Codex, and human development sessions.

Last update: 2026-09-11 — Phase 20 combat-fairness milestone COMPLETE and user manual QA GREEN for normal enemies and Ash Warden geometry. Checkpoints: 20dae97 and 40e27a0. This signs off combat fairness only, not all of Phase 20. Stop and await explicit direction before further polish.

## Historical update log

The entries below retain evidence and instructions from earlier checkpoints; their next-step instructions are superseded by Current Project State.

- D10 Blacksmith verification: Unity compilation succeeded with only the three known legacy TMP CS0618 warnings. One production-scene smoke PASSED: D10 waves/deaths/XP and upgrade resolution, exact L11 52/5767 exit, +3 points, actual return-button transition, Archer Tier II dialogue/reveal/auto-equip, missed Tier I recovery without duplicate IDs, persisted once-only flags, Warrior/Gunner Tier II claim/equip, and Hub reload without replay. Phase12StateSnapshot restored test state. No broad regressions or licensing retries. Next: USER MANUAL QA of the D10 reward presentation and Armory exit/re-entry/app restart; do not push or start Phase 20.
- Phase 19 gameplay, Hollow Castellan, Tier II and the D5 Warrior spacing patch are user manual QA GREEN. The new D10 Blacksmith presentation is the only pending manual QA task; no push and no Phase 20.
- D10 Blacksmith polish: after normal XP, upgrade choices and completion finish, the unchanged Haritaya Dön action routes once to Hub_Armory. The existing BlacksmithIntroSequence presents four editable Turkish lines, the existing fade, and the active hero's Tier II reward with automatic claim/equip. An additive `secondBlacksmithIntroSeen` boolean in save v3 commits with ownership/equipment; older saves default to unseen and can receive the sequence on their next eligible Hub visit. A missed Tier I presentation is safely settled in that same transaction by adding only the active hero's missing Tier I weapon, retaining Tier II equipped, and marking the earlier intro seen. Other heroes retain normal free milestone claims. No combat, XP, economy, stat layers, or carousel changes.
- Phase 19 checkpoint `56d4972` is complete locally and awaits manual QA. Shared D5 boss HUD regression passed 9/9. The requested separate D5 spacing patch is implemented and its focused real-melee smoke passed 7 checks; final feel remains user QA. No push and no Phase 20.
- Phase 19 implemented after Phase 18 manual QA GREEN: Dungeon 10, Hollow Castellan, shared boss HUD identity contract, Tier II claim/equip and campaign-completion acknowledgement. D10 lifecycle smoke passed. A separately requested D5 Warrior-spacing patch is next before final manual QA handoff; do not begin Phase 20 or push.
- Phase 18 implemented after Phase 17 manual QA GREEN. D8 was completed and passed its actual scene lifecycle before D9 was authored and tested. Both focused smoke paths passed; Phase 18 awaits user manual QA. No push; Phase 19 not started.
- Phase 17 implemented after Phase 16 Blacksmith manual QA GREEN (`04bf1a9`). D6 was built and passed a real scene lifecycle before D7 was built. D6 and D7 content/lifecycle smoke checks both passed; new arena gameplay requires manual QA before expanding to Phase 18. No push.
- Phase 16 functional manual QA GREEN at `cb48f63`; Blacksmith narrative refinement now awaits presentation QA. The completed D5 victory screen offers DEMİRCİYE GİT only after the existing XP/upgrade/completion flow finishes. That deliberate Continue action enters Hub_Armory; legacy D5-completed saves receive the intro on their next Hub visit. Four short Turkish lines and a 0.35-second fade lead to a named Tier I reward reveal and automatic equip. One additive global `firstBlacksmithIntroSeen` flag remains in save v3; reward claim, equipped ID and flag commit in one save. Interruption before reward permits retry; after reward the introduction never repeats. Other heroes retain free affinity-scoped claims through normal Blacksmith interaction. Only the selected character shows weapon name/tier and strong emphasis. No combat, XP, stat-layering or encounter changes; no Phase 17 and no push. D5/Tier I and future D10/Tier II are boss milestones, not character levels; D15/Tier III remains concept only.
- Master campaign mission: Phase 15 implemented (2026-09-10). Existing nine-node trees now cost 1/2/2 per branch (15 total); IDs, tiers, effects and prerequisites unchanged. Rewards D1-D10 are 1/1/1/1/2/1/1/2/2/3, first-clear per character. Save v1 migrates to v2 with points and purchases grandfathered, null/duplicate ID lists normalized, reward history retained, and one future legacy D5 claim allowed. Phase15Smoke passed in Unity, including migration/reload idempotency, new purchase costs, isolation and campaign reward total. Old Phase 12-14 economy assertions describe historical balance and are not current economy acceptance tests. Next action: implement Phase 16 Hub_Armory and Tier I weapons, then stop for user manual QA. No push authorized for this mission.
- Phase 14 Final Sign-off (2026-09-10): Phase 14 is COMPLETE. User manual QA is GREEN for the D5 boss encounter, progression and completion flow, World Map carousel and keyboard navigation, Warrior melee slash feedback, and the final minimal boss HUD. Dungeon 5 is `dungeon_5`, The Ashen Sanctum / Kül Mabedi: 1.4x enemy HP, 1.3x enemy damage, three waves, 18 normal enemies plus The Ash Warden / Kül Muhafızı, 19 enemies, and 1,200 XP total. The Ash Warden has 1,200 base HP / 1,680 runtime HP, two phases, no adds, no persistent hazards, and 960 boss XP. D5 enters at Level 7, 7/1139 and exits at Level 8, 68/1709 (3,285 campaign XP); its first-clear permanent reward remains 0. Previous boss lifecycle, campaign, and completion-order checkpoints remain accepted. Final focused evidence: Boss Health Bar 9/9, Warrior Slash Feedback 8/8, World Map Carousel 525 checks, Phase 10 World Map QA 10/10, and Gate 14.3 passed. The final reruns of Phase 12.5 Skill Tree UI, Gate 14.1, and D5 boss lifecycle did not start because Unity batch licensing reported `Connection to channel LicenseClient-Alcor refused`; these are ENVIRONMENT BLOCKED, not production RED or passed suites. Gate 14.3 later ran successfully. Final polish checkpoints: `b38b9a9`, `b30b237`, and `e9ee929`. NEXT: Phase 15 — Permanent Progression Rebalance (planned only; not started).
- Phase 13 Final Sign-off (2026-09-09): Phase 13 is COMPLETE. Gates 13.1–13.5, the 2129/2129 final regression matrix, the D4 Wave 1 -> Wave 2 runtime regression (3/3), and user manual gameplay QA are GREEN. Dungeon 4, The Colonnade / Bölüm 4: Sütunlu Salon, has a 36m x 28m four-pillar arena, six spawn points, four waves (45 enemies / 705 XP), 1.3x HP and 1.2x damage scaling, and a character-scoped 3-point first-clear reward. Campaign reaches Level 7 during D4 (entry Level 6, 61/759; end Level 7, 7/1139). Manual-QA geometry, wave configuration, and Skill Tree overlay issues are resolved. Phase 14 (Dungeon 5 + First Boss) is NEXT.
- Phase 13 final manual-QA UI recovery (2026-09-09): Skill Tree overlay layering is automated GREEN. Opening the panel now moves its full-screen root to the final Canvas sibling position, ensuring dynamically created D4 World Map cards cannot render above it; close returns the unchanged map. Phase 12.5 Skill Tree UI regression passed 128 checks and Phase 10 QA World Map regression passed 10 checks.
- Phase 13 Gate 13.5 (2026-09-09): Full D4 campaign progression is automated GREEN. D4 remains 45 enemies / 705 XP; campaign total is 2085 XP. It enters at Level 6, 61/759 and ends at Level 7, 7/1139 under the production `RoundToInt` curve. First-clear reward remains 3 points, replay is idempotent, and another character retains its reward. Gate verifier and Phase 10.5 campaign regression passed.
- Phase 13 Gate 13.4 (2026-09-09): D4 combat scaling is automated GREEN. Runtime production rounding is verified as Zombie 65 HP/12 damage, Runner 32/7, Tank 195/26, and Ranged 46/10. The Gate verifier and the 43-check enemy combat regression passed with bounded exits. Gate 13.5 is next.
- Phase 13 Gate 13.3 (2026-09-09): Dungeon 4 production encounters are automated GREEN. Four grouped sequential waves use only Zombie, Runner, Tank, and Ranged: 45 enemies / 705 XP exactly. The D4 verifier and the 1497-check Phase 11 mixed-wave/projectile regression passed with bounded Unity exits. Gate 13.4 is next.
- Phase 13 Gate 13.2 (2026-09-09): The Colonnade arena is automated GREEN. `Dungeon_04.unity` now contains a 36m x 28m hall, south-facing-north player entry, four symmetric collision pillars at (-5.5, 4.5), (5.5, 4.5), (-5.5, -4.5), and (5.5, -4.5) on the horizontal plane, boundary walls, open central lane, flank routes, and six tactical spawn points. Bounded verifier passed with Unity exit code 0. Gate 13.3 is next.
- Phase 13 Gate 13.1 (2026-09-09): Dungeon 4 campaign foundation is automated GREEN. Added `dungeon_4` (Bölüm 4: Sütunlu Salon) after D3 with 1.3x HP / 1.2x damage scaling, six-scene build routing, six spawn-point foundation, fourth World Map card layout, and a 3-point character-scoped first-clear reward. The Gate 13.1 verifier, Phase 12 World Map/progression regression, Phase 10 campaign-flow regression, and D3/catalog regression passed with bounded Unity exits. Gates 13.2–13.5 remain.
- Phase 12 Final Sign-off (2026-09-09): Phase 12 is COMPLETE. Gates 12.1–12.5 automated GREEN (2432/2432 checks total across all 15 suites in the full regression matrix: Gate 12.1: 87/87, Gate 12.2: 99/99, Gate 12.3: 200/200, Gate 12.4: 226/226, Gate 12.5: 127/127, Suite 10.1: 11/11, Suite 10.2: 15/15, Suite 10.3: 5/5, Suite 10.4: 5/5, Suite 10.5: 11/11, Phase 10 QA: 10/10, Archer 8.3: 68/68, Gunner 8.4: 44/44, Suite 11.1: 43/43, Suite 11.5: 1485/1485). User manual gameplay QA is ALL GREEN. Implemented and verified character-specific permanent progression persistence (`PermanentProgression`), first-clear campaign economy (D1=2, D2=2, D3=3; 7 points max per character) with strict idempotency, 3-branch / 9-node ScriptableObject skill trees for Warrior, Archer, and Gunner, spawn-time fresh health scaling (135/135 HP), combat stat layering (`Base * Permanent * Temporary`), and the World Map Skill Tree purchase UI ("YETENEKLER" overlay with 4 deterministic node states). Phase 13 (Dungeon 4 + Encounter Design) is NEXT.
- Phase 12 Gate 12.5 World Map Permanent Skill Tree Purchase UI & Final Integration (2026-09-09): Gate 12.5 is COMPLETE and automated GREEN (127/127 checks passed, state restored, Unity exit code 0). Full regression matrix is 100% GREEN across all 15 suites. Implemented `SkillTreeNodeUI` card presenter with 4 deterministic states (`PURCHASED`, `AVAILABLE`, `INSUFFICIENT POINTS`, `LOCKED`), `SkillTreeUI` overlay controller in `WorldMap.unity` with character name, available points, 3 branches x 3 tier cards (9 nodes), and "KAPAT" close button, and wired dedicated "YETENEKLER" button on `WorldMapController` without leaving `WorldMap.unity`. Verified character switching isolation (Warrior -> Archer -> Gunner -> Warrior), spawn-time fresh health scaling (135/135 HP), combat stat layering (`Base * Permanent * Temporary`), campaign economy cap (7 pts max), session invariants, and fail-safe handling.
- Phase 12 Gate 12.4 Archer & Gunner Skill Trees & Combat Binding (2026-09-09): Gate 12.4 is COMPLETE and automated GREEN (226/226 checks passed, state restored, Unity exit code 0). Regressions GREEN (Gate 12.1: 87/87, Gate 12.2: 99/99, Gate 12.3: 200/200, Archer 8.3: 68/68, Gunner 8.4: 44/44, Suite 10.5: 11/11, Suite 11.1: 43/43; 778 checks total). Implemented single `SkillTreeDefinition` ScriptableObjects for Archer (`SkillTree_Archer.asset`) and Gunner (`SkillTree_Gunner.asset`) with 3 branches x 3 linear tiers (9 nodes each at 1 pt each). Archer has Precision (1.35x Dmg), Tempo (1.20x AtkSpd), and Survival (1.15x MoveSpd, 1.10x Max HP). Gunner has Firepower (1.35x Dmg), Cadence (1.20x AtkSpd), and Handling (1.10x MoveSpd, 1.10x Max HP). Bound trees to `Character_Archer.asset` and `Character_Gunner.asset`. Verified real combat binding with `BowWeapon` (20 base -> 27.0 effective, 0.6s -> 0.50s cd) and `RifleWeapon` (10 base -> 13.5 effective, 0.18s -> 0.15s cd), fresh health scaling (110/110 HP), upgrade layering (`Base * Permanent * Temporary`), run semantics, and deterministic 4-way character switching. Next task awaits Gate 12.5 authorization.
- Phase 12 Gate 12.3 Warrior Skill Tree & Spawn-Time Modifier Binding (2026-09-09): Gate 12.3 is COMPLETE and automated GREEN (200/200 checks passed, state restored, Unity exit code 0). Regressions GREEN (Gate 12.1: 87/87, Gate 12.2: 99/99, Suite 10.5: 11/11, Suite 11.1: 43/43, Suite 11.5: 1485/1485; 1925 checks total). Implemented single Warrior `SkillTreeDefinition` ScriptableObject with 3 branches (Durability: +35% Max HP, Power: +35% Damage, Tempo: +15% Attack Speed, +5% Move Speed), 3 linear tiers per branch, 9 nodes total at 1 point each. Bound skill tree asset to `Character_Warrior.asset`. Integrated spawn-time permanent modifier binding into `PlayerSpawner` -> `PlayerStats` and `PlayerHealth`, enforcing fresh health scaling (`BaseMaxHealth` 100 -> `MaxHealth` 135, `CurrentHealth` 135) and layered combat math (`Base * Permanent * Temporary`). Verified run semantics compatibility across Victory, Defeat Retry rollback, and EndRun, alongside complete cross-character isolation (Archer and Gunner retain neutral 1.0x multipliers and 100 HP). Next task awaits Gate 12.4 authorization.
- Phase 12 Gate 12.2 Reward Economy & Completion Integration (2026-09-09): Gate 12.2 is COMPLETE and automated GREEN (99/99 checks passed, state restored, Unity exit code 0). Regressions GREEN (Gate 12.1: 87/87, Suite 10.5: 11/11, Suite 11.1: 43/43, Suite 11.5: 1485/1485). Implemented approved first-clear economy in `PermanentProgression` (D1 = 2 pts, D2 = 2 pts, D3 = 3 pts; 7 pts total per character max) with case-insensitive and idempotent reward mapping. Integrated authoritative completion awarding into `DungeonCompletionController.FinalizeCompletion()`, preserving global `DungeonProgression` unlock semantics while recording character-isolated permanent rewards. Verified duplicate-clear rejection, cross-character isolation, exclusion from defeat/retry/return-to-map/run-XP, persistence across simulated reloads, and fail-safe handling on null/empty character/dungeon IDs. Next task awaits Gate 12.3 authorization.
- Phase 12 Gate 12.1 Foundation & Persistence (2026-09-09): Gate 12.1 is COMPLETE and automated GREEN (87/87 checks passed, state restored, Unity exit code 0). Regressions GREEN (Suite 10.5: 11/11 passed, Suite 11.5: 1485/1485 passed). Implemented `PermanentEffectType` enum, `SkillNodeDefinition` node data model, `SkillTreeDefinition` ScriptableObject, and `PermanentProgression` static persistence service with PlayerPrefs JSON storage, domain reload reset, idempotent dungeon first-clear point awarding (D1=2, D2=2, D3=3 per character; 7 max), purchase prerequisites/cost deduction, character isolation, and stat aggregation. Added permanent health scaling to `PlayerHealth` (`ApplyPermanentHealthMultiplier`, `BaseMaxHealth`) and layered multipliers in `PlayerStats`. Gate 12.2 is NEXT.
- Phase 11 Final Sign-off (2026-09-09): Phase 11 is COMPLETE. Gates 11.1–11.5 automated GREEN (1485/1485 in Gate 11.5 suite, 2117/2117 total across all 13 regression suites). User manual gameplay QA is ALL GREEN. Four enemy archetypes implemented (Zombie, Runner, Tank, Ranged) using shared composition and minimal `IEnemyAttack`. Visual hit feedback and unscaled corpse cleanup active. Ranged projectiles use authoritative sweep collision and shooter-owned cancellation on death. Wave compositions verified: D1 (38 enemies / 410 XP), D2 (35 enemies / 430 XP), D3 (42 enemies / 540 XP), Campaign total (115 enemies / 1380 XP). Roguelite run semantics preserved across Victory, Defeat Retry rollback, and Defeat Return to Map. Phase 12 (Permanent Progression / Skill Tree) is NEXT.
- Restart recovery (2026-09-09): recovered main at `2d489ac`, with intact uncommitted Gate 11.4 and partial Gate 11.5 work. Preserved the partial integration work separately, checkpointed Ranged as `1a0846b`, confirmed a clean tree, then restored and completed integration. Gate 11.5 focused verification is GREEN: 1485 checks, state restored, Unity exit 0. Full isolated regression is GREEN: 2117 passing checks across 13 suites.
- Gate 11.1 implementation (2026-09-08): minimal `IEnemyAttack`, Zombie hit feedback and corpse cleanup are implemented on `codex-gate11-test`, based on Phase 10 commit `8f11ae7`. Verification results are recorded in the current gate section below; prior Phase 10 notes are historical.
- Phase 10 Final Manual QA is GREEN & Approved:
  - Architecture Audit complete: explicit serialized bindings confirmed across production scenes; Editor-only asset lookups cleanly isolated behind `#if UNITY_EDITOR`; standalone builds decoupled from AssetDatabase; discrete state ownership confirmed across `CharacterSelectionSession`, `DungeonRunSession`, `RunProgressionSession`, and `DungeonProgression`; World Map 3-card catalog layout verified.
  - Test Suite Integrity: 10/10 checks PASSED in `Milestone10_QA_Verifier.cs`; 11/11 checks PASSED in `Milestone10_5_Verifier.cs` full regression suite; 0 compiler errors, 0 warnings (resolved legacy CS0219).
  - Core Roguelite Run Semantics Verified:
    - Victory -> run continues into subsequent dungeons, preserving Level, accumulated XP, and collected temporary upgrades.
    - Defeat -> Retry -> rolls back to the dungeon-entry checkpoint (preserving entry Level, XP, and upgrades).
    - Defeat -> Return to Map -> active run ends, resetting Level to 1, XP to 0, and upgrades to empty.
    - Persistent dungeon progression (`DungeonProgression`) remains intact across defeats and returns.
  - Manual Gameplay Evidence:
    - Player progressed through D1 -> D2 -> D3 reaching Dungeon 3 at Level 5 with build intact.
    - Defeat in Dungeon 3 -> "HARİTAYA DÖN" successfully ended the active run.
    - Re-entering Dungeon 3 started fresh at Level 1 / 0 XP / neutral progression while D1 & D2 completed and D3 unlocked states persisted.
    - Warrior melee reach of 2.5m manually accepted.

---

# Current Project State

## Phase 20 Ash Warden attack geometry — manual QA GREEN

- Normal enemy fairness checkpoint `20dae97` is manually GREEN. This follow-up changes Ash Warden only: Strike uses a committed 140-degree frontal sector at the existing 2.9m range, a 0.45s amber wind-up, 0.15s active window with at most one hit, and existing 0.85s recovery. Side/rear positioning outside the sector is safe; no backstab bonus or player-stat change.
- Slam remains 6.5m radius, 1.1s wind-up, one execution-time distance check and 1s recovery. Its red world-space outline now exactly matches the damaging radius, independent of prefab scale, with a fixed center throughout wind-up.
- Bolt retains purple presentation, existing damage/speed/projectiles, 0.7s wind-up and Phase II double shot. Aim is committed at wind-up start for both shots, instead of tracking again at firing time. All attack cues reuse the existing telegraph material; no new combat/VFX framework. Pending routines and outlines are cleared on death/disable.
- Warrior stats, boss HP, XP, D5 points, encounter/progression, Slam/Bolt damage and attack cooldowns are unchanged. The previous D5 balance acceptance remains the historical baseline; this geometry change is now accepted by user manual QA.
- Verification: WardenGeometrySmoke PASSED after the harness added selected Warrior/clean progression setup. Real prefab checks verified rear safety, unchanged front damage, committed direction, exact 6.5m Slam outline with inside-hit/outside-miss and no wind-up damage, purple Phase II double-shot direction/miss, death cancellation, 960 boss XP, completion, and exactly +2 first-clear D5 points. Compilation succeeded; the initial full compile had only the three known TMP CS0618 warnings. Phase12StateSnapshot restored user state. No broad suite; the already-completed successful rerun was not repeated during checkpoint recovery.
- User manual combat-fairness QA is GREEN for Ash Warden geometry (`40e27a0`): frontal versus side/rear positioning, red Slam boundary and purple ranged threat are accepted. No further tuning is included in this sign-off.

## Phase 20 normal enemy fairness — manual QA GREEN

- Normal melee previously dealt instant range-only damage. EnemyAttack now commits its origin and direction at wind-up start, then checks a fixed 120-degree sector during a 0.12-second active window, at most one hit per attack. Recovery is 0.18 seconds. Serialized wind-ups preserve archetype cadence: Zombie 0.40s, Runner 0.28s, Tank 0.65s. Existing damage, range and cooldown values are unchanged.
- EnemyRangedAttack now telegraphs a fixed firing direction for 0.45s and uses the existing straight swept-collision projectile; speed, damage and lifetime are unchanged. Its short 0.15s release recovery prevents immediate pursuit during firing.
- EnemyMovement holds translation and facing during normal attacks while maintaining grounding. EnemyVisualFeedback draws an amber committed sector/aim line, with the melee sector turning red only during its active window; separate damage-flash behavior is retained. Disable/death cancels pending attacks and hides cues. No boss controller or EnemyProjectile changes.
- HP, XP, difficulty scaling, campaign progression, equipment and player stats are unchanged. TryAttack now means wind-up accepted, not immediate damage. Historical tests expecting instant normal-enemy damage are no longer the acceptance contract; no broad matrix is requested.
- Verification: compilation succeeded with the three known TMP CS0618 warnings. One focused real-prefab smoke PASSED: Zombie wind-up with no instant damage, committed facing/origin, lateral dodge inside radial range, cue/active-window cleanup, unchanged single-hit damage against a stationary player, Ranged fixed-direction projectile dodge and stationary hit, and death cancellation. Phase12StateSnapshot restored user state. No broad regressions or licensing retry. Runner/Tank timing and all-hero feel were subsequently accepted by user manual QA.
- User manual QA is GREEN for normal enemy fairness (`20dae97`), covering Zombie, Runner, Tank and Ranged and Warrior/Archer/Gunner combat feel. The final sign-off is documentation-only; no new test execution or broad regression matrix was performed.

## Phase 15–19 campaign milestone — COMPLETE, manual QA GREEN (accepted baseline)

- Phase 15: existing nine-node trees cost 1/2/2 per branch, 15 points per character. D1–D10 first-clear rewards are 1/1/1/1/2/1/1/2/2/3, character-scoped and replay-idempotent. Save v1 -> v2 grandfathers points, purchases and reward history; legacy D5 receives one future claim.
- Phase 16: Hub_Armory, selected-character presentation, free milestone weapon claims/equipment, and the once-only D5 Blacksmith Tier I introduction are accepted. Save v2 -> v3 adds per-character equipment. PlayerStats preserves Permanent Skill × Equipped Weapon × Temporary Run layering; Retry/EndRun retain equipment.
- Phases 17–18: D6–D9 arenas, waves, unlock continuity, progression and Tier I campaign play are manually GREEN. No fifth normal enemy archetype or carousel redesign.
- Phase 19: D10 / Yıldızsız Taht / The Starless Throne and The Hollow Castellan / Boş Kalenin Muhafızı are manually GREEN. Three warm-up waves contain 80 enemies / 1,200 XP, followed by one 3,800-HP boss worth 2,000 XP. Two phases: cleave and line dash, then faster cadence and a three-shard fan at half HP. No adds or persistent hazards. Shared IBossPresentation HUD uses actual EnemyHealth and one red health bar.
- D10 grants 3 first-clear skill points per character, global Tier II access and persistent campaign completion, acknowledged in the Hub. Tier II damage/attack-speed multipliers: Warrior 1.18/1.15, Archer 1.16/1.18, Gunner 1.14/1.21; equipment replaces the previous weapon layer.
- D10 Blacksmith reward sequence (`4af5e5d`) is manually GREEN: XP, upgrade choices and completion resolve before Haritaya Dön routes once to the existing Blacksmith fade/dialogue. The active hero receives/equips Tier II; the normal Armory and return flow continue afterward. Save v3 adds only `secondBlacksmithIntroSeen`, committed with ownership/equipment. Legacy eligible Hub entry is supported; missed Tier I presentation is settled without duplicate ownership or downgrading equipment. Other heroes retain free affinity-scoped claims.
- D5 Warrior balance (`3cd757c`) is accepted for now: Strike/stop range 2.9m, Strike cooldown 2.1s, recovery 0.85s, telegraph 0.45s. Strike damage, Slam/Bolt, boss HP/XP and Warrior stats remain unchanged. Further melee/boss tuning is deferred to Phase 20 and requires evidence from play.

| Dungeon | Enemies | XP | Campaign exit | First-clear points |
|---|---:|---:|---|---:|
| D5 | 19 | 1,200 | L8 68/1709 | 2 |
| D6 | 55 | 900 | L8 968/1709 | 1 |
| D7 | 70 | 1,100 | L9 359/2563 | 1 |
| D8 | 80 | 1,300 | L9 1659/2563 | 2 |
| D9 | 90 | 1,600 | L10 696/3844 | 2 |
| D10 | 81 | 3,200 | L11 52/5767 | 3 |

D10 campaign cumulative XP: 11,385. The full fresh-character permanent tree is fundable at D10 (15 points).

## Verification and checkpoints

- Accepted focused automation: Phase15 migration/economy smoke; Phase16 equipment/stat-layer smoke; D5 Blacksmith smoke (9 checks); D6, D7, D8 and D9 real scene lifecycle smokes; D10 boss lifecycle smoke; shared D5 boss HUD (9/9); D5 melee-spacing smoke (7 checks); D10-to-Blacksmith production-scene smoke. Final implementation compilation succeeded with only the three known legacy TMP CS0618 warnings. State snapshots preserved user saves. Presentation and gameplay feel are accepted by user manual QA.
- Historical Phase 14 reruns still ENVIRONMENT BLOCKED before execution: Phase 12.5 Skill Tree UI, Phase 14 Gate 14.1, and D5 boss lifecycle regression (`Connection to channel LicenseClient-Alcor refused`). These were not passed or retried during sign-off. Gate 14.3 later PASSED. Later focused smokes do not imply those blocked legacy suites ran.
- Checkpoints: `84d2a5e` Phase 15; `cb48f63` Armory/Tier I; `04bf1a9` D5 Blacksmith; `0f5b9bb` D6/D7; `dfccc7e` D8/D9; `56d4972` D10/Castellan/Tier II; `3cd757c` D5 spacing; `4af5e5d` D10 Blacksmith.
- Final sign-off changes documentation only. No new Unity execution or broad regression matrix is required for this documentation update.

**NEXT: stop and await explicit user direction for the next Phase 20 task. Combat fairness is signed off; broader stabilization is not declared complete.** No automatic further tuning or D11–D15 expansion. The following history retains the earlier sign-off state.

---

# Historical Phase Records

## Historical Phase 15–19 implementation notes

These checkpoint notes are historical; the final acceptance above supersedes pending-QA and next-phase statements.

**Phase 19 and requested D5 spacing patch implemented — USER MANUAL QA REQUIRED**

D5 patch: Ash Warden Strike range and explicit movement stop distance are 2.9m; Strike cooldown 2.1s; recovery 0.85s (repository recovery was 0.8s, not 0.65s). The shared old melee range was split so Slam retains its original 3.4m trigger range. Strike telegraph remains 0.45s; all damage, Slam/Bolt routines and cadence, phase threshold, HP, XP, encounter and progression remain unchanged. Warrior stats/range and D10 balance are untouched by this patch. WardenSpacingSmoke passed 7 checks: normal Strike startup, evasion, unchanged Warrior attack configuration, two actual 25-damage melee hits during recovery, damage for staying close, Phase II, and 960 boss XP on death. That timing check isolates Strike by delaying the other attacks only in the harness; mixed-attack punish windows and feel require manual Warrior/Archer/Gunner QA. Compilation succeeded with only the existing TMP warnings; state restored. D10 lifecycle and the shared D5 HUD 9/9 checks were already GREEN before this separate patch.

- D10 / Yıldızsız Taht / The Starless Throne: 44x36m arena with four substantial pillars, D9 prerequisite, HP x1.90 / damage x1.55. Warm-ups are 20/220, 26/380, 34/600 (enemies/XP), then one Hollow Castellan. Total 81 enemies / 3,200 XP. Campaign L10 696/3844 -> L11 52/5767, cumulative XP 11,385. First clear grants 3 character-scoped points exactly once.
- The Hollow Castellan / Boş Kalenin Muhafızı uses a distinct angular statue/armor placeholder, 2,000 base / 3,800 runtime HP, and 2,000 XP. Two phases only: speed 3.0 -> 3.4 at <=50% real HP. Phase I: 120-degree cleave (base 24), locked-direction line dash (base 32). Phase II retains those with faster cadence and adds a telegraphed three-shard fan (base 12 each). D10 scaled damage rounds to 37/50/19. Pillars stop dash movement and obstruct attacks/projectiles. No adds, persistent hazards, or generic boss framework.
- `IBossPresentation` exposes only BossName; both bosses use the existing EnemyHealth-driven RectTransform HUD. No visible Phase II label. D10 completion in the existing DungeonProgression save is the single campaign-completion authority; `IsFirstCampaignCompleted` derives from it rather than duplicating persistent state.
- Tier II definitions reuse the v3 per-character claim/equip save. D10 globally unlocks free Armorer claims. Preliminary absolute damage/speed multipliers: Warrior Starbreaker Greatblade 1.18/1.15, Archer Midnight Recurve 1.16/1.18, Gunner Bastion Carbine 1.14/1.21. Tier I values are unchanged; equipped weapons replace rather than stack with each other. Exact Tier II balance awaits manual QA. The Armorer's DİĞER SİLAH button cycles eligible weapons, and the Hub acknowledges campaign completion in its existing hint area.
- Verification: D10 actual scene smoke PASSED with the real warm-up lifecycle, boss spawn/full HUD, cleave and dash execution, real damage across 50%/half HUD, three-projectile fan, boss death/zero HUD, exact campaign XP, +3 first-clear/replay protection, campaign milestone, Tier II claim/equip and EndRun/reload persistence. Test state restored with Phase12StateSnapshot. Harness keeps its player alive and repositions actors to exercise each attack; arena feel, boss difficulty/duration, telegraph readability and weapon balance remain manual QA.
- Manual route after the D5 tuning patch: play D9 -> D10 with Tier I gear; assess all warm-ups, pillar cover, cleave/dash evasion, Phase II fan, boss HUD, death/XP/upgrade/victory ordering, L11 target, 3 points, Hub acknowledgement, Tier II free claim/equip/cycling for each hero, Retry/Return Map and restart persistence. Phase 20 starts only after explicit D10 user feedback. Do not add D11-D15 or push automatically.

**Phase 18 — manual QA GREEN (historical implementation record)**

- D8 / Közlü Avlu / The Ember Courtyard: 42x32m, four 3x4x3m line-of-sight obelisks, eight spawn points, HP x1.70 / damage x1.45. Five waves: 13/130, 15/160, 16/260, 18/345, 18/405 (enemies/XP). Total 80 enemies / 1,300 XP. Campaign L9 359/2563 -> L9 1659/2563, cumulative XP 6,585. First clear awards 2 points per character, replay idempotent.
- D9 / Gölge Hisarı / The Gloam Bastion: 44x34m, two lanes separated by north/south spine blockers with a central cross-link, eight spawn points, HP x1.80 / damage x1.50. Six waves: 15/165, 17/250, 15/260, 14/275, 15/315, 14/335. Total 90 enemies / 1,600 XP. Campaign L9 1659/2563 -> L10 696/3844, cumulative XP 8,185. First clear awards 2 points per character, replay idempotent.
- Catalog and build settings append D8 then D9 with D7 -> D8 -> D9 prerequisites. No World Map carousel, enemy, combat, PlayerStats, equipment, Hub/Blacksmith, economy or save changes. Reused the Phase 17 runner/smoke for D8/D9 without renaming working infrastructure; Warrior is only the representative test hero, not a production restriction.
- Verification: compilation succeeded with only the three accepted legacy TMP CS0618 warnings. D8 and D9 actual scene lifecycle smoke tests PASSED: prerequisites, scene bindings, all real waves/enemy deaths, XP pickups, upgrade choice handling, exact level/XP thresholds, completion and run commit, +2 first-clear points/replay rejection, and retained Tier I equipment. State restored with Phase12StateSnapshot. No licensing blocker or production RED. Fast smoke kills do not establish encounter feel, visual quality or difficulty.
- Manual QA route: continue a D7-cleared campaign with Tier I equipment through D8 then D9. Check obelisk cover/Tank splitting versus fortress lane switching/flanks, crossfire readability, enemy movement around blockers, perceived HP scaling, nine-card carousel navigation, XP/level-up continuity, first-clear points, Retry and Return Map/equipment persistence. Expected exits above assume the accepted uninterrupted campaign XP path.
- Exact resume point: after Phase 18 user manual QA GREEN, begin Phase 19 with Dungeon 10 / Hollow Castellan and Tier II milestone foundation under the locked master mission. No D10 content or Tier II implementation exists yet. Do not push automatically; do not begin Phase 20 before D10 manual QA.

**Phase 17 — manual QA GREEN (historical implementation record)**

- D6 / Kırık Geçit / The Fractured Causeway: 40x30m, two long staggered blockers, six spawn points, five waves, HP x1.50 / damage x1.35. Wave counts/XP: 10/100, 10/110, 10/170, 13/265, 12/255. Total 55 enemies / 900 XP (20 Zombie, 15 Runner, 10 Ranged, 10 Tank). The locked mission's aggregate “13 Tank” was an arithmetic typo; the exact approved waves give 10 Tanks and were preserved. Entry L8 68/1709 -> exit L8 968/1709. First clear: 1 point.
- D7 / Çökmüş Sarnıç / The Sunken Cistern: 38x34m ring around an impassable central basin, eight spawn points, five waves, HP x1.60 / damage x1.40. Wave counts/XP: 14/155, 15/200, 14/220, 13/240, 14/285. Total 70 enemies / 1100 XP. Entry L8 968/1709 -> exit L9 359/2563. First clear: 1 point.
- D5 -> D6 -> D7 catalog prerequisites and build routes are serialized. Existing World Map carousel, Tier I equipment/stat layers, saves and combat code are unchanged. D6/D7 scenes retain the existing gameplay/UI bindings; only arena geometry, spawn locations and wave data differ. No fifth enemy archetype or new AI framework.
- Verification: compilation succeeded with the three known TMP warnings. Both actual production scenes loaded and ran all five waves using real EnemyHealth deaths, XP pickups, upgrade choice handling, completion, campaign-state commit and first-clear/replay checks. Phase12StateSnapshot restored saved/session state. D6's first harness attempt checked bindings before SceneManager activated the loaded scene; adding an active-scene wait fixed only the harness. No production RED or licensing blocker remains in this phase. These smoke checks intentionally kill enemies quickly and do not establish combat difficulty or geometry quality.
- Manual route: use a D5-cleared save with a Tier I weapon; navigate to D6, play all five waves, verify 900 XP/one point and D7 unlock; play D7 and verify 1100 XP/one point and L9 campaign target; try Retry and Return Map. Assess lane rotation, ranged sightlines, Tank pressure, cistern circulation, spawn safety, and the generic seven-card carousel. Existing enemies use direct pursuit rather than pathfinding; verify the new obstacles do not create frustrating stalls during normal movement. No enemy movement changes were made.
- Exact resume point: after D6/D7 manual QA feedback, continue Phase 18 (D8 fully, then D9) under the locked mission. D8-D10, Tier II and the second boss are not implemented. No push authorized.

**Phase 16 and Blacksmith introduction — manual QA GREEN**

Phase 16 core functionality is manually GREEN. Current QA is specifically the Blacksmith refinement: complete D5 and use DEMİRCİYE GİT, or enter Hub with an existing D5-completed save; advance dialogue with Continue/Enter; verify named reward and auto-equip, selected-character presentation, normal Armorer panel, another hero's free claim/equip, Hub re-entry and app restart without intro replay. The victory-to-Hub route and visual pacing require manual QA; the small Editor smoke covers the real dialogue reward path and persistence.

Blacksmith verification: compilation succeeded (only the three known legacy TMP warnings); focused Editor smoke PASSED, 9 checks, exit 0. It covers pre-D5 gating, legacy eligibility, affinity rejection, created dialogue hierarchy, real dialogue-driven auto-equip, persisted/idempotent intro completion, no replay, other-hero claim/equip, and all equipment plus old points surviving EndRun/reload. An initial harness failure was corrected by creating the empty test scene before loading asset references, avoiding Unity unloading them during scene replacement; production logic was unchanged for that correction. Phase12StateSnapshot restored test state. No licensing retry or broad regression matrix was needed.

- Phase 15 checkpoint: `84d2a5e feat: rebalance permanent progression`. Existing nine nodes cost 1/2/2 per branch, 15 per tree. D1-D10 first-clear rewards are 1/1/1/1/2/1/1/2/2/3, character-scoped and replay-idempotent. D6-D10 content is not implemented yet.
- Save v1 -> v2 grandfathers points, purchases and rewarded history, normalizes ID lists, and allows one future legacy D5 reward. Phase 16 adds v2 -> v3 claimed/equipped weapon IDs per character; no equipment fields were introduced in the v2 checkpoint.
- `Hub_Armory` is a small passive 3D scene with three character displays and one Armorer. Global D5 completion reveals its World Map entry. World Map remains the dungeon launch screen. Selecting another hero ends an active run owned by the previous hero; equipment persists.
- Tier I milestone blueprints are free to claim/equip per character. Warrior Emberfang Greatblade: damage x1.12 / speed x1.10; Archer Stormstring Recurve: x1.10 / x1.12; Gunner Coilburst Carbine: x1.08 / x1.15. Base weapon can be re-equipped. Optional reach/projectile-speed/range tuning is deferred; no currency or inventory system.
- `PlayerStats` owns effective damage/attack-speed multipliers: Permanent Skill x Equipped Weapon x Temporary Run. Existing attacks consume these stats; base damage and cooldown are unchanged. `PlayerSpawner` resolves equipment at spawn, including Retry; temporary reset and EndRun do not erase equipment.
- Verification: Phase15Smoke PASSED (migration/idempotency, affordability, isolation, 15-point reward total). Phase16Smoke PASSED in Unity batch, exit 0: additive v2 migration, D5 gating, claim/equip and affinity, reload/EndRun persistence, production prefab spawning with stat layering, temporary reset isolation, passive Hub hierarchy and serialized navigation references. These are focused Editor smoke checks, not a claim of runtime UI or subjective gameplay acceptance. Compilation succeeded with only the three accepted legacy TMP CS0618 warnings. Licensing did not block these new checks; historical Phase 14 blocked reruns remain unexecuted.
- Manual QA next: after D5 completion enter CEPHANELİK; select all three heroes, open the Armorer, claim/equip and switch to base; verify selected marker and equipment text; return to Map and enter a dungeon with each hero; test weapon feel, Retry, Return Map and app restart persistence. Check Skill Tree costs and grandfathered progression. No push until user approval.
- Exact resume point after Phase 16 manual QA GREEN: Phase 17, implement D6 fully before D7 using the locked wave tables. Do not start Phase 20 before D10 manual QA. No D6-D10 scenes, Tier II weapons or second boss have been created.

**Historical accepted baseline — Phase 14 COMPLETE**

- D5 / Kül Mabedi: three waves, 18 normal enemies plus The Ash Warden / Kül Muhafızı, 19 enemies, 1,200 XP, 1.4x enemy HP, and 1.3x enemy damage.
- Campaign XP target is preserved: enter Level 7 at 7/1139; exit Level 8 at 68/1709; campaign cumulative XP is 3,285. D5 permanent reward was 0 at Phase 14 sign-off and is now 2 under Phase 15.
- The Ash Warden uses 1,200 base HP / 1,680 runtime HP with two phases, no adds, and no persistent hazards. Boss death, XP, retry, return-to-map, and completion ordering are accepted GREEN.
- World Map uses the scalable three-card carousel: newest-unlocked initial focus, locked-card browsing, Enter gating, generic D6+ windows, bounded UI-arrow and Left/Right keyboard navigation, and Skill Tree modal-safe input/focus restoration.
- Warrior attacks have a visual-only 120-degree slash feedback arc from the successful melee attack path; it lasts 0.12 seconds and does not change combat values.
- Boss HUD is a centered name with one red RectTransform fill over a subtle dark track. It has no Phase II label, uses `CurrentHealth / MaxHealth`, reaches zero on death, and binds fresh on retry without stale listeners.
- User manual QA: ALL GREEN.
- Focused automation: Boss Health Bar 9/9; Warrior Slash Feedback 8/8; World Map Carousel 525 checks; Phase 10 World Map QA 10/10; Gate 14.3 PASSED.
- Final regression reruns blocked before verifier startup: Phase 12.5 Skill Tree UI, Gate 14.1, and D5 boss lifecycle. Unity batch reported `Connection to channel LicenseClient-Alcor refused`; this is an ENVIRONMENT BLOCKER, not production RED. Do not treat those three suites as passed.

**NEXT — Phase 16 user manual QA, then Phase 17: Dungeons 6 and 7.**

---



## PHASE 12 — Permanent Progression / Skill Tree (COMPLETE)

- Gate 12.1: Permanent Progression Foundation (Completed & Verified)
- Gate 12.2: Permanent Reward Economy & Dungeon Completion Integration (Completed & Verified)
- Gate 12.3: Warrior Permanent Skill Tree & Spawn-Time Modifier Binding (Completed & Verified)
- Gate 12.4: Archer & Gunner Permanent Skill Trees & Combat Binding (Completed & Verified)
- Gate 12.5: World Map Permanent Skill Tree Purchase UI / Final Integration (Completed & Verified)

---

## Gate 12.5 — World Map Permanent Skill Tree Purchase UI / Final Integration (Automated GREEN, 2026-09-09)

### Implemented

- `Assets/Scripts/UI/SkillTreeNodeUI.cs`:
  - Dedicated presentation component for individual permanent skill node cards.
  - Implemented 4 deterministic states via `SkillNodeUIState`:
    - `PURCHASED`: already bought, non-interactable button, emerald tint (`#14331F`), "SATIN ALINDI" status label.
    - `AVAILABLE`: prerequisites met, sufficient skill points, interactable button, bright cyan tint (`#1A2F47`), "SATIN AL" status label.
    - `INSUFFICIENT POINTS`: prerequisites met, insufficient points, non-interactable button, amber tint (`#382614`), "YETERSİZ PUAN" status label.
    - `LOCKED`: prerequisite node not yet unlocked, non-interactable button, dark charcoal tint (`#17171C`), "KİLİTLİ" status label.
  - Displays node display name, description, and cost (`1 Puan` or `Mevcut`).
  - Fires `OnPurchaseRequested` event on button click; delegates purchase transaction to controller.
- `Assets/Scripts/UI/SkillTreeUI.cs`:
  - Dedicated screen overlay controller for permanent skill purchases on the World Map scene.
  - Renders character display name, available skill points, 3 branch headers, and 9 `SkillTreeNodeUI` cards (3 branches x 3 linear tiers).
  - Listens to `PermanentProgression.OnSkillPurchased` and `PermanentProgression.OnSkillPointsChanged` to guarantee reactive visual updates.
  - Handles node purchase requests by delegating strictly to authoritative `PermanentProgression.TryPurchaseNode(charId, node, tree)`.
  - Supports dynamic character rebinding via `Open(CharacterDefinition)` supporting Warrior, Archer, and Gunner, with automatic fallback resolution.
  - Provides `Close()` closing the panel without mutating any session or navigation state.
- `Assets/Scripts/UI/WorldMapController.cs`:
  - Added serialized `skillTreeButton` ("YETENEKLER") and `skillTreePanel` (`SkillTreeUI`) references with public accessors.
  - Added `HandleSkillTreeClicked()` handler: opens `skillTreePanel` passing currently selected character from `CharacterSelectionSession.SelectedCharacter`.
  - Automatically resolves references and closes the panel on initialize to ensure clean state.
  - Added overloaded `SetReferences` method preserving 100% binary/source backwards compatibility with existing setup scripts and test runners.
  - Ensured `WorldMapController` remains strictly decoupled from persistence, points mutation, and purchase rules (`PermanentProgression` is sole authority).
- `Assets/Scenes/WorldMap/WorldMap.unity`:
  - Added `SkillTreeButton` ("YETENEKLER") at `(700, -420)`, size `(200, 50)`, symmetrically balanced with `BackButton` at `(-700, -420)`.
  - Added `SkillTreePanel` full-screen modal overlay with dark semi-transparent backdrop (`#0D121A`), header (`TitleText`, `HeroNameText`, `AvailablePointsText`), "KAPAT" close button (`#A62626`), and 3 branch columns (`Branch_1`, `Branch_2`, `Branch_3`) holding 9 `SkillTreeNodeUI` cards.
  - Initial state saved as inactive (`SetActive(false)`).
- `Assets/Editor/Milestone12_5_Setup.cs`:
  - Scene configuration utility `SetupWorldMapSkillTree()` for automated, idempotent creation and serialization of UI hierarchy in `WorldMap.unity`.
- `Assets/Editor/Phase12VerificationRunner.cs`:
  - Added menu item `[MenuItem("DungeonRoguelite/Phase 12/Run Gate 12.5 Verification")]` and suite routing for `12_5`.
- `Assets/Tests/Verification/Milestone12_5_Verifier.cs`:
  - Comprehensive 127-check Play Mode verification suite covering:
    - Build Settings 5-scene order and `WorldMap.unity` disk serialization integrity.
    - World Map UI hierarchy resolution: `SkillTreeButton` ("YETENEKLER"), `SkillTreePanel` (initially inactive), `CloseButton` ("KAPAT"), 9 cards.
    - Panel Open/Close mechanics without altering `RunProgressionSession`, resetting temporary upgrades, or mutating dungeon locks.
    - Deterministic 4-state node transitions on Warrior:
      - 0 points: Tier 1 nodes `InsufficientPoints`, Tier 2/3 `Locked`.
      - Award 2 points: Tier 1 nodes become `Available`.
      - Purchase Tier 1: node becomes `Purchased`, Tier 2 unlocks and becomes `Available`.
      - Purchase Tier 2: points reach 0, Tier 3 becomes `InsufficientPoints` (prereq met, but 0 points), other Tier 1 nodes revert to `InsufficientPoints`.
      - Guard against clicking non-interactable buttons (0 purchases occur).
    - Reactive external event synchronization via `OnSkillPointsChanged` and `OnSkillPurchased`.
    - Character switching isolation: Warrior (3 nodes bought) -> Archer (0 points, 0 bought) -> Gunner (0 points, 0 bought) -> Warrior (3 nodes bought, 0 points preserved).
    - Spawn-time dungeon integration: Warrior spawns with 135/135 fresh HP and neutral 1.0x damage, then layers temporary +20% damage upgrade multiplicatively to 1.20x (30.0 damage).
    - Campaign economy & idempotency: D1=2, D2=2, D3=3 (7 max per character), repeat clears award 0 points.
    - Fail-safe boundary handling on null session characters and double close calls.

### Verification

- Final Unity 6000.3.23f1 batch Play Mode run: 127/127 checks passed, exit code 0 (`Logs/gate12_5.log`). All completion and state restoration markers present.
- Regression Gate 12.1 (`Milestone12_1_Verifier`): 87/87 checks passed, exit code 0 (`Logs/gate12_1.log`).
- Regression Gate 12.2 (`Milestone12_2_Verifier`): 99/99 checks passed, exit code 0 (`Logs/gate12_2.log`).
- Regression Gate 12.3 (`Milestone12_3_Verifier`): 200/200 checks passed, exit code 0 (`Logs/gate12_3.log`).
- Regression Gate 12.4 (`Milestone12_4_Verifier`): 226/226 checks passed, exit code 0 (`Logs/gate12_4.log`).
- Full Legacy Regression Matrix (`Run-Phase11Regressions.ps1`):
  - Suite 10.1 (`Milestone10_1_Verifier`): 11/11 checks passed, exit code 0.
  - Suite 10.2 (`Milestone10_2_Verifier`): 15/15 checks passed, exit code 0.
  - Suite 10.3 (`Milestone10_3_Verifier`): 5/5 checks passed, exit code 0.
  - Suite 10.4 (`Milestone10_4_Verifier`): 5/5 checks passed, exit code 0.
  - Suite 10.5 (`Milestone10_5_Verifier`): 11/11 checks passed, exit code 0.
  - Phase 10 QA (`Milestone10_QA_Verifier`): 10/10 checks passed, exit code 0.
  - Archer 8.3 (`Milestone8_3_Verifier`): 68/68 checks passed, exit code 0.
  - Gunner 8.4 (`Milestone8_4_Verifier`): 44/44 checks passed, exit code 0.
  - Suite 11.1 (`Milestone11_1_Verifier`): 43/43 checks passed, exit code 0.
  - Suite 11.5 (`Milestone11_5_Verifier`): 1485/1485 checks passed, exit code 0.
- Total checks passed in Gate 12.5 sign-off: 2432 checks passed with zero errors, zero warnings.
- User Manual Gameplay QA: ALL GREEN. Verified opening "YETENEKLER" overlay from World Map, purchasing nodes across branches, character switching, 4-state node visual feedback, non-combat UI isolation, and successful transition into dungeon with active permanent multipliers.
- Phase 12 Final Sign-Off: COMPLETE. Next milestone is Phase 13 (Dungeon 4 + Encounter Design).

---

## Gate 12.4 — Archer & Gunner Permanent Skill Trees & Combat Binding (Automated GREEN, 2026-09-09)

### Implemented

- `Assets/ScriptableObjects/Progression/SkillTree_Archer.asset`:
  - Created single authoritative Archer `SkillTreeDefinition` ScriptableObject with 3 branches, 3 linear tiers per branch, 9 nodes total at 1 point per node:
    - **Precision** branch (`DamageMultiplier`): T1 `archer_precision_1` (+10%, prereq null), T2 `archer_precision_2` (+10%, prereq T1), T3 `archer_precision_3` (+15%, prereq T2). Total branch: +35% damage (1.35x).
    - **Tempo** branch (`AttackSpeedMultiplier`): T1 `archer_tempo_1` (+5%, prereq null), T2 `archer_tempo_2` (+5%, prereq T1), T3 `archer_tempo_3` (+10%, prereq T2). Total branch: +20% attack speed (1.20x).
    - **Survival** branch (`MovementSpeedMultiplier` / `MaxHealthMultiplier`): T1 `archer_survival_1` (+5% move speed, prereq null), T2 `archer_survival_2` (+10% max health, prereq T1), T3 `archer_survival_3` (+10% move speed, prereq T2). Total branch: +15% movement speed (1.15x), +10% max health (1.10x).
  - Validated cleanly via `SkillTreeDefinition.ValidateTree(out string error)`.
- `Assets/ScriptableObjects/Progression/SkillTree_Gunner.asset`:
  - Created single authoritative Gunner `SkillTreeDefinition` ScriptableObject with 3 branches, 3 linear tiers per branch, 9 nodes total at 1 point per node:
    - **Firepower** branch (`DamageMultiplier`): T1 `gunner_firepower_1` (+10%, prereq null), T2 `gunner_firepower_2` (+10%, prereq T1), T3 `gunner_firepower_3` (+15%, prereq T2). Total branch: +35% damage (1.35x).
    - **Cadence** branch (`AttackSpeedMultiplier`): T1 `gunner_cadence_1` (+5%, prereq null), T2 `gunner_cadence_2` (+5%, prereq T1), T3 `gunner_cadence_3` (+10%, prereq T2). Total branch: +20% attack speed (1.20x).
    - **Handling** branch (`MovementSpeedMultiplier` / `MaxHealthMultiplier`): T1 `gunner_handling_1` (+5% move speed, prereq null), T2 `gunner_handling_2` (+10% max health, prereq T1), T3 `gunner_handling_3` (+5% move speed, prereq T2). Total branch: +10% movement speed (1.10x), +10% max health (1.10x).
  - Validated cleanly via `SkillTreeDefinition.ValidateTree(out string error)`.
- `Assets/ScriptableObjects/Characters/Character_Archer.asset`:
  - Bound serialized `skillTree` property directly to `SkillTree_Archer.asset`.
- `Assets/ScriptableObjects/Characters/Character_Gunner.asset`:
  - Bound serialized `skillTree` property directly to `SkillTree_Gunner.asset`.
- `Assets/Editor/Phase12AssetBuilder.cs`:
  - Added `BuildArcherTree()`, `BuildGunnerTree()`, and `BuildAllSkillTrees()` editor menu utilities ensuring reproducible asset generation for all 3 playable archetypes.
- `Assets/Editor/Phase12VerificationRunner.cs`:
  - Added menu item `[MenuItem("DungeonRoguelite/Phase 12/Run Gate 12.4 Verification")]` and suite routing for `12_4`.
- `Assets/Tests/Verification/Milestone12_3_Verifier.cs`:
  - Updated character isolation assertions to allow assigned Archer/Gunner trees while enforcing zero bleeding and neutral 1.0x multipliers when 0 nodes are purchased, ensuring backward compatibility across test suites.
- `Assets/Tests/Verification/Milestone12_4_Verifier.cs`:
  - Comprehensive 226-check Play Mode verification suite covering:
    - Tree structure, IDs, branches, tiers, costs, prerequisites, and clean validation for Archer and Gunner.
    - Purchase progression rules, prerequisite gating, insufficient points, duplicate purchase rejection, and cross-character purchase rejection (Archer cannot purchase Warrior/Gunner nodes; Gunner cannot purchase Warrior/Archer nodes; Warrior cannot purchase Archer/Gunner nodes).
    - Campaign first-clear economy adherence (D1=2, D2=2, D3=3 = 7 pts max) and 7-node purchase cap.
    - Real combat binding: Archer `BowWeapon` (20 base -> 27.0 effective damage, 0.6s -> 0.50s cooldown), `PlayerMovement` (6.5 base -> 7.475 m/s), `PlayerHealth` (100 base -> 110 HP full scaled spawn).
    - Real combat binding: Gunner `RifleWeapon` (10 base -> 13.5 effective damage, 0.18s -> 0.15s cooldown), `PlayerMovement` (6.0 base -> 6.60 m/s), `PlayerHealth` (100 base -> 110 HP full scaled spawn).
    - Combat upgrade layering: `Base * Permanent * Temporary` for damage, attack speed, and movement speed with temporary bonuses and damage reception.
    - Roguelite run semantics compatibility: Victory preservation, Defeat Retry rollback with fresh scaled HP restoration, and EndRun session teardown across all archetypes.
    - Deterministic 4-way character switching: Warrior -> Archer -> Gunner -> Warrior verifying exact per-character stat restoration with zero bleeding.
    - Fail-safe boundary handling on null/empty inputs and unknown characters.

### Verification

- Final Unity 6000.3.23f1 batch Play Mode run: 226/226 checks passed, exit code 0 (`Logs/gate12_4.log`). All completion and state restoration markers present.
- Regression Gate 12.1 (`Milestone12_1_Verifier`): 87/87 checks passed, exit code 0 (`Logs/gate12_1_reg.log`).
- Regression Gate 12.2 (`Milestone12_2_Verifier`): 99/99 checks passed, exit code 0 (`Logs/gate12_2_reg.log`).
- Regression Gate 12.3 (`Milestone12_3_Verifier`): 200/200 checks passed, exit code 0 (`Logs/gate12_3_reg.log`).
- Regression Archer 8.3 (`Milestone8_3_Verifier`): 68/68 checks passed, exit code 0 (`Logs/regression_8_3.log`).
- Regression Gunner 8.4 (`Milestone8_4_Verifier`): 44/44 checks passed, exit code 0 (`Logs/regression_8_4.log`).
- Regression Suite 10.5 (`Milestone10_5_Verifier`): 11/11 checks passed, exit code 0 (`Logs/regression_10_5.log`).
- Regression Suite 11.1 (`Milestone11_1_Verifier`): 43/43 checks passed, exit code 0 (`Logs/regression_11_1.log`).
- Total Regression Checks: 778/778 passed in this validation pass.
- Compilation: 0 errors; 3 existing CS0618 warnings in legacy setup scripts (`TMP_Text.enableWordWrapping`).
- Working tree clean of accidental project setting changes.

### Decisions and Next Task

- All three character skill trees (`SkillTree_Warrior.asset`, `SkillTree_Archer.asset`, `SkillTree_Gunner.asset`) are authoritatively defined and bound to their character definitions.
- Combat components (`MeleeWeapon`, `BowWeapon`, `RifleWeapon`, `PlayerMovement`, `PlayerHealth`) dynamically consume permanent multipliers derived at spawn without custom weapon code.
- Full cross-character isolation and deterministic character switching verified.
- Gate 12.4 is complete and checkpointed locally. Gate 12.5 (World Map Permanent Skill Tree Purchase UI / Final Integration) is NEXT. Do NOT push.

---

## Gate 12.3 — Warrior Permanent Skill Tree & Spawn-Time Modifier Binding (Automated GREEN, 2026-09-09)

### Implemented

- `Assets/Scripts/Characters/CharacterDefinition.cs`:
  - Added serialized `skillTree` field (`SkillTreeDefinition`) with public getter `SkillTree` and configuration method `SetSkillTree(SkillTreeDefinition tree)`.
- `Assets/ScriptableObjects/Progression/SkillTree_Warrior.asset`:
  - Created single authoritative Warrior `SkillTreeDefinition` ScriptableObject with 3 branches, 3 tiers per branch, 9 nodes total at 1 point per node:
    - **Durability** branch (`MaxHealthMultiplier`): T1 `warrior_durability_1` (+10%, prereq null), T2 `warrior_durability_2` (+10%, prereq T1), T3 `warrior_durability_3` (+15%, prereq T2). Total branch: +35% max health (1.35x).
    - **Power** branch (`DamageMultiplier`): T1 `warrior_power_1` (+10%, prereq null), T2 `warrior_power_2` (+10%, prereq T1), T3 `warrior_power_3` (+15%, prereq T2). Total branch: +35% damage (1.35x).
    - **Tempo** branch (`AttackSpeedMultiplier` / `MovementSpeedMultiplier`): T1 `warrior_tempo_1` (+5% attack speed, prereq null), T2 `warrior_tempo_2` (+5% move speed, prereq T1), T3 `warrior_tempo_3` (+10% attack speed, prereq T2). Total branch: +15% attack speed (1.15x), +5% movement speed (1.05x).
  - Validated cleanly via `SkillTreeDefinition.ValidateTree(out string error)`.
- `Assets/ScriptableObjects/Characters/Character_Warrior.asset`:
  - Bound serialized `skillTree` property directly to `SkillTree_Warrior.asset`.
- `Assets/Scripts/Characters/PlayerSpawner.cs`:
  - Integrated spawn-time permanent modifier binding immediately following character instantiation.
  - Resolved character's `SkillTreeDefinition` (with Editor fallback for standalone unit testing).
  - Derived aggregated `PermanentStatModifiers` via `PermanentProgression.GetPermanentModifiers(characterId, skillTree)`.
  - Applied permanent multipliers to `PlayerStats` via `SetPermanentMultipliers(damage, atkSpeed, moveSpeed, maxHealth)`.
  - Applied permanent max health scaling to `PlayerHealth` via `ApplyPermanentHealthMultiplier(maxHealthMultiplier)`.
  - Enforced the fresh health rule: on entry, `BaseMaxHealth` is preserved at 100, `MaxHealth` is scaled from base (e.g. 100 * 1.35 = 135), and `CurrentHealth` initializes to full scaled capacity (135/135).
- `Assets/Editor/Phase12AssetBuilder.cs`:
  - Automated generator utility for building and linking the Warrior skill tree ScriptableObject asset and establishing folder meta files.
- `Assets/Editor/Phase12VerificationRunner.cs`:
  - Added menu item `[MenuItem("DungeonRoguelite/Phase 12/Run Gate 12.3 Verification")]` and suite routing for `12_3`.
- `Assets/Tests/Verification/Milestone12_3_Verifier.cs`:
  - Comprehensive 200-check Play Mode verification suite covering:
    - Tree asset integrity, node count, metadata, tier hierarchy, prerequisite linkage, and `ValidateTree()` clean execution.
    - Purchase progression rules: cost deduction, duplicate purchase rejection, prerequisite gating, cross-character purchase rejection, 7-point campaign cap verification, and PlayerPrefs reload persistence.
    - Additive modifier aggregation across tiers and full 9-node unlocks (`HP: 1.35x`, `Dmg: 1.35x`, `AtkSpd: 1.15x`, `MoveSpd: 1.05x`).
    - Spawn binding on real Warrior prefab: baseline stats (100 HP, 25 dmg, 0.5s cd, 6.0 speed) vs. upgraded stats (135 HP, 33.75 dmg, 0.4348s cd, 6.3 speed).
    - Layered combat math: `Base * Permanent * Temporary` with temporary `PlayerStats` bonuses and `MeleeWeapon`/`PlayerMovement` component outputs.
    - Roguelite run semantics compatibility: Victory preservation, Defeat Retry rollback with fresh scaled HP restoration, and EndRun session teardown.
    - Character isolation: Archer and Gunner retain null skill trees and strictly neutral 1.0x multipliers upon spawn.
    - Fail-safe clamping on null/empty inputs and negative multipliers.

### Verification

- Final Unity 6000.3.23f1 batch Play Mode run: 200/200 checks passed, exit code 0 (`Logs/gate12_3.log`). All completion and state restoration markers present.
- Regression Gate 12.1 (`Milestone12_1_Verifier`): 87/87 checks passed, exit code 0 (`Logs/gate12_1_reg.log`).
- Regression Gate 12.2 (`Milestone12_2_Verifier`): 99/99 checks passed, exit code 0 (`Logs/gate12_2_reg.log`).
- Regression Suite 10.5 (`Milestone10_5_Verifier`): 11/11 checks passed, exit code 0 (`Logs/regression_10_5.log`).
- Regression Suite 11.1 (`Milestone11_1_Verifier`): 43/43 checks passed, exit code 0 (`Logs/regression_11_1.log`).
- Regression Suite 11.5 (`Milestone11_5_Verifier`): 1485/1485 checks passed, exit code 0 (`Logs/regression_11_5.log`).
- Compilation: 0 errors; 3 existing CS0618 warnings in legacy setup scripts (`TMP_Text.enableWordWrapping`).
- Working tree clean of accidental project setting changes.

### Decisions and Next Task

- Single authoritative Warrior tree (`SkillTree_Warrior.asset`) is bound to `Character_Warrior.asset`.
- Multipliers stack additively within permanent progression (`1.0 + sum(magnitudes)`), and layer multiplicatively with temporary run upgrades (`Base * Permanent * Temporary`).
- Fresh health rule is strictly enforced at spawn and retry: player enters dungeon at full scaled max health.
- Character isolation is preserved: Archer and Gunner spawn with neutral 1.0x multipliers.
- Gate 12.3 is complete and checkpointed locally. Gate 12.4 (World Map Permanent Skill Tree Purchase UI) is NEXT. Do NOT push.

---

## Gate 12.2 — Permanent Reward Economy & Dungeon Completion Integration (Automated GREEN, 2026-09-09)

### Implemented

- `Assets/Scripts/Progression/PermanentProgression.cs`:
  - Added `GetDungeonFirstClearPoints(string dungeonId)` implementing the approved economy: `dungeon_1` = 2 points, `dungeon_2` = 2 points, `dungeon_3` = 3 points (7 points total per character max; unknown dungeons = 0).
  - Added `TryAwardDungeonFirstClear(string characterId, string dungeonId, out int pointsAwarded)` validating character/dungeon strings, economy values, and invoking `AwardDungeonClearReward`.
  - Updated `IsDungeonRewardClaimed(string characterId, string dungeonId)` and `AwardDungeonClearReward` to perform case-insensitive checking and normalized lowercase storage, guaranteeing robustness against casing differences.
- `Assets/Scripts/Dungeons/DungeonCompletionController.cs`:
  - Added `PlayableCharacter activeCharacter` reference and `ActiveCharacter` property.
  - Added `BindCharacter(PlayableCharacter character)` and updated `BindPlayer`, `HandlePlayerSpawned`, `ResolveReferences`, and `SetReferences` for complete character binding across all runtime scenarios and test fixtures.
  - In `FinalizeCompletion()`: resolved character identity with cascading fallbacks (`activeCharacter`, `playerSpawner.ActiveCharacter`, `CharacterSelectionSession.SelectedCharacterId`, `playerSpawner.DefaultCharacter.Id`). Awarded first-clear points via `PermanentProgression.TryAwardDungeonFirstClear` immediately alongside `DungeonProgression.RecordDungeonCompleted`.
  - Maintained complete separation between character-specific permanent rewards and global `DungeonProgression` campaign unlocks.
  - Maintained zero coupling with `RunProgressionSession` (runs, checkpoints, retries, and temporary XP remain strictly decoupled from permanent points).
  - Added graceful fail-safe handling: invalid/empty character or dungeon IDs emit warnings without throwing or crashing.
- `Assets/Editor/Phase12VerificationRunner.cs`:
  - Added `[MenuItem("DungeonRoguelite/Phase 12/Run Gate 12.2 Verification")]` entrypoint and dynamic runner support for Suite 12_2.
- `Assets/Tests/Verification/Milestone12_2_Verifier.cs`:
  - Comprehensive 99-check Play Mode verification suite testing economy points (2/2/3), duplicate clear rejection, cross-character isolation, authoritative victory awarding, defeat exclusion, retry exclusion, return-to-map exclusion, temporary run XP decoupling, PlayerPrefs reload persistence, global dungeon unlock independence, and fail-safe handling.

### Verification

- Final Unity 6000.3.23f1 batch Play Mode run: 99/99 checks passed, exit code 0 (`Logs/gate12_2.log`). All completion and state restoration markers present.
- Regression Gate 12.1 (`Milestone12_1_Verifier`): 87/87 checks passed, exit code 0 (`Logs/gate12_1_reg.log`).
- Regression Suite 10.5 (`Milestone10_5_Verifier`): 11/11 checks passed, exit code 0 (`Logs/regression_10_5_after12_2.log`).
- Regression Suite 11.1 (`Milestone11_1_Verifier`): 43/43 checks passed, exit code 0 (`Logs/regression_11_1_after12_2.log`).
- Regression Suite 11.5 (`Milestone11_5_Verifier`): 1485/1485 checks passed, exit code 0 (`Logs/regression_11_5_after12_2.log`).
- Compilation: 0 errors; 3 existing CS0618 warnings in legacy setup scripts (`TMP_Text.enableWordWrapping`).
- Working tree clean of accidental project setting changes.

### Decisions and Next Task

- Global `DungeonProgression` unlock semantics remain unchanged.
- Permanent skill points are awarded strictly on authoritative dungeon victory in `DungeonCompletionController`. Defeat, retry, and return to map award 0 points.
- Character isolation is verified: Warrior first clears do not affect Archer or Gunner rewards.
- Gate 12.2 is complete and checkpointed locally. Gate 12.3 (Character Skill Trees & World Map Purchase UI) is NEXT. Do NOT push.

---

---

## Gate 12.1 — Permanent Progression Foundation (Automated GREEN, 2026-09-09)

### Implemented

- `Assets/Scripts/Progression/PermanentEffectType.cs`: finite typed enum (`MaxHealthMultiplier`, `DamageMultiplier`, `AttackSpeedMultiplier`, `MovementSpeedMultiplier`).
- `Assets/Scripts/Progression/SkillNodeDefinition.cs`: serializable data model containing Node ID, Character ID, Branch, Tier, Display Name, Cost, Prerequisite Node ID, Effect Type, and Magnitude.
- `Assets/Scripts/Progression/SkillTreeDefinition.cs`: ScriptableObject container with `TryGetNode` lookup and `ValidateTree` (verifying character affinity, cost >= 1, prerequisite presence, branch/tier integrity, acyclic parent graph, non-zero magnitudes).
- `Assets/Scripts/Progression/PermanentProgression.cs`: static persistence and progression service.
  - Separate per-character permanent state from global `DungeonProgression` campaign unlocks.
  - Idempotent first-clear reward claiming per character (Dungeon 1 = 2 pts, Dungeon 2 = 2 pts, Dungeon 3 = 3 pts; max 7 points per character).
  - Purchase validation (character affinity, prerequisite purchased, sufficient unspent points, unpurchased state) and transactional purchase execution.
  - Permanent stat modifier aggregation by character (`GetPermanentModifier(characterId, effectType)`).
  - Robust JSON PlayerPrefs persistence with silent fallback/recovery on corrupt JSON.
  - `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` domain reload reset for clean test/playmode isolation.
- `Assets/Scripts/Player/PlayerStats.cs`: added `permanentMaxHealthMultiplier`, `PermanentMaxHealthMultiplier`, `MaxHealthMultiplier` property, and updated `SetPermanentMultipliers(dmg, atkSpd, spd, maxHp)`.
- `Assets/Scripts/Player/PlayerHealth.cs`: preserved `BaseMaxHealth` before multipliers; added `ApplyPermanentHealthMultiplier(float)` which scales `maxHealth` from base and resets `currentHealth` to full (preserving Phase 10 fresh-health dungeon-entry semantics).
- `Assets/Tests/Verification/Phase12StateSnapshot.cs`: snapshot/restore utility isolating static session state and `PermanentProgression.PrefsKey` across tests.
- `Assets/Tests/Verification/Milestone12_1_Verifier.cs`: comprehensive Play Mode verifier (87 checks: fresh profile, awarding points, tree validation, purchase rules, character isolation, save/load persistence, corrupt JSON recovery, reward idempotency, stat aggregation, PlayerStats layering, PlayerHealth scaling, domain reload cleanup).
- `Tools/Run-Phase12Suite.ps1`: batch runner script with log filtering and exit code checking.

### Verification

- Final Unity 6000.3.23f1 batch Play Mode run: 87/87 checks passed, exit code 0 (`Logs/gate12_1.log`). All completion and state restoration markers present.
- Focused regression 10.5 (`Milestone10_5_Verifier`): 11/11 checks passed, exit code 0 (`Logs/regression_10_5.log`).
- Focused regression 11.5 (`Milestone11_5_Verifier`): 1485/1485 checks passed, exit code 0 (`Logs/regression_11_5.log`).
- Compilation: 0 errors; 3 existing CS0618 warnings in legacy setup scripts (`TMP_Text.enableWordWrapping`).
- Working tree clean of accidental project setting changes.

### Decisions and Next Task

- Permanent points are strictly character-specific and awarded per dungeon first-clear per character (D1: 2, D2: 2, D3: 3; max 7 points total). Global `DungeonProgression` remains separate.
- Stat aggregation computes multiplicative modifiers (`1.0 + sum(magnitudes)` for each effect type).
- PlayerHealth scales max health from its clean `BaseMaxHealth` and initializes to full on dungeon entry.
- Gate 12.1 is complete and checkpointed locally. Gate 12.2 (Character Skill Trees & World Map Purchase UI) is NEXT. Do NOT push.

---
- Gate 11.5: Mixed Enemy Wave Compositions & Campaign Integration (Completed & Verified)

---

## Gate 11.1 — Enemy Attack Contract, Hit Feedback & Corpse Cleanup

The user explicitly replaced the previous 11.1 meta-currency/skill-tree scope with this gate. Permanent progression remains deferred; later gates are not renumbered here.

### Implemented

- `Assets/Scripts/Enemies/IEnemyAttack.cs`: exactly `InitializeAttack(float)` and `SetTarget(Transform)`.
- `EnemyAttack` implements the interface without changing attack math, target resolution, cooldown, range or death behavior.
- `WaveManager` resolves one root-level `IEnemyAttack` per spawned instance for runtime damage scaling and explicit player binding. Existing multiplier precedence and neutral-multiplier guards are unchanged.
- `EnemyVisualFeedback` listens to health decreases below maximum, flashes both Zombie renderers white for 0.08 unscaled seconds using cached property blocks, and restores original overrides. Initialization/restoration does not flash; repeated hits restart the timer; no material instantiation is used.
- `CorpseCleanup` independently schedules destruction of the dead root after 1.5 unscaled seconds. Existing authoritative death, collision shutdown, XP drop and wave/completion listeners remain unchanged and immediate.
- Zombie prefab includes both components with explicit renderer bindings. Base health 50, damage 10, speed 3, stopping distance 1.3, range 1.5, cooldown 1, gravity 20, turn speed 720 and XP 10 are preserved. D1/D2/D3 health/damage remain 50/10, 55/10, 60/11.
- `Milestone11_1_Verifier` uses production wave configuration/start APIs, real damage and completion flows, and an alternate interface implementation. No private spawning calls or new production test-only APIs.
- `Milestone11_1_VerificationRunner` runs the gate in an unsaved empty scene. Its batch regression entrypoint bypasses older launchers that save setup changes; persistent dungeon-progress data is restored after tests.

### Verification

- Final Unity 6000.3.23f1 batch Play Mode run: 43/43 checks passed, exit code 0 (`Logs/gate11_1_final.log`). Includes first-hit/lethal feedback, post-death suppression, interface substitution, unscaled flash/cleanup, final-kill XP, upgrades and victory ordering, and unchanged material counts/references.
- Phase 10.1–10.5 regression: 47/47 checks passed; Phase 10 QA regression: 10/10 passed. Archer 8.3: 68/68 passed; Gunner 8.4: 44/44 passed, including Warrior and XP/completion integration checks. Logs: `Logs/regression_10_1.log` through `regression_10_5.log`, `regression_10_QA.log`, `regression_8_3.log`, and `regression_8_4.log`. Total final gate plus regression coverage: 212 passing checks.
- Unity compilation succeeded with no C# errors. Existing obsolete `TMP_Text.enableWordWrapping` warnings are emitted from `Milestone6_1_Setup`, `Milestone8_5_Setup`, and `Milestone9_Setup`.
- Editor startup emits `ArgumentOutOfRangeException` from `UnityEditor.Search.SearchDatabase` before the verifier starts; this is not a gameplay exception. It is not fixed in this gate.
- Gate 11.1 manual gameplay QA is GREEN by user approval. A standalone player build has not been performed. Combined Phase 11 balance/readability QA follows Gate 11.5.
- Run the gate from `DungeonRoguelite/Phase 11/Run Gate 11.1 Verification`, or batch `-executeMethod DungeonRoguelite.Editor.Milestone11_1_VerificationRunner.Run`. The regression entrypoint is `RunRegression -gateSuite 10_2` (also supports 10_1, 10_3, 10_4, 10_5, 10_QA, 8_3 and 8_4).

### Decisions and next task

- Visual timers use unscaled time so upgrade/victory/defeat pauses cannot retain a flash or corpse indefinitely.
- Cleanup destroys the entire already-dead root, avoiding invisible component shells. No pooling/resurrection support is added.
- `CurrentWaveSpawned` may contain destroyed Unity references; `ActiveEnemies` remains authoritative. Consumers must null-check historical references.
- Gate 11.1 manual gameplay QA is GREEN based on explicit user approval on 2026-09-08.
- Gate 11.1 baseline checkpoint: `72817f6`. Gates 11.2 through 11.5 are now explicitly authorized sequentially, with local checkpoints and no push. Manual QA follows Gate 11.5.

---


## Gate 11.2 — Runner (automated GREEN, 2026-09-08)

- Canonical baseline confirmed: main at 72817f617a31b6a236ab9426dfc0a99eaee94c02, initially clean.
- Added Runner prefab and amber/orange URP material using unchanged shared enemy composition. HP 25, speed 5.5, stop 1.1, melee range 1.3, damage 6, cooldown 0.65, XP 10. Visual scale 0.85; controller height 1.7, radius 0.425, center Y 0.85. No production scripts, scenes, Zombie, or wave assets changed.
- Focused verifier: 41/41 PASSED; actual D1/D2/D3 scaling 25/6, 28/6, 30/7. Real Warrior/Archer/Gunner damage, pursuit, cooldown, pause, hit flash, immediate death/living count, exactly-once XP, final upgrade/victory ordering, unscaled corpse cleanup, and prefab immutability verified.
- Focused regressions: 11.1 43/43; 10.2 15/15; Archer 8.3 68/68; Gunner 8.4 44/44. Each isolated invocation had a success marker, restoration marker, bounded process exit, and exit code 0. Total: 211 passing focused/regression checks.
- Gate 10.2 verifier corrected to existing 20/10 ranged weapon damage and 20% upgrade math; private SpawnEnemy reflection replaced with public wave configuration/start. Production balance unchanged.
- Added test-only exact static state snapshot/restore, bounded coroutine waits, timeout guard, and Windows process wrapper. Intentional assertion and timeout probes both emitted STATE RESTORED and FAILED, then exited 1. Logs remain local under Logs; legacy root result log is ignored.
- Compilation: 0 errors; 3 existing CS0618 warnings (TMP_Text.enableWordWrapping in Milestone6_1_Setup, Milestone8_5_Setup, Milestone9_Setup). No gameplay/runtime exceptions in successful suites. Initial sandbox launch stalled before licensing; terminated and rerun with approved licensing access. Successful launches had Editor shutdown Curl error 42/ADB messages; no Search exception observed in these runs.
- Historical next step at the Runner checkpoint: Gate 11.3 Tank (now complete). No push. Combined manual gameplay/balance QA remains pending after Gate 11.5.
## Gate 11.3 — Tank (automated GREEN, 2026-09-08)

- Added Tank prefab and charcoal URP material with shared composition only: HP 150, speed/stop 1.8, melee range 2, damage 22, cooldown 1.8, XP 40. Visual scale 1.4; controller height 2.8, radius 0.7, center Y 1.4. No armor, mitigation, resistance, shield, or invulnerability.
- Final focused suite: 273/273 PASSED (Logs/gate11_3_final.log). Runtime D1/D2/D3 HP/damage: 150/22, 165/22, 180/24.
- Actual unupgraded hits D1/D2/D3: Warrior 6/7/8, Archer 8/9/9, Gunner 15/17/18. With the production UpgradeManager 20% damage upgrade: Warrior 5/6/6, Archer 7/7/8, Gunner 13/14/15. Every hit checks full damage; added nonlethal child collider verifies deduplication. Production prefab has only its controller; corpse collision shutdown, nonfinal wave transition, XP once, feedback, final upgrade/victory ordering, pause and cleanup verified.
- Required regressions: 11.1 43/43, 11.2 41/41, 10.2 15/15, 8.3 68/68, 8.4 44/44. All completion/restoration markers present, bounded exits 0. Total focused plus regressions: 484 passing checks.
- One implementation-time verifier compiler error CS1061 was corrected: PlayerExperience uses GainExperience, not AddExperience. Final compilation has 0 errors and the same 3 existing CS0618 warnings. Successful suites contain no gameplay/runtime exceptions or Search exceptions. Unity-generated font/TimeManager changes restored before checkpoint.
- Runner checkpoint: 8d0a718 feat: add runner enemy archetype. Tank checkpoint is 2d489ac; Gate 11.4 is now checkpointed as 1a0846b. No push; manual QA remains after integration.
## Gate 11.4 — Ranged (automated GREEN, recovered 2026-09-09)

- Recovered the exact `[GATE 11.4 COMPLETE] PASSED: 106 checks passed.` marker in `Logs/gate11_4_final.log`, plus state restoration and normal Unity shutdown. Implementation matched the approved baseline and was checkpointed without recreation as `1a0846b feat: add ranged enemy archetype`.
- HP 35, speed 2.8, stopping distance 7, range 9, damage 8, cooldown 1.6, projectile speed 10, hard lifetime 3 unscaled seconds, XP 15. Deep violet body and child orb marker.
- One authoritative swept projectile query, hierarchy exclusions, player-only damage, solid obstacle blocking, exactly-once resolution, and shooter-owned cancellation. No friendly-fire, Rigidbody callback authority, projectile manager, or knockback.

## Gate 11.5 — Mixed Waves & Integration (focused automated GREEN, 2026-09-09)

- Recovered partial waves used stale counts and ordering; corrected them to the user's approved 11 compositions. D1: 38 enemies / 410 XP. D2: 35 / 430. D3: 42 / 540. Campaign: 115 enemies / 1380 XP, +2.2% versus Phase 10. Full compositions are recorded in GAME_DESIGN.md.
- Updated all three approved Turkish dungeon descriptions. DungeonCatalog, production scenes, spawn intervals, enemy baselines, and combat feedback remain unchanged.
- Focused verifier: **1485/1485 PASSED**, `Logs/gate11_5_final.log`, state restored, bounded process exit 0. Checks independently assert asset compositions/references/order/counts/XP; spawn all archetypes under all three dungeon multipliers; test each archetype as the final survivor; and run real production D1 -> D2 -> D3 scenes for Warrior, Archer, and Gunner, including victory/map continuity, retry rollback, defeat EndRun, upgrades, persistent unlocks, corpse cleanup and projectile cancellation.
- Successful ends: D1 Level 3 / 160 of 225 XP; D2 and D3 entry Level 5 / 27 of 506 XP; D3 Level 6 / 61 of 759 XP.
- Gate 10.2 was already corrected to production Archer 20 / Gunner 10 / Damage +20%. Gate 10.4 now expects 42 enemies and 540 XP, preserving identity/catalog/build/scaling/continuity checks and replacing private SpawnEnemy reflection with public wave APIs.
- Added a 180-second real-time deadline to the legacy regression runner. Gate 11.5 cleans scene/test objects and restores sessions, PlayerPrefs, and time scale on completion or failure.
- Implementation-time verifier issues: corrected CS1061 (`EnemyEntries.Count`, not Length); corrected an accelerated campaign harness wait that allowed an XP choice to pause spawning. The failed harness run emitted STATE RESTORED and exited 1 after 264 passing checks. Pickup collection now uses the public TryCollect API before the next wait and checks duplicate rejection. No production balance changes were made to satisfy tests.
- Final focused run: 0 compiler errors and 0 gameplay/runtime exceptions. A previous compilation emitted the three existing CS0618 TMP enableWordWrapping warnings. Full final regression results are recorded below.
- Gate 11.1 manual gameplay QA remains GREEN. User manual gameplay QA for Phase 11 is ALL GREEN across all archetypes, encounters, and dungeons.

### Final isolated regression matrix — GREEN (2026-09-09)

| Suite | Passed checks | Unity exit |
| --- | ---: | ---: |
| 11.1 | 43 | 0 |
| 11.2 | 41 | 0 |
| 11.3 | 273 | 0 |
| 11.4 | 106 | 0 |
| 11.5 | 1485 | 0 |
| 10.1 | 11 | 0 |
| 10.2 | 15 | 0 |
| 10.3 | 5 | 0 |
| 10.4 | 5 | 0 |
| 10.5 | 11 | 0 |
| Phase 10 QA | 10 | 0 |
| 8.3 Archer | 68 | 0 |
| 8.4 Gunner | 44 | 0 |
| **Total** | **2117** | |

- Every isolated suite has an explicit COMPLETE/PASSED success marker (Gunner's canonical marker is M8.4 TEST SUCCESS / ALL 44 VERIFICATION CHECKS PASSED), STATE RESTORED, no failed assertions, and a bounded Unity process exit 0. The enclosing regression process also exited 0. Logs are local ignored `Logs/phase11_final_<suite>.log` files, never tracked. Warrior remains covered by combat and campaign suites.
- Final matrix logs: **0 compiler errors, 0 emitted compiler warnings, 0 gameplay/runtime exceptions**. Compilation earlier in this session emitted the **3 pre-existing CS0618 warnings** for TMP_Text.enableWordWrapping in Milestone6_1_Setup, Milestone8_5_Setup, and Milestone9_Setup; those warnings have not been fixed or claimed resolved.
- Separate environment evidence: the first sandboxed regression launch stalled before licensing initialization, was terminated before tests, and was rerun with approved licensing access. Successful launches report an unavailable licensing access token during refresh, then license successfully, and emit normal ADB shutdown messages. No Search exception appeared in the final matrix. Historical Search exceptions above remain historical.
- Restored unrelated Unity-generated LiberationSans fallback font and TimeManager changes. No production scenes, DungeonCatalog, prefab baselines, or production C# changed during Gate 11.5. No logs are tracked and no push occurred.
- Phase 11 local checkpoints: `72817f6` attack/feedback foundation; `8d0a718` Runner; `2d489ac` Tank; `1a0846b` Ranged; `5dc3d6d` mixed wave integration (`feat: integrate mixed enemy wave compositions`).
- **Phase 11 sign-off complete:** Gates 11.1–11.5 automated GREEN (2117/2117 checks). User manual gameplay QA is ALL GREEN. Phase 12 is NEXT.

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

- **Milestone 8.5 — Character Selection UI & Integration**:
  - Implemented `CharacterRoster.cs` ScriptableObject catalog under `Assets/Scripts/Characters/` holding an ordered `List<CharacterDefinition>`.
  - Created `CharacterRoster.asset` populated with `[Warrior, Archer, Gunner]`. Validates against null definitions, duplicate references, and duplicate IDs with zero combat/stat leakage.
  - Implemented `CharacterSelectionSession.cs` lightweight static runtime carrier under `Assets/Scripts/Characters/`:
    - Pure runtime transfer mechanism carrying active selection across `SceneManager.LoadScene()`.
    - Contains zero UI, spawning, or save logic.
    - Cleared on domain reload via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` and upon entering the selection scene.
    - Survives `Dungeon_Prototype` scene reloads (preserving selected character on dungeon restart).
  - Implemented `CharacterSelectionCard.cs` UI component under `Assets/Scripts/UI/`:
    - Binds `CharacterDefinition`, displays name and description, exposes `OnCardSelected` event, and manages active visual selection highlight with strict mutual exclusivity.
  - Implemented `CharacterSelectionController.cs` under `Assets/Scripts/UI/`:
    - Reads `CharacterRoster`, initializes cards, and establishes local default selection (Warrior / first valid roster entry).
    - Local selection vs committed session separation: card clicks mutate local selection only; `CharacterSelectionSession` remains empty until the Start button is pressed.
    - Mutual exclusivity: exactly one card is visually highlighted at any time.
    - Start action: validates selection, commits choice to `CharacterSelectionSession.SetSelection()`, guards against double-clicks, and loads `Dungeon_Prototype`. Start works immediately with default Warrior without requiring an extra card click.
  - Created dedicated selection scene at `Assets/Scenes/CharacterSelect/CharacterSelection.unity`:
    - Screen Space - Overlay Canvas, single EventSystem with `InputSystemUIInputModule`, and SelectionPanel layout.
    - Zero gameplay entities, zero player controllers, zero wave managers, zero verifiers.
  - Configured `EditorBuildSettings.scenes`:
    - `0`: `Assets/Scenes/CharacterSelect/CharacterSelection.unity` (enabled)
    - `1`: `Assets/Scenes/Dungeons/Dungeon_Prototype.unity` (enabled)
    - `2`: `Assets/Scenes/SampleScene.unity` (disabled, preserved)
  - Integrated `PlayerSpawner.cs`:
    - Resolves `(CharacterSelectionSession.HasSelection && CharacterSelectionSession.SelectedCharacter != null) ? CharacterSelectionSession.SelectedCharacter : defaultCharacter`.
    - Spawns selected character archetype without character-specific branching (`if warrior`, etc.).
    - Never mutates or overwrites `defaultCharacter` on disk; direct scene launch cleanly defaults to `Character_Warrior`.
  - Restart Semantics Verified:
    - Selected Warrior -> Dungeon -> Restart -> Warrior again.
    - Selected Archer -> Dungeon -> Restart -> Archer again.
    - Selected Gunner -> Dungeon -> Restart -> Gunner again.
  - Scene Cleanliness & Asset Integrity:
    - `Dungeon_Prototype.unity` remains 100% untouched on disk, defaulting to `Character_Warrior`.
    - Zero permanent verifiers or session test state serialized into scenes or assets.
  - Milestone 8.5 Manual QA Improvements:
    - Auto-Homing Experience Collection: `ExperiencePickup` automatically acquires active player (`PlayerExperience.ActiveInstance` or dynamic fallback), hovers for 0.2s, accelerates at 25 m/s² to 12 m/s max speed, and collects on proximity (0.75m), eliminating the need for ranged characters to run onto corpses. Respects pause (`Time.timeScale <= 0f`).
    - Gunner Combat Visual Feedback: `RifleWeapon` exposes `OnShotFired(origin, endPoint)` and triggers lightweight prototype feedback: 0.04s muzzle flash at `MuzzlePoint` and 0.05s hitscan tracer line (`LineRenderer`) consuming the single authoritative hitscan endpoint. Rejected cooldown clicks produce zero visual feedback. Zero duplicate raycasts, zero ammo/reload logic.
    - Character Selection UI Visuals & Localization: Dark card background (`#161B22`) with 4-side cyan outline border on selected card for clear contrast and readability at 1280×720; large legible font sizes (Header: 50, Names: 34, Descriptions: 22, Preview: 30, Start: 26); Turkish UI text (`KARAKTERİNİ SEÇ`, `Seçilen: ...`, `ZİNDANA BAŞLA`, `Savaşçı`, `Okçu`, `Nişancı`); internal IDs strictly preserved as `warrior`, `archer`, `gunner`.
  - Verification Results:
    - Initial Automated Play Mode verification suite: 41 checks PASSED with 0 errors.
    - Milestone 8.5 QA Pass Automated Play Mode suite: all 30 checks PASSED with 0 errors and 0 runtime exceptions (`playmode_m8_5_qa_pass.log`).
    - Milestone 8.4 regression suite: all 44 checks PASSED.
    - Milestone 8.3 regression suite: all 68 checks PASSED.
    - Final Manual QA verification: confirmed GREEN across character selection readability, Turkish labels, auto-homing XP, Gunner firing feedback, and Warrior / Archer / Gunner combat.

---

# Phase 9 Implementation & Verification Summary

Phase 9 (Dungeon Progression & Campaign Flow) was executed and verified via an integrated 5-gate pipeline:

- **Milestone 9.1 — Data-Driven Dungeon Architecture & Session Carrier (Gate 9.1)**:
  - `DungeonDefinition.cs`: ScriptableObject schema holding `id`, `displayName`, `description`, `sceneName`, `requiredDungeonId`, and `waves` (`WaveDefinition[]`).
  - `DungeonRunSession.cs`: In-memory runtime carrier for active dungeon context across scene loads (`ActiveDungeon`), with fallback resolution to `Dungeon_01.asset`.
  - Created `Dungeon_01.asset` (Dungeon 1: 3 waves) and `Dungeon_02.asset` (Dungeon 2: 4 waves) under `Assets/ScriptableObjects/Dungeons/`.
  - Updated `WaveManager.cs` to copy waves dynamically from `ActiveDungeon`, eliminating hardcoded wave counts.
  - Automated Play Mode verification passed all 19 checks (`cc2646b`).

- **Milestone 9.2 — Complete Run Lifecycle & Defeat Flow (Gate 9.2)**:
  - `PlayerDefeatController.cs`: Subscribes to `PlayerHealth.OnDied`, stops player input, halts wave manager, notifies UI, and enforces mutual exclusion against victory.
  - `PlayerDefeatUI.cs`: Turkish defeat screen (`YENİLDİN`) with `YENİDEN DENE` (restarts scene) and `HARİTAYA DÖN` (loads `WorldMap`).
  - Updated `WaveManager.cs` with `HaltDungeon()` to stop active spawning and set state to `WaveState.Inactive`.
  - Updated `DungeonCompletionController.cs` to strictly suppress defeat once victory occurs and added `ReturnToWorldMap()`.
  - Updated `DungeonCompleteUI.cs` with `ReturnToMapButton` wired to `DungeonCompletionController.ReturnToWorldMap()`.
  - Automated Play Mode verification passed all 16 checks (`e8760b5`).

- **Milestone 9.3 — Campaign Progression & Persistence Foundation (Gate 9.3)**:
  - `DungeonProgression.cs`: Persistent progression manager using JSON serialization via `PlayerPrefs` (`DungeonProgression_SaveData`).
  - Supports `IsDungeonCompleted(id)`, `RecordDungeonCompleted(id)`, and prerequisite derivation `IsDungeonUnlocked(dungeon)`.
  - Hooked into `DungeonCompletionController.FinalizeCompletion()`, recording completion upon dungeon clear.
  - Automated Play Mode verification passed all 11 checks (`14acd64`).

- **Milestone 9.4 — World Map Scene & Flow Re-routing (Gate 9.4)**:
  - `DungeonCatalog.cs`: ScriptableObject holding ordered list of all playable dungeons.
  - `WorldMapCard.cs`: Card UI component displaying dungeon name, description, wave count, status badge (`AÇIK`, `KİLİTLİ`, `TAMAMLANDI`), and launch button.
  - `WorldMapController.cs`: Orchestrator querying `DungeonProgression`, binding cards, updating session `ActiveDungeon`, and loading selected dungeon scene.
  - `WorldMap.unity`: Dedicated map scene at `Assets/Scenes/WorldMap/WorldMap.unity`.
  - Re-routed `CharacterSelectionController.cs` and `CharacterSelection.unity` to launch into `"WorldMap"` instead of directly into the dungeon.
  - Registered 3-scene Build Settings: `[0: CharacterSelection, 1: WorldMap, 2: Dungeon_Prototype]`.
  - Automated Play Mode verification passed all 13 checks (`0cc8f3c`).

- **Milestone 9.5 — Second Dungeon, Full Campaign Integration & Regressions (Gate 9.5)**:
  - Created 4 dedicated wave assets for Dungeon 2 (`Wave_02_01.asset` to `Wave_02_04.asset`).
  - Configured `Dungeon_02.asset` with 4 waves and prerequisite `dungeon_1`.
  - Created `Dungeon_02.unity` scene cloned from prototype rig, with 4-wave `WaveManager`, complete defeat/completion UI, and atmospheric lighting.
  - Registered 4-scene Build Settings: `[0: CharacterSelection, 1: WorldMap, 2: Dungeon_Prototype, 3: Dungeon_02]`.
  - Automated Play Mode verification passed all 12 checks (`da18a04`).
  - Regression suites confirmed 100% green: M8.3 (68/68), M8.4 (44/44), M8.5 (30/30).

---

# Phase 9 Polish Pass & Final Verification: GREEN

## Phase 9 Manual QA Polish — GREEN

### 1. World Map Readability Polish
- Retained rich dark card body (`#101319`).
- Replaced solid fill highlight with a clean, 4-sided hollow cyan selection outline frame (`SelectionBorder`).
- High-contrast text readability: pure white titles, high-contrast off-white descriptions (`#E6ECF5`), and vibrant status badges (`AÇIK`, `KİLİTLİ`, `TAMAMLANDI`).
- Card button transitions disabled (`Transition.None`) to prevent tint washing over dark panels.

### 2. Player Health HUD
- Strictly event-driven architecture listening directly to `PlayerHealth.OnHealthChanged`.
- Zero `Update()` polling.
- Explicit runtime binding via `PlayerSpawner.OnPlayerSpawned` and clean unbinding on death/destruction.
- Universal support across all playable characters (Warrior, Archer, Gunner).
- Full retry and scene transition support with immediate rebound state.
- Accurately tracks dynamic damage and reaches `0 / 100` on death with immediate defeat popup trigger.

### 3. Dungeon Arena Layouts
- **Dungeon 1 (`Dungeon_Prototype`)**: Expanded floor scale from 30x30 to 34x34 (+13.3% linear width/depth, ~28% usable combat area), boundary walls placed at `±17m`, decorative pillars pushed outward to `(±6, ±6)`, and spawn points positioned at `(±11.5, ±11.5)`. Intentionally more open for primary encounters.
- **Dungeon 2 (`Dungeon_02`)**: Preserves tight 30x30 floor layout with pillars at `(±5, ±5)` and walls at `±15m`. Intentionally tighter and more claustrophobic for escalating 4-wave difficulty.

### 4. Manual QA Verification (Passed)
- Character Selection -> World Map -> Dungeon 1 flow verified.
- Dungeon 1 completed successfully and recorded into persistent progression.
- Dungeon 2 unlocked and visual card status updated from `KİLİTLİ` to `AÇIK`.
- Dungeon 2 entered successfully with active hero and completed 4 waves.
- Player Health UI verified visually in gameplay (current/max HP and slider fill).
- Health reduction on enemy hits verified.
- Reaching 0 HP triggers Defeat popup (`YENİLDİN`) correctly.
- Overall Phase 9 campaign flow approved: GREEN.

- **Phase 10 — Campaign Run Progression & Scaling (Milestones 10.1–10.5 Complete & Verified)**:
  - **Milestone 10.1 — Campaign Run Progression Checkpoints**:
    - Implemented `RunProgressionSession.cs` pure static runtime session carrier for transient cross-dungeon run progression (`currentLevel`, `currentXP`, `totalXPEarned`, `appliedUpgrades`).
    - Enforced strict **Commit vs. Checkpoint** semantics:
      - Progression earned during a dungeon is committed to durable session memory *only* upon achieving Dungeon Victory (`CommitActiveRunState()`).
      - An entry checkpoint is captured upon entering any dungeon (`SaveEntryCheckpoint()`).
    - Full health restoration: health is strictly excluded from `RunProgressionSession`; player always starts every dungeon and retry at 100% full health.
    - Decoupled character ownership validation: `ownerCharacterId` validates that active run progression matches `CharacterSelectionSession.SelectedCharacter.id`. If a different hero is selected, run progression resets cleanly without mutating `CharacterSelectionSession`.
    - Implemented runtime restoration without side effects:
      - `PlayerExperience.RestoreState(...)` restores level and carryover XP without triggering false level-up choice prompts or UI modal popups.
      - `UpgradeManager.ReconstructUpgrades(...)` reapplies upgrade multipliers to `PlayerStats` without opening UI or incrementing pending choice queues.
      - `PlayerSpawner.Spawn(...)` seamlessly restores the fresh clone upon instantiation.
    - Automated Play Mode verification suite (`Milestone10_1_Verifier.cs`) ran and passed all 11 checks with 0 errors. Checkpoint commit `f5f7829`.
  - **Milestone 10.2 — Data-Driven Enemy Scaling**:
    - Extended `DungeonDefinition.cs` with `EnemyHealthMultiplier` (default 1.0) and `EnemyDamageMultiplier` (default 1.0) ScriptableObject fields.
    - Implemented runtime scaling in `EnemyHealth.cs` (`InitializeHealth(float)`) and `EnemyAttack.cs` (`InitializeAttack(float)`).
    - `WaveManager.SpawnEnemy(...)` initializes each spawned enemy clone with the active dungeon's multipliers.
    - Zero asset corruption: base `Zombie.prefab` and ScriptableObject definitions remain untouched; instances scale dynamically per dungeon.
    - Tuned campaign difficulty curve:
      - Dungeon 1: 1.0x HP (50 HP), 1.0x Damage (10 dmg).
      - Dungeon 2: 1.1x HP (55 HP), 1.0x Damage (10 dmg).
      - Dungeon 3: 1.2x HP (60 HP), 1.1x Damage (11 dmg).
    - Preserved weapon kill breakpoints: player upgrade progression outpaces subtle enemy scaling, maintaining player empowerment.
    - Automated Play Mode verification suite (`Milestone10_2_Verifier.cs`) ran and passed all 7 checks with 0 errors. Checkpoint commit `7f2554b`.
  - **Milestone 10.3 — Run Lifecycle & End-Run Semantics**:
    - Defeat Retry Rollback: when the player chooses **YENİDEN DENE** (Retry), `RunProgressionSession` rolls back to the dungeon entry checkpoint (`RestoreEntryCheckpoint()`), discarding all XP and upgrades gained during the failed attempt, completely preventing death-farming exploits.
    - Defeat Return to Map: when the player chooses **HARİTAYA DÖN** (Return to Map), the active run is terminated (`EndActiveRun()`), resetting temporary progression back to baseline (Level 1, 0 XP, 0 upgrades).
    - Clear Turkish UI Communication: updated `PlayerDefeatUI.cs` with dynamic subtitle labels under the defeat buttons:
      - `[YENİDEN DENE]`: *"Zindana girişteki gelişiminle yeniden başla."*
      - `[HARİTAYA DÖN]`: *"Koşuyu sonlandır ve haritaya dön."*
    - Automated Play Mode verification suite (`Milestone10_3_Verifier.cs`) ran and passed all 5 checks with 0 errors. Checkpoint commit `729f373`.
  - **Milestone 10.4 — Playable Dungeon 3 & Multi-Dungeon Continuity**:
    - Created 4 new wave configurations for Dungeon 3 (`Wave_03_01.asset` through `Wave_03_04.asset`) totaling 50 enemies (500 XP).
    - Created `Dungeon_03.asset` (`id = "dungeon_3"`, `displayName = "Bölüm 3: Mahzenin Derinlikleri"`, `requiredDungeonId = "dungeon_2"`, 1.2x HP, 1.1x Dmg).
    - Created playable scene `Assets/Scenes/Dungeons/Dungeon_03.unity` with cold-blue crypt lighting, full gameplay rig, 4 spawn points, and obstacles.
    - Updated `DungeonCatalog.asset` to include exactly 3 playable dungeons `[dungeon_1, dungeon_2, dungeon_3]`. (No non-playable D4/D5 placeholders).
    - Configured `EditorBuildSettings` to exactly 5 scenes in sequential order:
      - `0`: `Assets/Scenes/CharacterSelect/CharacterSelection.unity`
      - `1`: `Assets/Scenes/WorldMap/WorldMap.unity`
      - `2`: `Assets/Scenes/Dungeons/Dungeon_Prototype.unity`
      - `3`: `Assets/Scenes/Dungeons/Dungeon_02.unity`
      - `4`: `Assets/Scenes/Dungeons/Dungeon_03.unity`
    - Verified multi-dungeon progression continuity across D1 -> D2 -> D3.
    - Automated Play Mode verification suite (`Milestone10_4_Verifier.cs`) ran and passed all 5 checks with 0 errors. Checkpoint commit `82bac42`.
  - **Milestone 10.5 — Progression Foundations & Architecture Invariants**:
    - Implemented 3-tier multiplier foundation in `PlayerStats.cs`:
      - Permanent multipliers: `permanentDamageMultiplier`, `permanentAttackSpeedMultiplier`, `permanentMovementSpeedMultiplier` (default 1.0).
      - Temporary multipliers: `damageMultiplier`, `attackSpeedMultiplier`, `movementSpeedMultiplier` (default 1.0).
      - Effective multipliers: `DamageMultiplier => damageMultiplier * permanentDamageMultiplier;`, etc.
    - `ResetModifiers()` clears temporary bonuses while preserving permanent foundations.
    - Invariant verification: confirmed strict architectural separation between `CharacterSelectionSession` (hero ownership), `DungeonRunSession` (dungeon ownership), and `RunProgressionSession` (campaign progression ownership). Confirmed zero health persistence in session.
    - Automated Play Mode verification suite (`Milestone10_5_Verifier.cs`) ran and passed all 10 checks with 0 errors. Checkpoint commit `c144210`.
    - Full regression suites: Gate 9.4 (World Map 13/13 checks) and Phase 9 Polish (17/17 checks) verified GREEN.
  - **Phase 10 — Manual QA Blocking Fix Pass (Finding 1 & Finding 2)**:
    - **Finding 1 (Dungeon 3 Visibility & Layout)**:
      - Root Cause: Fixed 2-card array in `WorldMapController` and `WorldMap.unity` ignored `DungeonCatalog[2]`.
      - Resolution: `WorldMapController` dynamically matches cards to catalog size (`EnsureCardsMatchCatalog()`), positions them cleanly in a horizontal row (`ApplyHorizontalCardLayout()`, X=[-500, 0, +500], width 440), and updates `Milestone9_Setup` / `Milestone10_Setup` to pre-serialize 3 cards.
      - Dynamic state: D1 AÇIK, D2 KİLİTLİ, D3 KİLİTLİ initially; D1 completed -> D2 AÇIK; D2 completed -> D3 AÇIK.
    - **Finding 2 (Run Progression Reset On Victory & Replay)**:
      - Root Causes:
        1. In `PlayerSpawner.Spawn()`, `upgradeMgr.ReconstructUpgrades` was called before `OnPlayerSpawned`, while `UpgradeManager.playerStats` was null, discarding all reconstructed upgrades.
        2. `PlayerSpawner.Spawn()` restored from entry checkpoints instead of the authoritative committed run state (`Level`, `CurrentXP`, `TotalXP`, `CommittedUpgradeIds`), and never initialized entry checkpoints upon dungeon arrival.
        3. `DungeonCompletionController.FinalizeCompletion()` read `playerExperience` and `upgradeManager` without defensive fallbacks, risking default Level 1 commits.
      - Resolution: `PlayerSpawner.Spawn()` explicitly binds `upgradeMgr.BindPlayer(exp, stats)` before reconstruction, restores from committed run state, and creates the dungeon entry checkpoint immediately. `UpgradeManager.ReconstructUpgrades` clears collected IDs before rebuilding to maintain strict idempotency. `DungeonCompletionController` defensively resolves instances via `PlayerExperience.ActiveInstance` and `FindFirstObjectByType<UpgradeManager>()`.
      - Result: Campaign run builds (Level, Total XP, accumulated upgrades, effective stat multipliers) persist flawlessly across victories and replays, roll back to entry checkpoints upon retry, and cleanly terminate only upon defeat-return-to-map or character reselection.
    - Automated Play Mode verification suite (`Milestone10_QA_Verifier.cs`) verified all 10 checks GREEN.

---

# Next Task

Phase 12: Permanent Progression / Skill Tree — NEXT.
Phase 11 (Combat & Enemy Variety) is COMPLETE.
- Gates 11.1–11.5 automated GREEN (2117/2117 regression checks).
- User manual gameplay QA ALL GREEN.
- Four archetypes implemented: Zombie, Runner, Tank, Ranged.
- Wave totals confirmed: D1 = 38 enemies / 410 XP, D2 = 35 enemies / 430 XP, D3 = 42 enemies / 540 XP, Campaign = 115 enemies / 1380 XP.

Future roadmap: Phase 12 Permanent Progression / Skill Tree; Phase 13 Dungeon 4 + Encounter Design; Phase 14 Dungeon 5 + First Boss.

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
- Character Roster Architecture: Data catalog `CharacterRoster.cs` ScriptableObject holding ordered `List<CharacterDefinition> [Warrior, Archer, Gunner]`. Pure catalog with validation against null entries and duplicate references/IDs. Zero stats, progression, or selection state.
- Character Selection Session Architecture: Pure static runtime carrier `CharacterSelectionSession.cs` passing active `CharacterDefinition` across scene transitions. Local UI selection in `CharacterSelectionController` remains decoupled until Start button is clicked. Reset on domain reload via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` and upon entering `CharacterSelection.unity`. Survives `Dungeon_Prototype` scene reloads on dungeon restart.
- PlayerSpawner Runtime Selection & Fallback: Authoritative character factory resolves `(CharacterSelectionSession.HasSelection && CharacterSelectionSession.SelectedCharacter != null) ? CharacterSelectionSession.SelectedCharacter : defaultCharacter` with zero character-type branching. Serialized `defaultCharacter` remains `Character_Warrior` on disk; direct launch safely defaults to Warrior.
- Scene Flow & Build Settings: five enabled scenes in order: CharacterSelection, WorldMap, Dungeon_Prototype, Dungeon_02, Dungeon_03. Zero gameplay entities or input interference in selection scene.
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
- **Corpse Cleanup and Object Pooling Deferred**: Dead enemies must not remain permanently in the dungeon. On death, gameplay collision, movement, and attack stop immediately; when character animations are introduced, play a short death animation, followed by a short fade/dissolve after ~1–2 seconds, then remove/destroy the corpse. Object pooling will be considered later only if enemy counts/performance justify it. Intentionally deferred until enemy model/animation pipeline is introduced.
- **Localization & Settings Deferred**: Current player-facing prototype UI is in Turkish. Proper localization (Turkish, English) selectable via a future Settings menu is planned. Stable internal IDs (`warrior`, `archer`, `gunner`) remain language-independent.
- **Gunner Full-Auto / Held-Fire Input Semantics Deferred**: Gunner prototype uses semi-auto input with a 0.18s cooldown. Full-auto / held-fire input semantics remain deferred and will be re-evaluated after manual gameplay testing with muzzle flash and tracer feedback.
- **Final Presentation Polish Deferred**: Final 3D character models, character animations, weapon animations, death animations, final muzzle flash / projectile VFX, sound effects, and final UI art / portraits are deferred until dedicated visual/audio polish phases.
- **Archer Firing Feedback Polish Deferred**: Bow attack could be made more visually explicit later (e.g. bow release / arrow launch feedback). Fully functional currently; polish is deferred.
- **Auto-Homing Experience Collection (Implemented in M8.5 QA Pass)**: Previously deferred pickup magnetism was resolved by implementing physical auto-homing pickups in Milestone 8.5 QA pass (0.2s hover, 25 m/s² acceleration, 12 m/s max speed toward player).
- **Direct Pursuit vs Pathfinding**: Current `EnemyMovement` uses direct `CharacterController` pursuit. This is sufficient for the prototype but does not provide full pathfinding around complex dungeon geometry. Re-evaluate NavMesh when dungeon layouts require real obstacle navigation.
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

## Milestone 8.5 Verification

Automated Play Mode verification suite ran and passed all 41 checks in Unity (`playmode_m8_5.log`):
- **Check 1**: CharacterDefinition schema contains zero combat calculation or stat fields (PASSED).
- **Check 2**: CharacterRoster.asset exists and loaded successfully (PASSED).
- **Check 3**: CharacterRoster order verified: [Warrior, Archer, Gunner] (PASSED).
- **Check 4**: CharacterRoster.ValidateRoster passed with zero errors or duplicates (PASSED).
- **Check 5**: All roster CharacterDefinitions point to valid prefabs tagged 'Player' (PASSED).
- **Check 6**: EditorBuildSettings registered: 0 = CharacterSelection, 1 = Dungeon_Prototype, SampleScene disabled (PASSED).
- **Check 7**: CharacterSelection scene contains zero gameplay PlayableCharacter instances (PASSED).
- **Check 8**: Exactly one EventSystem exists with InputSystemUIInputModule (PASSED).
- **Check 9**: Dungeon_Prototype.unity on disk defaults to Character_Warrior (PASSED).
- **Check 10**: CharacterSelectionSession is strictly empty upon entering CharacterSelection (PASSED).
- **Check 11**: CharacterSelectionController cards bound to [Warrior, Archer, Gunner] (PASSED).
- **Check 12**: Default local selection is Warrior upon scene entry (PASSED).
- **Check 13**: Mutual exclusivity verified on start: only Warrior card is visually selected (PASSED).
- **Check 14**: Preview text shows: 'Selected: Warrior' (PASSED).
- **Check 15**: Selecting Archer updated local selection and card highlight (mutual exclusivity preserved) (PASSED).
- **Check 16**: Local card selection did NOT mutate CharacterSelectionSession (session remains empty) (PASSED).
- **Check 17**: Selecting Gunner updated local selection and card highlight (PASSED).
- **Check 18**: Re-selecting Warrior restores Warrior selection and highlight (PASSED).
- **Check 19**: CharacterSelectionSession.SetSelection committed Archer accurately (PASSED).
- **Check 20**: CharacterSelectionSession.Clear cleanly resets session (PASSED).
- **Check 21**: PlayerSpawner contains zero character-specific branching methods (PASSED).
- **Check 22**: Empty session cleanly falls back to default Character_Warrior (PASSED).
- **Check 23**: PlayerSpawner.defaultCharacter was NOT overwritten by session selection (PASSED).
- **Check 24**: PlayerSpawner consumed session selection and spawned Archer with BowWeapon (PASSED).
- **Check 25**: PlayerSpawner consumed session selection and spawned Gunner with RifleWeapon (PASSED).
- **Check 26**: PlayerSpawner consumed session selection and spawned Warrior with MeleeWeapon (PASSED).
- **Check 27**: Warrior selection survives simulated scene reload for dungeon restart (PASSED).
- **Check 28**: Archer selection survives simulated scene reload for dungeon restart (PASSED).
- **Check 29**: Gunner selection survives simulated scene reload for dungeon restart (PASSED).
- **Check 30**: Exactly one player spawned in Dungeon_Prototype: Gunner (PASSED).
- **Check 31**: CameraFollow explicitly bound to runtime Gunner transform (PASSED).
- **Check 32**: WaveManager playerTarget explicitly bound to runtime Gunner transform (PASSED).
- **Check 33**: UpgradeManager explicitly bound to runtime Gunner PlayerExperience and PlayerStats (PASSED).
- **Check 34**: PlayerExperienceUI explicitly bound to runtime Gunner PlayerExperience (PASSED).
- **Check 35**: DungeonCompletionController explicitly bound to runtime Gunner PlayerExperience (PASSED).
- **Check 36**: Gunner hitscan combat executed successfully in dungeon (PASSED).
- **Check 37**: Archer ranged combat regression verified: bow fired arrow (PASSED).
- **Check 38**: Warrior melee combat regression verified: sword swing executed (PASSED).
- **Check 39**: Universal PlayerStats damage bonus scales EffectiveDamage (+20%) (PASSED).
- **Check 40**: DungeonCompletionController completion flow finalized successfully (PASSED).
- **Check 41**: CharacterSelectionSession cleanly reset after test execution (PASSED).

### Milestone 8.5 Manual QA Fix Pass Verification

Automated Play Mode verification suite ran and passed all 30 checks in Unity (`playmode_m8_5_qa_pass.log`):
- **Checks 1–9 (Auto-Homing Experience Collection)**:
  - Check 1: ActiveInstance resolved in O(1) on PlayerExperience start (PASSED).
  - Check 2: Dynamic fallback resolution functions correctly when static instance unassigned (PASSED).
  - Check 3: ActiveInstance cleanly resets on domain reload/scene unmount (PASSED).
  - Check 4: ExperiencePickup delay period (0.2s) strictly enforced; zero premature movement (PASSED).
  - Check 5: ExperiencePickup moves continuously toward player after delay expires (PASSED).
  - Check 6: ExperiencePickup speed accelerates cleanly (PASSED).
  - Check 7: ExperiencePickup collects at collection distance (0.75m) and awards XP (PASSED).
  - Check 8: Single collection guaranteed; zero duplicate XP awards or double trigger events (PASSED).
  - Check 9: Time.timeScale = 0 freezes pickup movement completely (PASSED).
- **Checks 10–15 (Gunner Firing Feedback)**:
  - Check 10: Successful RifleWeapon shot triggers OnShotFired event with exact hitscan origin and endPoint (PASSED).
  - Check 11: Cooldown-rejected shot does NOT trigger OnShotFired or visual feedback (PASSED).
  - Check 12: Muzzle flash visual active on shot and disables after 0.04s (PASSED).
  - Check 13: Tracer LineRenderer points accurately from muzzle to endpoint and disables after 0.05s (PASSED).
  - Check 14: Single authoritative query; zero duplicate raycasts or physics queries (PASSED).
  - Check 15: Zero ammo, reload, or magazine state logic present in weapon component (PASSED).
- **Checks 16–24 (Character Selection UI Visuals & Localization)**:
  - Check 16: Header title displays 'KARAKTERİNİ SEÇ' (PASSED).
  - Check 17: Warrior card displays 'Savaşçı' and Turkish description (PASSED).
  - Check 18: Archer card displays 'Okçu' and Turkish description (PASSED).
  - Check 19: Gunner card displays 'Nişancı' and Turkish description (PASSED).
  - Check 20: Card dimensions are 340x480 with dark background (#161B22) (PASSED).
  - Check 21: Selected card displays 4-sided cyan outline border with zero full cyan fill (PASSED).
  - Check 22: Unselected cards hide selection outline border (PASSED).
  - Check 23: Start button displays 'ZİNDANA BAŞLA' (PASSED).
  - Check 24: Preview label displays 'Seçilen: Savaşçı' dynamically (PASSED).
- **Checks 25–30 (Scene Integrity & Multi-Character Integration Regression)**:
  - Check 25: CharacterSelectionSession transfers selected character cleanly to spawner (PASSED).
  - Check 26: PlayerSpawner cleanly falls back to default Character_Warrior without mutation (PASSED).
  - Check 27: PlayerSpawner contains zero character-specific branching methods (PASSED).
  - Check 28: Character selection survives simulated scene reload for restart across all 3 characters (PASSED).
  - Check 29: Full scene transition into Dungeon_Prototype instantiated Gunner and explicitly bound all scene systems (PASSED).
  - Check 30: Dungeon_Prototype.unity on disk retains Character_Warrior default and zero permanent verifiers (PASSED).

Milestone 8.4 Gunner regression suite re-run: all 44 checks PASSED (`playmode_m8_4_regression.log`).
Milestone 8.3 Archer regression suite re-run: all 68 checks PASSED (`playmode_m8_3_regression.log`).
Manual player-facing verification: confirmed GREEN across character selection readability, Turkish labels, auto-homing XP, Gunner firing feedback, and Warrior / Archer / Gunner combat.

---

# Recent Git Checkpoint

```text
Phase 10 commit chain (on branch main):
f5f7829 feat: add campaign run progression checkpoints (Gate 10.1)
7f2554b feat: add data-driven dungeon difficulty scaling (Gate 10.2)
729f373 feat: integrate run lifecycle and rollback semantics (Gate 10.3)
82bac42 feat: add third dungeon and multi-dungeon run continuity (Gate 10.4)
c144210 feat: add Phase 10 progression foundations (Gate 10.5)
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
- Phase 10 (Campaign Run Progression & Scaling, Milestones 10.1–10.5) executed and verified across all 5 gates.
- Assets/Scripts/Progression/RunProgressionSession.cs: pure static runtime session carrier for transient campaign progression with commit/checkpoint semantics.
- Assets/Scripts/Player/PlayerExperience.cs: added RestoreState(level, xp, totalXp) for silent state rehydration without triggering level-up popups.
- Assets/Scripts/Upgrades/UpgradeManager.cs: added ReconstructUpgrades(appliedUpgrades) to reapply stat bonuses without UI modal queues.
- Assets/Scripts/Characters/PlayerSpawner.cs: integrated run state restoration into Spawn() factory, decoupling spawner from progression logic.
- Assets/Scripts/Dungeons/DungeonDefinition.cs: added enemyHealthMultiplier and enemyDamageMultiplier scaling fields.
- Assets/Scripts/Combat/EnemyHealth.cs: added InitializeHealth(multiplier) for runtime instance scaling.
- Assets/Scripts/Combat/EnemyAttack.cs: added InitializeAttack(multiplier) for runtime instance scaling.
- Assets/Scripts/Waves/WaveManager.cs: updated SpawnEnemy to scale instantiated enemies via active dungeon multipliers.
- Assets/Scripts/Dungeons/PlayerDefeatController.cs: wired defeat retry rollback to entry checkpoint and defeat map return to end active run.
- Assets/Scripts/UI/PlayerDefeatUI.cs: added dynamic Turkish explanatory subtitle labels under defeat buttons.
- Assets/Scenes/Dungeons/Dungeon_03.unity: created third playable dungeon scene with cold-blue crypt ambiance, 4 spawn points, and obstacles.
- Assets/ScriptableObjects/Dungeons/Dungeon_03.asset: created Dungeon 3 definition (1.2x HP, 1.1x Dmg, prereq: dungeon_2).
- Assets/ScriptableObjects/Waves/Wave_03_01.asset .. Wave_03_04.asset: created 4 wave assets for Dungeon 3 (50 enemies, 500 XP).
- Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset: updated to include exactly 3 playable dungeons [Dungeon_01, Dungeon_02, Dungeon_03].
- ProjectSettings/EditorBuildSettings.asset: updated to 5 scenes in sequential order:
  [0: CharacterSelection, 1: WorldMap, 2: Dungeon_Prototype, 3: Dungeon_02, 4: Dungeon_03].
- Assets/Scripts/Player/PlayerStats.cs: implemented 3-tier multiplier foundation (Effective = Base * Permanent * Temporary) with SetPermanentMultipliers().
- Assets/Editor/Milestone10_Setup.cs: comprehensive automation tool for setup and verification across all 5 Phase 10 gates.
- Assets/Tests/Verification/Milestone10_1_Verifier.cs through Milestone10_5_Verifier.cs: automated Play Mode verification suites for all gates.
```

## Changed Files

```text
- Assets/Editor/Milestone10_Setup.cs (+ .meta)
- Assets/Scenes/Dungeons/Dungeon_02.unity
- Assets/Scenes/Dungeons/Dungeon_03.unity (+ .meta)
- Assets/Scenes/Dungeons/Dungeon_Prototype.unity
- Assets/ScriptableObjects/Dungeons/DungeonCatalog.asset
- Assets/ScriptableObjects/Dungeons/Dungeon_01.asset
- Assets/ScriptableObjects/Dungeons/Dungeon_02.asset
- Assets/ScriptableObjects/Dungeons/Dungeon_03.asset (+ .meta)
- Assets/ScriptableObjects/Waves/Wave_03_01.asset .. Wave_03_04.asset (+ .meta)
- Assets/Scripts/Characters/PlayerSpawner.cs
- Assets/Scripts/Combat/EnemyAttack.cs
- Assets/Scripts/Combat/EnemyHealth.cs
- Assets/Scripts/Dungeons/DungeonCompletionController.cs
- Assets/Scripts/Dungeons/DungeonDefinition.cs
- Assets/Scripts/Dungeons/PlayerDefeatController.cs
- Assets/Scripts/Experience/PlayerExperience.cs
- Assets/Scripts/Player/PlayerStats.cs
- Assets/Scripts/Progression/RunProgressionSession.cs (+ .meta)
- Assets/Scripts/UI/CharacterSelectionController.cs
- Assets/Scripts/UI/PlayerDefeatUI.cs
- Assets/Scripts/Upgrades/UpgradeManager.cs
- Assets/Scripts/Waves/WaveManager.cs
- Assets/Tests/Verification/Milestone10_1_Verifier.cs .. Milestone10_5_Verifier.cs (+ .meta)
- ProjectSettings/EditorBuildSettings.asset
- GAME_DESIGN.md
- ROADMAP.md
- PROJECT_STATUS.md
```

## Tested

```text
- Gate 10.1 Play Mode Verification: 11/11 checks PASSED (Commit/checkpoint, silent restoration, character change reset).
- Gate 10.2 Play Mode Verification: 15/15 checks PASSED (Enemy scaling math, instance scaling without asset mutation, Warrior 2.5m melee reach, weapon arc/facing isolation).
- Gate 10.3 Play Mode Verification: 5/5 checks PASSED (Defeat retry rollback, map return end run, Turkish UI subtitles, direct launch neutrality).
- Gate 10.4 Play Mode Verification: 5/5 checks PASSED (Dungeon 3 playable, 4 waves, 5 scenes registered in build settings, full D1->D2->D3 run continuity).
- Gate 10.5 Play Mode Verification: 11/11 checks PASSED (PlayerStats 3-tier layering, session decoupling invariants, zero HP in session, Boss/Chapter classification foundation).
- Full Phase 10 Sequential Regression: 47/47 checks PASSED back-to-back in Unity batchmode with 0 errors and 0 warnings.
- 0 compile errors, 0 runtime exceptions across all test suites.
```

## Manual QA Verification Plan (For User)

```text
1. Full Campaign Run Flow (D1 -> D2 -> D3):
   - Start from CharacterSelection scene.
   - Pick any character (e.g. Archer).
   - Enter Dungeon 1, defeat enemies, level up and pick upgrades (e.g. Damage +20%).
   - Clear Dungeon 1 -> Click "HARİTAYA DÖN" or proceed to World Map.
   - Confirm Dungeon 2 is unlocked. Select and enter Dungeon 2.
   - Verify character enters Dungeon 2 with the Level, XP, and upgrades earned in Dungeon 1.
   - Verify health starts at 100% full.
   - Clear Dungeon 2 -> Confirm Dungeon 3 is unlocked.
   - Enter Dungeon 3 -> Verify character enters at current Level with accumulated upgrades intact.

2. Defeat Retry Rollback (Anti-Farming Test):
   - In Dungeon 2 or 3, earn some XP/levels, then intentionally let zombies kill the player.
   - Observe defeat UI subtitles:
     - [YENİDEN DENE]: "Zindana girişteki gelişiminle yeniden başla."
     - [HARİTAYA DÖN]: "Koşuyu sonlandır ve haritaya dön."
   - Click "YENİDEN DENE".
   - Verify the dungeon restarts with the EXACT stats/level from when you first entered the dungeon (XP gained during failed run is cleanly discarded).
   - Verify health starts at 100% full.

3. Defeat Map Return (Run Termination Test):
   - In any dungeon, die and click "HARİTAYA DÖN".
   - From World Map, re-enter Dungeon 1.
   - Verify the character starts a fresh run at Level 1 with 0 XP and 0 upgrades.

4. Warrior Melee Reach (2.5m Reach Feel Check):
   - Select Warrior from CharacterSelection.
   - Enter Dungeon 1.
   - Test sword swing spacing against approaching zombies.
   - Verify sword connects at comfortable spacing without requiring point-blank collision, maintaining clean hit confirmation.
```

## Known Issues

```text
Unity Editor Search.SearchDatabase throws a startup indexing ArgumentOutOfRangeException in this worktree before tests begin.
Three legacy setup scripts emit obsolete TMP_Text.enableWordWrapping warnings; no C# compilation errors or gameplay errors observed.
```

## Next Task

```text
Phase 12: Permanent Progression / Skill Tree.
Phase 11 is complete and signed off (Gates 11.1–11.5 automated GREEN, 2117/2117 regression checks, user manual gameplay QA ALL GREEN).
Awaiting Phase 12 kickoff.
```

## Latest Verified Commit

```text
5dc3d6d feat: integrate mixed enemy wave compositions
Phase 11 automated verification GREEN (2117/2117); user manual gameplay QA ALL GREEN.
Phase 11 complete and signed off.
```
