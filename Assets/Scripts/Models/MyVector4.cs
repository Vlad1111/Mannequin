using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class MyVector4
{
    public float x, y, z, w;

    public MyVector4(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public static implicit operator Vector4(MyVector4 v) => new Vector4(v.x, v.y, v.z, v.w);
    public static implicit operator MyVector4(Vector4 v) => new MyVector4(v.x, v.y, v.z, v.w);

    public override string ToString()
    {
        return "MyVector4(" + x + ", " + y + ", " + z + ", " + w + ")";
    }
}
