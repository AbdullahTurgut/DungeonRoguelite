using System;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Characters;
using UnityEngine;

namespace DungeonRoguelite.Presentation
{
    [Serializable]
    public struct WeaponVisualEntry
    {
        public string weaponId;
        public GameObject visualObject;
    }

    public class HubHeroPresentation : MonoBehaviour
    {
        [SerializeField] private CharacterDefinition characterDef;
        [SerializeField] private GameObject baseWeaponVisual;
        [SerializeField] private WeaponVisualEntry[] tierWeapons;
        [SerializeField] private Animator animator;
        [SerializeField] private string idleStateName = "Idle";

        private void OnEnable()
        {
            RefreshVisuals();
            if (animator != null && !string.IsNullOrEmpty(idleStateName))
            {
                animator.Play(idleStateName, 0, UnityEngine.Random.value);
            }
        }

        public void RefreshVisuals()
        {
            if (characterDef == null) return;
            var equipped = PermanentProgression.GetEquippedWeapon(characterDef.Id, characterDef.WeaponCatalog);

            string equippedId = equipped != null ? equipped.Id : "";

            // Select one valid visual before changing activation; missing tiers retain the starter.
            GameObject selectedVisual = baseWeaponVisual;
            if (tierWeapons != null)
            {
                foreach (var entry in tierWeapons)
                {
                    if (entry.visualObject != null &&
                        string.Equals(entry.weaponId, equippedId, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedVisual = entry.visualObject;
                        break;
                    }
                }
            }
            if (baseWeaponVisual != null) baseWeaponVisual.SetActive(false);
            if (tierWeapons != null)
                foreach (var entry in tierWeapons)
                    if (entry.visualObject != null) entry.visualObject.SetActive(false);
            if (selectedVisual != null) selectedVisual.SetActive(true);
        }
    }
}
