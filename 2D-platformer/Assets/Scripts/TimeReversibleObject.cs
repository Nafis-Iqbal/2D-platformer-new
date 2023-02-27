using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TimeReversibleObject : MonoBehaviour {
    private LinkedList<PointInTime> pointsInTime;
    private Rigidbody2D rigidbody;

    void Awake() {
        pointsInTime = new LinkedList<PointInTime>();
        rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update() {
        if (WorldReverse.Instance.isRewinding) {
            if (rigidbody != null) {
                rigidbody.isKinematic = true;
            }
        } else {
            if (rigidbody != null) {
                rigidbody.isKinematic = false;
            }
        }
    }

    void FixedUpdate() {
        if (WorldReverse.Instance.isRewinding) {
            Rewind();
        } else {
            Record();
        }
    }

    void Rewind() {
        if (pointsInTime.Count > 0) {
            PointInTime pointInTime = pointsInTime.First.Value;
            transform.position = pointInTime.position;
            transform.rotation = pointInTime.rotation;
            if (rigidbody != null) {
                rigidbody.velocity = pointInTime.velocity;
            }
            pointsInTime.RemoveFirst();
        }

    }

    void Record() {
        if (pointsInTime.Count > Mathf.Round(WorldReverse.Instance.recordTime / Time.fixedDeltaTime)) {
            pointsInTime.RemoveLast();
        }
        if (rigidbody != null) {
            pointsInTime.AddFirst(new PointInTime(transform.position, transform.rotation, rigidbody.velocity));
        } else {
            pointsInTime.AddFirst(new PointInTime(transform.position, transform.rotation));
        }
    }
}
