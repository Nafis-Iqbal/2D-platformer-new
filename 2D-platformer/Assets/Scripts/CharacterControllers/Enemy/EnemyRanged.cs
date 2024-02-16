using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRanged : EnemyBase
{
    public Transform projectileSpawnPosition;
    private bool canThrow = true;
    public GameObject row;
    float timeBetweenShoots;
    private float dist;
    private float nextX;
    private float baseY;
    private float height;
    private Vector3 startPos;
    private EnemyClassInfo enemyData;
    public int enemyID;

    public override void OnEnable()
    {
        base.OnEnable();
        isReadyToClimb = false;
        isRepositioning = true;
        isPatrolling = true;
        canAttackPlayer = true;

        enemyData = EnemyManager.Instance.enemyData[enemyID];

        enemyClassSpeed = enemyData.enemyMovementSpeed;
        playerHorizontalDetectionDistance = enemyData.enemyHorizontalDetectionDistance;
        playerVerticalDetectionLevels = enemyData.enemyVerticalDetectionLevels;

        enemyAttackRange = enemyData.enemyAttackRange;
        minTimeBetweenAttacks = enemyData.enemyMinTimeBetweenAttacks;
        enemyHealth = enemyData.enemyHealth;
        enemyStamina = enemyData.enemyStamina;
        enemyBattleCryRange = enemyData.enemyBattleCryRange;
        acquireDependencies();
    }

    IEnumerator Throw()
    {
        canThrow = false;
        enemySpineAnimator.Play("Idle");
        yield return new WaitForSeconds(timeBetweenShoots);
        //Play attack animation;
        enemySpineAnimator.Play("attacking");
        GameObject throwRow = Instantiate(row, projectileSpawnPosition.position, Quaternion.identity);
        throwRow.transform.position = projectileSpawnPosition.position;
        throwRow.transform.rotation = transform.rotation;

        int dir;
        if (transform.position.x > playerTransform.position.x)
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
        randomNumber = Random.Range(1, 5);
        canThrow = true;
    }

    public override void handleAttackPlayerAnimation()
    {
        if (isGrounded)
            rb2d.velocity = Vector3.zero;
        if (canThrow)
        {
            StartCoroutine(Throw());
        }

        if (randomNumber == 3)
        {
            canAttackPlayer = false;
            enemySpineAnimator.Play("BattleCry");
        }
    }

    public override void Reposition(Vector2 tar)
    {
        if (tar.y > transform.position.y)
        {
            Vector2 RepoStart = enemyClosestRepositionStartPoint();
            // Debug.Log(RepoStart.x+ " "+ RepoStart.y);
            if (Mathf.Abs(transform.position.x - RepoStart.x) > 0.05f)
            {
                if (!isReadyToClimb)
                {
                    towardsRepositionPoint(RepoStart);
                }
            }
            else
            {
                isReadyToClimb = true;
            }

            if (isReadyToClimb)
            {
                // transform.position = tar;
                rb2d.gravityScale = 0f;
                towardsFinalTarget(tar);
            }
            float dist = Mathf.Abs(transform.position.y - tar.y);
            if (dist < 0.05f)
            {
                isRepositioning = true;
                rb2d.gravityScale = 1f;
                isReadyToClimb = false;
            }
        }
        else
        {
            startPos = transform.position;
            rb2d.gravityScale = 0f;
            tar.y += 1f;
            calculateVelocity(tar);
        }
    }

    void towardsFinalTarget(Vector2 tar)
    {
        enemySpineAnimator.Play("repositionUp");
        transform.position = Vector2.MoveTowards(transform.position, tar, 1f * Time.fixedDeltaTime);
    }

    void towardsRepositionPoint(Vector2 tar)
    {
        enemySpineAnimator.Play("Patrolling animation");
        transform.position = Vector2.MoveTowards(transform.position, tar, enemyCurrentMovementSpeed / 50f * Time.fixedDeltaTime);
    }

    void calculateVelocity(Vector2 tar)
    {
        dist = tar.x - startPos.x;
        nextX = Mathf.MoveTowards(transform.position.x, tar.x, 5f * Time.fixedDeltaTime);
        baseY = Mathf.Lerp(startPos.y, tar.y, (nextX - startPos.x) / dist);
        height = 1f * (nextX - startPos.x) * (nextX - tar.x) / (-.25f * dist * dist);

        Vector3 movePosition = new Vector3(nextX, baseY + height, transform.position.z);
        transform.position = movePosition;

        if (tar.x == transform.position.x && tar.y == transform.position.y)
        {
            if (!isGrounded)
            {
                rb2d.gravityScale = 20f;
            }
            else
            {
                isRepositioning = true;
                rb2d.gravityScale = 1f;
            }

        }
    }
}