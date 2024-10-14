using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using Newtonsoft.Json;



public class JsonHandler : MonoBehaviour
{
    private string filePath;

    private void Start()
    {
        // JSON dosyasının yolunu belirleyin
        filePath = Path.Combine(Application.persistentDataPath, "ObjectData.json");
    }

    public void SaveObjectDataToJson(List<ObjectMata> objectDataList)
    {
        // ObjectMata listesini JSON formatına dönüştür
        string json = JsonConvert.SerializeObject(objectDataList, Newtonsoft.Json.Formatting.Indented);

        // JSON'u belirtilen dosyaya yaz
        File.WriteAllText(filePath, json);

        Debug.Log("Data saved to JSON file: " + filePath);
    }

    public List<ObjectMata> LoadObjectDataFromJson()
    {
        // Eğer dosya varsa, JSON'dan veriyi yükle
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            List<ObjectMata> objectDataList = JsonConvert.DeserializeObject<List<ObjectMata>>(json);
            return objectDataList;
        }
        else
        {
            Debug.LogError("JSON file not found at: " + filePath);
            return null;
        }
    }
}

[System.Serializable]
    public class ObjectDataList
    {
        public List<ObjectMata> objectData;
    }
