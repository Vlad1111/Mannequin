using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyParentRestriction : MonoBehaviour
{
    public Transform target;

    [Space(20)]
    public bool position;
    public bool rotation;
    public bool scale;

    [Space(20)]
    public Transform lookAtThis;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    public Vector3 lookAtRotationOffset;

    private void Awake()
    {
        lastPosition = transform.localPosition;
        lastRotation = transform.localRotation;
    }

    public void UpdateOnce()
    {
        LateUpdate();
    }

    void LateUpdate()
    {
        if (lookAtThis != null)
        {
            if((lastPosition.x != transform.localPosition.x ||
                lastPosition.y != transform.localPosition.y ||
                lastPosition.z != transform.localPosition.z) &&
                (transform.position-lookAtThis.position).magnitude > 0.05f)
            {
                //var axes = lookAtAxes.ToVector3(transform.parent);
                //Debug.Log(axes);
                transform.LookAt(lookAtThis);
                //transform.localEulerAngles -= lookAtRotationOffset;
                transform.Rotate(lookAtRotationOffset, Space.Self);

                lastPosition = transform.localPosition;
            }
            else
            {
                lookAtRotationOffset += (transform.localRotation * Quaternion.Inverse(lastRotation)).eulerAngles;
            }
            lastRotation = transform.localRotation;
        }
            
        if (target != null)
        {
            if (position)
                target.localPosition = transform.localPosition;
            if (rotation)
                target.localRotation = transform.localRotation;
            if (scale)
                target.localScale = transform.localScale;
        }
    }
}
