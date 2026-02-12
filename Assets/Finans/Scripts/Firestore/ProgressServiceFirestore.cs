using System.Collections.Generic;
using System.Threading.Tasks;

public class ProgressServiceFirestore : IProgressServiceFirestore
{
	private readonly ChildProgressRepository repository;

	public ProgressServiceFirestore(ChildProgressRepository repository)
	{
		this.repository = repository;
	}

	public Task<Dictionary<string, object>> GetChildProgressAsync(string parentId, string childId)
	{
		return repository.TryGetChildProgressAsync(parentId, childId);
	}

	public Task<bool> UpdateChildProgressAsync(string parentId, string childId, Dictionary<string, object> updates)
	{
		return repository.TryUpdateChildProgressAsync(parentId, childId, updates);
	}
}




