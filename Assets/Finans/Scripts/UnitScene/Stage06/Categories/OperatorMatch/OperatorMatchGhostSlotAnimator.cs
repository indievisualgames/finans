using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Categories.OperatorMatch
{
    /// <summary>
    /// Moves active ghost slot visuals toward a target, fading and shrinking them before hiding.
    /// Intended for UI RectTransform-based slots.
    /// </summary>
    public sealed class OperatorMatchGhostSlotAnimator : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float duration = 0.75f;
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float endScaleMultiplier = 0.4f;
        [SerializeField] private bool deactivateOnComplete = true;
        [SerializeField] private AudioSource postAnimationAudioSource;
        [SerializeField] private AudioClip postAnimationClip;

        private readonly Dictionary<Transform, Vector3> initialLocalPositions = new();
        private readonly Dictionary<Transform, Vector3> initialLocalScales = new();
        private readonly Dictionary<Transform, float> initialAlphas = new();
        private readonly HashSet<Transform> running = new();
        private bool playedPostClip;

        public void ResetPostClip()
        {
            playedPostClip = false;
        }

        public void AnimateSlots(IEnumerable<GameObject> slots)
        {
            if (target == null || slots == null) return;

            foreach (var slotGo in slots)
            {
                if (slotGo == null) continue;
                if (!slotGo.activeInHierarchy) continue;

                var tf = slotGo.transform;
                CacheInitialState(tf);

                if (!running.Contains(tf))
                {
                    StartCoroutine(FlyAndHide(tf));
                }
            }
        }

        private void CacheInitialState(Transform tf)
        {
            if (!initialLocalPositions.ContainsKey(tf))
            {
                initialLocalPositions[tf] = tf.localPosition;
            }

            if (!initialLocalScales.ContainsKey(tf))
            {
                initialLocalScales[tf] = tf.localScale;
            }

            if (!initialAlphas.ContainsKey(tf))
            {
                initialAlphas[tf] = GetAlpha(tf);
            }
        }

        private IEnumerator FlyAndHide(Transform tf)
        {
            running.Add(tf);

            var startPos = tf.position;
            var startScale = tf.localScale;
            var startAlpha = GetAlpha(tf);

            float t = 0f;
            while (t < duration && tf != null)
            {
                t += Time.deltaTime;
                float u = moveCurve.Evaluate(Mathf.Clamp01(t / duration));

                tf.position = Vector3.Lerp(startPos, target.position, u);
                tf.localScale = Vector3.Lerp(startScale, startScale * endScaleMultiplier, u);
                SetAlpha(tf, Mathf.Lerp(startAlpha, 0f, u));

                yield return null;
            }

            if (tf != null)
            {
                SetAlpha(tf, 0f);
                if (deactivateOnComplete)
                {
                    tf.gameObject.SetActive(false);
                }
                RestoreInitial(tf, startAlpha);
            }

            running.Remove(tf);
            TryPlayPostClip();
        }

        private void RestoreInitial(Transform tf, float defaultAlpha)
        {
            if (tf == null) return;

            if (initialLocalPositions.TryGetValue(tf, out var pos))
            {
                tf.localPosition = pos;
            }

            if (initialLocalScales.TryGetValue(tf, out var scale))
            {
                tf.localScale = scale;
            }

            SetAlpha(tf, initialAlphas.TryGetValue(tf, out var alpha) ? alpha : defaultAlpha);
        }

        private void TryPlayPostClip()
        {
            if (playedPostClip) return;
            if (postAnimationAudioSource == null || postAnimationClip == null) return;

            postAnimationAudioSource.PlayOneShot(postAnimationClip);
            playedPostClip = true;
        }

        private static float GetAlpha(Transform tf)
        {
            if (tf == null) return 1f;

            var g = tf.GetComponent<Graphic>();
            if (g != null) return g.color.a;

            var cg = tf.GetComponent<CanvasGroup>();
            if (cg != null) return cg.alpha;

            var tmp = tf.GetComponent<TMP_Text>();
            if (tmp != null) return tmp.color.a;

            return 1f;
        }

        private static void SetAlpha(Transform tf, float alpha)
        {
            if (tf == null) return;

            var g = tf.GetComponent<Graphic>();
            if (g != null)
            {
                var c = g.color;
                c.a = alpha;
                g.color = c;
            }

            var cg = tf.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = alpha;
            }

            var tmp = tf.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                var c = tmp.color;
                c.a = alpha;
                tmp.color = c;
            }
        }
    }
}

