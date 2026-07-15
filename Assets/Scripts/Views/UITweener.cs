using UnityEngine;
using System.Collections;
using System;

namespace PolarBond.Views
{
    public static class UITweener
    {
        private class TweenerHelper : MonoBehaviour { }
        private static TweenerHelper helper;

        private static TweenerHelper Helper
        {
            get
            {
                if (helper == null)
                {
                    GameObject go = new GameObject("UITweener_Helper");
                    GameObject.DontDestroyOnLoad(go);
                    helper = go.AddComponent<TweenerHelper>();
                }
                return helper;
            }
        }

        public static Coroutine ScaleTo(Transform target, Vector3 endScale, float duration, Action onComplete = null)
        {
            return Helper.StartCoroutine(ScaleCoroutine(target, endScale, duration, onComplete));
        }

        public static Coroutine FadeTo(CanvasGroup group, float endAlpha, float duration, Action onComplete = null)
        {
            return Helper.StartCoroutine(FadeCoroutine(group, endAlpha, duration, onComplete));
        }

        private static float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private static IEnumerator ScaleCoroutine(Transform target, Vector3 endScale, float duration, Action onComplete)
        {
            if (target == null) yield break;
            Vector3 startScale = target.localScale;
            float time = 0;
            while (time < duration)
            {
                if (target == null) yield break;
                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / duration);
                
                // Use EaseOutBack for bouncing effect when scaling up
                float ease = (endScale.magnitude > startScale.magnitude) ? EaseOutBack(t) : Mathf.SmoothStep(0f, 1f, t);
                
                target.localScale = Vector3.LerpUnclamped(startScale, endScale, ease);
                yield return null;
            }
            if (target != null) target.localScale = endScale;
            onComplete?.Invoke();
        }

        private static IEnumerator FadeCoroutine(CanvasGroup group, float endAlpha, float duration, Action onComplete)
        {
            if (group == null) yield break;
            float startAlpha = group.alpha;
            float time = 0;
            while (time < duration)
            {
                if (group == null) yield break;
                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / duration);
                group.alpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            if (group != null) group.alpha = endAlpha;
            onComplete?.Invoke();
        }

        public static void StopTween(Coroutine coroutine)
        {
            if (coroutine != null && helper != null)
            {
                helper.StopCoroutine(coroutine);
            }
        }
    }
}
