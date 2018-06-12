//Used for Sweep and Prune

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint { // : IComparer<EndPoint> {

	public HRigidBody body;
	public float value;
	public bool isMin;
	public bool even = false;

	public EndPoint(HRigidBody i, float v, bool m){
		body = i;
		value = v;
		isMin = m;
	}

	public int Compare(EndPoint a, EndPoint b){
		return a.value.CompareTo (b.value);
	}

}
