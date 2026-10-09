using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceExplorer
{
    /// <summary>
    /// Loads scenes behind a fade-to-black overlay. Creates itself on first use and survives
    /// scene loads, so no scene has to contain it.
    /// </summary>
    public sealed class SceneLoader : MonoBehaviour
    {
        const float FadeDuration = 0.35f;

        static SceneLoader instance;

        CanvasGroup overlay;
        bool isLoading;

        /// <summary>Fades out, loads <paramref name="sceneName"/>, then fades back in.</summary>
        public static void Load(string sceneName)
        {
            if (instance == null) instance = Create();
            if (!instance.isLoading) instance.StartCoroutine(instance.LoadRoutine(sceneName));
        }

        // Supports "Enter Play Mode" without domain reload.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => instance = null;

        static SceneLoader Create()
        {
            var root = new GameObject(nameof(SceneLoader));
            DontDestroyOnLoad(root);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            var fade = new GameObject("Fade", typeof(RectTransform), typeof(Image));
            fade.transform.SetParent(root.transform, false);
            var rect = (RectTransform)fade.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            fade.GetComponent<Image>().color = Color.black;

            var loader = root.AddComponent<SceneLoader>();
            loader.overlay = root.AddComponent<CanvasGroup>();
            loader.overlay.alpha = 0f;
            loader.overlay.blocksRaycasts = false;
            return loader;
        }

        IEnumerator LoadRoutine(string sceneName)
        {
            isLoading = true;
            overlay.blocksRaycasts = true; // swallow clicks during the transition

            yield return Fade(0f, 1f);
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return Fade(1f, 0f);

            overlay.blocksRaycasts = false;
            isLoading = false;
        }

        IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < FadeDuration; t += Time.unscaledDeltaTime)
            {
                overlay.alpha = Mathf.Lerp(from, to, t / FadeDuration);
                yield return null;
            }
            overlay.alpha = to;
        }
    }
}
