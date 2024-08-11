using System;
using UnityEngine;

[Serializable]
public class MyVector3
{
    public float x, y, z;

    public MyVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static implicit operator Vector3(MyVector3 v) => new Vector3(v.x, v.y, v.z);
    public static implicit operator MyVector3(Vector3 v) => new MyVector3(v.x, v.y, v.z);

    public override string ToString()
    {
        return "MyVector3(" + x + ", " + y + ", " + z + ")";
    }
}