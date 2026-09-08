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
                new GameObject("Gate11_2Verifier").AddComponent<Tests.Milestone11_2_Verifier>();
            };
        }
        public static void Run()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }
    }
}
