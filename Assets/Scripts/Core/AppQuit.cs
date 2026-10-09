using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>Quits the player build, or leaves Play Mode when running in the Editor.</summary>
    public static class AppQuit
    {
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
