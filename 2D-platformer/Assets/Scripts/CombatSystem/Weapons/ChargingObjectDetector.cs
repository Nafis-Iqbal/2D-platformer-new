using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingObjectDetector : MonoBehaviour
{
    public EnemyBase enemyBaseScript;
    int enemyID;
    PlayerCombatSystem playerCombatScript;
    private EnemyClassInfo enemyClassData = null;
    bool chargeForceApplied;

    // Start is called before the first frame update
    void OnEnable()
    {
        enemyID = enemyBaseScript.enemyID;
        enemyClassData = EnemyManager.Instance.enemyData[enemyID];
        chargeForceApplied = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled || chargeForceApplied) return;
        bool attackFromRight;

        if (other.transform.CompareTag("Player") && other.isTrigger == false)
        {
            playerCombatScript = other.transform.GetComponent<PlayerCombatSystem>();
            if (playerCombatScript.isPlayerRolling) return;

            int currentAttackID = enemyBaseScript.currentAttackID;

            if (other.transform.position.x > transform.root.transform.position.x) attackFromRight = false;
            else attackFromRight = true;

            playerCombatScript.TakeDamage(enemyClassData.enemyAttacks[currentAttackID], attackFromRight, true);//4th argument: Charge Attack, 5th argument: blockable status
        }
    }
}
