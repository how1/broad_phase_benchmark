//Simple brute force algorithm for collision detection

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simple : MonoBehaviour {

	public NarrowPhase narrowPhase;
	public int count= 0;

	public void SearchForCollisions () {
		/*
		for (int i = narrowPhase.physicsEngines.Length - 1; i > 0; i--) {
			for (int j = 0; j < i; j++) {
				if (!(narrowPhase.physicsEngines [i].isStatic && narrowPhase.physicsEngines [j].isStatic)) {
					narrowPhase.addParticleCollision (i, j);
					//narrowPhase.physicsEngines [i].colRecord.Add (new CollisionObject (narrowPhase.physicsEngines [i], narrowPhase.physicsEngines [j]));
				}
			}
		}
*/
		for (int i = 0; i < narrowPhase.physicsEngines.Length; i++) {
			for (int k = 0; k < narrowPhase.planeIndices.Length; k++) {
				narrowPhase.addPlaneCollision (i, k);
			}
		}
		for (int i = narrowPhase.physicsEngines.Length - 1; i >= 0; i--) {
			for (int j = 0; j < i; j++) {
				if (narrowPhase.physicsEngines [i] != narrowPhase.physicsEngines [j] && (!narrowPhase.physicsEngines[i].isStatic && !narrowPhase.physicsEngines[j].isStatic)) {
					narrowPhase.addParticleCollision (j, i);
				}
			}
		}
	}
}
