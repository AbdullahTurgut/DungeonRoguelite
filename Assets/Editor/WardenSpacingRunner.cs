using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DungeonRoguelite.Editor
{
    [InitializeOnLoad]
    public static class WardenSpacingRunner
    {
        static WardenSpacingRunner(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("WardenSpacing",false)){SessionState.EraseBool("WardenSpacing");new GameObject("WardenSpacing").AddComponent<Tests.WardenSpacingSmoke>();} if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("WardenGeometry",false)){SessionState.EraseBool("WardenGeometry");new GameObject("WardenGeometry").AddComponent<Tests.WardenGeometrySmoke>();}};}
        public static void Run(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SessionState.SetBool("WardenSpacing",true);EditorApplication.EnterPlaymode();}
        public static void RunGeometry(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SessionState.SetBool("WardenGeometry",true);EditorApplication.EnterPlaymode();}
    }
}
