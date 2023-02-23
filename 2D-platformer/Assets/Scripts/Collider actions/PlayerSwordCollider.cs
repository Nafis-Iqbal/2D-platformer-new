using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwordCollider : MonoBehaviour {
    private PlayerSwordAttack playerSwordAttack;

    private void Awake() {
        playerSwordAttack = GameManager.Instance.playerTransform.GetComponent<PlayerSwordAttack>();
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Enemy") {
            // other.transform.GetComponent<EnemyCombatManager>().takeDamage(PlayerCombatManager.SwordDamage);
            other.transform.GetComponent<EnemyBase>().health -= PlayerCombatManager.SwordDamage;
            Debug.Log("enemy damage(sword): " + PlayerCombatManager.SwordDamage);
        }
    }
}
