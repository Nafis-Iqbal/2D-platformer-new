using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DemoEnemyUnblockableAttack : MonoBehaviour {
    [SerializeField] private int attackDamage = 10;

    void OnDrawGizmos() {
        Handles.Label(transform.position + (Vector3.up * 0.5f), "Unblockable attack");
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            PlayerCombatManager.Instance.TakeDamage(attackDamage, false);
        }
    }

}
