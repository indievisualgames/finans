using System.Collections.Generic;
using System.Threading.Tasks;

public interface IProgressServiceFirestore
{
	Task<Dictionary<string, object>> GetChildProgressAsync(string parentId, string childId);
	Task<bool> UpdateChildProgressAsync(string parentId, string childId, Dictionary<string, object> updates);
}




