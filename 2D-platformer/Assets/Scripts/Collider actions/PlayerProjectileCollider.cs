using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectileCollider : MonoBehaviour {
    private PlayerProjectile playerProjectile;
    private void Awake() {
        playerProjectile = GetComponent<PlayerProjectile>();
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Enemy") {
            // other.transform.GetComponent<EnemyCombatManager>().takeDamage(PlayerCombatManager.ProjectileDamage);
            other.transform.GetComponent<EnemyBase>().health -= PlayerCombatManager.Instance.projectileDamage;
            Debug.Log("enemy damage(projectile): " + PlayerCombatManager.Instance.projectileDamage);
        }

        playerProjectile.Hit();
    }
}
