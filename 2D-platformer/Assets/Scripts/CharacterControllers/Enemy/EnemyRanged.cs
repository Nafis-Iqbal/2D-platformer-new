using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRanged : EnemyBase
{
    public Transform rowPossition;
    private bool canThrow = true;
    public GameObject row;
    float timeBetweenShoots;

    public override void Start(){
        isReadyToClimb = false;
        isRepositioning = true;
        shouldBePatrolling = true;
        canHit = true;
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        timeBetweenShoots = EnemyManager.Instance.enemyAttackSpeed;
        attackRange = EnemyManager.Instance.archerRange;
        battleCryRange = EnemyManager.Instance.genRange;
        health = EnemyManager.Instance.enemyHealth;
        animator = GetComponent<Animator>();
        startNesesarries();
        walkSpeed = speed;
    }

    IEnumerator Throw(){
        canThrow = false;
        animator.Play("Idle");
        yield return new WaitForSeconds(timeBetweenShoots);
        //Play attack animation;
        animator.Play("attacking");
        GameObject throwRow = Instantiate(row, rowPossition.position, Quaternion.identity);
        throwRow.transform.position = rowPossition.position;
        throwRow.transform.rotation = transform.rotation;

        int dir;
        if (transform.position.x > player.position.x)
        {
            dir = -1;
        }
        else
        {
            dir = 1;
        }

        Vector2 rowScale = throwRow.transform.localScale;
        rowScale.x *= dir;
        throwRow.transform.localScale = rowScale;
        yield return new WaitForSeconds(.5f);
        randomNumber = Random.Range(1,5);
        canThrow = true;
    }

    public override void attack(){
        if(isGrounded)
            rb.velocity = Vector3.zero;
        if (canThrow){
            farAnimationPlayed = false;
            StartCoroutine(Throw());
        }

        if(randomNumber == 3){
            canHit = false;
            animator.Play("BattleCry");
        }
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
                if (!isReadyToClimb)
                {
                    towardsRepoPoint(RepoStart);
                }
            }
            else
            {
                isReadyToClimb = true;
            }

            if (isReadyToClimb)
            {
                // transform.position = tar;
                rb.gravityScale = 0f;
                towardsFinalTarget(tar);
            }
            float dist = Mathf.Abs(transform.position.y - tar.y);
            if (dist < 0.05f)
            {
                isRepositioning = true;
                rb.gravityScale = 1f;
                isReadyToClimb = false;
                notInRepositioningPhase = true;
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
        nextX = Mathf.MoveTowards(transform.position.x, tar.x, 5f * Time.fixedDeltaTime);
        baseY = Mathf.Lerp(startPos.y , tar.y , (nextX - startPos.x) / dist);
        height = 1f * (nextX - startPos.x) * (nextX    - tar.x) / (-.25f * dist * dist);

        Vector3 movePosition = new Vector3(nextX, baseY + height, transform.position.z);
        transform.position = movePosition;


        if (tar.x == transform.position.x && tar.y == transform.position.y)
        {
            if(!isGrounded) {
                rb.gravityScale = 20f;
            }else{
                isRepositioning = true;
                rb.gravityScale = 1f;
                notInRepositioningPhase = true;
            }
            
        }
    }
}