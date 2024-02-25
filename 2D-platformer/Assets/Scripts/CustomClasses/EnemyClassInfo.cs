using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyClassInfo
{
    [Header("IDENTITY")]
    public string enemyName;
    public int enemyID;

    [Header("TYPE")]
    public bool groundBased;
    public bool canChangePlatforms;
    public bool groundAirBased;
    public bool airBased;

    [Header("COMBAT PROPERTIES")]
    public bool hasToAndFroMovement;
    public bool movesAwayAfterAttack;
    public bool changesDirectionDuringAttack;
    public float toAndFroMoveFrequency;

    [Header("SPEED")]
    public float enemyMovementSpeed;
    public float enemyPatrolSpeedMultiplier;
    public float enemyCombatWalkSpeedMultiplier;
    public float enemyCombatRunSpeedMultiplier;

    [Header("PATROL")]
    public float enemyHorizontalDetectionDistance;
    public int enemyVerticalDetectionLevels;

    [Header("RANGE")]
    public float enemyTrackingRange;//Distance within which the enemy will stay of the player once he gets near
    public float enemyAttackRange;//Distance required to score a hit or prompt an attack once the enemy attacks
    public float playerAvoidRange;

    [Header("HEALTH & OTHERS")]
    public float enemyMinTimeBetweenAttacks;
    public float enemyHealth;
    public float enemyStamina;
    public float enemyBattleCryRange;

    [Header("ATTACKS")]
    public EnemyAttackInfo[] enemyAttacks = new EnemyAttackInfo[3];
}
