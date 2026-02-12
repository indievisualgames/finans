using System.Collections;
using UnityEngine;
using TMPro;

namespace Game.Categories.OperatorMatch
{
    /// <summary>
    /// A small view that toggles operator visuals. This intentionally keeps the old
    /// serialized field names from the previous Jelly reactor so existing scene
    /// references continue to work without re-wiring.
    ///
    /// Mapping (legacy field names → operator):
    /// - lowVisual    → +
    /// - highVisual   → −
    /// - lonelyVisual → ×
    /// - largeVisual  → ÷
    /// </summary>
    public sealed class OperatorMatchPopupView : MonoBehaviour
    {
        // Operator visuals are resolved automatically from children under this GameObject.
        // Expected child names in the current scene: Plus, Minus, Multiply, Divide.
        private GameObject plusVisual;
        private GameObject minusVisual;
        private GameObject multiplyVisual;
        private GameObject divideVisual;

        [Header("Operator Message (optional)")]
        [SerializeField] private TMP_Text operatorMessageText;
        [Tooltip("If `operatorMessageText` is not wired, try to find `Msg_Panel/Operator_message` automatically.")]
        [SerializeField] private bool autoFindOperatorMessageText = true;

        [TextArea(1, 4)]
        [SerializeField] private string addMessage = "Addition (+)";
        [TextArea(1, 4)]
        [SerializeField] private string subtractMessage = "Subtraction (−)";
        [TextArea(1, 4)]
        [SerializeField] private string multiplyMessage = "Multiplication (×)";
        [TextArea(1, 4)]
        [SerializeField] private string divideMessage = "Division (÷)";

        [Header("Optional Animator (kept for compatibility)")]
        [SerializeField] private Animator jellyAnimator;
        [SerializeField] private string reactTrigger = "React";
        [SerializeField] private string idleTrigger = "Idle";
        [SerializeField] private float reactionHold = 1f;

        private OperatorSign? lastSyncedSign;
        private bool hasLoggedMissingVisualsWarning;
        private bool hasNoVisuals;

        private void Awake()
        {
            ResolveOperatorVisualsIfNeeded();
            TryAutoWireOperatorMessageText();
        }

        private void OnEnable()
        {
            ResolveOperatorVisualsIfNeeded();
            TryAutoWireOperatorMessageText();
            SyncMessageFromActiveVisuals();
        }

        private void LateUpdate()
        {
            // Early exit if there's no message text or no visuals to sync
            if (operatorMessageText == null || hasNoVisuals) return;
            
            // Keep message in sync even if some other system toggles visuals directly.
            SyncMessageFromActiveVisuals();
        }

        public void Show(OperatorSign sign)
        {
            ShowOnly(sign);
            UpdateMessage(sign);
            PlayReact();
        }

        public void HideAll()
        {
            if (plusVisual) plusVisual.SetActive(false);
            if (minusVisual) minusVisual.SetActive(false);
            if (multiplyVisual) multiplyVisual.SetActive(false);
            if (divideVisual) divideVisual.SetActive(false);
        }

        private void ShowOnly(OperatorSign sign)
        {
            ResolveOperatorVisualsIfNeeded();
            if (plusVisual) plusVisual.SetActive(sign == OperatorSign.Add);
            if (minusVisual) minusVisual.SetActive(sign == OperatorSign.Subtract);
            if (multiplyVisual) multiplyVisual.SetActive(sign == OperatorSign.Multiply);
            if (divideVisual) divideVisual.SetActive(sign == OperatorSign.Divide);
        }

        private void UpdateMessage(OperatorSign sign)
        {
            if (operatorMessageText == null) return;

            operatorMessageText.text = GetMessageForSign(sign);
        }

        private void SyncMessageFromActiveVisuals()
        {
            if (operatorMessageText == null || hasNoVisuals) return;

            if (!TryGetActiveSign(out var activeSign))
            {
                if (lastSyncedSign.HasValue)
                {
                    lastSyncedSign = null;
                    // Clear message when no sign is active
                    operatorMessageText.text = string.Empty;
                }
                return;
            }

            // Only update if sign changed (avoid expensive string comparison every frame)
            if (!lastSyncedSign.HasValue || lastSyncedSign.Value != activeSign)
            {
                lastSyncedSign = activeSign;
                operatorMessageText.text = GetMessageForSign(activeSign);
            }
        }

