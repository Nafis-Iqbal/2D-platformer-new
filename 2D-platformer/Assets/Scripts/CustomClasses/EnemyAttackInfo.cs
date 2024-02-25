using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyAttackInfo
{
    public int attackID;
    public string attackName;
    public int attackDamage;
    public int attackStaminaCost;
    public float attackMinCooldownTime;
    public float lastAttackUsedTime = -1.0f;
    public string postAttackBehaviour;
    public float attackRange;
    public bool isRanged;
    public bool isMelee;
    public bool isCombo;
    public bool isUnblockable;
    public bool isCustomAttack;
    public float appliedAttackMomentum;

    public void useAttack()
    {
        lastAttackUsedTime = Time.time;
    }
}

