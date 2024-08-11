using System;
using UnityEngine;

[Serializable]
public class MyVector2
{
    public float x, y;

    public MyVector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public static implicit operator Vector2(MyVector2 v) => new Vector3(v.x, v.y);
    public static implicit operator MyVector2(Vector2 v) => new MyVector2(v.x, v.y);

    public override string ToString()
    {
        return "MyVector2(" + x + ", " + y + ")";
    }
}