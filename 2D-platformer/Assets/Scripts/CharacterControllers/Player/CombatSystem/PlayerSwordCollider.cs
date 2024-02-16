using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwordCollider : MonoBehaviour
{
    private PlayerCombatSystem playerCombatSystemScript;
    [SerializeField] private float environmentCollisionForce;

    private Color debugColliderColor = Color.blue;

    private void OnDrawGizmos()
    {
        var collider = GetComponent<Collider2D>();
        Gizmos.color = debugColliderColor;
        Gizmos.DrawCube(collider.bounds.center, collider.bounds.size);
    }

    private void Awake()
    {
        playerCombatSystemScript = GameManager.Instance.playerTransform.GetComponent<PlayerCombatSystem>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            debugColliderColor = Color.red;
            // other.transform.GetComponent<EnemyCombatManager>().takeDamage(PlayerCombatManager.SwordDamage);
            other.transform.GetComponent<EnemyBase>().enemyHealth -= PlayerCombatManager.Instance.swordDamage;
            Debug.Log("enemy damage(sword): " + PlayerCombatManager.Instance.swordDamage);
        }
        else
        {
            debugColliderColor = Color.blue;
            if (other.transform.TryGetComponent<Rigidbody2D>(out Rigidbody2D otherRigidbody))
            {
                var force = (Vector2.right * GameManager.Instance.playerTransform.localScale) * environmentCollisionForce;
                otherRigidbody.AddForce(force, ForceMode2D.Impulse);
                Debug.Log("in rigid");
            }
        }
    }
}
