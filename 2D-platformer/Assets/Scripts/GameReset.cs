using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameReset : MonoBehaviour {
    [SerializeField] private Transform[] allObjects;
    private List<Vector3> objectPositions;
    private List<Quaternion> objectRotations;

    private void Awake() {
        objectPositions = new List<Vector3>();
        objectRotations = new List<Quaternion>();

        foreach (var item in allObjects) {
            objectPositions.Add(item.position);
            objectRotations.Add(item.rotation);
        }
    }

    public void ResetEverything() {
        for (int i = 0; i < allObjects.Length; i++) {
            allObjects[i].position = objectPositions[i];
            allObjects[i].rotation = objectRotations[i];
        }
    }
}
