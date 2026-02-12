using UnityEngine;
using System.Collections;
using System.Linq;

namespace Game.Core
{
    public sealed class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        [Header("Clips")] 
        [SerializeField] private AudioClip keyPress;
        [SerializeField] private AudioClip correct;
        [SerializeField] private AudioClip wrong;
        [SerializeField] private AudioClip memoryStore;
        [SerializeField] private AudioClip memoryRetrieve;
        [SerializeField] private AudioClip complete; // distinct completion cue

        [Header("Immediate Sounds (First Sound - Plays with VFX)")]
        [Tooltip("First sound that plays immediately when correct answer is given (plays with VFX).")]
        [SerializeField] public AudioClip correctImmediateSound;
        [Tooltip("First sound that plays immediately when wrong answer is given (plays with VFX).")]
        [SerializeField] public AudioClip wrongImmediateSound;

        [Header("Random Audio Groups (Second Sound - Plays After Delay)")]
        [Tooltip("Array of correct audio clips for the second sound (plays after delay). If populated, PlayCorrect() will randomly select from this array.")]
        [SerializeField] private AudioClip[] correctAudioGroup;
        [Tooltip("Array of wrong audio clips for the second sound (plays after delay). If populated, PlayWrong() will randomly select from this array.")]
        [SerializeField] private AudioClip[] wrongAudioGroup;

        [Header("Audio Delay Settings")]
        [Tooltip("Delay in seconds before playing correct or wrong sound effects. Default: 1.5 seconds.")]
        [SerializeField] public float correctWrongSoundDelay = 1.5f;

        private AudioSource oneShotSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            oneShotSource = gameObject.AddComponent<AudioSource>();
        }

        public void PlayKeyPress() => Play(keyPress);
        
        /// <summary>
        /// Plays a random correct audio clip from the correctAudioGroup array if available,
        /// otherwise falls back to the single correct clip.
        /// Includes a delay before playing (configurable via correctWrongSoundDelay).
        /// </summary>
        public void PlayCorrect()
        {
            StartCoroutine(PlayCorrectDelayed());
        }
        
        private IEnumerator PlayCorrectDelayed()
        {
            yield return new WaitForSeconds(correctWrongSoundDelay);
            
            if (correctAudioGroup != null && correctAudioGroup.Length > 0)
            {
                // Filter out null clips and select random
                var validClips = correctAudioGroup.Where(clip => clip != null).ToArray();
                if (validClips.Length > 0)
                {
                    int randomIndex = Random.Range(0, validClips.Length);
                    Play(validClips[randomIndex]);
                    yield break;
                }
            }
            // Fallback to single clip
            Play(correct);
        }
        
        /// <summary>
        /// Plays a random wrong audio clip from the wrongAudioGroup array if available,
        /// otherwise falls back to the single wrong clip.
        /// Includes a delay before playing (configurable via correctWrongSoundDelay).
        /// </summary>
        public void PlayWrong()
        {
            StartCoroutine(PlayWrongDelayed());
        }
        
        private IEnumerator PlayWrongDelayed()
        {
            yield return new WaitForSeconds(correctWrongSoundDelay);
            
            if (wrongAudioGroup != null && wrongAudioGroup.Length > 0)
            {
                // Filter out null clips and select random
                var validClips = wrongAudioGroup.Where(clip => clip != null).ToArray();
                if (validClips.Length > 0)
                {
                    int randomIndex = Random.Range(0, validClips.Length);
                    Play(validClips[randomIndex]);
                    yield break;
                }
            }
            // Fallback to single clip
            Play(wrong);
        }
        
        /// <summary>
        /// Plays the immediate correct audio clip (no delay).
        /// Uses the single correctImmediateSound field.
        /// Use this when VFX is triggered to play sound synchronously.
        /// </summary>
        public void PlayCorrectImmediate()
        {
            if (correctImmediateSound != null)
            {
                Play(correctImmediateSound);
            }
            else
            {
                // Fallback to default correct clip if immediate sound not assigned
                Play(correct);
            }
        }
        
        /// <summary>
        /// Plays the immediate wrong audio clip (no delay).
        /// Uses the single wrongImmediateSound field.
        /// Use this when VFX is triggered to play sound synchronously.
        /// </summary>
        public void PlayWrongImmediate()
        {
            if (wrongImmediateSound != null)
            {
                Play(wrongImmediateSound);
            }
            else
            {
                // Fallback to default wrong clip if immediate sound not assigned
                Play(wrong);
            }
        }
        
        public void PlayMemoryStore() => Play(memoryStore);
        public void PlayMemoryRetrieve() => Play(memoryRetrieve);
        public void PlayComplete() => Play(complete);

        private void Play(AudioClip clip)
        {
            if (clip == null) return;
            oneShotSource.PlayOneShot(clip);
        }
    }
}


