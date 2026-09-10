using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DungeonRoguelite.Editor
{
    [InitializeOnLoad]
    public static class EnemyFairnessRunner
    {
        static EnemyFairnessRunner()
        {
            EditorApplication.playModeStateChanged += state => {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("EnemyFairness",false)) return;
                SessionState.EraseBool("EnemyFairness");
                new GameObject("EnemyFairnessSmoke").AddComponent<Tests.EnemyFairnessSmoke>();
            };
        }
        public static void Run()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SessionState.SetBool("EnemyFairness",true); EditorApplication.EnterPlaymode();
        }
    }
}
