using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonRoguelite.Editor
{
    [InitializeOnLoad]
    public static class Milestone11_1_VerificationRunner
    {
        private const string Pending = "DungeonRoguelite.Gate11.1.Pending";
        private const string Suite = "DungeonRoguelite.Gate11.1.Suite";
        private const string SaveBackup = "DungeonRoguelite.Gate11.1.SaveBackup";
        private static Tests.Phase11StateSnapshot stateSnapshot;

        static Milestone11_1_VerificationRunner()
        {
            EditorApplication.playModeStateChanged += HandlePlayMode;
            EditorApplication.quitting += () => { stateSnapshot?.Dispose(); RestoreSave(); };
        }

        [MenuItem("DungeonRoguelite/Phase 11/Run Gate 11.1 Verification")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                {
                    Debug.LogError("[GATE 11.1] Save or discard existing scene edits before running verification.");
                    if (Application.isBatchMode) EditorApplication.Exit(1);
                    return;
                }
            }
            BackupSave();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetString(Suite, "11_1");
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }

        /// <summary>Runs existing suites without their legacy asset/scene setup mutations.</summary>
        public static void RunRegression()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            int index = System.Array.IndexOf(args, "-gateSuite");
            string suite = index >= 0 && index + 1 < args.Length ? args[index + 1] : "";
            if (suite != "10_1" && suite != "10_2" && suite != "10_3" && suite != "10_4" &&
                suite != "10_5" && suite != "10_QA" && suite != "8_3" && suite != "8_4")
            {
                Debug.LogError("Specify a supported -gateSuite: 10_1 through 10_5, 10_QA, 8_3 or 8_4.");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }
            BackupSave();
            string scene = suite == "10_QA" ? "Assets/Scenes/WorldMap/WorldMap.unity" :
                "Assets/Scenes/Dungeons/Dungeon_Prototype.unity";
            EditorSceneManager.OpenScene(scene, OpenSceneMode.Single);
            SessionState.SetString(Suite, suite);
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }

        private static void BackupSave()
        {
            string key = Dungeons.DungeonProgression.PrefsKey;
            SessionState.SetBool(SaveBackup, true);
            SessionState.SetBool(SaveBackup + ".Exists", PlayerPrefs.HasKey(key));
            SessionState.SetString(SaveBackup + ".Value", PlayerPrefs.GetString(key, ""));
        }

        private static void RestoreSave()
        {
            if (!SessionState.GetBool(SaveBackup, false)) return;
            string key = Dungeons.DungeonProgression.PrefsKey;
            if (SessionState.GetBool(SaveBackup + ".Exists", false))
                PlayerPrefs.SetString(key, SessionState.GetString(SaveBackup + ".Value", ""));
            else PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            SessionState.EraseBool(SaveBackup);
        }

        private static void HandlePlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode) RestoreSave();
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending, false)) return;
            SessionState.EraseBool(Pending);
            string suite = SessionState.GetString(Suite, "11_1");
            stateSnapshot = new Tests.Phase11StateSnapshot();
            var type = System.Type.GetType($"DungeonRoguelite.Tests.Milestone{suite}_Verifier, Assembly-CSharp", true);
            new GameObject("Gate_Verifier_" + suite).AddComponent(type);
        }
    }
}
