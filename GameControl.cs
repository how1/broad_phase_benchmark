//Controls test parameters from run to run

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameControl : MonoBehaviour {

	public static GameControl gameControl;
	[HideInInspector]
	public CreateObjects createObjects;
	public int bounds = 32; //size of scene
	public int numObjects = 1; 
	public int whichBroad = 0; //which broad-phase algorithm: 0-BruteForce, 1-SpatialMasking, 2-Oct-Tree, 3-SAP, 4-Adaptive
	public int adaptBroad = 0; //
	public float radius = 1f; //size of objects
	public float radiusRange = 0; //range of radii size
	public float avgRadius = 1f;
	public float maxRadius = 1;
	public float minRadius = 1;
	public float percObjVol = 0;
	public float drag = 1.0f; //air resistance
	//public int spacing = 3;
	public string fileName = "NTimeTest0"; //file being written to if testing (NarrowPhase.testing)
	public float objectMass = 1.0f; //object mass
	public bool gravity;
	[Range(0,1)]
	public float cof;
	public float colTol;
	public int samples = 0; //number of frames averaged per test run
	public float prevTime = 0;
	public AudioSource audioSource; //tells you when test is done
	public float speed; //speed of objects (speed^2 * 3)^(1/2) = m/s
	public int testNum = 0; //can determine file name
	public int repetition = 0;
	public float cof = 0.8f;
	public bool gravity;
	//public int objectRadius;
	//public StreamWriter sr;
	bool open = false;

	void Awake () {
		if (gameControl == null) {
			DontDestroyOnLoad (gameObject);
			gameControl = this;
			createObjects = FindObjectOfType<CreateObjects> ();
		} else if (gameControl != this)
			Destroy (gameObject);
	}

}
