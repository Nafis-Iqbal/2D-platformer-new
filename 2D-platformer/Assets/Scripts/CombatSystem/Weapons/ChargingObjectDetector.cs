using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingObjectDetector : MonoBehaviour
{
    public EnemyBase enemyBaseScript;
    int enemyID;
    PlayerCombatSystem combatSystemScript;
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
        Debug.Log("Bal1");
        if (!enabled || chargeForceApplied) return;
        bool attackFromRight;
        Debug.Log("Bal2: " + other.gameObject);

        if (other.transform.CompareTag("Player"))
        {
            combatSystemScript = other.transform.GetComponent<PlayerCombatSystem>();

            int currentAttackID = enemyBaseScript.currentAttackID;
            if (other.transform.position.x > transform.root.transform.position.x) attackFromRight = false;
            else attackFromRight = true;

            Debug.Log("Bal3");
            combatSystemScript.TakeDamage(enemyClassData.enemyAttacks[currentAttackID], attackFromRight, true);//4th argument: Charge Attack, 5th argument: blockable status
            Debug.Log("Bal4");
        }
    }
}
