using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Categories.OperatorMatch
{
    /// <summary>
    /// Plays operator-specific audio from a 4-slot clip array.
    /// Slot order:
    /// 0: + (Add)
    /// 1: − (Subtract)
    /// 2: × / * (Multiply)
    /// 3: ÷ / / (Divide)
    /// </summary>
    public sealed class OperatorMatchOperatorAudioPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        [Tooltip("Primary clips. Order: 0:+  1:−  2:×  3:÷")]
        [SerializeField] private AudioClip[] operatorClips = new AudioClip[4];

        [Tooltip("Secondary clips (played after primary). Order: 0:+  1:−  2:×  3:÷")]
        [SerializeField] private AudioClip[] operatorSecondaryClips = new AudioClip[4];

        [Tooltip("Common clip played after primary+secondary for every operator (if assigned).")]
        [SerializeField] private AudioClip commonAfterEachOperatorClip;

        [Header("Optional UI Hide Before Common")]
        [Tooltip("If not assigned, will try to find the Image on `Msg_Panel`.")]
        [SerializeField] private Image msgPanelImage;
        [Tooltip("If not assigned, will try to find the Image on `Msg_Panel/Message_Bubble_Image`.")]
        [SerializeField] private Image messageBubbleImage;
        [Tooltip("If not assigned, will try to find TMP on `Msg_Panel/Operator_message`.")]
        [SerializeField] private TMP_Text operatorMessageText;

        [Range(0f, 1f)]
        [SerializeField] private float volume = 1f;

        private Coroutine playRoutine;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = GetComponentInChildren<AudioSource>(true);
                }
            }

            EnsureUiRefs();
        }

        private void OnDisable()
        {
            // Safety: never leave UI disabled.
            SetMessageUiEnabled(true);
        }

        public void PlayFor(OperatorSign sign)
        {
            if (audioSource == null) return;
            int idx = ToIndex(sign);
            if (idx < 0) return;

            var primary = (operatorClips != null && idx < operatorClips.Length) ? operatorClips[idx] : null;
            var secondary = (operatorSecondaryClips != null && idx < operatorSecondaryClips.Length) ? operatorSecondaryClips[idx] : null;

            if (primary == null && secondary == null && commonAfterEachOperatorClip == null) return;

            // Next round reset: re-enable message UI at the start of a new operator prompt.
            EnsureUiRefs();
            SetMessageUiEnabled(true);

            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

            // Stop any current clip for clean sequencing.
            audioSource.Stop();
            audioSource.clip = null;

            playRoutine = StartCoroutine(PlaySequence(primary, secondary, commonAfterEachOperatorClip));
        }

        private IEnumerator PlaySequence(AudioClip primary, AudioClip secondary, AudioClip common)
        {
            yield return PlayClip(primary);
            yield return PlayClip(secondary);

            // Requirement: after primary(+secondary) and just before the common clip, hide message UI.
            if (common != null)
            {
                EnsureUiRefs();
                SetMessageUiEnabled(false);
            }

            yield return PlayClip(common);
            playRoutine = null;
        }

        private IEnumerator PlayClip(AudioClip clip)
        {
            if (audioSource == null || clip == null) yield break;

            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();
            yield return new WaitForSeconds(clip.length);
        }

        private void EnsureUiRefs()
        {
            if (msgPanelImage != null && operatorMessageText != null && messageBubbleImage != null) return;

            var panel = GameObject.Find("Msg_Panel");
            if (panel != null)
            {
                msgPanelImage ??= panel.GetComponent<Image>();

                var bubbleTf = panel.transform.Find("Message_Bubble_Image");
                if (bubbleTf != null)
                {
                    messageBubbleImage ??= bubbleTf.GetComponent<Image>();
                }

                var msgTf = panel.transform.Find("Operator_message");
                if (msgTf != null)
                {
                    operatorMessageText ??= msgTf.GetComponent<TMP_Text>();
                }
            }
        }

        private void SetMessageUiEnabled(bool enabled)
        {
            if (msgPanelImage != null) msgPanelImage.enabled = enabled;
            if (messageBubbleImage != null) messageBubbleImage.enabled = enabled;
            if (operatorMessageText != null) operatorMessageText.enabled = enabled;
        }

        private static int ToIndex(OperatorSign sign)
        {
            return sign switch
            {
                OperatorSign.Add => 0,
                OperatorSign.Subtract => 1,
                OperatorSign.Multiply => 2,
                OperatorSign.Divide => 3,
                _ => -1
            };
        }
    }
}


