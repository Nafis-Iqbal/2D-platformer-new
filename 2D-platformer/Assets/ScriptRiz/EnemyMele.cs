using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMele : EnemyBase
{
    public override void Start(){
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        notPatrolling = true;
        canHit = true;
        attackRange = EnemyManager.Instance.attackMelRange;
        timeBetweenHits = EnemyManager.Instance.enemyAttackSpeed;
        health = EnemyManager.Instance.enemyHealth;
        animator = GetComponent<Animator>();

        startNesesarries();

        walkSpeed = speed;
    }
}
