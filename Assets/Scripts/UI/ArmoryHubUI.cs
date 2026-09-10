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
        [SerializeField] private Button cycleWeaponButton;
        [SerializeField] private TMP_Text campaignAcknowledgement;
        private int selectedIndex;
        private int offeredIndex;
        private WeaponDefinition offeredWeapon;
        private BlacksmithIntroSequence intro;
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
            if (cycleWeaponButton != null) cycleWeaponButton.onClick.AddListener(() => { offeredIndex++; Refresh(); });
            if (campaignAcknowledgement != null && DungeonProgression.IsFirstCampaignCompleted)
                campaignAcknowledgement.text = "YILDIZSIZ TAHT AŞILDI • İLK SEFER TAMAMLANDI • TIER II HAZIR";
            equipmentPanel.SetActive(false);
            SelectCharacter(selectedIndex);
            if (PermanentProgression.NeedsBlacksmithIntro)
            {
                intro = gameObject.AddComponent<BlacksmithIntroSequence>();
                intro.Begin(SelectedCharacter, SelectedCharacter.WeaponCatalog.Weapons.FirstOrDefault(w => w != null && w.CharacterId == SelectedCharacter.Id && w.Tier == 1), Refresh);
            }
        }

        public void SelectCharacter(int index)
        {
            if (intro != null && intro.IsRunning) return;
            if (index < 0 || index >= characters.Length) return;
            selectedIndex = index;
            offeredIndex = 0;
            // Switching heroes follows existing run ownership: a build cannot transfer to another hero.
            if (RunProgressionSession.HasActiveRun && !RunProgressionSession.ValidateOwner(SelectedCharacter.Id)) RunProgressionSession.EndRun();
            CharacterSelectionSession.SetSelection(SelectedCharacter);
            Refresh();
        }

        public void OpenArmorer() { if (intro != null && intro.IsRunning) return; equipmentPanel.SetActive(true); Refresh(); }
        private void Claim() { PermanentProgression.TryClaimWeapon(SelectedCharacter.Id, offeredWeapon); Refresh(); }
        private void Equip() { PermanentProgression.TryEquipWeapon(SelectedCharacter.Id, offeredWeapon); Refresh(); }

        private void Refresh()
        {
            for (int i = 0; i < characters.Length; i++)
            {
                var equipped = PermanentProgression.GetEquippedWeapon(characters[i].Id, characters[i].WeaponCatalog);
                bool selected = i == selectedIndex;
                characterLabels[i].text = characters[i].DisplayName + (selected ? "\n" + (equipped != null ? equipped.DisplayName + " • Tier " + (equipped.Tier == 1 ? "I" : "II") : "Başlangıç silahı") : "");
                characterLabels[i].color = selected ? Color.white : new Color(.65f, .68f, .72f);
                characterButtons[i].GetComponent<Image>().color = selected ? new Color(.22f,.32f,.4f,.9f) : new Color(.1f,.13f,.17f,.35f);
                selectionMarkers[i].SetActive(i == selectedIndex);
            }
            var choices = SelectedCharacter.WeaponCatalog.Weapons.Where(w => w != null && PermanentProgression.CanClaimWeapon(SelectedCharacter.Id,w)).ToArray();
            offeredWeapon = choices.Length > 0 ? choices[offeredIndex % choices.Length] : null;
            if (cycleWeaponButton != null) cycleWeaponButton.gameObject.SetActive(choices.Length > 1);
            var current = PermanentProgression.GetEquippedWeapon(SelectedCharacter.Id, SelectedCharacter.WeaponCatalog);
            if (offeredWeapon == null) { equipmentText.text = "Silah bulunamadı"; claimButton.interactable = equipButton.interactable = false; return; }
            bool claimed = PermanentProgression.IsWeaponClaimed(SelectedCharacter.Id, offeredWeapon.Id);
            equipmentText.text = SelectedCharacter.DisplayName + "\nKuşanılan: " + (current != null ? current.DisplayName : "Başlangıç silahı") + "\n\n" + offeredWeapon.DisplayName + "\n" + offeredWeapon.EnglishName + (offeredWeapon.Tier == 1 ? " — Tier I\n\n" : " — Tier II\n\n") +
                $"Hasar ×{offeredWeapon.DamageMultiplier:0.00}   Saldırı hızı ×{offeredWeapon.AttackSpeedMultiplier:0.00}\n\n" +
                (current == offeredWeapon ? "KUŞANILDI" : claimed ? "Alındı — kuşanılabilir" : (offeredWeapon.Tier == 1 ? "D5" : "D10") + " ödülü — ücretsiz al") +
                "\n\nKarakter değiştirmek mevcut seferi bitirir. Kalıcı silahlar korunur.";
            claimButton.interactable = !claimed && PermanentProgression.CanClaimWeapon(SelectedCharacter.Id, offeredWeapon);
            equipButton.interactable = claimed && current != offeredWeapon;
            baseButton.interactable = current != null;
        }
    }
}
