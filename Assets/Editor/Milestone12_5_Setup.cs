using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonRoguelite.Characters;
using DungeonRoguelite.UI;

namespace DungeonRoguelite.Editor
{
    public static class Milestone12_5_Setup
    {
        public const string WorldMapScenePath = "Assets/Scenes/WorldMap/WorldMap.unity";
        public const string WarriorDefPath = "Assets/ScriptableObjects/Characters/Character_Warrior.asset";

        [MenuItem("DungeonRoguelite/Phase 12/Setup WorldMap Skill Tree UI")]
        public static void SetupWorldMapSkillTree()
        {
            Debug.Log("[Milestone 12.5 Setup] Setting up World Map Skill Tree UI...");

            var scene = EditorSceneManager.OpenScene(WorldMapScenePath, OpenSceneMode.Single);
            var mapController = Object.FindFirstObjectByType<WorldMapController>();
            if (mapController == null)
            {
                Debug.LogError("[Milestone 12.5 Setup] WorldMapController not found in scene!");
                return;
            }

            var canvasGo = mapController.gameObject;

            // 1. Create or resolve SkillTreeButton ("YETENEKLER")
            var skillTreeBtnTransform = canvasGo.transform.Find("SkillTreeButton");
            GameObject skillTreeBtnGo = skillTreeBtnTransform != null ? skillTreeBtnTransform.gameObject : new GameObject("SkillTreeButton");
            skillTreeBtnGo.transform.SetParent(canvasGo.transform, false);

            var btnRect = skillTreeBtnGo.GetComponent<RectTransform>();
            if (btnRect == null) btnRect = skillTreeBtnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(700f, -420f); // Symmetrical to BackButton at (-700, -420)
            btnRect.sizeDelta = new Vector2(200f, 50f);

            var btnImg = skillTreeBtnGo.GetComponent<Image>();
            if (btnImg == null) btnImg = skillTreeBtnGo.AddComponent<Image>();
            btnImg.color = new Color(0.18f, 0.24f, 0.35f, 0.95f);

            var btnComp = skillTreeBtnGo.GetComponent<Button>();
            if (btnComp == null) btnComp = skillTreeBtnGo.AddComponent<Button>();

            var btnTextTransform = skillTreeBtnGo.transform.Find("Text");
            GameObject btnTextGo = btnTextTransform != null ? btnTextTransform.gameObject : new GameObject("Text");
            btnTextGo.transform.SetParent(skillTreeBtnGo.transform, false);

            var btnTextRect = btnTextGo.GetComponent<RectTransform>();
            if (btnTextRect == null) btnTextRect = btnTextGo.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.sizeDelta = Vector2.zero;
            btnTextRect.anchoredPosition = Vector2.zero;

            var btnTMP = btnTextGo.GetComponent<TextMeshProUGUI>();
            if (btnTMP == null) btnTMP = btnTextGo.AddComponent<TextMeshProUGUI>();
            btnTMP.text = "YETENEKLER";
            btnTMP.fontSize = 20f;
            btnTMP.fontStyle = FontStyles.Bold;
            btnTMP.alignment = TextAlignmentOptions.Center;
            btnTMP.color = Color.white;

            // 2. Create or resolve SkillTreePanel overlay
            var panelTransform = canvasGo.transform.Find("SkillTreePanel");
            GameObject panelGo = panelTransform != null ? panelTransform.gameObject : new GameObject("SkillTreePanel");
            panelGo.transform.SetParent(canvasGo.transform, false);

            var panelRect = panelGo.GetComponent<RectTransform>();
            if (panelRect == null) panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;

            var panelImg = panelGo.GetComponent<Image>();
            if (panelImg == null) panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.07f, 0.10f, 0.97f);

            var skillTreeUI = panelGo.GetComponent<SkillTreeUI>();
            if (skillTreeUI == null) skillTreeUI = panelGo.AddComponent<SkillTreeUI>();

            // Header Container
            var headerTransform = panelGo.transform.Find("Header");
            GameObject headerGo = headerTransform != null ? headerTransform.gameObject : new GameObject("Header");
            headerGo.transform.SetParent(panelGo.transform, false);
            var headerRect = headerGo.GetComponent<RectTransform>();
            if (headerRect == null) headerRect = headerGo.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0f, 180f);

            // Title
            var titleTMP = CreateTMP(headerGo, "TitleText", "YETENEK AĞACI", 34f, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(600f, 45f), new Color(1f, 0.85f, 0.3f, 1f));

