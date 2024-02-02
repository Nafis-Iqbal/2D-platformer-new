using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public bool isBlockable;
    public int id;
    PlayerCombatManager playerCombatManager;

    private void Start()
    {
        playerCombatManager = EnemyManager.Instance.player.GetComponent<PlayerCombatManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (id == 0)
            {
                playerCombatManager.TakeDamage(30, isBlockable);
            }
            else
            {
                playerCombatManager.TakeDamage(60, isBlockable);
            }

            gameObject.SetActive(false);
        }
    }
}