        private bool TryGetActiveSign(out OperatorSign sign)
        {
            ResolveOperatorVisualsIfNeeded();

            // Priority order: if multiple are active (should not happen), pick the first match.
            if (plusVisual != null && plusVisual.activeInHierarchy)
            {
                sign = OperatorSign.Add;
                return true;
            }
            if (minusVisual != null && minusVisual.activeInHierarchy)
            {
                sign = OperatorSign.Subtract;
                return true;
            }
            if (multiplyVisual != null && multiplyVisual.activeInHierarchy)
            {
                sign = OperatorSign.Multiply;
                return true;
            }
            if (divideVisual != null && divideVisual.activeInHierarchy)
            {
                sign = OperatorSign.Divide;
                return true;
            }

            sign = OperatorSign.Add;
            return false;
        }

        private string GetMessageForSign(OperatorSign sign)
        {
            // Fallback behavior: if a message is empty, show a default.
            return sign switch
            {
                OperatorSign.Add => string.IsNullOrWhiteSpace(addMessage) ? "Press +" : addMessage,
                OperatorSign.Subtract => string.IsNullOrWhiteSpace(subtractMessage) ? "Press −" : subtractMessage,
                OperatorSign.Multiply => string.IsNullOrWhiteSpace(multiplyMessage) ? "Press ×" : multiplyMessage,
                OperatorSign.Divide => string.IsNullOrWhiteSpace(divideMessage) ? "Press ÷" : divideMessage,
                _ => string.Empty
            };
        }

        private void ResolveOperatorVisualsIfNeeded()
        {
            // Early exit if already resolved
            if (plusVisual != null && minusVisual != null && multiplyVisual != null && divideVisual != null)
            {
                hasNoVisuals = false;
                return;
            }

            // Early exit if we already determined there are no visuals
            if (hasNoVisuals) return;

            // Find by current scene naming convention under OperatorPopup.
            // (These are children of the same GameObject that holds this component.)
            plusVisual ??= transform.Find("Plus")?.gameObject;
            minusVisual ??= transform.Find("Minus")?.gameObject;
            multiplyVisual ??= transform.Find("Multiply")?.gameObject;
            divideVisual ??= transform.Find("Divide")?.gameObject;

            if (plusVisual == null && minusVisual == null && multiplyVisual == null && divideVisual == null)
            {
                // Mark that we have no visuals to avoid future resolution attempts
                hasNoVisuals = true;
                
                // This component exists in some scenes on a placeholder OperatorPopup (no children).
                // Keep it safe: do nothing, but log once per instance in play mode for easier debugging.
                if (Application.isPlaying && !hasLoggedMissingVisualsWarning)
                {
                    hasLoggedMissingVisualsWarning = true;
                    Debug.LogWarning(
                        $"[{nameof(OperatorMatchPopupView)}] No operator visuals found under '{name}'. " +
                        "Expected children named Plus/Minus/Multiply/Divide. Message sync will not work for this instance.",
                        this);
                }
            }
            else
            {
                hasNoVisuals = false;
            }
        }

        private void TryAutoWireOperatorMessageText()
        {
            if (!autoFindOperatorMessageText || operatorMessageText != null) return;

            // Best-effort lookup by scene object names.
            var panel = GameObject.Find("Msg_Panel");
            if (panel == null) return;

            var messageTf = panel.transform.Find("Operator_message");
            if (messageTf == null) return;

            operatorMessageText = messageTf.GetComponent<TMP_Text>();
        }

        private void PlayReact()
        {
            if (jellyAnimator == null) return;
            jellyAnimator.ResetTrigger(idleTrigger);
            jellyAnimator.SetTrigger(reactTrigger);
            StopAllCoroutines();
            StartCoroutine(ReturnToIdle());
        }

        private IEnumerator ReturnToIdle()
        {
            yield return new WaitForSeconds(reactionHold);
            if (jellyAnimator != null)
            {
                jellyAnimator.ResetTrigger(reactTrigger);
                jellyAnimator.SetTrigger(idleTrigger);
            }
        }
    }
}


