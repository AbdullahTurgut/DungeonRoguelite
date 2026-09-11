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
