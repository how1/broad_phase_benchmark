//This NarrowPhase scripts takes care of many aspects of the physics and testing
//Many thanks to 
//Bourg, David M. “Chapter 13: Implementing Collision Response.” Physics for 
//Game Developers, 1st ed., O'Reilly Media, 2002, pp. 205–210. 
//for collision response physics (CheckGroundPlaneContacts, CheckForCollision, 
//ApplyImpulse, CalcDistanceFromPointToPlane, narrowPhasePlaneCollision, narrowPhaseParticleCollision)


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using UnityEngine.SceneManagement;

public class NarrowPhase : MonoBehaviour {

	#region Variables
	public bool gravity = true;
	public bool testing = false;
	public HRigidBody[] physicsEngines; 
	public bool physics = true;
	[HideInInspector]
	public SpatialMasking mask;
	[HideInInspector]
	public Bounds boundsThing;
	[HideInInspector]
	public Simple simple;
	[HideInInspector]
	public OctTreeAlg octTree;
	[HideInInspector]
	public SweepAndPrune sweepAndPrune;
	[HideInInspector]
	public int whichBroad = 0;
	const int SIMPLE = 0;
	const int SPATIAL = 1;
	const int OCTTREE = 2;
	const int SAP = 3;
	const int ADAPTIVE = 4;
	const int NOCOLLISION = 0;
	const int CONTACT = 1;
	const int PENETRATING = 2;
	int status = 0;     
	public float COLLISIONTOLERANCE = 0.2f;
	public float coefficientOfRestitution = 0.8f;
	[HideInInspector]
	public int bounds = 128;
	Vector3 vCollisionNormal;
	Vector3 vRelativeVelocity;
	Vector3 planeCollisionNormal;
	[HideInInspector]
	public Vector3 startPos;
	const double tol = 0.0000000000000000001f;
	Vector3[] pointsOnPlanes;
	Vector3[] boundsPlanes;
	[HideInInspector]
	public int boundsTol = 1;
	Plane front;// = { Vector3.up, Vector3.right, pointsOnPlanes [0] };
	Plane back;// = { Vector3.up, Vector3.right, pointsOnPlanes [5] };
	Plane left;// = { Vector3.up, Vector3.forward, pointsOnPlanes [0] };
	Plane right;// = { Vector3.up, Vector3.forward, pointsOnPlanes [3] };
	Plane top;// = { Vector3.forward, Vector3.right, pointsOnPlanes [1] };
	public Plane bottom;// = { Vector3.forward, Vector3.right, pointsOnPlanes [0] };
	public Plane[] planeIndices;// = { front, back, left, right, top, bottom };
	public List<CollisionObject> cols = new List<CollisionObject>();
	bool write = false;
	Stopwatch stopwatch;
	#endregion

	void Start(){
		if (!testing) {
			StartNarrowPhase ();
		}
	}
	int frameCount = 0;
	int testNum = 0;
	public void StartNarrowPhase () {
		mask = GetComponent<SpatialMasking> ();
		simple = GetComponent<Simple> ();
		octTree = GetComponent<OctTreeAlg> ();
		sweepAndPrune = GetComponent<SweepAndPrune> ();
		boundsThing = gameObject.AddComponent<Bounds> ();
		//COLLISIONTOLERANCE = GameControl.gameControl.radius / 5f;
		bounds = GameControl.gameControl.bounds;
		whichBroad = GameControl.gameControl.whichBroad;
		GameControl.gameControl.fileName = testNum + "TimeTest" + whichBroad + ".dat";
		pastLaptimes = new List<double> ();

		physicsEngines = FindObjectsOfType<HRigidBody> ();
		if (!physics) {
			boundsThing.StartBounds ();
		}

		#region planes init
		pointsOnPlanes = new Vector3[] {
			new Vector3 (0, 0, 0),
			new Vector3 (0, bounds - boundsTol, 0),
			new Vector3 (0, 0, 0),
			new Vector3 (0, 0, bounds - boundsTol),
			new Vector3 (0, 0, 0),
			new Vector3 (bounds - boundsTol, 0, 0)
		};
		boundsPlanes = new Vector3[] {
			new Vector3 (0, 0, 0),
			new Vector3 (bounds, 0, 0),
			new Vector3 (bounds, 0, bounds),
			new Vector3 (0, 0, bounds),
			new Vector3 (0, 0, 0),
			new Vector3 (0, bounds, 0),
			new Vector3 (bounds, bounds, 0),
			new Vector3 (bounds, bounds, bounds),
			new Vector3 (0, bounds, bounds),
			new Vector3 (0, 0, bounds),
			new Vector3 (0, 0, 0),
			new Vector3 (bounds, 0, 0),
			new Vector3 (bounds, bounds, 0),
			new Vector3 (bounds, bounds, bounds),
			new Vector3 (bounds, 0, bounds),
			new Vector3 (bounds, bounds, bounds),
			new Vector3 (0, bounds, bounds),
			new Vector3 (0, bounds, 0)

		};

		front = new Plane( "front", Vector3.right, Vector3.up, new Vector3 (0, 0, 1) );
		back = new Plane ( "back", Vector3.up, Vector3.right, new Vector3 (0, 0, bounds - boundsTol) );
		left = new Plane ( "left", Vector3.up, Vector3.forward, new Vector3 (1, 0, 0) );
		right = new Plane ( "right", Vector3.forward, Vector3.up, new Vector3 (bounds-boundsTol, 0, 0) );
		top = new Plane ("top", Vector3.right, Vector3.forward, new Vector3 (0, bounds-boundsTol, 0));
		bottom = new Plane ( "bottom", Vector3.forward, Vector3.right, new Vector3 (0, 1, 0) );
		planeIndices = new [] { top, bottom, front, back, left, right };
		#endregion

		if (testing) {
			//StartCoroutine (timer());
			//StartCoroutine (record());
		}
		if (whichBroad == SPATIAL) {
			mask.StartMasking (GameControl.gameControl.avgRadius);
		} else if (whichBroad == OCTTREE) {
			octTree.StartOctTree (GameControl.gameControl.minRadius);
		} else if (whichBroad == SAP) {
			sweepAndPrune.StartSweepAndPrune ();
		} else {
			simple.narrowPhase = this;
		}
	}

