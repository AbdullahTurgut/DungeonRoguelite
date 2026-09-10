using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DungeonRoguelite.Editor
{
    [InitializeOnLoad]
    public static class Phase19Runner
    {
        static Phase19Runner()
        {
            EditorApplication.playModeStateChanged+=state=>{
                if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("D10Smoke",false))
                { SessionState.EraseBool("D10Smoke");new GameObject("D10Smoke").AddComponent<Tests.Phase19Smoke>(); }
            };
        }
        public static void Run()
        {
            Phase19Setup.Build();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SessionState.SetBool("D10Smoke",true);EditorApplication.EnterPlaymode();
        }
    }
}
