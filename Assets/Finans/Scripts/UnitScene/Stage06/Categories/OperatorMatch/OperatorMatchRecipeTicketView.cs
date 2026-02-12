using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.Categories.OperatorMatch
{
    [Serializable]
    public struct OperatorRecipeTicket
    {
        public OperatorSign Sign;
        public int Amount;

        public OperatorRecipeTicket(OperatorSign sign, int amount)
        {
            Sign = sign;
            Amount = Mathf.Max(1, amount);
        }
    }

    /// <summary>
    /// Displays a recipe ticket that pairs an operator icon with a quantity.
    /// Shows ghost blobs for the quantity, then optionally reveals the digit and burns away.
    /// All visuals are optional/null-safe so designers can wire only what exists in the scene.
    /// </summary>
    public sealed class OperatorMatchRecipeTicketView : MonoBehaviour
    {
        [Header("Roots")]
        [SerializeField] private GameObject ticketRoot;
        [SerializeField] private Animator ticketAnimator;
        [SerializeField] private string showTrigger = "Show";
        [SerializeField] private string confirmTrigger = "Confirm";
        [SerializeField] private string burnTrigger = "Burn";

        [Header("Operator Icons")]
        [Tooltip("Parent that contains the four operator icons (e.g., Operator_icon).")]
        [SerializeField] private Transform operatorIconRoot;
        [SerializeField] private GameObject addIcon;
        [SerializeField] private GameObject subtractIcon;
        [SerializeField] private GameObject multiplyIcon;
        [SerializeField] private GameObject divideIcon;

        [Header("Amount Visuals")]
        [SerializeField] private TMP_Text amountDigitText;
        [SerializeField] private GameObject amountDigitGroup;
        [SerializeField] private GameObject ghostContainer;
        [SerializeField] private List<GameObject> amountGhostBlobs = new();
        [SerializeField] private bool disableGhostsWhenDigitShown = true;

        [Header("Timings")]
        [SerializeField] private float digitRevealDelay = 0.75f;
        [SerializeField] private float burnHideDelay = 0.35f;

        [Header("VFX (Optional)")]
        [SerializeField] private ParticleSystem burnVfx;

        private int lastAmount = 1;

        private void Awake()
        {
            if (ticketRoot == null)
            {
                ticketRoot = gameObject;
            }

            ResolveOperatorIconsIfNeeded();
            FixDuplicateIconAssignments();
        }

        public void ShowTicket(OperatorRecipeTicket recipe, bool revealDigitImmediately = false)
        {
            lastAmount = Mathf.Max(1, recipe.Amount);
            EnsureRootActive(true);
            ApplyOperatorIcons(recipe.Sign);
            RenderGhosts(lastAmount);
            RenderDigit(lastAmount, revealDigitImmediately);

            if (ticketAnimator != null && !string.IsNullOrWhiteSpace(showTrigger))
            {
                ticketAnimator.ResetTrigger(showTrigger);
                ticketAnimator.SetTrigger(showTrigger);
            }
        }

        public void ShowOperatorConfirmed()
        {
            if (ticketAnimator != null && !string.IsNullOrWhiteSpace(confirmTrigger))
            {
                ticketAnimator.SetTrigger(confirmTrigger);
            }
        }

        public IEnumerator RevealDigit(int? amountOverride = null, float? delaySeconds = null)
        {
            int amount = Mathf.Max(1, amountOverride ?? lastAmount);
            if (delaySeconds.HasValue && delaySeconds.Value > 0f)
            {
                yield return new WaitForSeconds(delaySeconds.Value);
            }
            else if (digitRevealDelay > 0f)
            {
                yield return new WaitForSeconds(digitRevealDelay);
            }

            RenderDigit(amount, true);
        }

        public IEnumerator BurnAndHide()
        {
            if (burnVfx != null && ticketRoot != null)
            {
                burnVfx.transform.position = ticketRoot.transform.position;
                burnVfx.Play();
            }

            if (ticketAnimator != null && !string.IsNullOrWhiteSpace(burnTrigger))
            {
                ticketAnimator.ResetTrigger(burnTrigger);
                ticketAnimator.SetTrigger(burnTrigger);
            }

            if (burnHideDelay > 0f)
            {
                yield return new WaitForSeconds(burnHideDelay);
            }
            EnsureRootActive(false);
        }

        private void RenderGhosts(int amount)
        {
            if (amountGhostBlobs == null || amountGhostBlobs.Count == 0) return;
            if (ghostContainer != null) ghostContainer.SetActive(true);

            for (int i = 0; i < amountGhostBlobs.Count; i++)
            {
                bool on = i < amount;
                var blob = amountGhostBlobs[i];
                if (blob != null) blob.SetActive(on);
            }
        }

        private void RenderDigit(int amount, bool showDigit)
        {
            if (amountDigitText != null)
            {
                amountDigitText.text = amount.ToString();
            }

            if (amountDigitGroup != null)
            {
                amountDigitGroup.SetActive(showDigit);
            }

            if (ghostContainer != null && disableGhostsWhenDigitShown && showDigit)
            {
                ghostContainer.SetActive(false);
            }
        }

        private void ApplyOperatorIcons(OperatorSign sign)
        {
            ResolveOperatorIconsIfNeeded();
            FixDuplicateIconAssignments();

            if (addIcon != null) addIcon.SetActive(sign == OperatorSign.Add);
            if (subtractIcon != null) subtractIcon.SetActive(sign == OperatorSign.Subtract);
            if (multiplyIcon != null) multiplyIcon.SetActive(sign == OperatorSign.Multiply);
            if (divideIcon != null) divideIcon.SetActive(sign == OperatorSign.Divide);

            ToggleOperatorIconRoot(sign);
        }

        private void EnsureRootActive(bool active)
        {
            if (ticketRoot != null)
            {
                ticketRoot.SetActive(active);
            }
            else
            {
                gameObject.SetActive(active);
            }
        }

        private void ResolveOperatorIconsIfNeeded()
        {
            var root = ticketRoot != null ? ticketRoot.transform : transform;
            if (root == null) return;

            if (operatorIconRoot == null)
            {
                var iconRootGo = FindChildByAnyName(root, "Operator_icon", "OperatorIcon", "OperatorIcons", "Icons");
                if (iconRootGo != null) operatorIconRoot = iconRootGo.transform;
            }

            addIcon ??= FindChildByAnyName(root, "Add", "Plus", "OperatorAdd", "Operator_Plus");
            subtractIcon ??= FindChildByAnyName(root, "Subtract", "Minus", "OperatorSubtract", "Operator_Minus");
            multiplyIcon ??= FindChildByAnyName(root, "Multiply", "Times", "OperatorMultiply", "Operator_Times", "X", "Cross");
            divideIcon ??= FindChildByAnyName(root, "Divide", "Slash", "Obelus", "OperatorDivide", "Operator_Divide");
        }

        private GameObject FindChildByAnyName(Transform parent, params string[] names)
        {
            foreach (var n in names)
            {
                var tf = parent.Find(n);
                if (tf != null) return tf.gameObject;

                // fallback: recursive search in case the icons sit deeper (e.g., under Operator_icon)
                tf = FindChildRecursive(parent, n);
                if (tf != null) return tf.gameObject;
            }
            return null;
        }

        private Transform FindChildRecursive(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (string.Equals(child.name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return child;
                }

                var result = FindChildRecursive(child, name);
                if (result != null) return result;
            }

            return null;
        }

        private void FixDuplicateIconAssignments()
        {
            // If the same object was wired to multiple fields, try to re-resolve by name so
            // minus does not accidentally show the plus icon.
            if (addIcon != null && subtractIcon != null && addIcon == subtractIcon)
            {
                subtractIcon = null;
                ResolveOperatorIconsIfNeeded();
            }
            if (addIcon != null && multiplyIcon != null && addIcon == multiplyIcon)
            {
                multiplyIcon = null;
                ResolveOperatorIconsIfNeeded();
            }
            if (addIcon != null && divideIcon != null && addIcon == divideIcon)
            {
                divideIcon = null;
                ResolveOperatorIconsIfNeeded();
            }
            if (subtractIcon != null && multiplyIcon != null && subtractIcon == multiplyIcon)
            {
                multiplyIcon = null;
                ResolveOperatorIconsIfNeeded();
            }
            if (subtractIcon != null && divideIcon != null && subtractIcon == divideIcon)
            {
                divideIcon = null;
                ResolveOperatorIconsIfNeeded();
            }
            if (multiplyIcon != null && divideIcon != null && multiplyIcon == divideIcon)
            {
                divideIcon = null;
                ResolveOperatorIconsIfNeeded();
            }
        }

        private void ToggleOperatorIconRoot(OperatorSign sign)
        {
            if (operatorIconRoot == null) return;

            // Disable all children first.
            for (int i = 0; i < operatorIconRoot.childCount; i++)
            {
                var child = operatorIconRoot.GetChild(i);
                if (child != null) child.gameObject.SetActive(false);
            }

            // Try to enable the matching child by name keywords.
            var target = FindChildRecursive(operatorIconRoot, GetNameHintForSign(sign));
            if (target != null)
            {
                target.gameObject.SetActive(true);
                return;
            }

            // If no direct match, fall back to the serialized icon references.
            switch (sign)
            {
                case OperatorSign.Add:
                    if (addIcon != null) addIcon.SetActive(true);
                    break;
                case OperatorSign.Subtract:
                    if (subtractIcon != null) subtractIcon.SetActive(true);
                    break;
                case OperatorSign.Multiply:
                    if (multiplyIcon != null) multiplyIcon.SetActive(true);
                    break;
                case OperatorSign.Divide:
                    if (divideIcon != null) divideIcon.SetActive(true);
                    break;
            }
        }

        private string GetNameHintForSign(OperatorSign sign)
        {
            return sign switch
            {
                OperatorSign.Add => "Plus",
                OperatorSign.Subtract => "Minus",
                OperatorSign.Multiply => "Multiply",
                OperatorSign.Divide => "Divide",
                _ => string.Empty
            };
        }
    }
}



