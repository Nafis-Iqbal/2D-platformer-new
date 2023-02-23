using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPhysicsSlowDown : MonoBehaviour {
    public Vector3 originalPosition;
    public float resetDuration = 1f;

    private void Start() {
        originalPosition = transform.position;
        resetPosition();
    }

    public void resetPosition() {
        StartCoroutine(resetPositionRoutine(resetDuration));
    }

    IEnumerator resetPositionRoutine(float duration) {
        yield return new WaitForSeconds(duration);
        transform.position = originalPosition;
        resetPosition();
    }
}
