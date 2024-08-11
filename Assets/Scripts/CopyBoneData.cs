using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[UnityEngine.ExecuteInEditMode]
public class CopyBoneData : MonoBehaviour
{
    public Transform original;
    public Transform target;
    public bool doIt = false;
    public bool createMissingBones = false;
    public bool onlyComponents = false;
    public bool copySkinnedRendererBonesOrder = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void CopySpecialComponents(GameObject _sourceGO, GameObject _targetGO)
    {
        foreach (var component in _sourceGO.GetComponents<Component>())
        {
            var componentType = component.GetType();
            if (componentType != typeof(Transform) &&
                componentType != typeof(MeshFilter) &&
                componentType != typeof(MeshRenderer)
                )
            {
                //Debug.Log(componentType);
                ////Debug.Log("Found a component of type " + component.GetType());
                //UnityEditorInternal.ComponentUtility.CopyComponent(component);
                //var alreadyComponent = _targetGO.GetComponent(componentType);
                //if(alreadyComponent != null)
                //    UnityEditorInternal.ComponentUtility.PasteComponentValues(alreadyComponent);
                //else
                //    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(_targetGO);
                ////Debug.Log("Copied " + component.GetType() + " from " + _sourceGO.name + " to " + _targetGO.name);
            }
        }
    }

    private void Copy(Transform original, Transform target)
    {
        if (original == null || target == null)
            return;

        if (onlyComponents)
        {
            CopySpecialComponents(original.gameObject, target.gameObject);
        }
        else
        {
            target.localPosition = original.localPosition;
            target.localRotation = original.localRotation;
            target.localScale = original.localScale;
        }

        foreach (Transform o_child in original)
        {
            var isFound = false;
            foreach (Transform t_child in target)
                if (o_child.name == t_child.name)
                {
                    isFound = true;
                    Copy(o_child, t_child);
                    break;
                }

            if (!isFound)
            {
                Debug.Log("Bone " + o_child.name + " not found");
                if(createMissingBones)
                {
                    var newBone = Instantiate(o_child, target);
                    newBone.name = o_child.name;
                    Copy(o_child, newBone);
                }
            }
        }
    }

    private static void RecursiveAddBoneChildren(Transform currentBone, ref List<Transform> boneArray)
    {
        boneArray.Add(currentBone);
        for (var i = 0; i < currentBone.childCount; i++)
        {
            RecursiveAddBoneChildren(currentBone.GetChild(i), ref boneArray);
        }
    }
    private void CopySkinMeshes(SkinnedMeshRenderer origin_rend, SkinnedMeshRenderer target_rend)
    {
        List<Transform> newBones = new List<Transform>();
        List<Transform> targetBones = new List<Transform>();
        RecursiveAddBoneChildren(target_rend.rootBone, ref targetBones);

        foreach (var orig_bone in origin_rend.bones)
            foreach (var targ_bone in targetBones)
                if (orig_bone.name == targ_bone.name)
                    newBones.Add(targ_bone);

        target_rend.sharedMesh = origin_rend.sharedMesh;
        //targetBones.Sort((a, b) => 1 - 2 * Random.Range(0, 1));
        target_rend.bones = newBones.ToArray();
        target_rend.bounds = target_rend.bounds;
    }
    private void CopyBones(Transform original, Transform target)
    {
        var origin_rends = original.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        var target_rends = target.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        foreach(var orig_rend in origin_rends)
            foreach(var targ_rend in target_rends)
                if(orig_rend.sharedMesh == targ_rend.sharedMesh)
                    CopySkinMeshes(orig_rend, targ_rend);
    }
    // Update is called once per frame
    void Update()
    {
        if (doIt)
        {
            Copy(original, target);
            doIt = false;
        }
        if (onlyComponents)
        {
            Copy(original, target);
            onlyComponents = false;
        }
        if (copySkinnedRendererBonesOrder)
        {
            CopyBones(original, target);
            copySkinnedRendererBonesOrder = false;
        }
    }
}
