//Sweep and Prune broad-phase algorithm
//Thanks to http://www.codercorner.com/SAP.pdf <-- for general explanation
//and https://github.com/mattleibow/jitterphysics/wiki/Sweep-and-Prune <--for SortAxis function

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class SweepAndPrune : MonoBehaviour {
	[HideInInspector]
	public NarrowPhase narrowPhase;
	HRigidBody[] objects;
	public float tol = 0.2f;
	pairManager pm;
	bool BpOnly = false;
	bool BpNp = false;
	public List<EndPoint> xEndPoints;
	public List<EndPoint> yEndPoints;
	public List<EndPoint> zEndPoints;

	class pairManager{
		HRigidBody[] objects;
		byte[,] pairs;

		public pairManager(HRigidBody[] objs){
			objects = objs;
			pairs = new byte[objects.Length, objects.Length];
		}
		public void addPair(int i, int j){
			pairs [i, j] = 1;
		}
		public void removePair(int i, int j){
			pairs [i, j] = 0;
		}
		public List<CollisionObject> getCollisionPairs(){
			List<CollisionObject> cols = new List<CollisionObject> ();
			for (int i = 0; i < objects.Length; i++) {
				for (int j = 0; j < objects.Length; j++) {
					if (pairs [i, j] == 1) {
						cols.Add (new CollisionObject (objects[i], objects[j]));
					}
				}
			}
			return cols;
		}
	}

	// Use this for initialization
	public void StartSweepAndPrune () {
		narrowPhase = GetComponent<NarrowPhase> ();
		BpOnly = narrowPhase.BpOnly;
		BpNp = narrowPhase.BpNp;
		objects = narrowPhase.physicsEngines;
		pm = new pairManager (objects);
		int i = 0;
		foreach (HRigidBody h in objects) {
			h.index = i;
			i++;
		}
		xEndPoints = new List<EndPoint> ();
		yEndPoints = new List<EndPoint> ();
		zEndPoints = new List<EndPoint> ();
		foreach (HRigidBody h in objects) {
			h.mins = new EndPoint[3];
			h.maxs = new EndPoint[3];
		}
		setEndPoints ();
		//collectInitialOverlaps ();
	}

	Stopwatch stopwatch;
	double lapTime;
	double BpTime = 0;
	double NpTime = 0;
	public List<CollisionObject> cols;
	public void getSAPCollisions(){
		cols = wallCollisions ();
		cols.AddRange (pm.getCollisionPairs ());
	}

	public void setEndPoints(){
		xEndPoints.Clear ();
		yEndPoints.Clear ();
		zEndPoints.Clear ();
		for (int i = 0; i < objects.Length; i++) {
			float rad = objects [i].radius;
			Vector3 pos = objects [i].transform.position;
			EndPoint xMin = new EndPoint (objects [i], pos.x - rad - tol, true);
			EndPoint yMin = new EndPoint (objects [i], pos.y - rad - tol, true);
			EndPoint zMin = new EndPoint (objects [i], pos.z - rad - tol, true);

			EndPoint xMax = new EndPoint (objects [i], pos.x + rad + tol, false);
			EndPoint yMax = new EndPoint (objects [i], pos.y + rad + tol, false);
			EndPoint zMax = new EndPoint (objects [i], pos.z + rad + tol, false);

			objects [i].mins [0] = xMin;
			objects [i].mins [1] = yMin;
			objects [i].mins [2] = zMin;

			objects [i].maxs [0] = xMax;
			objects [i].maxs [1] = yMax;
			objects [i].maxs [2] = zMax;

			xEndPoints.Add (xMin);
			yEndPoints.Add (yMin);
			zEndPoints.Add (zMin);

			xEndPoints.Add (xMax);
			yEndPoints.Add (yMax);
			zEndPoints.Add (zMax);

		}
		sortEndpoints ();
	}
					
	public void sortEndpoints(){
		xEndPoints.Sort ((a,b) => a.value.CompareTo(b.value));
		yEndPoints.Sort ((a,b) => a.value.CompareTo(b.value));
		zEndPoints.Sort ((a,b) => a.value.CompareTo(b.value));
	}

	bool overlapTest(int i, int k){
		HRigidBody a = objects [i];
		HRigidBody b = objects [k];
		if (a.mins [0].value < b.mins [0].value && a.maxs [0].value > b.mins [0].value 
			|| b.mins [0].value < a.mins [0].value && b.maxs [0].value > a.mins [0].value
			|| a.mins[0].value > b.mins[0].value && a.maxs[0].value < b.maxs[0].value
			|| b.mins[0].value > a.mins[0].value && b.maxs[0].value < a.maxs[0].value) {
			if (a.mins [1].value < b.mins [1].value && a.maxs [1].value > b.mins [1].value 
				|| b.mins [1].value < a.mins [1].value && b.maxs [1].value > a.mins [1].value
				|| a.mins[1].value > b.mins[1].value && a.maxs[1].value < b.maxs[1].value
				|| b.mins[1].value > a.mins[1].value && b.maxs[1].value < a.maxs[1].value) {
				if (a.mins [2].value < b.mins [2].value && a.maxs [2].value > b.mins [2].value 
					|| b.mins [2].value < a.mins [2].value && b.maxs [2].value > a.mins [2].value
					|| a.mins[2].value > b.mins[2].value && a.maxs[2].value < b.maxs[2].value
					|| b.mins[2].value > a.mins[2].value && b.maxs[2].value < a.maxs[2].value) {
					pm.addPair (i, k);
					return true;
				}
			}
		}
		return false;
	}

	List<CollisionObject> wallCollisions(){
		List<CollisionObject> cols = new List<CollisionObject> ();
		//top, bottom, front, back, left, right
		for (int i = 0; i < objects.Length; i++) {
			//bottom
			if (objects [i].mins [1].value < 0 + narrowPhase.boundsTol + tol) {
				cols.Add (new CollisionObject (objects [i], narrowPhase.planeIndices [1]));
			}
			//top
			else if (objects [i].maxs [1].value > narrowPhase.bounds - narrowPhase.boundsTol - tol) {
				cols.Add (new CollisionObject (objects [i], narrowPhase.planeIndices [0]));
			}
			//front
			if (objects [i].mins [2].value < 0 + narrowPhase.boundsTol + tol) {
				cols.Add (new CollisionObject (objects [i], narrowPhase.planeIndices [2]));
			}
			//back
			else if (objects [i].maxs [2].value > narrowPhase.bounds - narrowPhase.boundsTol - tol) {
				cols.Add (new CollisionObject (objects [i], narrowPhase.planeIndices [3]));
			}
			//left
			if (objects [i].mins [0].value < 0 + narrowPhase.boundsTol + tol) {
				cols.Add (new CollisionObject (objects [i], narrowPhase.planeIndices [4]));
			}
			//right
			else if (objects [i].maxs [0].value > narrowPhase.bounds - narrowPhase.boundsTol - tol) {
				cols.Add (new CollisionObject (objects [i], narrowPhase.planeIndices [5]));
			}
		}
		return cols;
	}

	public void updateEndpoints(){
		for (int i = 0; i < objects.Length; i++) {
			float rad = objects [i].radius;
			Vector3 pos = objects [i].transform.position;
			objects [i].mins [0].value = pos.x - rad - tol;
			objects [i].mins [1].value = pos.y - rad - tol;
			objects [i].mins [2].value = pos.z - rad - tol;

			objects [i].maxs[0].value = pos.x + rad + tol;
			objects [i].maxs[1].value = pos.y + rad + tol;
			objects [i].maxs[2].value = pos.z + rad + tol;
		}
		SortAxis (xEndPoints);
		SortAxis (yEndPoints);
		SortAxis (zEndPoints);
	}

	void InsertionSort(List<EndPoint> list){
		int i = 1;
		while (i < list.Count) {
			int j = i;
			while (j > 0 && list [j - 1].value > list [j].value) {
				Swap (list [j], list [j - 1]);
				j = j - 1;
			}
			i++;
		}
	}

	void Swap(EndPoint a, EndPoint b){
		EndPoint tmp = a;
		a = b;
		b = tmp;

		if (b.isMin && !a.isMin) {
			if (!overlapTest (a.body.index, b.body.index)) {
				pm.removePair (a.body.index, b.body.index);
			}
		} else if (!b.isMin && a.isMin) {
			if (overlapTest (a.body.index, b.body.index)) {
				pm.addPair (a.body.index, b.body.index);
			}
		}

	}

	private void SortAxis(List<EndPoint> axis)
	//https://github.com/mattleibow/jitterphysics/wiki/Sweep-and-Prune
	{
		for (int j = 1; j < axis.Count; j++)
		{
			EndPoint keyelement = axis[j];
			float key = keyelement.value;

			int i = j - 1;

			while (i >= 0 && axis[i].value > key)
			{
				EndPoint swapper = axis[i];

				if (keyelement.isMin && !swapper.isMin)
				{
					if (CheckBoundingBoxes(swapper.body, keyelement.body))
					{
						pm.addPair(swapper.body.index, keyelement.body.index);
					}
				}

				if (!keyelement.isMin && swapper.isMin)
				{
					pm.removePair(swapper.body.index, keyelement.body.index);
				}

				axis[i + 1] = swapper;
				i = i - 1;
			}
			axis[i + 1] = keyelement;
		}
	}

	bool CheckBoundingBoxes(HRigidBody a1, HRigidBody b1){
		Vector3 a = a1.transform.position;
		float aRad = a1.radius;
		Vector3 b = b1.transform.position;
		float bRad = b1.radius;
		if (a.x + aRad + tol < b.x - bRad - tol) return false; // a is left of b
		if (a.x - aRad - tol > b.x + bRad + tol) return false; // a is right of b
		if (a.y + aRad + tol < b.y - bRad - tol) return false; // a is left of b
		if (a.y - aRad - tol > b.y + bRad + tol) return false; // a is right of b
		if (a.z + aRad + tol < b.z - bRad - tol) return false; // a is left of b
		if (a.z - aRad - tol > b.z + bRad + tol) return false; // a is right of b
		return true; // boxes overlap
	}

}
