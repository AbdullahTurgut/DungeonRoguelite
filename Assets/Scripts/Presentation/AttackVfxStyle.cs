using DungeonRoguelite.Characters;
using DungeonRoguelite.Progression;
using UnityEngine;
using UnityEngine.Rendering;

namespace DungeonRoguelite.Presentation
{
    // Shared rendering setup only; never supplies combat values or equipment state.
    internal static class AttackVfxStyle
    {
        private static Material material;
        public static Material Material => material != null ? material :
            (material = Resources.Load<Material>("PlayerAttackVfx"));

        public static int EquippedTier(Component weapon)
        {
            var character = weapon.GetComponentInParent<PlayableCharacter>();
            var definition = character != null ? character.CharacterDefinition : null;
            var equipped = definition != null
                ? PermanentProgression.GetEquippedWeapon(definition.Id, definition.WeaponCatalog) : null;
            return equipped != null ? Mathf.Clamp(equipped.Tier, 0, 2) : 0;
        }

        public static void Configure(LineRenderer line, Color color, float width)
        {
            line.sharedMaterial = Material;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.alignment = LineAlignment.View;
            line.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.15f);
            line.widthMultiplier = width;
            line.startColor = color;
            line.endColor = new Color(color.r, color.g, color.b, 0f);
            line.numCapVertices = 2;
        }
    }
}
