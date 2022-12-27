using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {
    [SerializeField] private float rollForce = 25f;
    private Rigidbody2D body;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        Vector2 v = new Vector2(transform.localScale.x, 0) * rollForce;
        body.AddForce(v, ForceMode2D.Impulse);
    }
}
