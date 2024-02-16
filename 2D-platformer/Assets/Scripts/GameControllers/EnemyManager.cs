using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject player;
    [Header("Base Movement Stats")]
    public float basePatrolSpeedMultiplier;
    public float baseCombatWalkSpeedMultiplier;
    public float baseCombatRunSpeedMultiplier;

    [Header("Enemy Info")]
    public EnemyClassInfo[] enemyData = new EnemyClassInfo[10];

    public static EnemyManager instance = null;

    public static EnemyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (EnemyManager)FindObjectOfType(typeof(EnemyManager));
            }
            return instance;
        }
    }
}
