using System.Collections.Generic;
using System.Threading.Tasks;

public interface IFirestoreOperator
{
    Task<bool> FirestoreDataSave(string collection, string documnet, Dictionary<string, object> data);
    Task<bool> FirestoreDataSave(string collection, string documnet, string subcollection, string subdocument, Dictionary<string, object> dataParent, Dictionary<string, object> dataChild);

    Task<bool> FirestoreDataSave(string collection, string documnet, string subcollection, string subdocument, Dictionary<string, object> data);
    Task<bool> FirestoreDataDeleteWithMerge(string collection, string documnet, string subcollection, string subdocument, Dictionary<string, object> data);

    // Task<bool> FirestoreDataSaveAndSync(string collection, string documnet, string subcollection, string subdocument, Dictionary<string, object> data);
    //Task FirestoreUpdateData(string collection, string document, string subcollection, string subdocument, Dictionary<string, object> updatedata);
    Task<Dictionary<string, object>> GetFirestoreDataField(string collection, string document, string _data);


    Task<Dictionary<string, object>> GetFirestoreDataField(string collection, string documnet, string _subcollection, string subdocument, string _data);
    Task<List<string>> GetFirestoreDocument(string collection, string document, string subcollection);
    Task<Dictionary<string, object>> GetFirestoreDocument(string collection, string documnet, string _subcollection, string subdocument);
    //Task<Dictionary<string, object>> GetFirestoreDataTemp(string collection, string documnet, string _subcollection, string subdocument, string _data);
    void LoadPointsAndScore(Dictionary<string, object> _points_score_data);
    void LoadGamePointsScore(Dictionary<string, object> _gamepoints_score_data);
    Task<bool> ValidateTheFirstLogin(string id);
}