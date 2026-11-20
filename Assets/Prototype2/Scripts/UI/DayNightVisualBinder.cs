using UnityEngine;

namespace Baricovats.Prototype2
{
    public sealed class DayNightVisualBinder : MonoBehaviour
    {
        [Header("Existing scene objects")]
        public CanvasGroup blackOverlay;
        [Min(0f)] public float fadeInSeconds = 0.35f;
        [Min(0f)] public float fadeOutSeconds = 0.5f;

        private Coroutine fadeRoutine;

        private void OnEnable()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StateChanged += Refresh;
                Refresh(GameStateManager.Instance.State);
            }
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.StateChanged -= Refresh;

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }
        }

        private void Start()
        {
            if (GameStateManager.Instance != null)
                Refresh(GameStateManager.Instance.State);
        }

        private void Refresh(CityState state)
        {
            if (state == null)
                return;

            if (blackOverlay == null)
                return;

            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            float targetAlpha = state.isNightTransition ? 1f : 0f;
            float duration = state.isNightTransition ? fadeInSeconds : fadeOutSeconds;
            fadeRoutine = StartCoroutine(FadeTo(targetAlpha, duration));
        }

        private System.Collections.IEnumerator FadeTo(float targetAlpha, float duration)
        {
            float startAlpha = blackOverlay.alpha;

            if (duration <= 0f)
            {
                blackOverlay.alpha = targetAlpha;
                fadeRoutine = null;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                blackOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            blackOverlay.alpha = targetAlpha;
            fadeRoutine = null;
        }
    }
}
