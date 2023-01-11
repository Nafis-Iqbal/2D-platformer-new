using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShurikenCollider : MonoBehaviour {
    private PlayerShuriken playerShuriken;
    private void Awake() {
        playerShuriken = GetComponent<PlayerShuriken>();
    }
    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
            other.transform.GetComponent<EnemyHealth>().takeDamage(5f);
            gameObject.SetActive(false);
        } else {
            playerShuriken.Stop();
        }
    }
}
