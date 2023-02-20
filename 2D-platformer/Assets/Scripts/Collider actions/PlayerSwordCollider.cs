using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwordCollider : MonoBehaviour {
    private PlayerSwordAttack playerSwordAttack;

    private void Awake() {
        playerSwordAttack = GameManager.Instance.playerTransform.GetComponent<PlayerSwordAttack>();
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
            other.GetComponent<EnemyHealth>().takeDamage(PlayerCombatManager.SwordDamage);
        }
    }
}
