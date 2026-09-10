using System;
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
    /// <summary>Passive character selection and one permanent weapon slot. No gameplay actors run in this scene.</summary>
    public sealed class ArmoryHubUI : MonoBehaviour
    {
        [SerializeField] private CharacterDefinition[] characters;
        [SerializeField] private Button[] characterButtons;
        [SerializeField] private TMP_Text[] characterLabels;
        [SerializeField] private GameObject[] selectionMarkers;
        [SerializeField] private Button armorerButton;
        [SerializeField] private Button returnButton;
        [SerializeField] private GameObject equipmentPanel;
        [SerializeField] private TMP_Text equipmentText;
        [SerializeField] private Button claimButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button baseButton;
        [SerializeField] private Button closeButton;
        private int selectedIndex;
        private WeaponDefinition offeredWeapon;
        public CharacterDefinition SelectedCharacter => characters[selectedIndex];

        private void Start()
        {
            if (!DungeonProgression.IsDungeonCompleted("dungeon_5")) { SceneManager.LoadScene("WorldMap"); return; }
            for (int i = 0; i < characters.Length; i++)
            {
                int index = i;
                characterButtons[i].onClick.AddListener(() => SelectCharacter(index));
                if (characters[i] == CharacterSelectionSession.SelectedCharacter) selectedIndex = i;
            }
            armorerButton.onClick.AddListener(OpenArmorer);
            returnButton.onClick.AddListener(() => SceneManager.LoadScene("WorldMap"));
            claimButton.onClick.AddListener(Claim);
            equipButton.onClick.AddListener(Equip);
            baseButton.onClick.AddListener(() => { PermanentProgression.TryEquipWeapon(SelectedCharacter.Id, null); Refresh(); });
            closeButton.onClick.AddListener(() => equipmentPanel.SetActive(false));
            equipmentPanel.SetActive(false);
            SelectCharacter(selectedIndex);
        }

        public void SelectCharacter(int index)
        {
            if (index < 0 || index >= characters.Length) return;
            selectedIndex = index;
            // Switching heroes follows existing run ownership: a build cannot transfer to another hero.
            if (RunProgressionSession.HasActiveRun && !RunProgressionSession.ValidateOwner(SelectedCharacter.Id)) RunProgressionSession.EndRun();
            CharacterSelectionSession.SetSelection(SelectedCharacter);
            Refresh();
        }

        public void OpenArmorer() { equipmentPanel.SetActive(true); Refresh(); }
        private void Claim() { PermanentProgression.TryClaimWeapon(SelectedCharacter.Id, offeredWeapon); Refresh(); }
        private void Equip() { PermanentProgression.TryEquipWeapon(SelectedCharacter.Id, offeredWeapon); Refresh(); }

        private void Refresh()
        {
            for (int i = 0; i < characters.Length; i++)
            {
                var equipped = PermanentProgression.GetEquippedWeapon(characters[i].Id, characters[i].WeaponCatalog);
                characterLabels[i].text = characters[i].DisplayName + (i == selectedIndex ? " • SEÇİLİ" : "") + "\n" + (equipped != null ? equipped.DisplayName : "Başlangıç silahı");
                selectionMarkers[i].SetActive(i == selectedIndex);
            }
            offeredWeapon = null;
            foreach (var weapon in SelectedCharacter.WeaponCatalog.Weapons)
                if (weapon != null && string.Equals(weapon.CharacterId, SelectedCharacter.Id, StringComparison.OrdinalIgnoreCase)) { offeredWeapon = weapon; break; }
            var current = PermanentProgression.GetEquippedWeapon(SelectedCharacter.Id, SelectedCharacter.WeaponCatalog);
            if (offeredWeapon == null) { equipmentText.text = "Silah bulunamadı"; claimButton.interactable = equipButton.interactable = false; return; }
            bool claimed = PermanentProgression.IsWeaponClaimed(SelectedCharacter.Id, offeredWeapon.Id);
            equipmentText.text = SelectedCharacter.DisplayName + "\n\n" + offeredWeapon.DisplayName + "\n" + offeredWeapon.EnglishName + " — Tier I\n\n" +
                $"Hasar ×{offeredWeapon.DamageMultiplier:0.00}   Saldırı hızı ×{offeredWeapon.AttackSpeedMultiplier:0.00}\n\n" +
                (current == offeredWeapon ? "KUŞANILDI" : claimed ? "Alındı — kuşanılabilir" : "D5 ödülü — ücretsiz al") +
                "\n\nKarakter değiştirmek mevcut seferi bitirir. Kalıcı silahlar korunur.";
            claimButton.interactable = !claimed && PermanentProgression.CanClaimWeapon(SelectedCharacter.Id, offeredWeapon);
            equipButton.interactable = claimed && current != offeredWeapon;
            baseButton.interactable = current != null;
        }
    }
}
