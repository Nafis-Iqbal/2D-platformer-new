using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHeavy : EnemyBase
{
    private float dist;
    private float nextX;
    private float baseY;
    private float height;
    private Vector3 startPos;
    public override void Start()
    {
        isRepositioning = true;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        timeBetweenHits = EnemyManager.Instance.heavyEnemyAttackSpeed;
        enemyAttackRange = EnemyManager.Instance.HeavyAttackRange;
        enemySpineAnimator.speed = .5f;
        activateDistance = 50f;

        enemyHealth = EnemyManager.Instance.enemyHealth;
        enemySpineAnimator = GetComponent<Animator>();

        acquireDependencies();

        enemyWalkSpeed = enemyMovementSpeed;
    }

    public override void Reposition(Vector2 tar)
    {
        startPos = transform.position;
        rb.gravityScale = 0f;
        tar.y += 1f;
        calculateVelocity(tar);
    }

    void calculateVelocity(Vector2 tar)
    {
        dist = tar.x - startPos.x;
        nextX = Mathf.MoveTowards(transform.position.x, tar.x, 5f * Time.fixedDeltaTime);
        baseY = Mathf.Lerp(startPos.y, tar.y, (nextX - startPos.x) / dist);
        height = 1f * (nextX - startPos.x) * (nextX - tar.x) / (-.25f * dist * dist);

        Vector3 movePosition = new Vector3(nextX, baseY + height, transform.position.z);
        // transform.rotation = LookAtTarget(movePosition - transform.position);
        transform.position = movePosition;


        if (tar.x == transform.position.x && tar.y == transform.position.y)
        {
            if (!isGrounded)
            {
                rb.gravityScale = 100f;
            }
            else
            {
                isRepositioning = true;
                rb.gravityScale = 1f;
            }

        }
    }
}