	void initializeVelocity(double velocity){
		for (int i = 0; i < physicsEngines.Length; i++) {
			//int randIndex = Random.Range (0, 2);
			int[] dirs = {-1, 1};
			//float[] vels = new float[3];
			float xVel = dirs[Random.Range(0,2)] * (float)(Random.value * velocity);
			float yVel = dirs[Random.Range(0,2)] * (float)(Random.value * (velocity - xVel));
			float zVel = dirs[Random.Range(0,2)] * (float)(velocity - Mathf.Sqrt (xVel * xVel + yVel * yVel));
			//vels [0] = xVel;
			//vels [1] = yVel;
			//vels [2] = zVel;
			//Vector3 velVec = new Vector3 ();
			//velVec.x = vels [randIndex];
			//vels[randIndex]
			physicsEngines [i].velocityVector = new Vector3 (xVel, yVel, zVel);
		}
	}

	void FixedUpdate(){
		if (!testing) {
			OnFixedUpdate ();
		}
	}
	int adaptBroad = 1;
	public void Adaptive(){
		if (adaptBroad != 1 && GameControl.gameControl.percObjVol * avgVelocity > 0.3f) {
			//GameControl.gameControl.whichBroad = 1;
			adaptBroad = 1;
			mask.StartMasking (GameControl.gameControl.avgRadius);
		} else if (adaptBroad != 3 && avgVelocity <= 50 && avgVelocity > 10) {
			//GameControl.gameControl.whichBroad = 3;
			adaptBroad = 3;
			sweepAndPrune.StartSweepAndPrune ();
		} else if (adaptBroad != 2 && avgVelocity <= 10) {
			adaptBroad = 2;
			octTree.StartOctTree (GameControl.gameControl.minRadius);
		}
		GameControl.gameControl.adaptBroad = adaptBroad;
		if (adaptBroad == SPATIAL) {
			stopwatch = Stopwatch.StartNew ();
			mask.searchForCollisions ();
			if (BpOnly) {
				stopwatch.Stop ();
				processCollisionObjects ();
			} else if (BpNp) {
				stopwatch.Stop ();
				broadPhaseTime = stopwatch.Elapsed.TotalMilliseconds;
				stopwatch = Stopwatch.StartNew ();
				processCollisionObjects ();
				stopwatch.Stop ();
				narrowPhaseTime = stopwatch.Elapsed.TotalMilliseconds;
			} else {
				processCollisionObjects ();
				stopwatch.Stop ();
			}
			lapTime = stopwatch.Elapsed.TotalMilliseconds;
		} else if (adaptBroad == OCTTREE) {
			stopwatch = Stopwatch.StartNew ();
			octTree.RestartOctTree ();
			if (BpOnly) {
				stopwatch.Stop ();
				processCollisionObjects ();
			} else if (BpNp) {
				stopwatch.Stop ();
				broadPhaseTime = stopwatch.Elapsed.TotalMilliseconds;
				stopwatch = Stopwatch.StartNew ();
				processCollisionObjects ();
				stopwatch.Stop ();
				narrowPhaseTime = stopwatch.Elapsed.TotalMilliseconds;
			} else {
				processCollisionObjects ();
				stopwatch.Stop ();
			}
			lapTime = stopwatch.Elapsed.TotalMilliseconds;
		} else if (adaptBroad == SAP) {
			double[] lapTimes = sweepAndPrune.getSAPCollisions ();
			if (BpNp) {
				broadPhaseTime = lapTimes [0];
				narrowPhaseTime = lapTimes [1];
			} else {
				lapTime = lapTimes [0];
			}
		}
	}
		
