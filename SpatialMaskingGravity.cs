//Spatial Masking Broad-Phase algorithm
//Thanks to http://www.cs.cmu.edu/~jbruce/thesis/chapters/thesis-ch03.pdf
//for explanation of extent masks

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SpatialMaskingGravity : MonoBehaviour {

	//public BoundsSpheres boundsSpheres;
	[HideInInspector]
	public NarrowPhase narrowPhase;
	public ulong[,] bitmasks;
	//public HRigidBody[] narrowPhase.physicsEngines;
	public int totalRange;
	public float cellSize = 1f;
	string[] axes = { "x", "y", "z" };
	ulong[] boundsBitmasks;
	ulong boundsBitmask;
	public int numberOfCells; 
	bool asdf = false;
	private const float gravitationalConstant = 6.67408e-11f; // [m^3 kg^-1 s^-2]

	public void StartMasking (float avgRadius) {
		narrowPhase = GetComponent<NarrowPhase> ();
		totalRange = narrowPhase.bounds;
		if (totalRange / (avgRadius * 2) < 64) {
			cellSize = avgRadius * 2;
		} else
			cellSize = totalRange / 64f;
		bitmasks = new ulong[narrowPhase.physicsEngines.Length, 3];
		boundsBitmasks = new ulong[6];
		boundsBitmasks [0] = CalcBoundingBitmask (totalRange - cellSize * 3, totalRange - 1);// top
		boundsBitmasks [1] = CalcBoundingBitmask (0, cellSize * 2);						 // bottom
		boundsBitmasks [2] = CalcBoundingBitmask (0, cellSize * 2);						 // front
		boundsBitmasks [3] = CalcBoundingBitmask (totalRange - cellSize * 3, totalRange - 1);// back
		boundsBitmasks [4] = CalcBoundingBitmask (0, cellSize * 2);						 // left
		boundsBitmasks [5] = CalcBoundingBitmask (totalRange - cellSize * 3, totalRange - 1);// right
	}

	public void CreateMask(){
		int i;
		for (i = 0; i < narrowPhase.physicsEngines.Length; i++) {
			ulong[] ar = GetBitmask (narrowPhase.physicsEngines [i].transform.position, narrowPhase.physicsEngines[i].radius);
			bitmasks [i, 0] = ar [0];
			bitmasks [i, 1] = ar [1];
			bitmasks [i, 2] = ar [2];
		}
	}

	public void searchForCollisions(){
		CreateMask ();
		for (int j = narrowPhase.physicsEngines.Length-1; j >= 0; j--) {
			for (int i = 0; i < j; i++) {
				if (narrowPhase.physicsEngines [j] != narrowPhase.physicsEngines [i]) {
					if (!narrowPhase.physicsEngines[j].isStatic || !narrowPhase.physicsEngines[i].isStatic){
						if ((bitmasks [j, 0] & bitmasks [i, 0]) > 0
							&& (bitmasks [j, 1] & bitmasks [i, 1]) > 0 
							&& (bitmasks [j, 2] & bitmasks [i, 2]) > 0) {
							Vector3 gravitationalForceVector = narrowPhase.physicsEngines [j].transform.position - narrowPhase.physicsEngines [i].transform.position;
							float rSquared = Mathf.Pow (Vector3.Distance (narrowPhase.physicsEngines [j].transform.position, narrowPhase.physicsEngines [i].transform.position), 2);
							float gravitationalForce = gravitationalConstant * narrowPhase.physicsEngines [j].mass * narrowPhase.physicsEngines [i].mass / rSquared;
							gravitationalForceVector = gravitationalForce * gravitationalForceVector.normalized;
							narrowPhase.physicsEngines [j].AddForce (-gravitationalForceVector);
						}
					}
				}
			}
		}
	}

	ulong[] GetBitmask(Vector3 position, float radius){
		ulong[] three = new ulong[3];
		three [0] = CalcBitmask (position.x, radius);
		three [1] = CalcBitmask (position.y, radius);
		three [2] = CalcBitmask (position.z, radius);
		return three;
	}

	ulong CalcBoundingBitmask(float a, float b){
		ulong Bitmask = 0;
		int aa = GetBitPosition (a);
		int bb = GetBitPosition (b);
		ulong Begin = (ulong)1 << aa;
		ulong End = (ulong)1 << bb;
		for (int i = aa; i < bb; i++) {
			Begin |= (ulong)1 << i;
		}
		Bitmask = Begin | End;
		return Bitmask;
	}

	ulong CalcBitmask(float position, float shipRadius){
		ulong Bitmask = 0;

		if (position > totalRange) {
			position = totalRange - shipRadius;
		}
		else if (position < 0) {
			position = 0 + shipRadius;
		}

		int a = GetBitPosition(position - shipRadius);
		int b = GetBitPosition (position + shipRadius);
		//ulong Begin = (ulong)Mathf.Pow (2, a);
		//ulong End = (ulong)Mathf.Pow (2, b);
		ulong Begin = (ulong)1 << a;
		ulong End = (ulong)1 << b;
		for (int i = a; i < b; i++) {
			//Begin |= (ulong)Mathf.Pow (2, i);
			Begin |= (ulong)1 << i;
		}
		Bitmask = Begin | End;
		return Bitmask;
	}
	//
	int GetBitPosition(float position){
		int bitPosition = 0;
		if (cellSize >= 1f) {
			bitPosition = (int)Mathf.Round (position / cellSize);
		} else {
			bitPosition = (int)Mathf.Round (position * cellSize);
		}
		return bitPosition;
	}
}
