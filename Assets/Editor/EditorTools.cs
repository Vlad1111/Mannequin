using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public class EditorTools : Editor
{
    [MenuItem("Assets/Utilities/Remesh Skin")]
    static void RemeshSkin()
    {
        var allSkinnedRenderers = Selection.activeGameObject.transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach(var renderer in allSkinnedRenderers)
        {
            renderer.ResetBounds();
            populateBoneArray(renderer);
        }

        //
        //Selection.activeGameObject.transform.GetComponent<SkinnedMeshRenderer>() = BuildBonesArray(Selection.activeGameObject.transform.GetComponent<SkinnedMeshRenderer>().rootBone, Selection.activeGameObject.transform.GetComponent<SkinnedMeshRenderer>().bones);
    }
    public static void populateBoneArray(SkinnedMeshRenderer skinnedMesh)
    {
        if (skinnedMesh.rootBone == null)
        {
            return;
            //throw new System.Exception(
            //    "Missing root bone; please ensure that the root bone is set before attempting"
            //    + " to populate the bone array for the skinned mesh."
            //);
        }

        var boneArray = new List<Transform>();
        var currentBone = skinnedMesh.rootBone;
        recursiveAdd(currentBone, ref boneArray);

        skinnedMesh.bones = boneArray.ToArray();
    }

    private static void recursiveAdd(Transform currentBone, ref List<Transform> boneArray)
    {
        boneArray.Add(currentBone);
        for (var i = 0; i < currentBone.childCount; i++)
        {
            recursiveAdd(currentBone.GetChild(i), ref boneArray);
        }
    }

}