	List<double> pastLaptimes;
	double averageTime = 0; //elapsed milliseconds * 1000 = mirco
	double lapTime;
	double broadPhaseTime;
	double narrowPhaseTime;
	double avgBpTime = 0;
	double avgNpTime = 0;
	bool first = true;
	int count = 0;
	[HideInInspector]
	public bool BpOnly = false;
	[HideInInspector]
	public bool BpNp = false;
	public bool rollingAverage;
	public void OnFixedUpdate () {
		calculateVelocityStdDev ();
		if (GameControl.gameControl.whichBroad != whichBroad) {
			whichBroad = GameControl.gameControl.whichBroad;
			if (whichBroad == SPATIAL) {
				mask.StartMasking (GameControl.gameControl.avgRadius);
			} else if (whichBroad == OCTTREE) {
				octTree.StartOctTree (GameControl.gameControl.minRadius);
			} else if (whichBroad == SAP) {
				sweepAndPrune.StartSweepAndPrune ();
			}
		}
		if (whichBroad == SIMPLE) {
			stopwatch = Stopwatch.StartNew ();
			simple.SearchForCollisions ();
			if (BpOnly) {
				stopwatch.Stop ();
				processCollisionObjects ();
			} else if (BpNp) {
				stopwatch.Stop ();
				broadPhaseTime = stopwatch.Elapsed.TotalMilliseconds;
				stopwatch = Stopwatch.StartNew ();
				processCollisionObjects ();
				stopwatch.Stop ();
				narrowPhaseTime = stopwatch.Elapsed.TotalMilliseconds;
			} else {
				processCollisionObjects ();
				stopwatch.Stop ();
			}
			lapTime = stopwatch.Elapsed.TotalMilliseconds;
		} else if (whichBroad == SPATIAL) {
			stopwatch = Stopwatch.StartNew ();
			mask.searchForCollisions ();
			if (BpOnly) {
				stopwatch.Stop ();
				processCollisionObjects ();
			} else if (BpNp) {
				stopwatch.Stop ();
				broadPhaseTime = stopwatch.Elapsed.TotalMilliseconds;
				stopwatch = Stopwatch.StartNew ();
				processCollisionObjects ();
				stopwatch.Stop ();
				narrowPhaseTime = stopwatch.Elapsed.TotalMilliseconds;
			} else {
				processCollisionObjects ();
				stopwatch.Stop ();
			}
			lapTime = stopwatch.Elapsed.TotalMilliseconds;
		} else if (whichBroad == OCTTREE) {
			stopwatch = Stopwatch.StartNew ();
			octTree.RestartOctTree ();
			if (BpOnly) {
				stopwatch.Stop ();
				processCollisionObjects ();
			} else if (BpNp) {
				stopwatch.Stop ();
				broadPhaseTime = stopwatch.Elapsed.TotalMilliseconds;
				stopwatch = Stopwatch.StartNew ();
				processCollisionObjects ();
				stopwatch.Stop ();
				narrowPhaseTime = stopwatch.Elapsed.TotalMilliseconds;
			} else {
				processCollisionObjects ();
				stopwatch.Stop ();
			}
			lapTime = stopwatch.Elapsed.TotalMilliseconds;
		} else if (whichBroad == SAP) {
			double[] lapTimes = sweepAndPrune.getSAPCollisions ();
			if (BpNp) {
				broadPhaseTime = lapTimes [0];
				narrowPhaseTime = lapTimes [1];
			} else {
				lapTime = lapTimes [0];
			}
		} else if (whichBroad == 4) {
			Adaptive ();
		}
		if (testing) {
			//Record time
			if (rollingAverage) {
				count++;
				if (pastLaptimes.Count == 0) {
					pastLaptimes.Add (lapTime);
					averageTime = lapTime;
				} else if (pastLaptimes.Count < 5) {
					averageTime = ((averageTime * pastLaptimes[0]) - averageTime) / (count - 1);
					averageTime = averageTime + ((lapTime - averageTime) / count);
					pastLaptimes.Add (lapTime);
				}
				else {
					averageTime = ((averageTime * pastLaptimes[0]) - averageTime) / (count - 1);
					averageTime = averageTime + ((lapTime - averageTime) / count);
					pastLaptimes.Remove (0);
					pastLaptimes.Add (lapTime);
				}
			} else {
				count++;
				if (BpNp) {
					avgBpTime = avgBpTime + ((broadPhaseTime * 1000f) - avgBpTime / count);
					avgNpTime = avgNpTime + ((narrowPhaseTime * 1000f) - avgNpTime / count);
				} else {
					averageTime = averageTime + (((lapTime * 1000f) - averageTime) / count);
				}
			}
		}
		frameCount++;
		if (frameCount == GameControl.gameControl.samples && testing) {
			//endTest ();
			//timer();
		}
	}
	

