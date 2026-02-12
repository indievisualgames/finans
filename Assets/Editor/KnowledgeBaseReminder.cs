// Editor reminder and menu to review PROJECT_KNOWLEDGE_BASE.md on project open
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EparentFinans.Editor
{
    [InitializeOnLoad]
    public static class KnowledgeBaseReminder
    {
        private const bool StartupPromptEnabled = false; // disable popup on launch
        private const string MenuOpenPath = "Tools/Knowledge Base/Open Project Knowledge Base";
        private const string MenuDisableRemindersPath = "Tools/Knowledge Base/Don't remind me again";
        private const string MenuEnableRemindersPath = "Tools/Knowledge Base/Re-enable reminders";

        private const string PrefKeyLastPrompt = "EparentFinans.KnowledgeBase.LastPromptDate";
        private const string PrefKeySuppress = "EparentFinans.KnowledgeBase.Suppress";
        private const string KnowledgeBaseFileName = "PROJECT_KNOWLEDGE_BASE.md";

        static KnowledgeBaseReminder()
        {
            EditorApplication.update += OnEditorUpdateOnce;
        }

        private static void OnEditorUpdateOnce()
        {
            EditorApplication.update -= OnEditorUpdateOnce;
            if (!StartupPromptEnabled) return; // no popup in Unity editor

            if (EditorPrefs.GetBool(PrefKeySuppress, false)) return;

            bool open = EditorUtility.DisplayDialog(
                "Review Project Knowledge Base",
                "Please review/update PROJECT_KNOWLEDGE_BASE.md for this session.",
                "Open Now",
                "Later");
            if (open)
            {
                OpenKnowledgeBase();
            }
        }

        [MenuItem(MenuOpenPath, priority = 200)]
        public static void OpenKnowledgeBase()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string kbPath = Path.Combine(projectRoot, KnowledgeBaseFileName);
            if (File.Exists(kbPath))
            {
                EditorUtility.OpenWithDefaultApp(kbPath);
                return;
            }

            bool create = EditorUtility.DisplayDialog(
                "Knowledge Base Not Found",
                $"Could not find {KnowledgeBaseFileName} at the project root. Create it now?",
                "Create",
                "Cancel");
            if (create)
            {
                try
                {
                    File.WriteAllText(kbPath, "## Eparent-Finans Knowledge Base\n\n");
                    AssetDatabase.Refresh();
                    EditorUtility.OpenWithDefaultApp(kbPath);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to create {KnowledgeBaseFileName}: {ex.Message}");
                }
            }
        }

        [MenuItem(MenuDisableRemindersPath, priority = 201)]
        public static void DisableReminders()
        {
            EditorPrefs.SetBool(PrefKeySuppress, true);
            EditorUtility.DisplayDialog("Knowledge Base", "Daily reminders disabled.", "OK");
        }

        [MenuItem(MenuEnableRemindersPath, priority = 202)]
        public static void EnableReminders()
        {
            EditorPrefs.SetBool(PrefKeySuppress, false);
            EditorPrefs.DeleteKey(PrefKeyLastPrompt);
            EditorUtility.DisplayDialog("Knowledge Base", "Daily reminders enabled.", "OK");
        }
    }
}


