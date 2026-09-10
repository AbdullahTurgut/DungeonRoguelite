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
                if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("D10BlacksmithSmoke",false))
                { SessionState.EraseBool("D10BlacksmithSmoke");new GameObject("D10BlacksmithSmoke").AddComponent<Tests.D10BlacksmithSmoke>(); }
            };
        }
        public static void Run()
        {
            Phase19Setup.Build();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SessionState.SetBool("D10Smoke",true);EditorApplication.EnterPlaymode();
        }
        public static void RunBlacksmith()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SessionState.SetBool("D10BlacksmithSmoke",true);EditorApplication.EnterPlaymode();
        }
    }
}
