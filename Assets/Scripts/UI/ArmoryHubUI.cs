using System;
using System.Linq;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Dungeons;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DungeonRoguelite.UI
{
    /// <summary>Weapon management for the character selected by the campaign flow.</summary>
    public sealed class ArmoryHubUI : MonoBehaviour
    {
        [SerializeField] private CharacterDefinition[] characters;
        [SerializeField] private DungeonRoguelite.Presentation.HubHeroPresentation[] heroPresentations;
        [SerializeField] private Button[] characterButtons;
        [SerializeField] private TMP_Text[] characterLabels;
        [SerializeField] private GameObject[] selectionMarkers;
        [SerializeField] private GameObject[] heroDisplayRoots;
        [SerializeField] private GameObject[] heroPedestals;
        [SerializeField] private Button armorerButton;
        [SerializeField] private Button returnButton;
        [SerializeField] private GameObject equipmentPanel;
        [SerializeField] private TMP_Text equipmentText;
        [Header("Weapon Preview")]
        [SerializeField] private Image weaponPreviewImage;
        [SerializeField] private TMP_Text weaponPreviewFallback;
        [SerializeField] private WeaponPreviewEntry[] weaponPreviews;

        [Serializable]
        private struct WeaponPreviewEntry
        {
            public CharacterDefinition character;
            [Tooltip("Leave empty for this character's Base weapon.")]
            public WeaponDefinition weapon;
            public Sprite sprite;
        }
        [SerializeField] private Button claimButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button baseButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button cycleWeaponButton;
        [SerializeField] private TMP_Text campaignAcknowledgement;
        private int selectedIndex = -1;
        private int offeredIndex;
        private WeaponDefinition offeredWeapon;
        private BlacksmithIntroSequence intro;
        public CharacterDefinition SelectedCharacter => selectedIndex >= 0 ? characters[selectedIndex] : null;

        private void Start()
        {
            if (!DungeonProgression.IsDungeonCompleted("dungeon_5")) { SceneManager.LoadScene("WorldMap"); return; }
            if (!ConfigureForCurrentCharacter())
            {
                Debug.LogWarning("Armory requires an existing selected character; returning to the map.");
                SceneManager.LoadScene("WorldMap");
                return;
            }
            armorerButton.onClick.AddListener(OpenArmorer);
            returnButton.onClick.AddListener(() => SceneManager.LoadScene("WorldMap"));
            claimButton.onClick.AddListener(Claim);
            equipButton.onClick.AddListener(Equip);
            baseButton.onClick.AddListener(() => { PermanentProgression.TryEquipWeapon(SelectedCharacter.Id, null); offeredIndex = 0; Refresh(); });
            closeButton.onClick.AddListener(CloseArmorer);
            if (cycleWeaponButton != null) cycleWeaponButton.onClick.AddListener(() => { offeredIndex++; Refresh(); });
            if (campaignAcknowledgement != null && DungeonProgression.IsFirstCampaignCompleted)
                campaignAcknowledgement.text = "YILDIZSIZ TAHT AŞILDI • İLK SEFER TAMAMLANDI • TIER II HAZIR";
            equipmentPanel.SetActive(false);
            Refresh();
            if (PermanentProgression.NeedsSecondBlacksmithIntro || PermanentProgression.NeedsBlacksmithIntro)
            {
                int tier = PermanentProgression.NeedsSecondBlacksmithIntro ? 2 : 1;
                intro = gameObject.AddComponent<BlacksmithIntroSequence>();
                intro.Begin(SelectedCharacter, SelectedCharacter.WeaponCatalog.Weapons.FirstOrDefault(w => w != null && w.CharacterId == SelectedCharacter.Id && w.Tier == tier), Refresh);
            }
        }

        private bool ConfigureForCurrentCharacter()
        {
            string id = CharacterSelectionSession.SelectedCharacterId;
            if (string.IsNullOrEmpty(id) && RunProgressionSession.HasActiveRun)
                id = RunProgressionSession.OwnerCharacterId;
            selectedIndex = Array.FindIndex(characters, c => c != null &&
                string.Equals(c.Id, id, StringComparison.OrdinalIgnoreCase));
            for (int i = 0; i < characters.Length; i++)
            {
                bool customer = i == selectedIndex;
                heroDisplayRoots[i].SetActive(customer);
                heroPedestals[i].SetActive(customer);
                characterButtons[i].interactable = false;
                characterButtons[i].gameObject.SetActive(false);
                selectionMarkers[i].SetActive(false);
            }
            offeredIndex = 0;
            offeredWeapon = null;
            return selectedIndex >= 0;
        }

        public void OpenArmorer()
        {
            if (SelectedCharacter == null) return;
            if (intro != null && intro.IsRunning) return;
            if (!equipmentPanel.activeSelf) offeredIndex = 0;
            equipmentPanel.SetActive(true);
            Refresh();
            closeButton.Select();
        }
        private void CloseArmorer()
        {
            equipmentPanel.SetActive(false);
            Refresh();
            armorerButton.Select();
        }
        private void Claim() { PermanentProgression.TryClaimWeapon(SelectedCharacter.Id, offeredWeapon); Refresh(); }
        private void Equip() { PermanentProgression.TryEquipWeapon(SelectedCharacter.Id, offeredWeapon); Refresh(); }

        private void Refresh()
        {
            if (SelectedCharacter == null) return;
            if (heroPresentations != null)
                foreach (var presentation in heroPresentations)
                    if (presentation != null) presentation.RefreshVisuals();
            for (int i = 0; i < characters.Length; i++)
            {
                var equipped = PermanentProgression.GetEquippedWeapon(characters[i].Id, characters[i].WeaponCatalog);
                bool selected = i == selectedIndex;
                characterLabels[i].text = characters[i].DisplayName + (selected ? "\n" + (equipped != null ? equipped.DisplayName + " • Tier " + (equipped.Tier == 1 ? "I" : "II") : "Başlangıç silahı") : "");
                characterLabels[i].color = selected ? Color.white : new Color(.65f, .68f, .72f);
                characterButtons[i].GetComponent<Image>().color = selected ? new Color(.22f,.32f,.4f,.9f) : new Color(.1f,.13f,.17f,.35f);
                selectionMarkers[i].SetActive(false);
            }
            string characterId = SelectedCharacter.Id;
            var choices = SelectedCharacter.WeaponCatalog.Weapons.Where(w => w != null &&
                string.Equals(w.CharacterId, characterId, StringComparison.OrdinalIgnoreCase) &&
                PermanentProgression.CanClaimWeapon(characterId, w)).OrderBy(w => w.Tier).ToArray();
            // Slot zero is Base; the remaining slots belong only to this hero.
            offeredIndex %= choices.Length + 1;
            offeredWeapon = offeredIndex == 0 ? null : choices[offeredIndex - 1];
            RefreshWeaponPreview();
            if (cycleWeaponButton != null) cycleWeaponButton.gameObject.SetActive(choices.Length > 0);
            var current = PermanentProgression.GetEquippedWeapon(SelectedCharacter.Id, SelectedCharacter.WeaponCatalog);
            if (offeredWeapon == null)
            {
                equipmentText.text = SelectedCharacter.DisplayName + "\nBA\u015eLANGI\u00c7" +
                    (current == null ? "\n<color=#00FF00>KU\u015eANILDI</color>" : "");
                claimButton.gameObject.SetActive(false);
                equipButton.gameObject.SetActive(false);
                baseButton.gameObject.SetActive(current != null);
                baseButton.interactable = current != null;
                var label = baseButton.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = "BA\u015eLANGI\u00c7";
                return;
            }
            bool claimed = PermanentProgression.IsWeaponClaimed(SelectedCharacter.Id, offeredWeapon.Id);

            string stateText = current == offeredWeapon ? "<color=#00FF00>KUŞANILDI</color>" :
                               claimed ? "<color=#FFFFFF>ALINDI</color>" :
                               "<color=#FFAA00>ÖDÜL BEKLİYOR</color>";

            equipmentText.text = $"<size=120%><b>{offeredWeapon.DisplayName}</b></size>\n" +
                                 $"<color=#AAAAAA>Tier {(offeredWeapon.Tier == 1 ? "I" : "II")} Silah</color>\n\n" +
                                 $"Hasar Çarpanı: <color=#FFFFFF>{offeredWeapon.DamageMultiplier:0.00}x</color>\n" +
                                 $"Hız Çarpanı: <color=#FFFFFF>{offeredWeapon.AttackSpeedMultiplier:0.00}x</color>\n\n" +
                                 $"{stateText}";

            bool isEquipped = current == offeredWeapon;

            claimButton.interactable = !claimed && PermanentProgression.CanClaimWeapon(SelectedCharacter.Id, offeredWeapon);
            claimButton.gameObject.SetActive(!claimed);

            equipButton.interactable = claimed && !isEquipped;
            equipButton.gameObject.SetActive(claimed && !isEquipped);

            baseButton.interactable = current != null;
            baseButton.gameObject.SetActive(current != null);

            var cText = claimButton.GetComponentInChildren<TMP_Text>(); if (cText) cText.text = "AL";
            var eText = equipButton.GetComponentInChildren<TMP_Text>(); if (eText) eText.text = "KUŞAN";
            var bText = baseButton.GetComponentInChildren<TMP_Text>(); if (bText) bText.text = "BAŞLANGIÇ";
            var xText = closeButton.GetComponentInChildren<TMP_Text>(); if (xText) xText.text = "KAPAT";

        }

        private void RefreshWeaponPreview()
        {
            if (weaponPreviewImage == null) return;
            Sprite sprite = null;
            if (weaponPreviews != null)
                foreach (var entry in weaponPreviews)
                    if (entry.character == SelectedCharacter && entry.weapon == offeredWeapon)
                    {
                        sprite = entry.sprite;
                        break;
                    }
            // Always clear the previous selection, including when the next icon is missing.
            weaponPreviewImage.sprite = sprite;
            weaponPreviewImage.enabled = sprite != null;
            weaponPreviewImage.preserveAspect = true;
            weaponPreviewImage.raycastTarget = false;
            if (weaponPreviewFallback != null)
                weaponPreviewFallback.gameObject.SetActive(sprite == null);
        }

    }
}
