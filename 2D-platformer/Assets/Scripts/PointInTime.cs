using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointInTime {

    public Vector3 position;
    public Quaternion rotation;
    public Vector2 velocity;

    public PointInTime(Vector3 _position, Quaternion _rotation) {
        position = _position;
        rotation = _rotation;
    }
    public PointInTime(Vector3 _position, Quaternion _rotation, Vector3 _velocity) {
        position = _position;
        rotation = _rotation;
        velocity = _velocity;
    }

}
