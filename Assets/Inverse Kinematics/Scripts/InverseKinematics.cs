using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]

public class InverseKinematics : MonoBehaviour {

	public Transform upperArm;
	public Transform forearm;
	public Transform hand;
	public Transform elbow;
	public Transform target;
	[Space(20)]
	public Vector3 uppperArm_OffsetRotation;
	public Vector3 forearm_OffsetRotation;
	public Vector3 hand_OffsetRotation;
	[Space(20)]
	public bool handMatchesTargetRotation = true;
	[Space(20)]
	public Transform upperArmPivot;
	public AnimationCurve upperArmPivotInfulence;
	//public AnimationCurve upperArmPivotInfluenceX;
	//public AnimationCurve upperArmPivotInfluenceY;
	//public AnimationCurve upperArmPivotInfluenceZ;
	private bool hadOriginalRotationSet = false;
	private Quaternion originalUpperArmRotation;
	private Vector3 originalUpperArmRotationEuler;
	private Quaternion originalUpperArmPivotRotation;
	private Vector3 originalUpperArmPivotRotationEuler;
	private Vector3 originalUpperArmFront;
	private Vector3 originalUpperArmDirection;
	private Vector3 originalUpperArmRight;
	private float lastDeltaUpperArm;
	public Transform forearmPivot;
	public AnimationCurve forearmPivotInfulence;
	public float forearmPivotInfulenceX;
	public float forearmPivotInfulenceY;
	public float forearmPivotInfulenceZ;
	private Vector3 originalForearmPivotRotationEuler;
	private Quaternion originalHandRotation;
	[Space(20)]
	public bool debug;

	float angle;
	float upperArm_Length;
	float forearm_Length;
	float arm_Length;
	float targetDistance;
	float adyacent;

	private void SetOriginalRotations()
	{
		if (!hadOriginalRotationSet)
		{
			if (upperArmPivot != null)
			{
				originalUpperArmRotation = upperArm.localRotation;
				originalUpperArmRotationEuler = upperArm.localEulerAngles;

				originalUpperArmFront = upperArm.forward;
				originalUpperArmDirection = upperArm.up;
				originalUpperArmRight = upperArm.right;

				originalUpperArmPivotRotation = upperArmPivot.localRotation;
				originalUpperArmPivotRotationEuler = upperArmPivot.localEulerAngles;
			}

			if (forearmPivot != null)
			{
				originalForearmPivotRotationEuler = forearmPivot.localEulerAngles;

				originalHandRotation = hand.localRotation;
			}
			hadOriginalRotationSet = true;
		}
	}

	void Start()
	{
		SetOriginalRotations();
	}

	public void UpdateOnce()
	{
		SetOriginalRotations();
		LateUpdate();
	}

	private float GetAngle(Vector3 a, Vector3 b)
    {
		var aux = (a.x * a.x + a.y * a.y + a.z * a.z) *
				(b.x * b.x + b.y * b.y + b.z * b.z);
		if (aux == 0)
			aux = 0.000001f;

		aux = (a.x * b.x + a.y * b.y + a.z * b.z) / Mathf.Sqrt(aux);

		if (aux < -1 || aux > 1)
			return 0;

		return Mathf.Acos(aux) * Mathf.Rad2Deg;
    }
	
