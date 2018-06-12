//Stores information about a collision between two objects

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionObject{
	public HRigidBody body1;
	public HRigidBody body2;
	public Plane plane;
	public Vector3 relativeVelocity;
	public Vector3 collisionNormal;
	public Vector3 oldPos1;
	public Vector3 oldPos2;

	public CollisionObject(HRigidBody ba, HRigidBody bb){
		body1 = ba;
		body2 = bb;
	}
	public CollisionObject(HRigidBody b1, Plane p){
		body1 = b1;
		plane = p;
	}
	public void print(){
		Debug.Log (body1.name.Substring(6) + " " + body2.name.Substring(6));
	}
}
