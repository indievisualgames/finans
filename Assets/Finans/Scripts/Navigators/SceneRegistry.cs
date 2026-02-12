using System.Collections.Generic;

public static class SceneRegistry
{
	private static readonly Dictionary<string, string> StageToScene = new Dictionary<string, string>
	{
		{ "maingame", "MainGame" },
		{ "lesson", "Lesson" },
		{ "flashcard", "FlashCard" },
		{ "fashcard", "FlashCard" },
		{ "minigames", "MiniGames" },
		{ "vocabs", "Vocabs" },
		{ "calculator", "Calculator" },
		{ "video", "Video" },
		{ "storybook", "StoryBook" },
		{ "worksheets", "WorkSheets" },
		{ "funfact", "FunFact" },
		{ "finsong", "FinSong" },
		{ "megaquiz", "MegaQuiz" }
	};

	public static bool TryResolve(string stageKeyLower, out string sceneName)
	{
		return StageToScene.TryGetValue(stageKeyLower, out sceneName);
	}
}








