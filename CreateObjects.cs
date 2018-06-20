//Used to initialize scene with objects arranged randomly or in a grid

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CreateObjects : MonoBehaviour {

	public NarrowPhase narrowPhase;
	public float offset;
	public float staticRadius;
	public int dimension;
	int space;
	public Vector3 spacing;
	public int numOfNonStaticPart;
	public Vector3[] nonStaticPartLocs;
	public float nonstaticRadius;
	public float mass;
	Simple simple;
	float drag;
	int bounds;
	float speed;
	float radiusRange;
	public float averageRadius = 0;
	float maxRadius = 0;
	float minRadius = 0;

	//string fileName = "TimeTest1.dat";
	//StreamWriter sr;
	//timing variables
	int frames = 0;
	bool write = true;


	int frameCount = 0;
	float nextUpdate = 0.0f;
	float fps = 0.0f;
	float updateRate = 4.0f;  // 4 updates per sec.
	#region planes
	Vector3[] pointsOnPlanes;
	Vector3[] boundsPlanes;
	Plane[] planeIndices;
	Plane front;
	Plane back;
	Plane right;
	Plane left;
	Plane top;
	Plane bottom;
	#endregion

	void Start(){
		GameControl.gameControl.test = this;
		simple = FindObjectOfType<Simple> ();
		mass = GameControl.gameControl.objectMass;
		speed = GameControl.gameControl.speed;
		drag = GameControl.gameControl.drag;
		bounds = GameControl.gameControl.bounds;
		//space = GameControl.gameControl.spacing;
		staticRadius = GameControl.gameControl.radius;
		radiusRange = GameControl.gameControl.radiusRange;
		//spacing = new Vector3 (space, space, space);
		dimension = GameControl.gameControl.numObjects;
		nextUpdate = Time.time;
		definePlanes (bounds);
		int count = 0;

		while (count < dimension) {
			GameObject particle = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			float radius = GameControl.gameControl.radius;
			//radius += Random.value * radiusRange;
			//float radius = radiusRandomizer ();
			if (radius > maxRadius)
				maxRadius = radius;
			if (radius < minRadius)
				minRadius = radius;
			particle.transform.localScale = new Vector3 (radius * 2, radius * 2, radius * 2);
			particle.GetComponent<Renderer> ().material.color = Color.red;
			particle.name = "Sphere (" + count + ")";
			count++;
			HRigidBody h = particle.AddComponent<HRigidBody> ();
			h.mass = mass;
			h.velocityExponent = 2.0f;
			h.drag = drag;
			h.isStatic = false;
			h.radius = radius;
			averageRadius += radius;
				
			narrowPhase.bounds = GameControl.gameControl.bounds;

			distributeObject (h);
			int[] direction = { -1, 1 };
			float speedRange = 0f;
			h.velocityVector = new Vector3 (direction [Random.Range (0, 2)]
				* (Random.value * speedRange + speed), direction [Random.Range (0, 2)]
				* (Random.value * speedRange + speed), direction [Random.Range (0, 2)]
				* (Random.value * speedRange + speed));
			
		}

		#region arrange objects in grid
		/*
		//static objects in grid
		for (int k = 0; k < dimension; k++) {
			for (int j = 0; j < dimension; j++) {
				for (int i = 0; i < dimension; i++) {
					GameObject particle = GameObject.CreatePrimitive (PrimitiveType.Sphere);
					float radius = Random.value * radiusRange + staticRadius;
					//if (count > dimension * dimension * dimension - 100) {
					//	radius = 10.0f; 
					//} else
					//	radius = 0.05f;
					particle.transform.localScale = new Vector3 (radius * 2, radius * 2, radius * 2);
					//particle.transform.localScale = new Vector3 (rad * 2, rad * 2, rad * 2);
					particle.transform.position = new Vector3 (i * spacing.x + offset, j * spacing.y + offset, k * spacing.z + offset);
					particle.GetComponent<Renderer> ().material.color = Color.red;
					particle.name = "Sphere (" + count + ")";
					count++;
					HRigidBody h = particle.AddComponent<HRigidBody> ();
					h.mass = mass;
					h.velocityExponent = 2.0f;
					h.drag = drag;
					h.isStatic = false;
					h.radius = radius;
					averageRadius += radius;
					if (!randomPlacement){
						//h.velocityVector = new Vector3 (-1, 2, -3);
					}
					if (randomPlacement) {
						narrowPhase.bounds = GameControl.gameControl.bounds;
						int[] direction = { -1, 1 };
						float speedRange = speed / 5f;
						h.velocityVector = new Vector3 (direction [Random.Range (0, 2)]
							* (Random.value * speedRange + speed), direction [Random.Range (0, 2)]
							* (Random.value * speedRange + speed), direction [Random.Range (0, 2)]
							* (Random.value * speedRange + speed));
						bool done = false;
						HRigidBody[] tmp = FindObjectsOfType<HRigidBody> ();
						int loop = 0;
						while (!done) {
							particle.transform.position = new Vector3 ((bounds - 4f) * Random.value + 2f, (bounds - 4f) * Random.value + 2f, (bounds - 4f) * Random.value + 2f);
							if (tmp.Length == 1)
								done = true;
							else {
								for (int m = 0; m < tmp.Length; m++) {
									if (h.name != tmp [m].name) {
										bool overlap = CheckBoundingBoxes (h, tmp [m]);
										if (!overlap)
											done = true;
									}
								}
							}
							for (int l = 0; l < planeIndices.Length; l++) {
								int result = narrowPhase.CheckGroundPlaneContacts (new CollisionObject (h, planeIndices [l]));
								if (result == 2 | result == 1) {
									done = false;
								}
							}
							//loop++;
						}
						h.oldPosition = h.transform.position;
					}
				}
			}
		}
		*/
		#endregion
		averageRadius /= count;
		GameControl.gameControl.avgRadius = averageRadius;
		GameControl.gameControl.maxRadius = maxRadius;
		GameControl.gameControl.minRadius = minRadius;
		#region variance test

		float avgVol = (4f / 3f) * Mathf.PI * Mathf.Pow (averageRadius, 3f);
		float percObjVol = (dimension * avgVol)/(bounds * bounds * bounds);
		GameControl.gameControl.percObjVol = percObjVol;
		/*
		bounds = GameControl.gameControl.bounds = narrowPhase.bounds = (int)Mathf.Pow((avgVol * dimension) / 0.01f, 1f/3f);
		definePlanes (bounds);
		speed = (float)bounds / 25.6f;
		GameControl.gameControl.speed = speed;
		foreach (HRigidBody h in FindObjectsOfType<HRigidBody>()) {
			distributeObject (h);
			int[] direction = { -1, 1 };
			float speedRange = 0f;
			h.velocityVector = new Vector3 (direction [Random.Range (0, 2)]
				* (Random.value * speedRange + speed), direction [Random.Range (0, 2)]
				* (Random.value * speedRange + speed), direction [Random.Range (0, 2)]
				* (Random.value * speedRange + speed));
		}

		if (GameControl.gameControl.whichBroad == 1) {
			StreamWriter sr;
			GameControl.gameControl.fileName = "j" + GameControl.gameControl.testNum + "radiusDist" + ".dat";
			if (!File.Exists ("j" + GameControl.gameControl.testNum + "radiusDist" + ".dat")) {
				//sr = File.CreateText ("j" + GameControl.gameControl.testNum + "TimeTest" + whichBroad + ".dat");
				sr = File.CreateText ("j" + GameControl.gameControl.testNum + "radiusDist" + ".dat");
				sr.WriteLine ("#     X     Y     Z");
			} else {
				//sr = File.AppendText ("j" + GameControl.gameControl.testNum + "TimeTest" + whichBroad + ".dat");
				sr = File.AppendText ("j" + GameControl.gameControl.testNum + "radiusDist" + ".dat");
			}
			sr.WriteLine (perc1);
			sr.WriteLine (perc2);
			sr.WriteLine (perc3);
			sr.WriteLine (perc4);
			sr.WriteLine (perc5);
			sr.Close ();
		}
		*/
		#endregion
		//StartCoroutine (timer ());
		narrowPhase.StartNarrowPhase ();

	}
	/*
	int n = 0;
	void Update(){
		frameCount++;
		if (Time.time > nextUpdate)
		{
			nextUpdate += 1.0f/updateRate;
			fps = frameCount * updateRate;
			frameCount = 0;
			n++;
			if (write)
				sr.WriteLine (" " + (n + 1) + "   " + fps);
		}
	}
	*/
	void FixedUpdate(){
		narrowPhase.OnFixedUpdate ();
	}

	void distributeObject(HRigidBody h){
		bool done = false;
		HRigidBody[] tmp = FindObjectsOfType<HRigidBody> ();
		int loop = 0;
		float buffer = bounds / 16f;
		float buffer2 = buffer * 2;
		while (!done) {
			h.transform.position = new Vector3 ((bounds - buffer2) * Random.value + buffer, (bounds - buffer2) * Random.value + buffer, (bounds - buffer2) * Random.value + buffer);
			if (tmp.Length == 1)
				done = true;
			else {
				for (int m = 0; m < tmp.Length; m++) {
					if (h.name != tmp [m].name) {
						bool overlap = CheckBoundingBoxes (h, tmp [m]);
						if (!overlap)
							done = true;
					}
				}
			}
			for (int l = 0; l < planeIndices.Length; l++) {
				int result = narrowPhase.CheckGroundPlaneContacts (new CollisionObject (h, planeIndices [l]));
				if (result == 2 | result == 1) {
					done = false;
				}
			}
		}
		h.oldPosition = h.transform.position;
	}

	bool CheckBoundingBoxes(HRigidBody a1, HRigidBody b1){
		float tol = 0.1f;
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

	private IEnumerator timer(){
		int i = 0;
		yield return new WaitForSecondsRealtime (10);
		write = false;
		//sr.Close ();
		Debug.Log ("done");
	}

	int perc1 = 0;
	int perc2 = 0;
	int perc3 = 0;
	int perc4 = 0;
	int perc5 = 0;
	//Used for variance test
	float radiusRandomizer(){
		int testNum = GameControl.gameControl.testNum;
		float radRange = GameControl.gameControl.radiusRange;
		int rand = Random.Range (0, 12);
		//normal dist
		if (testNum == 0) {
			if (rand == 0) {
				perc1++;
				return Random.value * (radRange * 0.2f) + 0.001f;
			} else if (rand > 0 && rand < 3) {
				perc2++;
				return Random.value * (radRange * 0.2f) + (radRange * 0.2f);
			} else if (rand >= 3 && rand < 9) {
				perc3++;
				return Random.value * (radRange * 0.2f) + (radRange * 0.4f);
			} else if (rand >= 9 && rand < 11) {
				perc4++;
				return Random.value * (radRange * 0.2f) + (radRange * 0.6f);
			} else {
				perc5++;
				return Random.value * (radRange * 0.2f) + (radRange * 0.8f);
			}

		} else if (testNum == 2) {
			//most particles have 2-3m radius
			if (rand == 0) {
				perc1++;
				return Random.value * (radRange * 0.2f) + 0.001f;
			} else if (rand == 1) {
				perc2++;
				return Random.value * (radRange * 0.2f) + (radRange * 0.2f);
			} else if (rand >= 2 && rand <= 9) {
				perc3++;
				return Random.value * (radRange * 0.2f) + (radRange * 0.4f);
			} else if (rand == 10) {
				perc4++;
				return Random.value * (radRange * 0.2f) + (radRange * 0.6f);
			} else {
				perc5++;
				return Random.value * (radRange * 0.2f) + (radRange * 0.8f);
			}
		} else if (testNum == 4) {
			//most particles have either 0-1m radius or 4-5m radius
			rand = Random.Range(1,12);
			if (rand < 4) {
				perc1++;
				return Random.value * (radRange * 0.2f) + 0.001f;
			} else if (rand == 5 || rand == 4) {
				perc2++;
				return Random.value * (radRange * 0.2f) + (radRange * 0.2f);
			} else if (rand == 6) {
				perc3++;
				return Random.value * (radRange * 0.2f) + (radRange * 0.4f);
			} else if (rand == 7 || rand == 8) {
				perc4++;
				return Random.value * (radRange * 0.2f) + (radRange * 0.6f);
			} else {
				perc5++;
				return Random.value * (radRange * 0.2f) + (radRange * 0.8f);
			}
		} else if (testNum == 6) {
			float rad = Random.value * radRange + 0.001f;
			if (rad < radRange * .2f)
				perc1++;
			else if (rad < radRange * .4f)
				perc2++;
			else if (rad < radRange * .6f)
				perc3++;
			else if (rad < radRange * .8f)
				perc4++;
			else
				perc5++;
			return rad;
		} else {
			perc3++;
			return GameControl.gameControl.radius;
		}
	}
	void definePlanes(int dimension){
		#region planes init
		pointsOnPlanes = new Vector3[] {
			new Vector3 (0, 0, 0),
			new Vector3 (0, dimension, 0),
			new Vector3 (0, 0, 0),
			new Vector3 (0, 0, dimension),
			new Vector3 (0, 0, 0),
			new Vector3 (dimension, 0, 0)
		};
		boundsPlanes = new Vector3[] {
			new Vector3 (0, 0, 0),
			new Vector3 (dimension, 0, 0),
			new Vector3 (dimension, 0, dimension),
			new Vector3 (0, 0, dimension),
			new Vector3 (0, 0, 0),
			new Vector3 (0, dimension, 0),
			new Vector3 (dimension, dimension, 0),
			new Vector3 (dimension, dimension, dimension),
			new Vector3 (0, dimension, dimension),
			new Vector3 (0, 0, dimension),
			new Vector3 (0, 0, 0),
			new Vector3 (dimension, 0, 0),
			new Vector3 (dimension, dimension, 0),
			new Vector3 (dimension, dimension, dimension),
			new Vector3 (dimension, 0, dimension),
			new Vector3 (dimension, dimension, dimension),
			new Vector3 (0, dimension, dimension),
			new Vector3 (0, dimension, 0)

		};
		front = new Plane( "front", Vector3.right, Vector3.up, new Vector3 (0, 0, 1) );
		back = new Plane ( "back", Vector3.up, Vector3.right, new Vector3 (0, 0, dimension - 1) );
		left = new Plane ( "left", Vector3.up, Vector3.forward, new Vector3 (1, 0, 0) );
		right = new Plane ( "right", Vector3.forward, Vector3.up, new Vector3 (dimension-1, 0, 0) );
		top = new Plane ("top", Vector3.right, Vector3.forward, new Vector3 (0, dimension-1, 0));
		bottom = new Plane ( "bottom", Vector3.forward, Vector3.right, new Vector3 (0, 1, 0) );
		planeIndices = new Plane[] { top, bottom, front, back, left, right };
		#endregion
	}
}
