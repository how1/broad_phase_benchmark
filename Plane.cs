using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane{
	public string name;
	public Vector3 vec1;
	public Vector3 vec2;
	public Vector3 pop; //point on plane

	public Plane(string n, Vector3 v1, Vector3 v2, Vector3 p){
		name = n;
		vec1 = v1;
		vec2 = v2;
		pop = p;
	}
}