using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySword : MonoBehaviour {
    private PlayerCombatManager playerCombatManager;
    [SerializeField] private int swordDamage = 5;

    private void Awake() {
        playerCombatManager = GameManager.Instance.playerTransform.GetComponent<PlayerCombatManager>();
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            playerCombatManager.TakeDamage(swordDamage , true);
        }
    }
}
