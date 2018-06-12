//HRigidBody stores physics information about each particle
//Thanks to
//https://www.udemy.com/gamephysics/learn/v4/overview
//for a tutorial

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HRigidBody : MonoBehaviour {
	public bool showTrails = true;
	[HideInInspector]
	public Vector3 olderPosition;
	[HideInInspector]
	public Vector3 oldPosition;
	public float radius;
	public List<CollisionObject> colRecord = new List<CollisionObject> ();
	[HideInInspector]
	public Vector3 directions;

	public float mass;				// [kg]
	//[HideInInspector]
	public Vector3 velocityVector;	// [m s^-1]
	public float oldVelMag;
	[HideInInspector]
	public Vector3 netForceVector;	// N [kg m s^-2]
	public bool isStatic = false;
	//public bool atRest = false;
	[Space(2)]
	const float rho = 1.225f;
	[Range(1,2)]
	public float velocityExponent;		// [none]	
	float projectedArea;
	public float drag = 1;
	[HideInInspector]
	public bool applyGravity = true;
	//public bool updated = false;
	public EndPoint[] mins;
	public EndPoint[] maxs;
	public int[] minIndex;
	public int[] maxIndex;
	public int index;
	[HideInInspector]
	public bool didMove;


	private List<Vector3> forceVectorList = new List<Vector3>();

	void Start () {
		//Physics
		oldPosition = transform.position;
		netForceVector = Vector3.zero;
		projectedArea = Mathf.PI * (radius * radius);
		//colRecord = new List<CollisionObject> ();
	}

	void FixedUpdate(){
		if (!isStatic) {
			updateDrag ();
		}
		//oldPosition = transform.position;
	}

	public void AddForce(Vector3 forceVector){
		forceVectorList.Add(forceVector);
	}

	public void SumForces(){
		netForceVector = Vector3.zero;
		foreach (Vector3 vec in forceVectorList){
			netForceVector += vec;
		}
		forceVectorList.Clear ();
	}

	public void UpdatePosition(){
		velocityVector += (netForceVector / mass * Time.deltaTime);
		transform.position += velocityVector * Time.deltaTime;
		SumForces ();
	}

	public void UpdatePosition(float dt){
		velocityVector += (netForceVector / mass * dt);
		transform.position += velocityVector * dt;
		SumForces ();
	}

	void updateDrag(){
		float speed = velocityVector.magnitude;
		float dragSize = CalculateDrag (speed);
		Vector3 dragVector = drag * dragSize * -velocityVector.normalized;
		AddForce (dragVector);
	}

	float CalculateDrag(float speed){
		//F_D =0.5 rho * v^2 * C_d * A
		float num = 0.5f * rho * Mathf.Pow(speed, velocityExponent) * projectedArea;
		return num;
	}
		
}

