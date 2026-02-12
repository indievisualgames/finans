using System.Threading.Tasks;

public class SceneNavigatorAdapter : ISceneNavigator
{
	public Task<bool> LoadAsync(string sceneName, bool additive = false)
	{
		return SceneNavigator.SafeLoadSceneAsync(sceneName, additive);
	}
}








