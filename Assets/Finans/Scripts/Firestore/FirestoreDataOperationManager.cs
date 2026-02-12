using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Firebase.Firestore;
using Firebase.Extensions;
using static IFirestoreEnums;
using System.Data.Common;

public class FirestoreDataOperationManager : IFirestoreOperator
{
    private readonly FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
    private string context = "FirestoreDataOperationManager";
    /*Used for parent data*/
    public async Task<bool> FirestoreDataSave(string _collection, string _document, Dictionary<string, object> data)
    {

        DocumentReference docRef = db.Collection(_collection).Document(_document);
        Debug.Log($"Debug Log:: DocumentReference is {docRef.Path}");
        try
        {
            await docRef.SetAsync(data, SetOptions.MergeAll);
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }


    }
    /*Used for parent and child data together*/
    public async Task<bool> FirestoreDataSave(string _collection, string _document, string _subcollection, string _subdocument, Dictionary<string, object> dataParent, Dictionary<string, object> dataChild)
    {
        DocumentReference docRefParent = db.Collection(_collection).Document(_document);
        DocumentReference docRefChild = db
            .Collection(_collection).Document(_document)
            .Collection(_subcollection).Document(_subdocument);
        try
        {
            await docRefParent.SetAsync(dataParent, SetOptions.MergeAll);
            Debug.Log($"Debug log: ParentDatasave done ");
            await docRefChild.SetAsync(dataChild, SetOptions.MergeAll);
            Debug.Log($"Debug log: ChildDatasave done ");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log($"Debug log: Datasave error occured {ex.Message} ");
            return false;
        }
    }
    /*Used for child data*/
    public async Task<bool> FirestoreDataSave(string _collection, string _document, string _subcollection, string _subdocument, Dictionary<string, object> data)
    {
        DocumentReference docRef = db
            .Collection(_collection).Document(_document)
            .Collection(_subcollection).Document(_subdocument);
        try
        {
            Params.ChildDataloaded = false;
            await docRef.SetAsync(data, SetOptions.MergeAll);
            Logger.LogInfo($"Success..,Datasave done ", "FirestireDataOpertaionManager");
            return true;
        }
        catch (System.Exception ex)
        {
            Logger.LogError("Datasave error occured", "FirestoreDataOperationManager", ex);
            return false;
        }
    }
    public async Task<bool> FirestoreDataDeleteWithMerge(string _collection, string _document, string _subcollection, string _subdocument, Dictionary<string, object> data)
    {
        DocumentReference docRef = db
            .Collection(_collection).Document(_document)
            .Collection(_subcollection).Document(_subdocument);
        try
        {

            await docRef.SetAsync(data, SetOptions.MergeAll);
            Debug.Log($"Debug log: Field data delete done ");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log($"Debug log: Datasave error occured {ex.Message} ");
            return false;
        }
    }
    /* public async Task<bool> FirestoreDataSaveAndSync(string _collection, string _document, string _subcollection, string _subdocument, Dictionary<string, object> data)
     {
         DocumentReference docRef = db
            .Collection(_collection).Document(_document)
            .Collection(_subcollection).Document(_subdocument);
         try
         {
             await docRef.SetAsync(data, SetOptions.MergeAll);
             Debug.Log($"Debug log: ChildDatasave done ");
             return true;
         }
         catch (System.Exception ex)
         {
             Debug.Log($"Debug log: Datasave error occured {ex.Message} ");
             return false;
         }
     }*/
    /* public async Task FirestoreUpdateData(string _collection, string _document, string _subcollection, string _subdocument, Dictionary<string, object> _updatedata)
    {
        DocumentReference docRef = db
           .Collection(_collection).Document(_document)
           .Collection(_subcollection).Document(_subdocument);
        await docRef.SetAsync(_updatedata, SetOptions.MergeAll);

         docRef.UpdateAsync(_updatedata).ContinueWithOnMainThread(task =>
          {
              Debug.Log(
                      $"Updated the {_document} document in the {_collection} collection.");
          });
    }*/
    public async Task<Dictionary<string, object>> GetFirestoreDataField(string _collection, string _document, string _data)
    {
        DocumentReference docRef = db.Collection(_collection).Document(_document);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists)
        {
            Debug.Log($"Document not found: {_collection}/{_document}");
            return new Dictionary<string, object>();
        }
        Dictionary<string, object> user = snapshot.ToDictionary();
        if (!user.TryGetValue(_data, out object value) || value is not Dictionary<string, object> dict)
        {
            Debug.Log($"Field '{_data}' missing in document: {_collection}/{_document}");
            return new Dictionary<string, object>();
        }
        return dict;
    }
    public async Task<Dictionary<string, object>> GetFirestoreDataField(string _collection, string _document, string _subcollection, string _subdocument, string _data)
    {
        try
        {
            DocumentReference docRef = db
            .Collection(_collection).Document(_document)
            .Collection(_subcollection).Document(_subdocument);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists)
            {
                Debug.Log($"Document not found: {_collection}/{_document}/{_subcollection}/{_subdocument}");
                return new Dictionary<string, object>();
            }
            Dictionary<string, object> user = snapshot.ToDictionary();
            if (!user.TryGetValue(_data, out object value) || value is not Dictionary<string, object> returndata)
            {
                Debug.Log($"Field '{_data}' missing in subdocument: {_collection}/{_document}/{_subcollection}/{_subdocument}");
                return new Dictionary<string, object>();
            }
            return returndata;
        }
        catch (System.Exception ex)
        {
            Debug.Log($"Data field loading error occured {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    /*  public async Task<Dictionary<string, object>> GetFirestoreDataTemp(string _collection, string _document, string _subcollection, string _subdocument, string _data)
      {
          try
          {
              DocumentReference docRef = db
              .Collection(_collection).Document(_document)
              .Collection(_subcollection).Document(_subdocument);
              DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
              if (snapshot.Exists)
              {
                  Dictionary<string, object> user = snapshot.ToDictionary();
                  if (snapshot.ContainsField("unit01"))
                  {
                      Debug.Log("It contains field!");
                  }



                  Dictionary<string, object> returndata = (Dictionary<string, object>)user[_data];
                  Debug.Log(message: $"GetFirestoreData Point score data values are {returndata}");
                  return returndata;
              }
              else
              {
                  Debug.Log("Document {0} does not exist!");
                  return new Dictionary<string, object>();
              }
          }
          catch (System.Exception)
          {

              throw;
          }
      }

  */
    public async Task<Dictionary<string, object>> GetFirestoreDocument(string _collection, string _document, string _subcollection, string _subdocument)
    {
        try
        {
            DocumentReference docRef = db
            .Collection(_collection).Document(_document)
            .Collection(_subcollection).Document(_subdocument);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists)
            {
                Debug.Log($"Document not found: {_collection}/{_document}/{_subcollection}/{_subdocument}");
                Params.ChildDataloaded = false;
                return new Dictionary<string, object>();
            }
            Dictionary<string, object> returndata = snapshot.ToDictionary();
            Params.ChildDataloaded = true;
            return returndata;
        }
        catch (System.Exception ex)
        {
            Params.ChildDataloaded = false;
            Debug.Log($"Document loading error occured {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    public async Task<List<string>> GetFirestoreDocument(string _collection, string _document, string _subcollection)
    {
        List<string> kidsID = new List<string>();
        Query subcollectionQuery = db
             .Collection(_collection).Document(_document).Collection(_subcollection);

        QuerySnapshot subcollectionQuerySnapshot = await subcollectionQuery.GetSnapshotAsync();
        foreach (DocumentSnapshot documentSnapshot in subcollectionQuerySnapshot.Documents)
        {
            Debug.Log(System.String.Format("Document data for {0} document:", documentSnapshot.Id));

            kidsID.Add(documentSnapshot.Id);

        }

        return kidsID;
    }

    // Non-breaking safe helper variants (additive)
    public async Task<(bool Success, Dictionary<string, object> Document)> TryGetDocumentAsync(string collection, string document, string subcollection, string subdocument)
    {
        try
        {
            var doc = await GetFirestoreDocument(collection, document, subcollection, subdocument);
            return (true, doc);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"TryGetDocumentAsync failed: {ex.Message}");
            return (false, new Dictionary<string, object>());
        }
    }

    public async Task<bool> TryUpdateAsync(string collection, string document, string subcollection, string subdocument, Dictionary<string, object> updates)
    {
        try
        {
            return await FirestoreDataSave(collection, document, subcollection, subdocument, updates);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"TryUpdateAsync failed: {ex.Message}");
            return false;
        }
    }

    // Perform partial field deletion using FieldValue.Delete via UpdateAsync
    public async Task<bool> DeleteFieldsAsync(string collection, string document, string subcollection, string subdocument, IEnumerable<string> fieldPaths)
    {
        try
        {
            var docRef = db.Collection(collection).Document(document).Collection(subcollection).Document(subdocument);
            var updates = new Dictionary<string, object>();
            foreach (var path in fieldPaths)
            {
                updates[path] = FieldValue.Delete;
            }
            await docRef.UpdateAsync(updates);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log($"DeleteFieldsAsync failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ValidateTheFirstLogin(string id)
    {
        bool yes = true;
        DocumentReference docRef = db.Collection("parent").Document(id);
        Debug.Log($"Debug Log:: DocRef document id is {docRef.Id}");
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            Debug.Log($"Debug Log::snapshot Exists");
            if (snapshot.Id == id)
            {
                Dictionary<string, object> _data = snapshot.ToDictionary();
                if (_data.ContainsKey("profile"))
                {
                    yes = false;
                }

            }
        }

        return yes;
    }

    public async Task<Dictionary<string, object>> GetFirestoreParentDocument(string id)
    {
        DocumentReference docRef = db.Collection(FSCollection.parent.ToString()).Document(id);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists)
        {
            return new Dictionary<string, object>();
        }
        if (snapshot.Id != id)
        {
            return new Dictionary<string, object>();
        }
        Dictionary<string, object> parentData = snapshot.ToDictionary();
        return parentData;


    }

    public void LoadPointsAndScore(Dictionary<string, object> _points_score_data)

    {
        foreach (var keyItem in _points_score_data)
        {
            //Debug.Log(message: $"Point score data values from database in childdashboard for {keyItem.Key} is {keyItem.Value}");
            if (keyItem.Key == HUD.xp.ToString())
            {
                // PointSystem.XP = (int)keyItem.Value; 
                int.TryParse(keyItem.Value.ToString(), out int parsedValue);
                PointSystem.XP = parsedValue;
                Logger.LogInfo(message: $"XP Point is {PointSystem.XP}", "context");
            }

            if (keyItem.Key == HUD.visit.ToString())
            {
                //PointSystem.Visit = (int)keyItem.Value;
                int.TryParse(keyItem.Value.ToString(), out int parsedValue);
                PointSystem.Visit = parsedValue;
                Logger.LogInfo(message: $"Visit Point are {PointSystem.Visit}", context);
            }

            if (keyItem.Key == HUD.stars.ToString())
            {
                //PointSystem.Stars = (int)keyItem.Value; 
                int.TryParse(keyItem.Value.ToString(), out int parsedValue);
                PointSystem.Stars = parsedValue;
                Logger.LogInfo(message: $"Stars Point are {PointSystem.Stars}", context);

            }
            if (keyItem.Key == HUD.view.ToString())
            {
                //PointSystem.Stars = (int)keyItem.Value; 
                int.TryParse(keyItem.Value.ToString(), out int parsedValue);
                PointSystem.View = parsedValue;
                Logger.LogInfo(message: $"View Point are {PointSystem.View}", context);

            }

            if (keyItem.Key == HUD.coins.ToString())
            {
                //PointSystem.Coins = (int)keyItem.Value; 
                int.TryParse(keyItem.Value.ToString(), out int parsedValue);
                PointSystem.Coins = parsedValue;
                Logger.LogInfo(message: $"Coins Point are {PointSystem.Coins}", context);

            }
            if (keyItem.Key == MainGame.passes.ToString())
            {
                int.TryParse(keyItem.Value.ToString(), out int parsedValue);
                PointSystem.Passes = parsedValue;
                Logger.LogInfo(message: $"Total Passes are {PointSystem.Passes}", context);

            }
        }
    }

    public void LoadGamePointsScore(Dictionary<string, object> _gamepoints_score_data)

    {
        foreach (var keyItem in _gamepoints_score_data)
        {
            //Debug.Log(message: $"Point score data values from database in childdashboard for {keyItem.Key} is {keyItem.Value}");
            /*  if (keyItem.Key == GameScore.lesson_points.ToString())
              {
                  int.TryParse(keyItem.Value.ToString(), out int parsedValue);
                  PointSystem.LessonPoints = parsedValue;
                  //                Debug.Log(message: $"XP Point is {PointSystem.XP}");
              }
              if (keyItem.Key == GameScore.flash_points.ToString())
              {
                  int.TryParse(keyItem.Value.ToString(), out int parsedValue);
                  PointSystem.FlashPoints = parsedValue;
                  //                Debug.Log(message: $"XP Point is {PointSystem.XP}");
              }
              if (keyItem.Key == GameScore.video_points.ToString())
              {
                  int.TryParse(keyItem.Value.ToString(), out int parsedValue);
                  PointSystem.VideoPoints = parsedValue;
                  //                Debug.Log(message: $"XP Point is {PointSystem.XP}");
              }*/
            if (keyItem.Key == GameScore.life.ToString())
            {
                int.TryParse(keyItem.Value.ToString(), out int parsedValue);
                PointSystem.Life = parsedValue;
                //                Debug.Log(message: $"XP Point is {PointSystem.XP}");
            }
            if (keyItem.Key == GameScore.game_points.ToString())
            {
                int.TryParse(keyItem.Value.ToString(), out int parsedValue);
                PointSystem.GamePoints = parsedValue;
                //                Debug.Log(message: $"XP Point is {PointSystem.XP}");
            }
            if (keyItem.Key == GameScore.powerup_health.ToString())
            {
                int.TryParse(keyItem.Value.ToString(), out int parsedValue);
                PointSystem.PowerupHealth = parsedValue;
                //                Debug.Log(message: $"XP Point is {PointSystem.XP}");
            }

        }

    }


}