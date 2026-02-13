using System;
using System.Collections.Generic;
using UnityEngine;

namespace Finans.ActivitySystem
{
    /*
     * This file defines the Serializable data classes that match the JSON structure
     * for ActivityContent. These are used by ActivityManager (to load JSON)
     * and ActivityCardBinder (to display data).
     */

    [Serializable]
    public class ActivityContent
    {
        public string activityId;
        public string activityName;
        public string activityType; // e.g. "Challenge", "Project"
        public int dayNumber;
        public bool isLocked;

        public MetaInformation metaInformation;
        public HeaderInfo headerInfo;
        public BigIdea bigIdea;
        public Introduction introduction;
        public Rewards rewards;
        
        // Learning
        public List<LearningGoal> learningGoals;
        public List<LearningOutcome> learningOutcomes;
        
        // Details
        public Materials materials;
        public InteractiveActivities interactiveActivities;
        public Deliverables deliverables;
        
        // Visual
        public string iconReference;
        public ColorTheme colorTheme;
    }

    [Serializable]
    public class MetaInformation
    {
        public string timeToComplete;
        public string difficultyLevel;
        public string ageRange;
        public int completionReward;
    }

    [Serializable]
    public class HeaderInfo
    {
        public string title;
        public string subtitle;
    }

    [Serializable]
    public class BigIdea
    {
        public string heroText;
    }

    [Serializable]
    public class Introduction
    {
        public string heroLine;
        public string supportingCopy;
    }

    [Serializable]
    public class Rewards
    {
        public int experiencePoints;
        public int starsEarned;
    }

    [Serializable]
    public class LearningGoal
    {
        public string goalText;
    }

    [Serializable]
    public class LearningOutcome
    {
        public string outcomeText;
    }

    [Serializable]
    public class Materials
    {
        public List<MediaType> mediaTypes;
    }

    [Serializable]
    public class MediaType
    {
        public string description;
    }

    [Serializable]
    public class InteractiveActivities
    {
        public string description;
    }

    [Serializable]
    public class Deliverables
    {
        public List<LearnerOutput> learnerOutputs;
    }

    [Serializable]
    public class LearnerOutput
    {
        public string outputText;
    }

    [Serializable]
    public class ColorTheme
    {
        public string primary;       // Hex color
        public string gradientStart; // Hex color
        public string gradientEnd;   // Hex color
    }
}
