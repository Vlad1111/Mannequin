using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class MyMeshData
{
    public MyVector3[] vertices;
    public int[] triangles;
    public MyVector2[] uv;
    public MyVector3[] normals;
    public MyVector4[] tangents;
    //public 

    public static implicit operator MyMeshData(Mesh m) =>
                            new MyMeshData()
                            {
                                vertices = m.vertices.Select(x => (MyVector3)x).ToArray(),
                                uv = m.uv.Select(x => (MyVector2)x).ToArray(),
                                triangles = m.triangles,
                                normals = m.normals.Select(x => (MyVector3)x).ToArray(),
                                tangents = m.tangents.Select(x => (MyVector4)x).ToArray(),
                            };

    public static implicit operator Mesh(MyMeshData m) =>
                            new Mesh()
                            {
                                vertices = m?.vertices.Select(x => (Vector3)x).ToArray(),
                                uv = m?.uv.Select(x => (Vector2)x).ToArray(),
                                triangles = m?.triangles,
                                normals = m?.normals.Select(x => (Vector3)x).ToArray(),
                                tangents = m?.tangents.Select(x => (Vector4)x).ToArray(),
                            };
}
