//Thanks to
//https://www.gamedev.net/articles/programming/general-and-gameplay-programming/introduction-to-octrees-r3529/
//This code is heavily based on that tutorial

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctTree {
	#region variables 
	bool drawOcts = false;
	float minSize = 2f;
	public string name;
	public BoundingBox region;
	public List<HRigidBody> objects;
	public OctTree[] children;
	public OctTree parent;
	public bool isRoot = false;
	byte activeChildren = 0;
	public bool isActive = false;
	NarrowPhase narrowPhase;
	int maxLifeSpan = 8;
	int curLifeSpan = -1;
	bool isBuilt = false;
	bool hasChildren = false;
	#endregion

	public OctTree(BoundingBox r, List<HRigidBody> o, NarrowPhase n, float min){
		region = r;
		objects = o;
		narrowPhase = n;
		minSize = min;
	}

	public void BuildTree(){
		if (objects.Count < 2 || (region.Max.y - region.Min.y) / 2 < minSize){
			isBuilt = true;
			return;
		}
		children = new OctTree[8];
		Vector3 dimensions = region.Max - region.Min;
		Vector3 center = region.Min + (dimensions / 2.0f);
		BoundingBox[] octant = new BoundingBox[8];
		octant[0] = new BoundingBox(region.Min, center);
		octant[1] = new BoundingBox(new Vector3(center.x, region.Min.y, region.Min.z), new Vector3(region.Max.x, center.y, center.z));
		octant[2] = new BoundingBox(new Vector3(center.x, region.Min.y, center.z), new Vector3(region.Max.x, center.y, region.Max.z));
		octant[3] = new BoundingBox(new Vector3(region.Min.x, region.Min.y, center.z), new Vector3(center.x, center.y, region.Max.z));
		octant[4] = new BoundingBox(new Vector3(region.Min.x, center.y, region.Min.z), new Vector3(center.x, region.Max.y, center.z));
		octant[5] = new BoundingBox(new Vector3(center.x, center.y, region.Min.z), new Vector3(region.Max.x, region.Max.y, center.z ));
		octant[6] = new BoundingBox(center, region.Max);
		octant[7] = new BoundingBox(new Vector3(region.Min.x, center.y, center.z), new Vector3(center.x, region.Max.y, region.Max.z));

		if (drawOcts) {
			DrawOctTree.drawOctTree.list.Add (octant [0]);
			DrawOctTree.drawOctTree.list.Add (octant [1]);
			DrawOctTree.drawOctTree.list.Add (octant [2]);
			DrawOctTree.drawOctTree.list.Add (octant [3]);
			DrawOctTree.drawOctTree.list.Add (octant [4]);
			DrawOctTree.drawOctTree.list.Add (octant [5]);
			DrawOctTree.drawOctTree.list.Add (octant [6]);
			DrawOctTree.drawOctTree.list.Add (octant [7]);
		}
		//Build children
		//List<HRigidBody>[] toChildren = new List<HRigidBody>[8];
		List<HRigidBody>[] toChildren = new List<HRigidBody>[8];
		for(int i = 0; i < 8; i++){
			toChildren[i] = new List<HRigidBody>();
			//toChildren[i] = new List<HRigidBody>();
		}
		//List<HRigidBody> objectsToStay = new List<HRigidBody> ();
		List<HRigidBody> objectsToStay = new List<HRigidBody>();
		bool addedToChild = false;
		for(int i = 0; i < objects.Count;i++){
			for (int j = 0; j < 8; j++){
				if (octant[j].Contains(objects[i].transform, objects[i].radius)){
					toChildren [j].Add (objects [i]);
					addedToChild = true;
					break;
				} 
			}
			if (!addedToChild) {
				objectsToStay.Add (objects [i]);
			}
			addedToChild = false;
		}
		objects = objectsToStay;
		for (int i = 0 ; i < 8; i++){
			if (toChildren [i] != null) {
				//if (toChildren [i].Count > 0) {
				if (toChildren [i].Count > 0) {
					children [i] = new OctTree (octant [i], toChildren [i], narrowPhase, minSize);
					hasChildren = true;
					children [i].parent = this;
					children [i].name = name + i;
					children [i].isActive = true;
					activeChildren |= (byte)(1 << i);
					children [i].BuildTree ();
				}
			} 
		}
		isBuilt = true;
	}
		
	//Get collisions for root node
	public void GetCollisions(){
		for (int i = 0; i < objects.Count; i++) {
			for (int k = 0; k < narrowPhase.planeIndices.Length; k++) {
				narrowPhase.addPlaneCollision (objects [i], k);
			}
		}
		for (int i = objects.Count - 1; i >= 0; i--) {
			for (int j = 0; j < i; j++) {
				if (objects [i] != objects [j] && !(objects [i].isStatic && objects [j].isStatic)) {
					narrowPhase.addParticleCollision (objects [i], objects [j]);
				}
			}
		}

		if (activeChildren < 1)
			return;
		/*
		if (activeChildren == 0) {
			return;
		}
		*/

		for (int flags = activeChildren, index = 0; flags > 0; flags >>= 1, index++)
			if ((flags & 1) == 1) children[index].GetCollisions(objects);
	}

	//Get collisions for non-root node
	public void GetCollisions(List<HRigidBody> objs){
		for (int i = 0; i < objects.Count; i++) {
			for (int k = 0; k < narrowPhase.planeIndices.Length; k++) {
				narrowPhase.addPlaneCollision (objects [i], k);
			}
		}
		for (int i = objects.Count - 1; i >= 0; i--) {
			for (int j = 0; j < i; j++) {
				if (objects [i] != objects [j] && !(objects [i].isStatic && objects [j].isStatic)) {
					narrowPhase.addParticleCollision (objects [i], objects [j]);
				}
			}
		}

		for (int i = 0; i < objects.Count; i++) {
			for (int j = 0; j < objs.Count; j++) {
				if (objects [i] != objs [j]) {
					if (!(objects [i].isStatic && objs [j].isStatic)) {
						narrowPhase.addParticleCollision (objects [i], objs [j]);
					}
				}
			}
		}
		if (activeChildren < 1)
			return;
		List<HRigidBody> tmp = new List<HRigidBody>();
		tmp.AddRange (objs);
		tmp.AddRange (objects);
		List<HRigidBody> combinedObjs = tmp;
		for (int flags = activeChildren, index = 0; flags > 0; flags >>= 1, index++)
			if ((flags & 1) == 1) children[index].GetCollisions(combinedObjs);

	}
	public void UpdateTree(){
		if (isBuilt) {
			if (objects.Count == 0) {
				if (!hasChildren) {
					if (curLifeSpan == -1) {
						curLifeSpan = maxLifeSpan;
					} else if (curLifeSpan > 0)
						curLifeSpan--;
				}
			} else {
				if (curLifeSpan != -1) {
					if (maxLifeSpan <= 64) {
						maxLifeSpan *= 2;
					}
					curLifeSpan = -1;
				}
			}

			List<HRigidBody> movedObjects = new List<HRigidBody> ();
			for (int i = 0; i < objects.Count; i++) {
				if (!objects [i].isStatic || objects [i].didMove) {
					movedObjects.Add (objects [i]);
				}
			}

			for (int flags = activeChildren, index = 0; flags > 0; flags >>= 1, index++)
				if ((flags & 1) == 1)
					children [index].UpdateTree ();

			for (int i = 0; i < movedObjects.Count; i++) {
				OctTree current = this;
				while (!current.region.Contains (movedObjects [i].transform, movedObjects [i].radius)) {
					if (current.parent != null) {
						current = current.parent;
					} else {
						break;
					}
				}
				objects.Remove (movedObjects [i]);
				current.Insert (movedObjects [i]);
				/*
				string result = "";
				foreach (HRigidBody h in tmpObjects) {
					result += h.name + " ";
				}
				Debug.Log ("!" + name + " " + result);
				*/
			}
			//prune out any dead branches in the tree
			if (children != null) {
				for (int flags = activeChildren, index = 0; flags > 0; flags >>= 1, index++) {
					if ((flags & 1) == 1 && children [index].curLifeSpan == 0) {
						children [index] = null;
						activeChildren ^= (byte)(1 << index);       //remove the node from the active nodes flag list
					}
				}
			}
			
		}
	}

	public void Insert(HRigidBody body){
		if (objects.Count <= 1 && activeChildren == 0) {
			curLifeSpan = -1;
			objects.Add (body);
			return;
		}
		Vector3 dimensions = region.Max - region.Min;
		if ((region.Max.y - region.Min.y) / 2 < minSize) {
			objects.Add (body);
			return;
		}
		#region octants
		Vector3 half = dimensions / 2.0f;
		Vector3 center = region.Min + half;
		//Find or create subdivided regions for each octant in the current region
		BoundingBox[] childOctant = new BoundingBox[8];
		if (children == null){
			children = new OctTree[8];
		}
		childOctant[0] = (children[0] != null) ? children[0].region : new BoundingBox(region.Min, center);
		childOctant[1] = (children[1] != null) ? children[1].region : new BoundingBox(new Vector3(center.x, region.Min.y, region.Min.z), new Vector3(region.Max.x, center.y, center.z));
		childOctant[2] = (children[2] != null) ? children[2].region : new BoundingBox(new Vector3(center.x, region.Min.y, center.z), new Vector3(region.Max.x, center.y, region.Max.z));
		childOctant[3] = (children[3] != null) ? children[3].region : new BoundingBox(new Vector3(region.Min.x, region.Min.y, center.z), new Vector3(center.x, center.y, region.Max.z));
		childOctant[4] = (children[4] != null) ? children[4].region : new BoundingBox(new Vector3(region.Min.x, center.y, region.Min.z), new Vector3(center.x, region.Max.y, center.z));
		childOctant[5] = (children[5] != null) ? children[5].region : new BoundingBox(new Vector3(center.x, center.y, region.Min.z), new Vector3(region.Max.x, region.Max.y, center.z));
		childOctant[6] = (children[6] != null) ? children[6].region : new BoundingBox(center, region.Max);
		childOctant[7] = (children[7] != null) ? children[7].region : new BoundingBox(new Vector3(region.Min.x, center.y, center.z), new Vector3(center.x, region.Max.y, region.Max.z));

		if (drawOcts) {
			DrawOctTree.drawOctTree.list.Add (childOctant [0]);
			DrawOctTree.drawOctTree.list.Add (childOctant [1]);
			DrawOctTree.drawOctTree.list.Add (childOctant [2]);
			DrawOctTree.drawOctTree.list.Add (childOctant [3]);
			DrawOctTree.drawOctTree.list.Add (childOctant [4]);
			DrawOctTree.drawOctTree.list.Add (childOctant [5]);
			DrawOctTree.drawOctTree.list.Add (childOctant [6]);
			DrawOctTree.drawOctTree.list.Add (childOctant [7]);
		}
		#endregion
		//First, is the item completely contained within the root bounding box?
		//note2: I shouldn't actually have to compensate for this. If an object is out of our predefined bounds, then we have a problem/error.
		//          Wrong. Our initial bounding box for the terrain is constricting its height to the highest peak. Flying units will be above that.
		//             Fix: I resized the enclosing box to 256x256x256. This should be sufficient.
		if (region.Contains (body.transform, body.radius)) {
			bool found = false;
			//we will try to place the object into a child node. If we can't fit it in a child node, then we insert it into the current node object list.
			for (int a = 0; a < 8; a++) {
				//is the object contained within a child quadrant?
				if (childOctant [a].Contains (body.transform, body.radius)) {
					if (children [a] != null) {
						children [a].Insert (body);   //Add the item into that tree and let the child tree figure out what to do with it
					}
					else {
						List<HRigidBody> newList = new List<HRigidBody> ();
						newList.Add (body);
						children [a] = new OctTree (childOctant [a], newList, narrowPhase, minSize);
						hasChildren = true;
						children [a].parent = this;
						children [a].name = name + a;
						children [a].isActive = true;
						activeChildren |= (byte)(1 << a);
						children [a].BuildTree ();
					}
					found = true;
				}
			}
			if (!found) {
				objects.Add (body);
			}
		} else {
			objects.Add (body);
		}
	}

	public void toString(string space){
		string result = "";
		for (int i = 0; i < objects.Count; i++) {
			result += objects[i].name + " ";
		}
		Debug.Log (name + space + result);
		if (activeChildren == 0) {
			return;
		}

		for (int flags = activeChildren, index = 0; flags > 0; flags >>= 1, index++)
			if ((flags & 1) == 1) children[index].toString(space + " ");
	}
		
}