	//Check for plane collision
	public int CheckGroundPlaneContacts(CollisionObject col){
		float d;
		status = NOCOLLISION;
		//check distance from body1 to the ground plane
		d = CalcDistanceFromPointToPlane(col.body1.transform.position, col.plane.vec1, col.plane.vec2, col.plane.pop);
		float distance = d - col.body1.radius;
		//Debug.Log (distance);
		float Vrn = Vector3.Dot (col.body1.velocityVector, planeCollisionNormal);
		if (Mathf.Abs (distance) <= COLLISIONTOLERANCE && Vrn < 0.0) {
			status = CONTACT;
		} else if (distance < 0 ) {//-COLLISIONTOLERANCE
			status = PENETRATING;
		}
		return status;
	}

	//Input: pt in body, u and v define the plane, ptOnplane is a point that lies in the plane
	//Output: Distance from body to plane
	float CalcDistanceFromPointToPlane(Vector3 pt, Vector3 u, Vector3 v, Vector3 ptOnPlane){
		planeCollisionNormal = Vector3.Cross(u, v);
		Vector3 PQ = pt - ptOnPlane;
		return Vector3.Dot (PQ, planeCollisionNormal);
	}
		
	public int CheckForCollision(CollisionObject col){
		float r = col.body1.radius + col.body2.radius;
		Vector3 d = col.body1.transform.position - col.body2.transform.position;
		float s = d.magnitude - r;
		vCollisionNormal = d.normalized;
		vRelativeVelocity = col.body1.velocityVector - col.body2.velocityVector;   
		float Vrn = Vector3.Dot (vRelativeVelocity, vCollisionNormal);
		if ((Mathf.Abs(s) <= COLLISIONTOLERANCE) && (Vrn < 0.0)) {
			return CONTACT;
		} else if (s < 0) {//-COLLISIONTOLERANCE
			return PENETRATING;
		} else {
			return NOCOLLISION;
		}
	}

	void narrowPhasePlaneCollision(CollisionObject col){
		bool tryAgain = true;
		int planesCheck = 0;
		bool didPen = false;
		float dt = Time.deltaTime;
		/*
		if (!physics) {
			planesCheck = CheckGroundPlaneContacts (col);
			if (planesCheck == CONTACT) {
				ApplyImpulse (col.body1);
			}
		} else {
		*/
		while (tryAgain && dt > tol) {
			tryAgain = false;
			planesCheck = CheckGroundPlaneContacts (col);
			if (planesCheck == PENETRATING) {
				dt = dt / 2;	
				tryAgain = true;
				didPen = true;
				col.body1.transform.position = col.body1.oldPosition;
				col.body1.transform.position += col.body1.velocityVector * dt;
			} else if (planesCheck == CONTACT) {
				ApplyImpulse (col.body1);
			}
		}
	}

	void narrowPhaseParticleCollision(CollisionObject col){
		bool tryAgain = true;
		int particlesCheck = 0;
		bool didPen = false;
		float dt = Time.deltaTime;

		while (tryAgain && dt > tol) {
			tryAgain = false;

			particlesCheck = CheckForCollision (col);
			if (particlesCheck == PENETRATING) {
				dt = dt / 2;
				tryAgain = true;
				didPen = true;
				col.body1.transform.position = col.body1.oldPosition;
				col.body1.transform.position += col.body1.velocityVector * dt;
				col.body2.transform.position = col.body2.oldPosition;
				col.body2.transform.position += col.body2.velocityVector * dt;
			} else if (particlesCheck == CONTACT) {
				col.body1.applyGravity = false;
				col.body2.applyGravity = false;
				ApplyImpulse (col);
			}
		}
	}

	public void stepSimulation(HRigidBody body){
		if (gravity && !body.isStatic && body.transform.position.y > body.radius + 1) {
			body.AddForce (body.mass * 9.8f * Vector3.down);
		}
		body.applyGravity = true;
		body.SumForces ();
		body.velocityVector += (body.netForceVector / body.mass * Time.deltaTime);
		body.oldPosition = body.transform.position;
		if (!physics) {
			body.transform.position += body.directions * Time.deltaTime;
		} else {
			body.transform.position += body.velocityVector * Time.deltaTime;
			if (body.transform.position == body.oldPosition) {
				body.didMove = false;
			} else
				body.didMove = true;
		}
	}

	public void addPlaneCollision(int h, int i){
		CollisionObject obj = new CollisionObject (physicsEngines [h], planeIndices[i]);
		cols.Add (obj);
	}

	public void addPlaneCollision(HRigidBody h, int i){
		CollisionObject obj = new CollisionObject (h, planeIndices[i]);
		cols.Add (obj);
	}

	public void addParticleCollision(int h, int i){
		CollisionObject obj = new CollisionObject(physicsEngines[h], physicsEngines[i]);
		cols.Add (obj);
	}

