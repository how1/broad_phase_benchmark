//A simple bounding box used for oct-trees

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBox {

	//public 
	public Vector3 Min;
	public Vector3 Max;

	public BoundingBox(Vector3 min, Vector3 max){
		Max = max;
		Min = min;
	}

	public bool Contains (Transform t, float r){
		Vector3 tmp = t.position;
		if (tmp.y - r > Min.y &&
			tmp.x - r > Min.x &&
		    tmp.z - r > Min.z &&
		    tmp.y + r < Max.y &&
		    tmp.x + r < Max.x &&
		    tmp.z + r < Max.z) {
			return true;
		} else
			return false;
	}
}
