using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject player;

    [Header("Archer Stats")]
    public float archerRange = 5f;
    public float enemyHealth = 50f;
    [Header("FootSoldier Stats")]
    [Header("HeavySoldier Stats")]
    public float heavyEnemyHealth = 300f;

    public float heavyEnemySpeed = 40f;
    public float heavyEnemyAttackSpeed = .8f;
    public float HeavyAttackRange = 1.5f;
    public float enemySpeed = 500f;
    public float enemyAttackSpeed = .35f;

    public float genRange = 10f;
    public float attackMelRange = .7f;

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
