using System.Collections.Generic;
using System.Diagnostics;
using static IFirestoreEnums;


public class FirestoreDatabase
{
    static Dictionary<string, object> firestore_parent_data = new Dictionary<string, object>();
    static Dictionary<string, object> firestore_child_data = new Dictionary<string, object>();
    static Dictionary<string, object> unit_stage_btn_staus = new Dictionary<string, object>();
    static Dictionary<string, object> unit_stage_data = new Dictionary<string, object>();
    static Dictionary<string, object> flashcard = new Dictionary<string, object>();
    static Dictionary<string, object> trivia = new Dictionary<string, object>();
    static Dictionary<string, object> profile = new Dictionary<string, object>();
    static Dictionary<string, object> points_score = new Dictionary<string, object>();
    static Dictionary<string, object> progress_data = new Dictionary<string, object>();

    public static Dictionary<string, object> ParentData
    {
        get { return firestore_parent_data; }
        set { firestore_parent_data = value; }
    }
    public static Dictionary<string, object> ChildData
    {
        get { return firestore_child_data; }
        set { firestore_child_data = value; }
    }
    public static Dictionary<string, object> UnitStageButtonStatus
    {
        get { return unit_stage_btn_staus; }
        set { unit_stage_btn_staus = value; }
    }

    public static Dictionary<string, object> UnitStageData
    {
        get { return unit_stage_data; }
        set { unit_stage_data = value; }
    }

    public static Dictionary<string, object> Flashcard
    {
        get { return flashcard; }
        set { flashcard = value; }
    }
    public static Dictionary<string, object> Trivia
    {
        get { return trivia; }
        set { trivia = value; }
    }
    public static Dictionary<string, object> Profile
    {
        get { return profile; }
        set { profile = value; }
    }
    public static Dictionary<string, object> PointScore
    {
        get { return points_score; }
        set { points_score = value; }
    }
    public static Dictionary<string, object> ProgressData
    {
        get { return progress_data; }
        set { progress_data = value; }
    }


    public static Dictionary<string, object> GetFirestoreChildFieldData(string _field)
    {
        Dictionary<string, object> returndata;

        try
        {
            if (firestore_child_data.ContainsKey(_field))
            {
                Logger.LogInfo($"-> Found data map field named {_field}", "FirestoreDatabase");
                returndata = (Dictionary<string, object>)firestore_child_data[_field];
            }
            else
            {
                Logger.LogWarning($"Field {_field} does not exist in firestore", "FirestoreDatabase");
                returndata = new Dictionary<string, object>();
            }
            return returndata;

        }
        catch (System.Exception ex)
        {

            throw ex;
        }
    }

    /// <summary>
    /// Try-get variant for child map fields. Returns false if the field is missing or not a dictionary.
    /// Never throws; logs a warning instead.
    /// </summary>
    public static bool TryGetFirestoreChildFieldData(string field, out Dictionary<string, object> result)
    {
        result = null;
        try
        {
            if (firestore_child_data != null &&
                firestore_child_data.TryGetValue(field, out var raw) &&
                raw is Dictionary<string, object> dict)
            {
                Logger.LogInfo($"-> Found data map field named {field}", "FirestoreDatabase");
                result = dict;
                return true;
            }

            Logger.LogWarning($"Field {field} does not exist in firestore child data", "FirestoreDatabase");
            result = null;
            return false;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"TryGetFirestoreChildFieldData failed for field {field}", "FirestoreDatabase", ex);
            result = null;
            return false;
        }
    }

    public static string GetChildTimeZoneOrDefault()
    {
        try
        {
            if (profile != null && profile.TryGetValue("timezone", out object tz) && tz is string tzid && !string.IsNullOrEmpty(tzid))
            {
                return tzid;
            }
        }
        catch { }
        return System.TimeZoneInfo.Local.Id;
    }
    public static Dictionary<string, object> GetFirestoreParentFieldData(string _field)
    {
        try
        {
            Dictionary<string, object> returndata = (Dictionary<string, object>)firestore_parent_data[_field];
            return returndata;
        }
        catch (System.Exception ex)
        {

            throw ex;
        }

    }

    /// <summary>
    /// Try-get variant for parent map fields. Returns false if the field is missing or not a dictionary.
    /// Never throws; logs a warning instead.
    /// </summary>
    public static bool TryGetFirestoreParentFieldData(string field, out Dictionary<string, object> result)
    {
        result = null;
        try
        {
            if (firestore_parent_data != null &&
                firestore_parent_data.TryGetValue(field, out var raw) &&
                raw is Dictionary<string, object> dict)
            {
                Logger.LogInfo($"-> Found parent data map field named {field}", "FirestoreDatabase");
                result = dict;
                return true;
            }

            Logger.LogWarning($"Field {field} does not exist in firestore parent data", "FirestoreDatabase");
            result = null;
            return false;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"TryGetFirestoreParentFieldData failed for field {field}", "FirestoreDatabase", ex);
            result = null;
            return false;
        }
    }
    public bool ValidateFirstLoginBy(string keyfield)
    {
        bool yes = true;
        /*  DocumentReference docRef = db.Collection("parent").Document(id);
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
          }*/

        try
        {
            if (firestore_parent_data.ContainsKey(keyfield))
            {
                yes = false;

            }
        }
        catch (System.Exception ex)
        {
            yes = true;
            Debug.WriteLine(ex.Message);
        }


        return yes;
    }
    public static Dictionary<string, object> GetFSMapData(string unit, string mapField)
    {
        try
        {
            Dictionary<string, object> unitdata = (Dictionary<string, object>)firestore_child_data[unit];
            Dictionary<string, object> returndata = (Dictionary<string, object>)unitdata[mapField];
            return returndata;
        }
        catch (System.Exception ex)
        {

            throw ex;
        }

    }

    /// <summary>
    /// Safe helper for nested child data: tries to resolve child[unitKey][mapField] as a dictionary.
    /// Returns false if any key is missing or the value type is unexpected; never throws.
    /// </summary>
    public static bool TryGetChildUnitMap(string unitKey, string mapField, out Dictionary<string, object> result)
    {
        result = null;
        try
        {
            if (firestore_child_data == null)
            {
                Logger.LogWarning("firestore_child_data is null in TryGetChildUnitMap", "FirestoreDatabase");
                return false;
            }

            if (!firestore_child_data.TryGetValue(FSMapField.unit_stage_data.ToString(), out var unitStageRaw) ||
                unitStageRaw is not Dictionary<string, object> unitStageDict)
            {
                Logger.LogWarning("unit_stage_data map not found in firestore_child_data", "FirestoreDatabase");
                return false;
            }

            if (!unitStageDict.TryGetValue(unitKey, out var unitRaw) ||
                unitRaw is not Dictionary<string, object> unitDict)
            {
                Logger.LogWarning($"Unit key {unitKey} not found in unit_stage_data", "FirestoreDatabase");
                return false;
            }

            if (!unitDict.TryGetValue(mapField, out var mapRaw) ||
                mapRaw is not Dictionary<string, object> mapDict)
            {
                Logger.LogWarning($"Map field {mapField} not found under unit {unitKey}", "FirestoreDatabase");
                return false;
            }

            result = mapDict;
            return true;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"TryGetChildUnitMap failed for unit={unitKey}, mapField={mapField}", "FirestoreDatabase", ex);
            result = null;
            return false;
        }
    }
    /*static Dictionary<string, object> stagedata;
    public static Dictionary<string, object> SetFSMapData
    {
        get { return stagedata; }
        set { stagedata = value; }
    }*/





}
