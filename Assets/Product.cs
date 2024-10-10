using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Product
{
    // Start is called before the first frame update
    public string GameObjectName { get; set; }  // Unity'deki GameObject ismi
    public string ChannelName { get; set; }     // OPC UA'dan gelen Channel ismi
    public string TagName { get; set; }         // OPC UA'dan gelen Tag ismi
    public object TagValue { get; set; }        // OPC UA'dan gelen Tag değeri

    public Product(string gameObjectName, string channelName, string tagName, object tagValue)
    {
        GameObjectName = gameObjectName;
        ChannelName = channelName;
        TagName = tagName;
        TagValue = tagValue;
    }

    public void UpdateTagValue(object newValue)
    {
        TagValue = newValue;
    }
}
