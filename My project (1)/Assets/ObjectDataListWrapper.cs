using System;
using System.Collections.Generic;

[Serializable]  // Sadece bir kez kullanılması yeterlidir.
public class ObjectDataListWrapper
{
    public List<ObjectMata> objects;

    public ObjectDataListWrapper(List<ObjectMata> objects)
    {
        this.objects = objects;
    }
}

