using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonRoguelite.Editor
{
    [InitializeOnLoad]
    public static class D5BossHealthBarRunner
    {
        private const string PendingKey = "D5BossHealthBar.Pending";

        static D5BossHealthBarRunner()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        public static void Run()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(PendingKey, true);
            EditorApplication.EnterPlaymode();
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(PendingKey, false)) return;
            SessionState.EraseBool(PendingKey);
            new GameObject("D5BossHealthBarVerifier").AddComponent<Tests.D5BossHealthBarVerifier>();
        }
    }
}
