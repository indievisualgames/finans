using System.Threading.Tasks;

public interface ISceneNavigator
{
	Task<bool> LoadAsync(string sceneName, bool additive = false);
}








