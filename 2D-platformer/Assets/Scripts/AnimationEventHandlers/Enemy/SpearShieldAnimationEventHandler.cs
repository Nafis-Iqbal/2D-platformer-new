using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearShieldAnimationEventHandler : MonoBehaviour
{
    public EnemySpearShield enemySpearShieldScript;
    public void SpearOnChargeAttackStarted()
    {
        enemySpearShieldScript.canMove = true;
        enemySpearShieldScript.isChragingTowardsPlayer = true;
    }

    public void SpearOnChargeAttackEnded()
    {
        enemySpearShieldScript.canMove = false;
        enemySpearShieldScript.isChragingTowardsPlayer = false;
    }
}
