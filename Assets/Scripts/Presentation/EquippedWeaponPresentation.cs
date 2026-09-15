using System;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Progression;
using UnityEngine;

namespace DungeonRoguelite.Presentation
{
    // Visual variants only. Combat components and their origins stay on the actor.
    [DisallowMultipleComponent]
    public sealed class EquippedWeaponPresentation : MonoBehaviour
    {
        [SerializeField] private PlayableCharacter character;
        [SerializeField] private GameObject baseWeaponVisual;
        [SerializeField] private WeaponVisualEntry[] tierWeapons;

        // PlayerSpawner assigns identity during Spawn, before the first Start/render.
        private void Start() => RefreshVisuals();

        public void RefreshVisuals()
        {
            var definition = character != null ? character.CharacterDefinition : null;
            var equipped = definition != null
                ? PermanentProgression.GetEquippedWeapon(definition.Id, definition.WeaponCatalog) : null;
            GameObject selected = baseWeaponVisual;
            if (tierWeapons != null)
                foreach (var entry in tierWeapons)
                    if (entry.visualObject != null && equipped != null &&
                        string.Equals(entry.weaponId, equipped.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        selected = entry.visualObject;
                        break;
                    }
            if (baseWeaponVisual != null) baseWeaponVisual.SetActive(false);
            if (tierWeapons != null)
                foreach (var entry in tierWeapons)
                    if (entry.visualObject != null) entry.visualObject.SetActive(false);
            if (selected != null) selected.SetActive(true);
        }
    }
}
