using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class n2Gravity : MonoBehaviour {

	NarrowPhase narrowPhase;
	private const float gravitationalConstant = 6.67408e-11f; // [m^3 kg^-1 s^-2]
	bool hasStarted = false;

	public void StartN2Gravity () {
		narrowPhase = FindObjectOfType<NarrowPhase> ();
		hasStarted = true;
	}
	
	void FixedUpdate () {
		if (hasStarted) {
			CalculateGravity ();
		}
	}

	public void CalculateGravity(){
		foreach (HRigidBody eng in narrowPhase.physicsEngines) {
			foreach (HRigidBody eng2 in narrowPhase.physicsEngines) {
				if (eng != eng2 && eng != this) {
					Vector3 gravitationalForceVector = eng.transform.position - eng2.transform.position;
					float rSquared = Mathf.Pow (Vector3.Distance (eng.transform.position, eng2.transform.position), 2);
					float gravitationalForce = gravitationalConstant * eng.mass * eng2.mass / rSquared;
					gravitationalForceVector = gravitationalForce * gravitationalForceVector.normalized;
					eng.AddForce (-gravitationalForceVector * eng.mass);
				}
			}
		}
	}
}
