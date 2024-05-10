using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailAnimationEventHander : MonoBehaviour
{
    public EnemyForestSnail enemySnailScript;

    public void CombatPromptsOnAcidAttack()
    {
        enemySnailScript.performAcidProjectileAttack();
    }
}
