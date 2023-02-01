using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyEnemy : EnemyBase
{
    public override void Start(){
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        timeBetweenHits = EnemyManager.Instance.heavyEnemyAttackSpeed;
        attackRange = EnemyManager.Instance.HeavyAttackRange;
        animator.speed = .5f;
        activateDistance = 50f;

        health = EnemyManager.Instance.enemyHealth;
        animator = GetComponent<Animator>();

        startNesesarries();

        walkSpeed = speed;
    }

    public override void Reposition(Vector2 target) {
        // transform.position = target;
        transform.position = new Vector2(target.x, target.y);
        inRepositioningPhase = false;
    }


    Vector2 calculateVelocity(Vector2 target) {
        float xDis = transform.position.x - target.x;
        float yDis = transform.position.y - target.y;

        float xVel = xDis / Time.fixedDeltaTime;
        float yVel = yDis / Time.fixedDeltaTime + .5f * Mathf.Abs(Physics2D.gravity.y * Time.fixedDeltaTime);

        return new Vector2(xVel, yVel);
    }
}
