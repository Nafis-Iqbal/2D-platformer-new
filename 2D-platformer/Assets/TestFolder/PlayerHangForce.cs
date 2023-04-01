using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHangForce : MonoBehaviour {
    public Rigidbody2D playerRigidBody;
    public float forceAmount = 2f;

    private void Update() {
        // if (Input.GetKey(KeyCode.A)) {
        //     Debug.Log("a pressed...");
        //     playerRigidBody.AddForce(Vector2.left * forceAmount);
        // } else if (Input.GetKey(KeyCode.D)) {
        //     Debug.Log("d pressed...");
        //     playerRigidBody.AddForce(Vector2.right * forceAmount);
        // }
    }
}
