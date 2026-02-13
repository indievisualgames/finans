using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Finans.ActivitySystem
{
    /// <summary>
    /// Binds ALL activity data directly from ActivityContent to 19 TMP fields.
    /// No manifest needed — everything comes from the single JSON file.
    /// </summary>
    public class ActivityCardBinder : MonoBehaviour
    {
        // ── Header Section ──
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI activityNameTMP;       // 1. activityName
        [SerializeField] private TextMeshProUGUI activityTypeTMP;       // 2. activityType
        [SerializeField] private TextMeshProUGUI dayBadgeTMP;           // 3. dayNumber

        // ── Meta Chips ──
        [Header("Meta Chips")]
        [SerializeField] private TextMeshProUGUI timeChipTMP;           // 4. metaInformation.timeToComplete
        [SerializeField] private TextMeshProUGUI levelChipTMP;          // 5. metaInformation.difficultyLevel
        [SerializeField] private TextMeshProUGUI ageChipTMP;            // 6. metaInformation.ageRange
        [SerializeField] private TextMeshProUGUI rewardChipTMP;         // 7. metaInformation.completionReward
        
        [SerializeField] private Image timeChipImage;
        [SerializeField] private Image levelChipImage;
        [SerializeField] private Image ageChipImage;
        [SerializeField] private Image rewardChipImage;

        // ── Content Section ──
        [Header("Content")]
        [SerializeField] private TextMeshProUGUI headerTitleTMP;        // 8. headerInfo.title
        [SerializeField] private TextMeshProUGUI headerSubtitleTMP;     // 9. headerInfo.subtitle
        [SerializeField] private TextMeshProUGUI heroTextTMP;           // 10. bigIdea.heroText
        [SerializeField] private TextMeshProUGUI introLineTMP;          // 11. introduction.heroLine
        [SerializeField] private TextMeshProUGUI descriptionTMP;        // 12. introduction.supportingCopy

        // ── Learning Section ──
        [Header("Learning")]
        [SerializeField] private TextMeshProUGUI learningGoalsTMP;      // 13. learningGoals (joined)
        [SerializeField] private TextMeshProUGUI learningOutcomesTMP;   // 14. learningOutcomes (joined)

        // ── Activity Details Section ──
        [Header("Activity Details")]
        [SerializeField] private TextMeshProUGUI materialsTMP;          // 15. materials.mediaTypes (joined)
        [SerializeField] private TextMeshProUGUI interactiveDescTMP;    // 16. interactiveActivities.description
        [SerializeField] private TextMeshProUGUI deliverablesTMP;       // 17. deliverables.learnerOutputs (joined)

        // ── Rewards Section ──
        [Header("Rewards")]
        [SerializeField] private TextMeshProUGUI xpPointsTMP;           // 18. rewards.experiencePoints
        [SerializeField] private TextMeshProUGUI starsTMP;              // 19. rewards.starsEarned

        // ── Visual Elements ──
        [Header("Visuals")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Lesson Cards")]
        [SerializeField] private Transform heroSection;
        [SerializeField] private Transform textHolder;
        [SerializeField] private GameObject[] lessonCards; // 7 slots for Day 1-7

        // ── State ──
        private GameObject spawnedLessonCard;

        // ── Data ──
        private string activityId;
        public event Action<string> OnCardTapped;

        /// <summary>
        /// Bind ALL data directly from the JSON-parsed ActivityContent.
        /// No manifest, no separate lookup — one call does everything.
        /// </summary>
        public void BindFullContent(ActivityContent content)
        {
            activityId = content.activityId;

            // ── Header ──
            SetTMP(activityNameTMP,    content.activityName);
            SetTMP(activityTypeTMP,    content.activityType?.ToUpper());
            SetTMP(dayBadgeTMP,        $"DAY {content.dayNumber:00}");

            // ── Meta Chips ──
            SetTMP(timeChipTMP,        $"TIME: {content.metaInformation?.timeToComplete}");
            SetTMP(levelChipTMP,       $"LEVEL: {content.metaInformation?.difficultyLevel}");
            SetTMP(ageChipTMP,         $"AGE: {content.metaInformation?.ageRange}");
            SetTMP(rewardChipTMP,      $"REWARD: {content.metaInformation?.completionReward ?? 0} XP");

            // ── Content ──
            SetTMP(headerTitleTMP,     content.headerInfo?.title);
            SetTMP(headerSubtitleTMP,  content.headerInfo?.subtitle);
            SetTMP(heroTextTMP,        content.bigIdea?.heroText);
            SetTMP(introLineTMP,       content.introduction?.heroLine);
            SetTMP(descriptionTMP,     content.introduction?.supportingCopy);

            // ── Learning ──
            SetTMP(learningGoalsTMP,    "<b>LEARNING GOALS:</b>\n" + JoinGoals(content.learningGoals));
            SetTMP(learningOutcomesTMP, "<b>OUTCOMES:</b>\n" + JoinOutcomes(content.learningOutcomes));

            // ── Activity Details ──
            SetTMP(materialsTMP,       "<b>WHAT YOU'LL NEED:</b>\n" + JoinMaterials(content.materials));
            SetTMP(interactiveDescTMP, content.interactiveActivities?.description);
            SetTMP(deliverablesTMP,    "<b>DELIVERABLES:</b>\n" + JoinDeliverables(content.deliverables));

            // ── Rewards ──
            SetTMP(xpPointsTMP,        $"+{content.rewards?.experiencePoints ?? 0} XP");
            SetTMP(starsTMP,           new string('★', content.rewards?.starsEarned ?? 0));

            // ── Visuals ──
            ApplyColorTheme(content.colorTheme);
            LoadIcon(content.iconReference);
            SetLockState(content.isLocked);

            // ── Lesson Card Instantiation ──
            UpdateLessonCard(content.dayNumber);

            // ── Chip Icons ──
            UpdateChipIcons();

            Debug.Log($"[CardBinder] Bound and styled 19 fields for: {content.activityName} (Day {content.dayNumber})");
        }

        /// <summary>
        /// Safe TMP text setter — skips if field is null
        /// </summary>
        private void SetTMP(TextMeshProUGUI tmp, string value)
        {
            if (tmp != null) tmp.text = value ?? "";
        }

        private void ApplyColorTheme(ColorTheme theme)
        {
            if (cardBackground == null) return;

            // 1. Set Background Image (BG2.png)
            Sprite bgSprite = Resources.Load<Sprite>("UI/Backgrounds/BG2");
            if (bgSprite != null)
            {
                cardBackground.sprite = bgSprite;
                cardBackground.type = Image.Type.Sliced; // Assuming it's a sliced sprite for UI
            }

            if (theme == null) return;

            // 2. Apply Opaque Primary Color
            if (ColorUtility.TryParseHtmlString(theme.primary, out Color primaryCol))
            {
                primaryCol.a = 1.0f; // Force Opaque
                cardBackground.color = primaryCol;
            }

            // 3. Apply Gradient (Removed as per request)
            // UIGradient usage removed to fix CS0246 error

        }

        private void UpdateChipIcons()
        {
            // Fail-safe auto-find for non-regenerated prefabs
            if (timeChipImage == null) timeChipImage = transform.Find("MainContent/MetaInfo/TimeChipTMP_Chip")?.GetComponent<Image>();
            if (levelChipImage == null) levelChipImage = transform.Find("MainContent/MetaInfo/LevelChipTMP_Chip")?.GetComponent<Image>();
            if (ageChipImage == null) ageChipImage = transform.Find("MainContent/MetaInfo/AgeChipTMP_Chip")?.GetComponent<Image>();
            if (rewardChipImage == null) rewardChipImage = transform.Find("MainContent/MetaInfo/RewardChipTMP_Chip")?.GetComponent<Image>();

            SetChipSprite(timeChipImage,   "UI/Icons/icon_time");
            SetChipSprite(levelChipImage,  "UI/Icons/icon_level");
            SetChipSprite(ageChipImage,    "UI/Icons/icon_age");
            SetChipSprite(rewardChipImage, "UI/Icons/icon_reward");
        }

        private void SetChipSprite(Image img, string path)
        {
            if (img == null) return;
            Sprite s = Resources.Load<Sprite>(path);
            if (s != null)
            {
                img.sprite = s;
                img.color = Color.white; // Ensure it's not tinted if using a graphic
            }
        }

        private void LoadIcon(string iconRef)
        {
            if (iconImage == null || string.IsNullOrEmpty(iconRef)) return;
            Sprite sprite = Resources.Load<Sprite>(iconRef);
            if (sprite != null) iconImage.sprite = sprite;
        }

        private void SetLockState(bool locked)
        {
            if (lockIcon != null) lockIcon.SetActive(locked);
            
            if (canvasGroup != null)
            {
                // Previously was locked ? 0.5f : 1f;
                // Keeping it at 1.0f now for maximum clarity as requested by the user.
                canvasGroup.alpha = 1.0f; 
                canvasGroup.interactable = !locked;
                canvasGroup.blocksRaycasts = !locked; // Also ensure doesn't block if locked
            }
        }

        private void UpdateLessonCard(int dayNumber)
        {
            // 1. Cleanup old card
            if (spawnedLessonCard != null)
            {
                Destroy(spawnedLessonCard);
                spawnedLessonCard = null;
            }

            // 2. Auto-bind references if missing (Fail-safe for non-regenerated prefabs)
            if (heroSection == null) heroSection = transform.Find("MainContent/HeroSection");
            if (textHolder == null) textHolder = transform.Find("MainContent/HeroSection/Text Holder");

            if (heroSection == null)
            {
                Debug.LogWarning("[CardBinder] Cannot find HeroSection in hierarchy. Lesson card won't spawn.");
                return;
            }

            // 3. Get Prefab
            GameObject prefabToSpawn = null;
            int index = dayNumber - 1;

            // Try Serialized Array first
            if (lessonCards != null && index >= 0 && index < lessonCards.Length && lessonCards[index] != null)
            {
                prefabToSpawn = lessonCards[index];
            }
            else
            {
                // Fallback: Try Resources.Load
                // Expected Path: Assets/Finans/Resources/Day Cards/Lesson_Card_01
                string resourcePath = $"Day Cards/Lesson_Card_{dayNumber:00}";
                prefabToSpawn = Resources.Load<GameObject>(resourcePath);
            }

            // 4. Instantiate if found
            if (prefabToSpawn != null)
            {
                spawnedLessonCard = Instantiate(prefabToSpawn, heroSection);
                spawnedLessonCard.name = $"Lesson_Card_Day_{dayNumber}";

                // Scale up by 75% (1.0 -> 1.75)
                spawnedLessonCard.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);

                // 5. Place above TextHolder if it exists
                if (textHolder != null)
                {
                    int textIndex = textHolder.GetSiblingIndex();
                    spawnedLessonCard.transform.SetSiblingIndex(textIndex);
                }
            }
            else
            {
                Debug.LogWarning($"[CardBinder] Could not find lesson card for Day {dayNumber} in array or Resources.");
            }
        }

        // ── Array-to-String Helpers ──

        private string JoinGoals(System.Collections.Generic.List<LearningGoal> goals)
        {
            if (goals == null || goals.Count == 0) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var g in goals)
                sb.AppendLine($"- {g.goalText}");
            return sb.ToString().TrimEnd();
        }

        private string JoinOutcomes(System.Collections.Generic.List<LearningOutcome> outcomes)
        {
            if (outcomes == null || outcomes.Count == 0) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var o in outcomes)
                sb.AppendLine($"- {o.outcomeText}");
            return sb.ToString().TrimEnd();
        }

        private string JoinMaterials(Materials mats)
        {
            if (mats?.mediaTypes == null || mats.mediaTypes.Count == 0) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var m in mats.mediaTypes)
                sb.AppendLine($"- {m.description}");
            return sb.ToString().TrimEnd();
        }

        private string JoinDeliverables(Deliverables del)
        {
            if (del?.learnerOutputs == null || del.learnerOutputs.Count == 0) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var d in del.learnerOutputs)
                sb.AppendLine($"- {d.outputText}");
            return sb.ToString().TrimEnd();
        }

        public void OnCardClicked()
        {
            if (!string.IsNullOrEmpty(activityId))
                OnCardTapped?.Invoke(activityId);
        }

        public string GetActivityId() => activityId;
    }
}
