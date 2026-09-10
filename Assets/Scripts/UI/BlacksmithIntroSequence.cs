using System;
using DungeonRoguelite.Characters;
using DungeonRoguelite.Progression;
using DungeonRoguelite.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DungeonRoguelite.UI
{
    /// <summary>One purpose-built Hub introduction; no combat or general dialogue system.</summary>
    public sealed class BlacksmithIntroSequence : MonoBehaviour
    {
        [SerializeField, TextArea(2, 3)] private string[] lines = {
            "Ben bu ocağın demircisiyim. Ateşin başında sana da yer var.",
            "Kül Muhafızı düştü. Yolun ilk büyük sınavını aştın.",
            "Önündeki karanlık için daha sağlam bir silaha ihtiyacın olacak.",
            "Bunu senin için dövdüm. Al; yoluna onunla devam et."
        };
        private CharacterDefinition hero;
        private WeaponDefinition reward;
        private Action onReward;
        private CanvasGroup overlay;
        private TMP_Text dialogue;
        private TMP_Text continueLabel;
        private int lineIndex;
        private float readyAt;
        public bool IsRunning { get; private set; }

        public void Begin(CharacterDefinition character, WeaponDefinition weapon, Action refresh)
        {
            if (!PermanentProgression.NeedsBlacksmithIntro || character == null || weapon == null) return;
            hero = character; reward = weapon; onReward = refresh;
            IsRunning = true;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            var root = new GameObject("BlacksmithIntroduction", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var rect = root.GetComponent<RectTransform>();
            rect.SetParent(transform, false); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0,0,0,.35f);
            overlay = root.GetComponent<CanvasGroup>(); overlay.alpha = 0;
            var panel = Rect(rect,"Dialogue",new Vector2(0,-260),new Vector2(1100,320));
            panel.gameObject.AddComponent<Image>().color = new Color(.055f,.065f,.08f,.98f);
            Text(panel,"Speaker","DEMİRCİ",new Vector2(0,115),new Vector2(1000,45),28);
            dialogue = Text(panel,"Line",lines[0],new Vector2(0,15),new Vector2(1000,145),28);
            var buttonRect = Rect(panel,"Continue",new Vector2(0,-110),new Vector2(400,55));
            var image = buttonRect.gameObject.AddComponent<Image>(); image.color = new Color(.24f,.3f,.36f);
            var button = buttonRect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(Advance);
            continueLabel = Text(buttonRect,"Label","DEVAM / ENTER",Vector2.zero,new Vector2(390,50),24);
            readyAt = Time.unscaledTime + .4f;
        }

        private void Update()
        {
            if (!IsRunning) return;
            overlay.alpha = Mathf.MoveTowards(overlay.alpha,1,Time.unscaledDeltaTime / .35f);
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)) Advance();
        }

        private void Advance()
        {
            if (!IsRunning || Time.unscaledTime < readyAt) return;
            readyAt = Time.unscaledTime + .2f; // A button submit and Enter in the same frame advance only once.
            lineIndex++;
            if (lineIndex < lines.Length) { dialogue.text = lines[lineIndex]; return; }
            if (lineIndex == lines.Length)
            {
                if (!PermanentProgression.CompleteBlacksmithIntro(hero.Id,reward))
                {
                    dialogue.text = "Silah teslim edilemedi. Lütfen cephaneliği yeniden ziyaret et.";
                    continueLabel.text = "KAPAT";
                    return;
                }
                dialogue.text = hero.DisplayName + "\n" + reward.DisplayName + "\nTier I — ALINDI VE KUŞANILDI";
                continueLabel.text = "CEPHANELİĞE DEVAM";
                onReward?.Invoke();
                return;
            }
            IsRunning = false;
            Destroy(overlay.gameObject);
        }

        private static RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size)
        {
            var rect = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent,false); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,.5f);
            rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        private static TMP_Text Text(Transform parent,string name,string value,Vector2 position,Vector2 size,float fontSize)
        {
            var text = Rect(parent,name,position,size).gameObject.AddComponent<TextMeshProUGUI>();
            text.text = value; text.fontSize = fontSize; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false; return text;
        }
    }
}
