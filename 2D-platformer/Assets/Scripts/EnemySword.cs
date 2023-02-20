using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySword : MonoBehaviour {
    private PlayerHealth playerHealth;
    [SerializeField] private float swordDamage = 5f;

    private void Awake() {
        playerHealth = GameManager.Instance.playerTransform.GetComponent<PlayerHealth>();
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            playerHealth.TakeDamage(swordDamage);
        }
    }
}
