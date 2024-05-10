using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearShieldAnimationEventHandler : MonoBehaviour
{
    public EnemySpearShield enemySpearShieldScript;
    public GameObject chargingObjectColider;
    public void SpearOnChargeAttackStarted()
    {
        enemySpearShieldScript.canMove = true;
        enemySpearShieldScript.isChragingTowardsPlayer = true;
        chargingObjectColider.SetActive(true);
    }

    public void SpearOnChargeAttackEnded()
    {
        enemySpearShieldScript.canMove = false;
        enemySpearShieldScript.isChragingTowardsPlayer = false;
        chargingObjectColider.SetActive(false);
    }
}
