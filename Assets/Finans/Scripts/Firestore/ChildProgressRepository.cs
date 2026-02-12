using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ChildProgressRepository
{
	private readonly FirestoreDataOperationManager firestore;

	public ChildProgressRepository(FirestoreDataOperationManager firestore)
	{
		this.firestore = firestore;
	}

	public async Task<Dictionary<string, object>> TryGetChildProgressAsync(string parentId, string childId)
	{
		try
		{
			return await firestore.GetFirestoreDocument(IFirestoreEnums.FSCollection.parent.ToString(), parentId, IFirestoreEnums.FSCollection.children.ToString(), childId);
		}
		catch (Exception ex)
		{
			Logger.LogError("Failed to get child progress", nameof(ChildProgressRepository), ex);
			return new Dictionary<string, object>();
		}
	}

	public async Task<bool> TryUpdateChildProgressAsync(string parentId, string childId, Dictionary<string, object> updates)
	{
		try
		{
			return await firestore.FirestoreDataSave(IFirestoreEnums.FSCollection.parent.ToString(), parentId, IFirestoreEnums.FSCollection.children.ToString(), childId, updates);
		}
		catch (Exception ex)
		{
			Logger.LogError("Failed to update child progress", nameof(ChildProgressRepository), ex);
			return false;
		}
	}
}








