using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthofPlayer : MonoBehaviour
{
    private float health = 100f;
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Knife")){
            health -= 15f;
            Debug.Log(health);
        }
        if(other.CompareTag("HeavyKnife")){
            health -= 30f;
        }
    }
private void FixedUpdate() {
        if(health <= 0f){
            Destroy(gameObject);
        }
    }
}
