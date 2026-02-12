
using UnityEngine;
using static IFirestoreEnums;

public class RefreshData : MonoBehaviour
{
    private IFirestoreOperator FirestoreClient;
    void Start()
    {
        FirestoreClient = new FirestoreDataOperationManager();
    }

    public async void RefreshChildData()
    {
        FirestoreDatabase.ChildData = await FirestoreClient.GetFirestoreDocument(FSCollection.parent.ToString(), PlayerInfo.AuthenticatedID, FSCollection.children.ToString(), PlayerInfo.AuthenticatedChildID);

    }
}
