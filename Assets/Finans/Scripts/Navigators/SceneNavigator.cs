using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using Ricimi;

public static class SceneNavigator
{
	private static bool isLoading;

	public static async Task<bool> SafeLoadSceneAsync(string sceneName, bool additive = false)
	{
		if (isLoading) return false;
		isLoading = true;
		try
		{
			if (additive)
			{
				AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
				if (op == null) return false;
				while (!op.isDone) await Task.Yield();
			}
			else
			{
				AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
				if (op == null) return false;
				while (!op.isDone) await Task.Yield();
			}
			return true;
		}
		catch (System.Exception ex)
		{
			Logger.LogError($"Failed to load scene {sceneName}", "SceneNavigator", ex);
			return false;
		}
		finally
		{
			isLoading = false;
		}
	}

	public static void LoadWithTransitionOrSceneManager(string sceneName)
	{
		try
		{
			TransitionAdditive.LoadLevel(sceneName, Params.SceneTransitionDuration, Params.SceneTransitionColor);
		}
		catch
		{
			SceneManager.LoadScene(sceneName);
		}
	}
}


