using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DungeonRoguelite.Tests;
namespace DungeonRoguelite.Editor
{
    [InitializeOnLoad]
    public static class D5BossTransitionRunner
    {
        static D5BossTransitionRunner(){ EditorApplication.playModeStateChanged += state => { if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("D5BossPending",false)){SessionState.EraseBool("D5BossPending");new GameObject("D5BossTransitionVerifier").AddComponent<D5BossTransitionVerifier>();} }; var args=System.Environment.GetCommandLineArgs();if(!EditorApplication.isPlayingOrWillChangePlaymode && System.Array.IndexOf(args,"-runD5BossTransition")>=0) EditorApplication.delayCall+=Run;}
        private static void Run(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SessionState.SetBool("D5BossPending",true);EditorApplication.EnterPlaymode();}
    }
}
