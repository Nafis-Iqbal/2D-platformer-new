using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwordCollider : MonoBehaviour {
    private PlayerSwordAttack playerSwordAttack;
    [SerializeField] private float environmentCollisionForce;

    private void Awake() {
        playerSwordAttack = GameManager.Instance.playerTransform.GetComponent<PlayerSwordAttack>();
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Enemy") {
            // other.transform.GetComponent<EnemyCombatManager>().takeDamage(PlayerCombatManager.SwordDamage);
            other.transform.GetComponent<EnemyBase>().health -= PlayerCombatManager.Instance.swordDamage;
            Debug.Log("enemy damage(sword): " + PlayerCombatManager.Instance.swordDamage);
        } else {
            if (other.transform.TryGetComponent<Rigidbody2D>(out Rigidbody2D otherRigidbody)) {
                var force = (Vector2.right * GameManager.Instance.playerTransform.localScale) * environmentCollisionForce;
                otherRigidbody.AddForce(force, ForceMode2D.Impulse);
            }
        }
    }
}
