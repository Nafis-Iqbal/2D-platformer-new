using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectileCollider : MonoBehaviour {
    private PlayerProjectile playerProjectile;
    private void Awake() {
        playerProjectile = GetComponent<PlayerProjectile>();
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
            other.transform.GetComponent<EnemyHealth>().takeDamage(playerProjectile.damageAmount);
        }
        
        playerProjectile.Hit();
    }
}
