using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyAnimationEventHandler : MonoBehaviour
{
    public EnemyHeavy enemyHeavyScript;
    public void HeavyOnChargeAttackStarted()
    {
        enemyHeavyScript.canMove = true;
        enemyHeavyScript.isChragingTowardsPlayer = true;
    }

    public void HeavyOnChargeAttackEnded()
    {
        enemyHeavyScript.canMove = false;
        enemyHeavyScript.isChragingTowardsPlayer = false;
    }
}