	private void UpdatePivotRotation()
	{
		SetOriginalRotations();
		if (upperArmPivot != null)
		{
			//originalUpperArmPivotRotationEuler = upperArmPivot.localEulerAngles - 
			//	new Vector3(0, lastDeltaUpperArm * upperArmPivotInfulence, 0);
			//originalUpperArmPivotRotation = upperArmPivot.localRotation * 
			//	Quaternion.Inverse(Quaternion.Euler(0, lastDeltaUpperArm * upperArmPivotInfulence, 0));

			var delta = (upperArm.localRotation * Quaternion.Inverse(originalUpperArmRotation)).eulerAngles.y;
			//var delta = (upperArm.localEulerAngles - originalUpperArmRotationEuler).y;

			//var dirDelta = GetAngle(originalUpperArmDirection, upperArm.up);
			//var forDelta = GetAngle(originalUpperArmFront, upperArm.forward);
			//var rigDelta = GetAngle(originalUpperArmRight, upperArm.right);
			//var delta = upperArmPivotInfluenceX.Evaluate(dirDelta) * dirDelta +
			//			upperArmPivotInfluenceY.Evaluate(forDelta) * forDelta +
			//			upperArmPivotInfluenceZ.Evaluate(rigDelta) * rigDelta;

			//delta = (delta + 360) % 360;
			//if (delta > 180) delta = 360 - delta;
			if (delta > 180)
				delta -= 360;
			
			//Debug.Log(upperArm.up + " " + upperArm.forward);
			//Debug.Log(dirDelta + "\t" + forDelta);
			//Debug.Log(delta);

			//Debug.Log(upperArm.localEulerAngles + " " + originalUpperArmRotationEuler);
			upperArmPivot.localRotation = originalUpperArmPivotRotation * Quaternion.Euler(0, upperArmPivotInfulence.Evaluate(delta), 0);
			//upperArmPivot.localEulerAngles = originalUpperArmPivotRotationEuler + new Vector3(0, delta * upperArmPivotInfulence, 0);

			//lastDeltaUpperArm = delta;
			//Debug.Log(upperArmPivot.name + " delta: " + delta);
		}

		if(forearmPivot != null)
		{
			var vector = (hand.localRotation * Quaternion.Inverse(originalHandRotation)).eulerAngles;
			//var vector = hand.localEulerAngles - originalHandRotationEuler;

			var delta = vector.x * forearmPivotInfulenceX + 
						vector.y * forearmPivotInfulenceY +
						vector.z * forearmPivotInfulenceZ;

			if (delta > 180)
				delta -= 360;
			if (delta < -180)
				delta += 360;
			//Debug.Log(delta);

			//forearmPivot.localRotation = originalForearmPivotRotation * Quaternion.Euler(0, forearmPivotInfulence.Evaluate(delta), 0);
			forearmPivot.localEulerAngles = originalForearmPivotRotationEuler + new Vector3(0, forearmPivotInfulence.Evaluate(delta), 0);
		}
	}

	private void UpdateArm()
	{
		upperArm.LookAt(target, elbow.position - upperArm.position);
		upperArm.Rotate(uppperArm_OffsetRotation);

		Vector3 cross = Vector3.Cross(elbow.position - upperArm.position, forearm.position - upperArm.position);



		upperArm_Length = Vector3.Distance(upperArm.position, forearm.position);
		forearm_Length = Vector3.Distance(forearm.position, hand.position);
		arm_Length = upperArm_Length + forearm_Length;
		targetDistance = Vector3.Distance(upperArm.position, target.position);
		targetDistance = Mathf.Min(targetDistance, arm_Length - arm_Length * 0.001f);

		adyacent = ((upperArm_Length * upperArm_Length) - (forearm_Length * forearm_Length) + (targetDistance * targetDistance)) / (2 * targetDistance);

		angle = Mathf.Acos(adyacent / upperArm_Length) * Mathf.Rad2Deg;

		upperArm.RotateAround(upperArm.position, cross, -angle);

		forearm.LookAt(target, cross);
		forearm.Rotate(forearm_OffsetRotation);

		if (handMatchesTargetRotation)
		{
			hand.rotation = target.rotation;
			hand.Rotate(hand_OffsetRotation);
		}
	}

	void LateUpdate () {
		if(upperArm != null && forearm != null && hand != null && elbow != null && target != null){
			UpdateArm();
			UpdatePivotRotation();

			if (debug)
			{
				if (forearm != null && elbow != null)
				{
					Debug.DrawLine(forearm.position, elbow.position, Color.blue);
				}

				if (upperArm != null && target != null)
				{
					Debug.DrawLine(upperArm.position, target.position, Color.red);
				}
			}
		}
	}

	void OnDrawGizmos(){
		if (debug) {
			if(upperArm != null && elbow != null && hand != null && target != null && elbow != null){
				Gizmos.color = Color.gray;
				Gizmos.DrawLine (upperArm.position, forearm.position);
				Gizmos.DrawLine (forearm.position, hand.position);
				Gizmos.color = Color.red;
				Gizmos.DrawLine (upperArm.position, target.position);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine (forearm.position, elbow.position);
			}
		}
	}

}
