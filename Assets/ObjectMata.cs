public class ObjectMata
{
    public string objectTag;
    public string objectName;
    public float minValue;
    public float maxValue;
    public float value;

    // Constructor
    public ObjectMata(string tag, string name, float minVal, float maxVal, float currentValue)
    {
        objectTag = tag;
        objectName = name;
        minValue = minVal;
        maxValue = maxVal;
        value = currentValue;
    }

    // ObjectMata değerini güncelle
    public void Update(float newValue, ObjectData dataComponent)
    {
        value = newValue;
        minValue = dataComponent.minValue;
        maxValue = dataComponent.maxValue;
    }
}
