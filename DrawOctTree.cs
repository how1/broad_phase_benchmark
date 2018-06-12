//Draws Oct-Tree partitions for debugging

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawOctTree : MonoBehaviour {

	public static DrawOctTree drawOctTree;
	int bounds = 128;
	BoundingBox region;

	public List<BoundingBox> list = new List<BoundingBox> ();

	void Awake(){
		DontDestroyOnLoad (gameObject);
		drawOctTree = this;
	}

	void Start(){
		region = new BoundingBox(new Vector3(bounds, bounds,bounds), new Vector3(0,0,0));
		Vector3 dimensions = region.Max - region.Min; 
		/*
		octant.Add( new BoundingBox(region.Min, center));
		octant.Add( new BoundingBox(new Vector3(center.x, region.Min.y, region.Min.z), new Vector3(region.Max.x, center.y, center.z)));
		octant.Add( new BoundingBox(new Vector3(center.x, region.Min.y, center.z), new Vector3(region.Max.x, center.y, region.Max.z)));
		octant.Add( new BoundingBox(new Vector3(region.Min.x, region.Min.y, center.z), new Vector3(center.x, center.y, region.Max.z)));
		octant.Add( new BoundingBox(new Vector3(region.Min.x, center.y, region.Min.z), new Vector3(center.x, region.Max.y, center.z)));
		octant.Add( new BoundingBox(new Vector3(center.x, center.y, region.Min.z), new Vector3(region.Max.x, region.Max.y, center.z )));
		octant.Add( new BoundingBox(center, region.Max));
		octant.Add( new BoundingBox(new Vector3(region.Min.x, center.y, center.z), new Vector3(center.x, region.Max.y, region.Max.z)));
		*/
	}

	void OnDrawGizmos () {
		if (!Application.isPlaying) return;
		Gizmos.color = Color.red;
		foreach (BoundingBox box in list) {
			Vector3 dimensions = box.Max - box.Min;
			Vector3 center = dimensions / 2.0f;
			Gizmos.DrawWireCube (box.Min + center, dimensions);
		}
	}
}
