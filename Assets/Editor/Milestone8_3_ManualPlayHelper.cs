using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DungeonRoguelite.Characters;

namespace DungeonRoguelite.Editor
{
    /// <summary>
    /// Editor-only manual play helper enabling safe manual playtesting of Archer in Dungeon_Prototype.unity.
    /// Temporarily overrides DefaultCharacter in memory only; never saves Archer to the scene on disk.
    /// Guarantees that upon exiting Play Mode, the scene reverts to Character_Warrior default.
    /// </summary>
    [InitializeOnLoad]
    public static class Milestone8_3_ManualPlayHelper
    {
        private const string ScenePath = "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";
        private const string ArcherAssetPath = "Assets/ScriptableObjects/Characters/Character_Archer.asset";
        private const string WarriorAssetPath = "Assets/ScriptableObjects/Characters/Character_Warrior.asset";
        private const string SessionKey = "M8_3_ArcherManualPlayActive";

        static Milestone8_3_ManualPlayHelper()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem("DungeonRoguelite/Play Archer in Prototype Scene")]
        public static void PlayArcherInPrototypeScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Milestone 8.3] Already in Play Mode.");
                return;
            }

            var currentScene = EditorSceneManager.GetActiveScene();
            if (currentScene.path != ScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return;
                }
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            var spawner = Object.FindFirstObjectByType<PlayerSpawner>();
            if (spawner == null)
            {
                Debug.LogError("[Milestone 8.3] PlayerSpawner not found in scene.");
                return;
            }

            var archerDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(ArcherAssetPath);
            if (archerDef == null)
            {
                Debug.LogError($"[Milestone 8.3] Character_Archer asset not found at {ArcherAssetPath}. Run Setup first.");
                return;
            }

            // Set DefaultCharacter in memory only
            spawner.SetDefaultCharacter(archerDef);
            SessionState.SetBool(SessionKey, true);

            Debug.Log("[Milestone 8.3] Launching Play Mode with Archer in memory (scene will not be saved with Archer on disk)...");
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (SessionState.GetBool(SessionKey, false))
                {
                    SessionState.SetBool(SessionKey, false);

                    // Revert in-memory reference to Warrior
                    var spawner = Object.FindFirstObjectByType<PlayerSpawner>();
                    var warriorDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(WarriorAssetPath);
                    if (spawner != null && warriorDef != null)
                    {
                        spawner.SetDefaultCharacter(warriorDef);
                    }

                    // Reload clean scene from disk to guarantee zero on-disk modifications
                    var activeScene = EditorSceneManager.GetActiveScene();
                    if (activeScene.path == ScenePath)
                    {
                        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                    }

                    Debug.Log("[Milestone 8.3] Play Mode exited. Dungeon_Prototype safely restored to Character_Warrior default.");
                }
            }
        }
    }
}