            // Hero Name
            var heroNameTMP = CreateTMP(headerGo, "HeroNameText", "Savaşçı - Yetenek Ağacı", 24f, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -85f), new Vector2(600f, 35f), Color.white);

            // Available Points
            var pointsTMP = CreateTMP(headerGo, "AvailablePointsText", "Yetenek Puanı: 0", 22f, FontStyles.Bold,
                new Vector2(0.5f, 1f), new Vector2(0f, -125f), new Vector2(400f, 35f), new Color(0.3f, 0.85f, 1f, 1f));

            // Close Button ("KAPAT")
            var closeBtnTransform = headerGo.transform.Find("CloseButton");
            GameObject closeBtnGo = closeBtnTransform != null ? closeBtnTransform.gameObject : new GameObject("CloseButton");
            closeBtnGo.transform.SetParent(headerGo.transform, false);
            var closeRect = closeBtnGo.GetComponent<RectTransform>();
            if (closeRect == null) closeRect = closeBtnGo.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-60f, -40f);
            closeRect.sizeDelta = new Vector2(140f, 45f);

            var closeImg = closeBtnGo.GetComponent<Image>();
            if (closeImg == null) closeImg = closeBtnGo.AddComponent<Image>();
            closeImg.color = new Color(0.65f, 0.15f, 0.15f, 0.95f);

            var closeBtnComp = closeBtnGo.GetComponent<Button>();
            if (closeBtnComp == null) closeBtnComp = closeBtnGo.AddComponent<Button>();

            var closeTextTMP = CreateTMP(closeBtnGo, "Text", "KAPAT", 18f, FontStyles.Bold,
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white, true);

            // Branches Container
            var branchesTransform = panelGo.transform.Find("BranchesContainer");
            GameObject branchesGo = branchesTransform != null ? branchesTransform.gameObject : new GameObject("BranchesContainer");
            branchesGo.transform.SetParent(panelGo.transform, false);
            var branchesRect = branchesGo.GetComponent<RectTransform>();
            if (branchesRect == null) branchesRect = branchesGo.AddComponent<RectTransform>();
            branchesRect.anchorMin = new Vector2(0.5f, 0.5f);
            branchesRect.anchorMax = new Vector2(0.5f, 0.5f);
            branchesRect.pivot = new Vector2(0.5f, 0.5f);
            branchesRect.anchoredPosition = new Vector2(0f, -50f);
            branchesRect.sizeDelta = new Vector2(1650f, 680f);

            float[] colX = new float[] { -540f, 0f, 540f };
            float[] tierY = new float[] { 170f, 10f, -150f };
            var nodeCards = new List<SkillTreeNodeUI>();
            var branchTMPs = new List<TextMeshProUGUI>();

            int nodeIndex = 0;
            for (int b = 0; b < 3; b++)
            {
                var branchColTransform = branchesGo.transform.Find($"Branch_{b + 1}");
                GameObject branchColGo = branchColTransform != null ? branchColTransform.gameObject : new GameObject($"Branch_{b + 1}");
                branchColGo.transform.SetParent(branchesGo.transform, false);

                var colRect = branchColGo.GetComponent<RectTransform>();
                if (colRect == null) colRect = branchColGo.AddComponent<RectTransform>();
                colRect.anchorMin = new Vector2(0.5f, 0.5f);
                colRect.anchorMax = new Vector2(0.5f, 0.5f);
                colRect.pivot = new Vector2(0.5f, 0.5f);
                colRect.anchoredPosition = new Vector2(colX[b], 0f);
                colRect.sizeDelta = new Vector2(500f, 660f);

                // Branch Title
                var bTitleTMP = CreateTMP(branchColGo, "BranchTitle", $"Dal {b + 1}", 24f, FontStyles.Bold,
                    new Vector2(0.5f, 0.5f), new Vector2(0f, 285f), new Vector2(480f, 40f), new Color(0.9f, 0.9f, 0.95f, 1f));
                branchTMPs.Add(bTitleTMP);

                // 3 Tier cards
                for (int t = 0; t < 3; t++)
                {
                    string cardName = $"NodeCard_{nodeIndex}";
                    var cardTransform = branchColGo.transform.Find(cardName);
                    GameObject cardGo = cardTransform != null ? cardTransform.gameObject : new GameObject(cardName);
                    cardGo.transform.SetParent(branchColGo.transform, false);

                    var cardRect = cardGo.GetComponent<RectTransform>();
                    if (cardRect == null) cardRect = cardGo.AddComponent<RectTransform>();
                    cardRect.anchorMin = new Vector2(0.5f, 0.5f);
                    cardRect.anchorMax = new Vector2(0.5f, 0.5f);
                    cardRect.pivot = new Vector2(0.5f, 0.5f);
                    cardRect.anchoredPosition = new Vector2(0f, tierY[t]);
                    cardRect.sizeDelta = new Vector2(480f, 130f);

                    var cardBg = cardGo.GetComponent<Image>();
                    if (cardBg == null) cardBg = cardGo.AddComponent<Image>();
                    cardBg.color = new Color(0.10f, 0.14f, 0.20f, 0.95f);

                    var cardBtn = cardGo.GetComponent<Button>();
                    if (cardBtn == null) cardBtn = cardGo.AddComponent<Button>();
                    cardBtn.targetGraphic = cardBg;

                    var nodeUI = cardGo.GetComponent<SkillTreeNodeUI>();
                    if (nodeUI == null) nodeUI = cardGo.AddComponent<SkillTreeNodeUI>();

                    // Border frame
                    var borderTransform = cardGo.transform.Find("Border");
                    GameObject borderGo = borderTransform != null ? borderTransform.gameObject : new GameObject("Border");
                    borderGo.transform.SetParent(cardGo.transform, false);
                    var borderRect = borderGo.GetComponent<RectTransform>();
                    if (borderRect == null) borderRect = borderGo.AddComponent<RectTransform>();
                    borderRect.anchorMin = Vector2.zero;
                    borderRect.anchorMax = Vector2.one;
                    borderRect.sizeDelta = Vector2.zero;
                    borderRect.anchoredPosition = Vector2.zero;
                    var borderImg = borderGo.GetComponent<Image>();
                    if (borderImg == null) borderImg = borderGo.AddComponent<Image>();
                    borderImg.color = new Color(0.25f, 0.78f, 0.40f, 1f);
                    borderImg.raycastTarget = false;

                    // Name
                    var nameTMP = CreateTMP(cardGo, "NameText", $"Yetenek {nodeIndex + 1}", 19f, FontStyles.Bold,
                        new Vector2(0f, 1f), new Vector2(20f, -22f), new Vector2(320f, 30f), Color.white, false, TextAlignmentOptions.Left);

                    // Cost
                    var costTMP = CreateTMP(cardGo, "CostText", "1 Puan", 17f, FontStyles.Bold,
                        new Vector2(1f, 1f), new Vector2(-20f, -22f), new Vector2(120f, 30f), new Color(1f, 0.85f, 0.3f, 1f), false, TextAlignmentOptions.Right);

                    // Description
                    var descTMP = CreateTMP(cardGo, "DescText", "Yetenek açıklaması burada yer alır.", 14f, FontStyles.Normal,
                        new Vector2(0.5f, 0.5f), new Vector2(0f, -2f), new Vector2(440f, 40f), new Color(0.85f, 0.85f, 0.85f, 1f), false, TextAlignmentOptions.Left);

                    // Status Badge
                    var statusTMP = CreateTMP(cardGo, "StatusText", "KİLİTLİ", 16f, FontStyles.Bold,
                        new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(440f, 26f), new Color(0.5f, 0.5f, 0.55f, 1f), false, TextAlignmentOptions.Center);

                    nodeUI.SetReferences(nameTMP, descTMP, costTMP, statusTMP, cardBtn, cardBg, borderImg);
                    nodeCards.Add(nodeUI);
                    nodeIndex++;
                }
            }

            var defaultWarrior = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorDefPath);
            skillTreeUI.SetReferences(panelGo, heroNameTMP, pointsTMP, closeBtnComp, nodeCards.ToArray(), branchTMPs.ToArray(), defaultWarrior);

            // Panel starts inactive
            panelGo.SetActive(false);

            // 3. Serialize references onto WorldMapController
            var sMap = new SerializedObject(mapController);
            sMap.FindProperty("skillTreeButton").objectReferenceValue = btnComp;
            sMap.FindProperty("skillTreePanel").objectReferenceValue = skillTreeUI;
            sMap.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Milestone 12.5 Setup] World Map Skill Tree UI configured and saved successfully.");
        }

        private static TextMeshProUGUI CreateTMP(
            GameObject parent,
            string name,
            string initialText,
            float fontSize,
            FontStyles fontStyle,
            Vector2 anchor,
            Vector2 pos,
            Vector2 size,
            Color color,
            bool stretchToParent = false,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var t = parent.transform.Find(name);
            GameObject go = t != null ? t.gameObject : new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            var rect = go.GetComponent<RectTransform>();
            if (rect == null) rect = go.AddComponent<RectTransform>();

            if (stretchToParent)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;
            }
            else
            {
                rect.anchorMin = anchor;
                rect.anchorMax = anchor;
                rect.pivot = anchor;
                rect.anchoredPosition = pos;
                rect.sizeDelta = size;
            }

            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = initialText;
            tmp.fontSize = fontSize;
            tmp.fontStyle = fontStyle;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }
    }
}
