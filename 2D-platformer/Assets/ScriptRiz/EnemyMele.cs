using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMele : EnemyBase
{

    int repoMode = 0;
    public override void Start(){
        isReadyToClimp = false;
        noReposition = true;
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

    private float dist;
    private float nextX;
    private float baseY;
    private float height;
    private Vector3 startPos;
    public override void Reposition(Vector2 tar) {
        if (tar.y > transform.position.y){
            Vector2 RepoStart = enemyClosestRepoStartPoint();
            // Debug.Log(RepoStart.x+ " "+ RepoStart.y);
            if (Mathf.Abs(transform.position.x - RepoStart.x) > 0.05f)
            {
                if (!isReadyToClimp)
                {
                    towardsRepoPoint(RepoStart);
                }
            }
            else
            {
                isReadyToClimp = true;
            }

            if (isReadyToClimp)
            {
                // transform.position = tar;
                rb.gravityScale = 0f;
                towardsFinalTarget(tar);
            }
            float dist = Mathf.Abs(transform.position.y - tar.y);
            if (dist < 0.05f)
            {
                noReposition = true;
                rb.gravityScale = 1f;
                isReadyToClimp = false;
            }
        }else{
            startPos = transform.position;
            rb.gravityScale = 0f;
            tar.y += 1f;
            calculateVelocity(tar);
        }
    }

    void towardsFinalTarget(Vector2 tar){
        animator.Play("repositionUp");
        transform.position = Vector2.MoveTowards(transform.position , tar , 1f * Time.fixedDeltaTime);
    }

    void towardsRepoPoint(Vector2 tar){
        animator.Play("Patrolling animation");
        transform.position = Vector2.MoveTowards(transform.position , tar , speed/50f * Time.fixedDeltaTime);
    }

    void calculateVelocity(Vector2 tar) {
        dist = tar.x - startPos.x;
        nextX = Mathf.MoveTowards(transform.position.x, tar.x, speed/50f * Time.fixedDeltaTime);
        baseY = Mathf.Lerp(startPos.y , tar.y , (nextX - startPos.x) / dist);
        height = 1f * (nextX - startPos.x) * (nextX    - tar.x) / (-.25f * dist * dist);

        Vector3 movePosition = new Vector3(nextX, baseY + height, transform.position.z);
        // transform.rotation = LookAtTarget(movePosition - transform.position);
        transform.position = movePosition;


        if (tar.x == transform.position.x && tar.y == transform.position.y)
        {
            if(!isGrounded) {
                rb.gravityScale = 100f;
            }else{
                noReposition = true;
                rb.gravityScale = 1f;
            }
            
        }
    }
}
