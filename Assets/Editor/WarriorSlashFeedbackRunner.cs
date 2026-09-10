using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonRoguelite.Editor
{
    [InitializeOnLoad]
    public static class WarriorSlashFeedbackRunner
    {
        private const string PendingKey = "WarriorSlashFeedback.Pending";

        static WarriorSlashFeedbackRunner()
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
            new GameObject("WarriorSlashFeedbackVerifier").AddComponent<Tests.WarriorSlashFeedbackVerifier>();
        }
    }
}
