# Warrior / Zombie visual provenance

Production subset retained after user manual QA ALL GREEN. No environment conversion or gameplay changes.

| Asset | Creator | Original source | License | Attribution | Raw redistribution |
| --- | --- | --- | --- | --- | --- |
| KayKit Adventurers FREE 2.0: Knight, sword, texture | Kay Lousberg | https://kaylousberg.itch.io/kaykit-adventurers | CC0 1.0 Universal | Not required; voluntary credit retained | Commercial use, modification and public source redistribution permitted |
| KayKit Character Animations FREE 1.1: four Rig_Medium sets | Kay Lousberg | https://kaylousberg.itch.io/kaykit-character-animations | CC0 1.0 Universal | Not required; voluntary credit retained | Commercial use, modification and public source redistribution permitted |
| Animated Characters 3: characterMedium and zombieMaleA texture | Kenney | https://kenney-assets.itch.io/animated-characters-3 | CC0 1.0 Universal | Not required; voluntary credit retained | Commercial use, modification and public source redistribution permitted |

License: https://creativecommons.org/publicdomain/zero/1.0/ . Original bundled license notices are retained alongside this record. Permissions were checked before import. Original FBX/PNG contents remain unmodified; Kenney zombieMaleA.png is named Zombie.png locally. Unity importer settings, URP/Lit materials, controllers and presentation bindings are project-authored adaptations.

Production files live under Assets/Art/ThirdParty, Materials and AnimationControllers. All four animation FBX containers are required by the controllers. They supply armed/unarmed idle, forward run, backward walk, left/right strafe, sword slice, unarmed punch and Death_A. Original source containers retain their vendor clip data; only required clips are selected for Unity import. No vendor controllers, demo scenes, environment assets or custom shaders are required.

Play Mode validation confirmed valid Knight/Kenney Humanoid avatars and retargeted movement, attacks and death. Root motion is disabled. Sword slice source length: 1.1 seconds; Death_A: 0.8 seconds. Presentation follows existing attack/health events; gameplay movement, damage, timing, colliders and anchors remain authoritative. User manual QA accepted the production integration, including the backward walk and Kenney's modern-clothing style compromise.

Rejected candidate history: Cartoon Zombie Rigged by Vinrax (https://opengameart.org/content/cartoon-zombie-rigged, CC BY 3.0) failed Humanoid hierarchy validation: LegLower.L is not an ancestor of Foot.L. No rig repair was forced. Its model, texture, material and validation-only tools/scene were removed; there is no Vinrax production dependency.

Phase 22A reuses these same CC0 sources: Kenney characterMedium for Runner and Ranged; KayKit Knight for Tank. Role tints and Ranged primitive hood/mantle are project-authored. Walking_A from the existing MovementBasic container is additionally imported for Tank locomotion. No new vendor downloads, shaders, controllers or license obligations. Presentation manual QA is ALL GREEN.

Phase 22B integrates visual models and animations for Archer and Gunner:
- Archer uses KayKit Adventurers FREE 2.0 Ranger model (`Ranger.fbx`), texture (`ranger_texture.png`), and bow (`bow.fbx`), with KayKit Character Animations FREE 1.1 `Rig_Medium_CombatRanged.fbx` (`Ranged_Bow_Idle`, `Ranged_Bow_Release`) and `Rig_Medium_MovementAdvanced.fbx` (`Running_HoldingBow`).
- Gunner uses KayKit Adventurers FREE 2.0 Rogue model (`Rogue.fbx`) and texture (`rogue_texture.png`), with KayKit Character Animations FREE 1.1 `Rig_Medium_CombatRanged.fbx` (`Ranged_2H_Aiming`, `Ranged_2H_Shooting`) and `Rig_Medium_MovementAdvanced.fbx` (`Running_HoldingRifle`).
- Gunner held firearm is a project-authored stylized carbine visual placeholder attached to Rogue's hand, using project materials `Firearm.mat` (brushed steel) and `FirearmDark.mat` (dark tactical polymer).
- All third-party assets are CC0 1.0 Universal from Kay Lousberg (KayKit). Combat origins, colliders, timings, and gameplay logic remain strictly authoritative and unchanged. Manual QA is ALL GREEN.
