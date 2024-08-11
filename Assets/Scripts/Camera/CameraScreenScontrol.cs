using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScreenScontrol : MonoBehaviour
{
    public Camera mainCamera;
    public MeshRenderer pivoteRenderer;
    public float movementSpeed = 1;
    public float rotationSpeed = 1;

    private float smallerScrrenEdge = 10;

    private Touch[] lastTouches = new Touch[0];
    private bool wasPressed = false;
    private bool blockMovement = false;
    // Start is called before the first frame update
    void Start()
    {
        smallerScrrenEdge = Mathf.Min(Screen.width, Screen.height);
    }

    private int pressedOnUIFirst = 0;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Fire1") > 0 && ((!EventSystem.current.IsPointerOverUIObject() && pressedOnUIFirst == 0) || pressedOnUIFirst > 0))
        {
            pressedOnUIFirst = 1;
            var curentTouches = Input.touches;
            if (curentTouches.Length == lastTouches.Length && curentTouches.Length > 0)
            {
                if (curentTouches.Length == 1)
                {
                    if (wasPressed == false)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(lastTouches[0].position);
                        if (CharacterCreation.IsRayOverTransformHandle(ray))
                            blockMovement = true;
                    }
                    if (!blockMovement)
                    {
                        var lp = lastTouches[0].position / smallerScrrenEdge;
                        var cp = curentTouches[0].position / smallerScrrenEdge;
                        var delta = lp - cp;

                        var rotation = transform.localEulerAngles;
                        rotation += new Vector3(delta.y, -delta.x, 0) * Time.deltaTime * rotationSpeed;
                        transform.localEulerAngles = rotation;
                    }
                }
                else if (curentTouches.Length == 2)
                {
                    var lp1 = lastTouches[0].position;
                    var lp2 = lastTouches[1].position;
                    var cp1 = curentTouches[0].position;
                    var cp2 = curentTouches[1].position;

                    var delta1 = lastTouches[0].deltaPosition;
                    var delta2 = lastTouches[1].deltaPosition;
                    var d1 = delta1.magnitude;
                    var d2 = delta2.magnitude;
                    var lpd = (lp1 - lp2).magnitude;
                    var cpd = (cp1 - cp2).magnitude;

                    if(lpd > 1 & cpd > 1)
                    {
                        var m1 = Mathf.Atan2(delta1.x, delta1.y);
                        var m2 = Mathf.Atan2(delta2.x, delta2.y);


                        var deltam = ((m1 - m2) + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;

                        //Debug.Log(deltam);

                        //if (Mathf.Abs(deltam) < Mathf.PI / 5)
                        {
                            var dx = (delta1.x + delta2.x) / 2;
                            var dy = (delta1.y + delta2.y) / 2;
                            var move = transform.up * dy + transform.right * dx;
                            transform.localPosition -= move * Time.deltaTime * movementSpeed;
                        }
                        //else
                        {
                            var d = lpd / cpd;
                            transform.localScale *= d;
                        }
                    }
                }
                wasPressed = true;
            }
            else
            {
                wasPressed = false;
                blockMovement = false;
            }
            lastTouches = curentTouches;
            pivoteRenderer.enabled = true;
        }
        else
        {
            if (Input.GetAxis("Fire1") > 0)
            {
                if (EventSystem.current.IsPointerOverUIObject() && pressedOnUIFirst == 0)
                    pressedOnUIFirst = -1;
            }
            else
            {
                pressedOnUIFirst = 0;
            }
            lastTouches = new Touch[0];
            wasPressed = false;
            blockMovement = false;
            pivoteRenderer.enabled = false;
        }
    }
}