	public void addParticleCollision(HRigidBody h, HRigidBody i){
		cols.Add (new CollisionObject (h, i));
	}

	double avgProcessCount = 0;
	int processCount = 0;
	int n = 0;
	public double avgVelocity = 0;
	int velCount = 0;
	public void processCollisionObjects(List<CollisionObject> collisions){
		for (int i = 0; i < collisions.Count; i++) {
			processCount++;
			if (collisions[i].body1 && collisions[i].body2) {
				narrowPhaseParticleCollision (collisions[i]);
			} else {
				narrowPhasePlaneCollision (collisions[i]);
			}
		}
		for (int i = 0; i < physicsEngines.Length; i++) {
			stepSimulation (physicsEngines [i]);
		}
		avgVelocity = 0;
		for (int i = 0; i < physicsEngines.Length; i++) {
			avgVelocity += physicsEngines [i].velocityVector.magnitude;
		}
		avgVelocity /= physicsEngines.Length;
		/*
		for (int i = 0; i < physicsEngines.Length; i++) {
			float velBefore = physicsEngines [i].oldVelMag;
			stepSimulation (physicsEngines [i]);
			float velAfter = physicsEngines [i].velocityVector.magnitude;
			if (velCount < physicsEngines.Length) {
				velCount++;
				avgVelocity = avgVelocity + ((velAfter - avgVelocity) / velCount);
			} else {
				avgVelocity = ((avgVelocity * velBefore) - avgVelocity) / (velCount - 1);
				avgVelocity = avgVelocity + ((velAfter - avgVelocity) / velCount);
			}
		}
		*/
		n++;
		avgProcessCount = avgProcessCount + ((processCount - avgProcessCount) / n);
	}

	public void processCollisionObjects(){
		for (int i = 0; i < cols.Count; i++) {
			processCount++;
			if (cols[i].body1 && cols[i].body2) {
				narrowPhaseParticleCollision (cols[i]);
			} else {
				narrowPhasePlaneCollision (cols[i]);
			}
		}
		for (int i = 0; i < physicsEngines.Length; i++) {
			stepSimulation (physicsEngines [i]);
		}
		avgVelocity = 0;
		for (int i = 0; i < physicsEngines.Length; i++) {
			avgVelocity += physicsEngines [i].velocityVector.magnitude;
		}
		avgVelocity /= physicsEngines.Length;
		cols.Clear ();
		n++;
		avgProcessCount = avgProcessCount + ((processCount - avgProcessCount) / n);
	}

	void checkBounds(HRigidBody body){
		if (body.transform.position.x > bounds - body.radius) {
			body.directions = -body.directions;
		}
		else if (body.transform.position.x < 0 + body.radius) {
			body.directions = -body.directions;
		}
		if (body.transform.position.y > bounds - body.radius) {
			body.directions = -body.directions;
		}
		else if (body.transform.position.y < 0 + body.radius) {
			body.directions = -body.directions;
		}
		if (body.transform.position.z > bounds - body.radius) {
			body.directions = -body.directions;
		}
		else if (body.transform.position.z < 0 + body.radius) {
			body.directions = -body.directions;
		}
	}

	//For particle/plane
	//public float bodyPlaneMultFactor = 1f;
	void ApplyImpulse(HRigidBody body){
		//float randomization = 0.000f;
		//Vector3 randomizedVector = new Vector3 (Random.Range (-1, 1) * randomization, Random.Range (-1, 1) * randomization, Random.Range (-1, 1) * randomization);
		//planeCollisionNormal = planeCollisionNormal + randomizedVector;
		Vector3 N = planeCollisionNormal;
		Vector3 V = body.velocityVector;
		Vector3 Vn = Vector3.Dot (N, V) * N;
		Vector3 Vt = V - Vn;
		Vector3 newVelocityVector = Vt - coefficientOfRestitution * Vn;
		body.velocityVector = newVelocityVector;

	}

