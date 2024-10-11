using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonHandler : MonoBehaviour
{
    private string filePath;

    void Start()
    {
        filePath = Application.persistentDataPath + "/ObjectData.json";
    }

    public void SaveObjectDataToJson(List<ObjectMata> objectDataList)
    {
        string jsonData = JsonUtility.ToJson(new ObjectDataList { objectData = objectDataList }, true);
        File.WriteAllText(filePath, jsonData);
        Debug.Log($"Data saved to {filePath}");
    }
    public ObjectMata GetObjectDataFromJson(string objectName)
    {
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            ObjectDataList objectDataList = JsonUtility.FromJson<ObjectDataList>(jsonData);

            // Find the specific ObjectMata by object name
            ObjectMata objectData = objectDataList.objectData.Find(data => data.objectName == objectName);

            if (objectData != null)
            {
                Debug.Log($"Data found for object: {objectName}");
                return objectData;  // Return the found ObjectMata
            }
            else
            {
                Debug.LogWarning($"No data found for object: {objectName}");
                return null;  // Return null if not found
            }
        }
        else
        {
            Debug.LogWarning("No JSON file found.");
            return null;  // Return null if no file exists
        }
    }

    [System.Serializable]
    public class ObjectDataList
    {
        public List<ObjectMata> objectData;
    }
}