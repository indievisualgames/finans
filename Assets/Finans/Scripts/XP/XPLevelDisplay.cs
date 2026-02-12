using TMPro;
using UnityEngine;

public class XPLevelDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private int baseXp = 10;
    [SerializeField] private float growth = 1.2f;
    [SerializeField] private string xpDisplayFormat = "XP {3}";

    private int lastTotalXp = int.MinValue;
    private int currentLevel;
    private int currentInLevelXp;
    private int currentLevelNeed;

    public int CurrentLevel => currentLevel;
    public int CurrentInLevelXp => currentInLevelXp;
    public int CurrentLevelNeed => currentLevelNeed;

    private void OnEnable()
    {
        Refresh(force: true);
    }

    private void Update()
    {
        if (PointSystem.XP != lastTotalXp)
        {
            Refresh(force: true);
        }
    }

    public void Refresh(bool force = false)
    {
        int totalXp = Mathf.Max(0, PointSystem.XP);
        if (!force && totalXp == lastTotalXp) return;

        if (baseXp < 1) baseXp = 1;
        if (growth < 1.0f) growth = 1.0f;

        ComputeLevel(totalXp, out currentLevel, out currentInLevelXp, out currentLevelNeed);
        lastTotalXp = totalXp;

        if (xpText != null)
        {
            // {0}: Level, {1}: In-Level XP, {2}: Needed XP, {3}: Total XP
            xpText.text = string.Format(xpDisplayFormat, currentLevel, currentInLevelXp, currentLevelNeed, totalXp);
        }
    }

    private void ComputeLevel(int totalXp, out int level, out int inLevelXp, out int neededForNext)
    {
        level = 1;
        inLevelXp = totalXp;
        int need = ThresholdForLevel(level);

        while (inLevelXp >= need)
        {
            inLevelXp -= need;
            level++;
            need = ThresholdForLevel(level);
        }

        neededForNext = Mathf.Max(1, need);
    }

    private int ThresholdForLevel(int level)
    {
        float required = baseXp * Mathf.Pow(growth, Mathf.Max(0, level - 1));
        return Mathf.CeilToInt(required);
    }
}