	//For particle/particle
	void ApplyImpulse(CollisionObject col){
		//float randomization = 0.001f;
		//Vector3 randomizedVector = new Vector3 (Random.Range (-1, 1) * randomization, Random.Range (-1, 1) * randomization, Random.Range (-1, 1) * randomization);
		//vCollisionNormal = vCollisionNormal + randomizedVector;

		//body1
		Vector3 V = col.body1.velocityVector;
		Vector3 Vn = Vector3.Dot (vCollisionNormal, V) * vCollisionNormal;

		//body2
		Vector3 V2 = col.body2.velocityVector;
		Vector3 Vn2 = Vector3.Dot (vCollisionNormal, V2) * vCollisionNormal;

		if (!col.body1.isStatic && !col.body2.isStatic) {
			float j = (-(1 + coefficientOfRestitution) *
				(Vector3.Dot (vRelativeVelocity, vCollisionNormal))) /
				((Vector3.Dot (vCollisionNormal, vCollisionNormal)) *
					(1 / col.body1.mass + 1 / col.body2.mass));
			col.body1.velocityVector += ((j * vCollisionNormal) / col.body1.mass);
			j = (-(1 + coefficientOfRestitution) *
				(Vector3.Dot (vRelativeVelocity, vCollisionNormal))) /
				((Vector3.Dot (vCollisionNormal, vCollisionNormal)) *
					(1 / col.body2.mass + 1 / col.body1.mass));
			col.body2.velocityVector -= ((j * vCollisionNormal) / col.body2.mass);
		} else if (col.body1.isStatic && !col.body2.isStatic) {
			float j = (-(1 + coefficientOfRestitution) *
				(Vector3.Dot (vRelativeVelocity, vCollisionNormal))) /
				((Vector3.Dot (vCollisionNormal, vCollisionNormal)) *
					(1 / col.body2.mass));
			col.body2.velocityVector -= ((j * vCollisionNormal) / col.body2.mass);
		} else if (!col.body1.isStatic && col.body2.isStatic) {
			float j = (-(1 + coefficientOfRestitution) *
				(Vector3.Dot (vRelativeVelocity, vCollisionNormal))) /
				((Vector3.Dot (vCollisionNormal, vCollisionNormal)) *
					(1 / col.body1.mass));
			col.body1.velocityVector += ((j * vCollisionNormal) / col.body1.mass);
		}
	}
	int recordCount = 0;
	private IEnumerator record(){
		while (write) {
			yield return new WaitForSecondsRealtime (1);
			recordCount++;
			StreamWriter sr;
			GameControl.gameControl.fileName = "j" + "TimeTest" + ".dat";
			if (!File.Exists ("j" + "TimeTest" + ".dat")) {
				sr = File.CreateText ("j" + "TimeTest" + ".dat");
				sr.WriteLine ("#     X     Y     Z");
			} else {
				sr = File.AppendText ("j" + "TimeTest" + ".dat");
			}
			sr.WriteLine (GameControl.gameControl.whichBroad + "   " + GameControl.gameControl.testNum + "   " + bounds + "   " + (averageTime * 1000) + "   " + GameControl.gameControl.radius + "   " + GameControl.gameControl.numObjects + "   " + avgVelocity);
			//sr.WriteLine (recordCount + "   " + avgVelocity + "   " + GameControl.gameControl.avgRadius + "   " + whichBroad + "   " + (averageTime * 1000));
			sr.Close ();
		}
	}
	//private IEnumerator timer(){
	void timer(){
		//yield return new WaitForSecondsRealtime (10);
		bool done = false;
		recordCount++;
		StreamWriter sr;
		GameControl.gameControl.fileName = "j" + "TimeTest" + ".dat";
		if (!File.Exists ("j" + "TimeTest" + ".dat")) {
			sr = File.CreateText ("j" + "TimeTest" + ".dat");
			sr.WriteLine ("#     X     Y     Z");
		} else {
			sr = File.AppendText ("j" + "TimeTest" + ".dat");
		}
		sr.WriteLine (GameControl.gameControl.whichBroad + "   " + GameControl.gameControl.testNum + "   " + bounds + "   " + (averageTime * 1000) + "   " + GameControl.gameControl.radius + "   " + GameControl.gameControl.numObjects + "   " + avgVelocity);
		//sr.WriteLine (recordCount + "   " + avgVelocity + "   " + GameControl.gameControl.avgRadius + "   " + whichBroad + "   " + (averageTime * 1000));
		sr.Close ();
		/*
		int timerCount = GameControl.gameControl.testNum;

		write = true;
		yield return new WaitForSecondsRealtime (20);
		bool done = false;

		if (timerCount == 0) {
			GameControl.gameControl.radius = 1.5f;
			GameControl.gameControl.numObjects = 301;
		} else if (timerCount == 1) {
			GameControl.gameControl.radius = 1.7f;
			GameControl.gameControl.numObjects = 234;
		} else if (timerCount == 2) {
			GameControl.gameControl.radius = 2f;
			GameControl.gameControl.numObjects = 169;
		} else if (timerCount == 3) {
			GameControl.gameControl.radius = 3f;
			GameControl.gameControl.numObjects = 75;
		}
		else if (timerCount == 4) {
			GameControl.gameControl.radius = 1f;
			GameControl.gameControl.numObjects = 471;
			GameControl.gameControl.whichBroad++;
			if (GameControl.gameControl.whichBroad == 2)
				GameControl.gameControl.whichBroad++;
			GameControl.gameControl.testNum = -1;
		}
		*/
		if (GameControl.gameControl.whichBroad == 3)
			done = true;
		GameControl.gameControl.whichBroad = 3;
		if (!done)
			SceneManager.LoadScene (0);
		else {
			UnityEngine.Debug.Log ("done");
		}

	}

