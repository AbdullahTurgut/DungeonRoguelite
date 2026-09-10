using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DungeonRoguelite.Editor
{
    [InitializeOnLoad]
    public static class Phase17Runner
    {
        static Phase17Runner()
        {
            EditorApplication.playModeStateChanged += state => {
                if(state!=PlayModeStateChange.EnteredPlayMode) return;
                int number=SessionState.GetInt("Phase17Dungeon",0);
                if(number==0)return;
                SessionState.EraseInt("Phase17Dungeon");
                new GameObject("Phase17Smoke").AddComponent<Tests.Phase17Smoke>().DungeonNumber=number;
            };
        }
        public static void Run6() { Phase17Setup.Build6(); Start(6); }
        public static void Run7() { Phase17Setup.Build7(); Start(7); }
        public static void Run8() { Phase18Setup.Build8(); Start(8); }
        public static void Run9() { Phase18Setup.Build9(); Start(9); }
        private static void Start(int number)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SessionState.SetInt("Phase17Dungeon",number);EditorApplication.EnterPlaymode();
        }
    }
}
