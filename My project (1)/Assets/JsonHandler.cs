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

    [System.Serializable]
    public class ObjectDataList
    {
        public List<ObjectMata> objectData;
    }
}