	/*
	private IEnumerator timer(){
		int i = 0;
		yield return new WaitForSecondsRealtime (20);
		StreamWriter sr;
		if (!File.Exists("QTimeTest" + whichBroad + ".dat"))
			sr = File.CreateText("QTimeTest" + whichBroad + ".dat");
		else 
			sr = File.AppendText("QTimeTest" + whichBroad + ".dat");
		sr.WriteLine (bounds + "   " + physicsEngines.Length + "   " + (averageSpatialTime/1000));
		sr.Close ();
		//GameControl.gameControl.sr.Close ();
		bool done = false;
		int spacing = 1;
		GameControl.gameControl.numObjects++;
		if (GameControl.gameControl.numObjects == 5) {
			GameControl.gameControl.numObjects = 1;
			GameControl.gameControl.bounds *= 2;
			if (GameControl.gameControl.bounds == 512) {
				GameControl.gameControl.numObjects = 1;
				GameControl.gameControl.bounds = 32;
				GameControl.gameControl.whichBroad++;
				GameControl.gameControl.fileName = "QTimeTest" + whichBroad + ".dat";
				if (GameControl.gameControl.whichBroad == 4) {
					done = true;
				}
			}
		}
		while (spacing * GameControl.gameControl.numObjects < GameControl.gameControl.bounds) {
			spacing++;
		}
		GameControl.gameControl.spacing = spacing;
		spacing--;
		if (!done)
			SceneManager.LoadScene (0);
		else 
			UnityEngine.Debug.Log ("done");
	}
	*/
	public bool timeTest = false;
	public bool varianceTest = false;
	public bool environmentTest = false;
	public bool speedTest = false;
	public bool BpNpTest = false;
	public bool adaptiveTest = false;
	//private IEnumerator timer(){
	void endTest(){
		int i = 0;
		//yield return new WaitForSecondsRealtime (10);
		StreamWriter sr;
		if (varianceTest) {
			GameControl.gameControl.fileName = "j" + GameControl.gameControl.testNum + "TimeTest" + ".dat";
			if (!File.Exists ("j" + GameControl.gameControl.testNum + "TimeTest" + ".dat")) {
				sr = File.CreateText ("j" + GameControl.gameControl.testNum + "TimeTest" + ".dat");
				sr.WriteLine ("#     X     Y     Z");
			} else {
				sr = File.AppendText ("j" + GameControl.gameControl.testNum + "TimeTest" + ".dat");
			}
		} else {
			GameControl.gameControl.fileName = "j" + GameControl.gameControl.testNum + "TimeTest" + whichBroad + ".dat";
			if (!File.Exists ("j" + "TimeTest" + whichBroad + ".dat")) {
				//sr = File.CreateText ("j" + GameControl.gameControl.testNum + "TimeTest" + whichBroad + ".dat");
				sr = File.CreateText ("j" + "TimeTest" + whichBroad + ".dat");
				sr.WriteLine ("#     X     Y     Z");
			} else {
				//sr = File.AppendText ("j" + GameControl.gameControl.testNum + "TimeTest" + whichBroad + ".dat");
				sr = File.AppendText ("j" + "TimeTest" + whichBroad + ".dat");
			}
		}
		if (timeTest)
			sr.WriteLine (bounds + "   " + GameControl.gameControl.numObjects + "   " + averageTime);
		else if (environmentTest) {
			sr.WriteLine (bounds + "   " + averageTime);
		} else if (speedTest) {
			if (whichBroad == 4) {
				sr.WriteLine (((long)((long)bounds*(long)bounds*(long)bounds)) + "   " + 
					bounds + "   " + averageTime + "   " + GameControl.gameControl.radius + "   "
					+ GameControl.gameControl.numObjects + "   " + avgVelocity + "   " +
					GameControl.gameControl.speed + "   " + whichBroad + "   " + GameControl.gameControl.testNum 
					+ "   " + GameControl.gameControl.avgRadius + "   " + adaptBroad  + "   " + calculateVelocityStdDev());

			} else
				sr.WriteLine (((long)((long)bounds*(long)bounds*(long)bounds)) + "   " + 
					bounds + "   " + averageTime + "   " + GameControl.gameControl.radius + "   "
					+ GameControl.gameControl.numObjects + "   " + avgVelocity + "   " +
					GameControl.gameControl.speed + "   " + whichBroad + "   " + GameControl.gameControl.testNum 
					+ "   " + GameControl.gameControl.avgRadius + "   " + calculateVelocityStdDev());
		} else if (BpNpTest) {
			sr.WriteLine (bounds + "   " + GameControl.gameControl.speed + "   " + avgBpTime + "   " + avgNpTime);
		} else if (adaptiveTest) {
			sr.WriteLine (bounds + "   " + GameControl.gameControl.speed + "   " + whichBroad + "   " + averageTime);
		}
		else sr.WriteLine (bounds + "   " + GameControl.gameControl.numObjects + "   " + avgProcessCount);
		sr.Close ();
		//GameControl.gameControl.sr.Close ();
		bool done = false;
		/*
		//Baseline
		GameControl.gameControl.numObjects += 100;
		if (GameControl.gameControl.numObjects > 3001) {
			GameControl.gameControl.numObjects = 1;
			GameControl.gameControl.whichBroad++;
			if (GameControl.gameControl.whichBroad > 3) {
				done = true;
			}
		}
		/*
		//Speed test
		GameControl.gameControl.numObjects++;
		if (GameControl.gameControl.numObjects > 8) {
			GameControl.gameControl.numObjects = 2;
			GameControl.gameControl.whichBroad++;
			if (GameControl.gameControl.whichBroad > 3) {
				GameControl.gameControl.whichBroad = 1;
				GameControl.gameControl.objectMass = 10;
				GameControl.gameControl.drag = 1f;
				GameControl.gameControl.speed = 0.1f;
				GameControl.gameControl.testNum++;
				if (GameControl.gameControl.testNum > 1)
					done = true;
			}
		}

		//Varying Speed test
		GameControl.gameControl.speed *= 1.66f;
		if (GameControl.gameControl.speed > 120000) {
			GameControl.gameControl.speed = 0.1f;
			GameControl.gameControl.whichBroad++;
			if (GameControl.gameControl.whichBroad > 4) {
					done = true;
			}
		}

		//Increasing radius test
		GameControl.gameControl.radius += 1f;
		if (GameControl.gameControl.radius > 15.1f) {
			GameControl.gameControl.radius = .1f;
			GameControl.gameControl.whichBroad++;
			if (GameControl.gameControl.whichBroad > 3) {
				done = true;
			}
		}
		*/
		//Size Variation
		GameControl.gameControl.radius = GameControl.gameControl.avgRadius;
		GameControl.gameControl.repetition++;
		if (GameControl.gameControl.repetition > 20) {
			GameControl.gameControl.repetition = 0;
			GameControl.gameControl.testNum++;
			if (GameControl.gameControl.testNum == 8) {
				GameControl.gameControl.testNum = 0;
				GameControl.gameControl.whichBroad++;
				if (GameControl.gameControl.whichBroad > 3) {
					done = true;
				}
			}
		}
		/*
		//Size Variation
		GameControl.gameControl.radiusRange += 1.0f;
		if (GameControl.gameControl.radiusRange > 10.0f) {
			GameControl.gameControl.radiusRange = 0.0f;
			GameControl.gameControl.whichBroad++;
			if (GameControl.gameControl.whichBroad > 3) {
				//GameControl.gameControl.whichBroad = 1;
				//GameControl.gameControl.objectMass = 10;
				//GameControl.gameControl.drag = 1f;
				//GameControl.gameControl.speed = 0.1f;
				GameControl.gameControl.testNum++;
				if (GameControl.gameControl.testNum > 0)
					done = true;
			}
		}

	
		//Environment Size
		GameControl.gameControl.bounds *= 2;
		if (GameControl.gameControl.bounds > 2048) {
			GameControl.gameControl.bounds = 32;
			GameControl.gameControl.whichBroad++;
			if (GameControl.gameControl.whichBroad > 3) {
				//GameControl.gameControl.whichBroad = 1;
				//GameControl.gameControl.objectMass = 10;
				//GameControl.gameControl.drag = 1f;
				//GameControl.gameControl.speed = 0.1f;
				//GameControl.gameControl.testNum++;
				//if (GameControl.gameControl.testNum > 0)
				done = true;
			}
		}
		*/
		/*
		int spacing = 1;
		while (spacing * GameControl.gameControl.numObjects < GameControl.gameControl.bounds) {
			spacing++;
		}
		spacing--;
		GameControl.gameControl.spacing = spacing;
		*/
		if (!done)
			SceneManager.LoadScene (0);
		else {
			UnityEngine.Debug.Log ("done");
		}
	}

	double calculateVelocityStdDev(){
		HRigidBody[] tmp = FindObjectsOfType<HRigidBody> ();
		double avgVel = 0;
		for (int i = 0; i < tmp.Length; i++) {
			avgVel += tmp [i].velocityVector.magnitude;
		}
		avgVel /= tmp.Length;
		double stdDev = 0;
		for (int i = 0; i < tmp.Length; i++) {
			stdDev += Mathf.Pow(Mathf.Abs (tmp [i].velocityVector.magnitude - (float)avgVel), 2);
		}
		stdDev /= tmp.Length;
		stdDev = Mathf.Pow ((float)stdDev, 0.5f);
		return stdDev;
	}

	//Draw bounds
	void OnDrawGizmos(){
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube (new Vector3 ((bounds) / 2, (bounds) / 2, (bounds) / 2), new Vector3 (bounds - boundsTol, bounds - boundsTol, bounds - boundsTol));
	}

}
