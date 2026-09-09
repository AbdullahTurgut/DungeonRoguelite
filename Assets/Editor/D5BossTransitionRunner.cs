using UnityEditor;
using UnityEngine;
using DungeonRoguelite.Tests;
namespace DungeonRoguelite.Editor
{
    [InitializeOnLoad]
    public static class D5BossTransitionRunner
    {
        static D5BossTransitionRunner(){var args=System.Environment.GetCommandLineArgs();if(System.Array.IndexOf(args,"-runD5BossTransition")>=0) EditorApplication.delayCall+=Run;}
        private static void Run(){var go=new GameObject("D5BossTransitionVerifier");go.AddComponent<D5BossTransitionVerifier>();}
    }
}
