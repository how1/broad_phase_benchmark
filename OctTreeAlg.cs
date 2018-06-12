//Used to initialize and control Oct-Tree

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctTreeAlg : MonoBehaviour {
	public bool build;
	public float minSize = 1f;
	static public int bounds;
	public bool draw = false;
	BoundingBox uBounds;
	public OctTree octTree;
	public NarrowPhase narrowPhase;
	List<HRigidBody> tmp;

	public void StartOctTree(float avgRadius){
		minSize = avgRadius * 2;
		bounds = narrowPhase.bounds;
		tmp = new List<HRigidBody> ();
		for (int i = 0; i < narrowPhase.physicsEngines.Length; i++) {
		tmp.Add (narrowPhase.physicsEngines [i]);
		}
		uBounds = new BoundingBox(new Vector3(0,0,0), new Vector3(bounds, bounds,bounds));
		octTree = new OctTree(uBounds, tmp, narrowPhase, minSize);
		octTree.name = "root";
		octTree.isRoot = true;
		octTree.BuildTree();
		octTree.GetCollisions ();
	}

	public void RestartOctTree(){
		//count = 0;
		if (build) {
			octTree = new OctTree (uBounds, tmp, narrowPhase, minSize);
			octTree.name = "root";
			octTree.BuildTree ();
			//octTree.toString ("");
			octTree.GetCollisions ();
		} else {
			//octTree.toString("");
			octTree.UpdateTree ();
			//octTree.toString("");
			octTree.GetCollisions ();
		}
	}
}
