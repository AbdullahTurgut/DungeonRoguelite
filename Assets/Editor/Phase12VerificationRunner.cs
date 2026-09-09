using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonRoguelite.Editor
{
    [InitializeOnLoad]
    public static class Phase12VerificationRunner
    {
        private const string Pending = "Phase12.Verification.Pending";
        static Phase12VerificationRunner()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending, false)) return;
                SessionState.EraseBool(Pending);
                string suite = SessionState.GetString("Phase12.Suite", "12_1");
                var type = System.Type.GetType($"DungeonRoguelite.Tests.Milestone{suite}_Verifier, Assembly-CSharp", true);
                new GameObject("GateVerifier_" + suite).AddComponent(type);
            };
        }

        [MenuItem("DungeonRoguelite/Phase 12/Run Gate 12.1 Verification")]
        public static void Run()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            int index = System.Array.IndexOf(args, "-gateSuite");
            SessionState.SetString("Phase12.Suite", index >= 0 ? args[index + 1] : "12_1");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("DungeonRoguelite/Phase 12/Run Gate 12.2 Verification")]
        public static void RunGate12_2()
        {
            SessionState.SetString("Phase12.Suite", "12_2");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("DungeonRoguelite/Phase 12/Run Gate 12.3 Verification")]
        public static void RunGate12_3()
        {
            SessionState.SetString("Phase12.Suite", "12_3");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("DungeonRoguelite/Phase 12/Run Gate 12.4 Verification")]
        public static void RunGate12_4()
        {
            SessionState.SetString("Phase12.Suite", "12_4");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }
    }
}
