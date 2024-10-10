using System;
using UnityEngine;

[Serializable]
public class ObjectMata
{
    public string objectName;  // Nesnenin ismi
    public string objectTag;   // Nesnenin tag'ı
    public float minValue;     // Minimum değer
    public float maxValue;
    public object value;// Maksimum değer

    public ObjectMata(string name, string tag, float min, float max,float val)
    {
        objectName = name;
        objectTag = tag;
        minValue = min;
        maxValue = max;
        value = val;
    }
}

