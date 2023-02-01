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
}