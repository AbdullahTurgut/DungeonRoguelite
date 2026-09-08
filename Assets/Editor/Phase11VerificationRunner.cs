using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonRoguelite.Editor
{
    [InitializeOnLoad]
    public static class Phase11VerificationRunner
    {
        private const string Pending = "Phase11.Verification.Pending";
        static Phase11VerificationRunner()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending, false)) return;
                SessionState.EraseBool(Pending);
                string suite = SessionState.GetString("Phase11.Suite", "11_2");
                var type = System.Type.GetType($"DungeonRoguelite.Tests.Milestone{suite}_Verifier, Assembly-CSharp", true);
                new GameObject("GateVerifier_" + suite).AddComponent(type);
            };
        }
        public static void Run()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            int index = System.Array.IndexOf(args, "-gateSuite");
            SessionState.SetString("Phase11.Suite", index >= 0 ? args[index + 1] : "11_2");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }
    }
}
