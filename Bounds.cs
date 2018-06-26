using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounds : MonoBehaviour {
	[HideInInspector]
	public NarrowPhase narrowPhase;
	public HRigidBody[] physicsEngines;
	public Transform[] objects;
	public Renderer[] rends;
	public Vector3[] directions;
	float xBound = 10;
	float yBound = 10;
	float zBound = 10;
	public float speed;



	// Use this for initialization
	public void StartBounds () {
		narrowPhase = GetComponent<NarrowPhase> ();
		physicsEngines = narrowPhase.physicsEngines;
		//directions = new Vector3[physicsEngines.Length];
		foreach (HRigidBody s in physicsEngines) {
			s.directions = new Vector3 (Random.Range (-4, 5), Random.Range (-4, 5), Random.Range (-4, 5));
		}
	}
}
