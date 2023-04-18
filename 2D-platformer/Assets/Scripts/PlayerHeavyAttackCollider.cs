using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeavyAttackCollider : MonoBehaviour {
    private PlayerHeavyAttack playerHeavyAttack;
    [SerializeField] private float environmentCollisionForce;
    private Color debugColliderColor = Color.blue;

    private void OnDrawGizmos() {
        var collider = GetComponent<Collider2D>();
        Gizmos.color = debugColliderColor;
        Gizmos.DrawCube(collider.bounds.center, collider.bounds.size);
    }

    private void Awake() {
        playerHeavyAttack = GameManager.Instance.playerTransform.GetComponent<PlayerHeavyAttack>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Enemy") {
            debugColliderColor = Color.red;
            // other.transform.GetComponent<EnemyCombatManager>().takeDamage(PlayerCombatManager.SwordDamage);
            other.transform.GetComponent<EnemyBase>().health -= PlayerCombatManager.Instance.heavyAttackDamage;
            Debug.Log("enemy damage(heavy attack): " + PlayerCombatManager.Instance.heavyAttackDamage);
        } else {
            debugColliderColor = Color.blue;
            if (other.transform.TryGetComponent<Rigidbody2D>(out Rigidbody2D otherRigidbody)) {
                var force = (Vector2.right * GameManager.Instance.playerTransform.localScale) * environmentCollisionForce * playerHeavyAttack.attackDamageMultiplier;
                otherRigidbody.AddForce(force, ForceMode2D.Impulse);
            }
        }
    }
}
