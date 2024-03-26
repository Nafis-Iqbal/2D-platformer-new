using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArcher : EnemyBase
{
    private float dist;
    private float nextX;
    private float baseY;
    private float height;
    private Vector3 startPos;
    [Header("ENEMY RANGED")]
    public Transform[] projectileSpawnPosition = new Transform[3];
    private EnemyAttackInfo[] enemyAttacks = new EnemyAttackInfo[3];
    public float arcShotFlightTime, powerShotFlightTime, normalShotFlightTime;
    public float arcShotTopHeight, powerShotTopHeight, normalShotTopHeight;
    private EnemyClassInfo enemyData;
    float currentTime;

    public override void OnEnable()
    {
        base.OnEnable();//Dependencies acquired here, common initialization done

        isPatrolling = true;
        canAttackPlayer = true;

        enemyData = EnemyManager.Instance.enemyData[enemyID];

        enemyClassSpeed = enemyData.enemyMovementSpeed;
        enemyPatrolSpeedMultiplier = enemyData.enemyPatrolSpeedMultiplier;
        enemyCombatRunSpeedMultiplier = enemyData.enemyCombatRunSpeedMultiplier;
        enemyCombatWalkSpeedMultiplier = enemyData.enemyCombatWalkSpeedMultiplier;

        playerHorizontalDetectionDistance = enemyData.enemyHorizontalDetectionDistance;
        playerVerticalDetectionLevels = enemyData.enemyVerticalDetectionLevels;
        enemyTrackingRange = enemyData.enemyTrackingRange;
        enemyAttackRange = enemyData.enemyAttackRange;
        minTimeBetweenAttacks = enemyData.enemyMinTimeBetweenAttacks;
        hasToAndFroMovement = enemyData.hasToAndFroMovement;

        canChangePlatform = enemyData.canChangePlatforms;
        groundBasedEnemy = enemyData.groundBased;
        movesAwayAfterAttack = enemyData.movesAwayAfterAttack;
        changesDirectionDuringAttack = enemyData.changesDirectionDuringAttack;

        enemyBattleCryRange = enemyData.enemyBattleCryRange;
        playerContactAvoidRange = enemyData.playerAvoidRange;

        enemyAttacks = enemyData.enemyAttacks;

        currentAttackID = -1;
    }

    public override void HandleRepositionAnimation()
    {
        if (!isFleeingFromPlayer)
        {
            faceTowardsPlayer();
            enemySpineAnimator.SetInteger("MoveSpeed", 2);
            if (hasToAndFroMovement) enemySpineAnimator.SetBool("BackWalk", false);

            if (enemyAlreadyAlerted == false)
            {
                enemyAlreadyAlerted = true;
                canMove = false;
                enemySpineAnimator.SetBool("PatrolMode", false);
            }
        }
        else
        {
            faceAwayFromPlayer();
            enemySpineAnimator.SetInteger("MoveSpeed", 2);
            if (hasToAndFroMovement) enemySpineAnimator.SetBool("BackWalk", false);
        }
    }

    public override void HandleRepositionMovement(Vector2 targetPlayerPosition)
    {
        //NEEDS CHANGES
        if (isFleeingFromPlayer)
        {
            characterMovementDirection.x = getMoveAwayFromPlayerDirection();
        }
        else characterMovementDirection.x = getMoveTowardsPlayerDirection();

        rb2d.velocity = new Vector2(enemyCurrentMovementSpeed * characterMovementDirection.x, rb2d.velocity.y);
    }

    public override void HandleAttackPlayerAnimation()
    {
        currentTime = Time.time;
        playerDistanceFromEnemy = horizontalDistanceFromPlayer();
        enemySpineAnimator.SetInteger("MoveSpeed", 1);

        if (doingAttackMove == true && changesDirectionDuringAttack) faceTowardsPlayer();
        else if (doingAttackMove == true) return;
        if (!doingAttackMove) faceTowardsPlayer();//face player if not in attack animation

        //canAttack variable changed from animation
        if (currentTime - lastAttackTime > minTimeBetweenAttacks && !doingAttackMove)
        {
            if (currentAttackID < 0)
            {
                currentAttackID = Random.Range(1, enemyAttacks.Length);//randomly generate
                Debug.Log("random attack id: " + currentAttackID);
                if (currentTime - lastAttackTime > enemyAttacks[currentAttackID].attackMinCooldownTime) currentAttackID = 1;//Generate random number within range here;
            }

            if (currentAttackID > 0)
            {
                if (playerDistanceFromEnemy > enemyAttacks[currentAttackID].attackRange)//Reduce distance to player immediately and attack
                {
                    closeInForAttack = true;
                    enemySpineAnimator.SetInteger("MoveSpeed", 1);
                }
                else
                {
                    closeInForAttack = false;
                    if (enemyAttacks[currentAttackID].isCustomAttack == false)
                    {
                        rb2d.velocity = Vector3.zero;

                        lastAttackTime = Time.time;
                        enemyAttacks[currentAttackID].useAttack();
                        enemySpineAnimator.SetTrigger("Attack");
                        enemySpineAnimator.SetInteger("AttackID", currentAttackID);
                        enemySpineAnimator.SetInteger("MoveSpeed", 0);
                        //currentAttackID = -1;
                    }
                    else
                    {
                        performCustomAttack(enemyAttacks[currentAttackID].attackName);
                    }
                }
            }
        }
        else if (doingAttackMove == false && closeInForAttack == false)
        {
            enemySpineAnimator.SetInteger("MoveSpeed", 0);
        }
    }

    public override void HandleAttackPlayerMovement()//MODIFY METHODS TO INTEGRATE MECANIM ANIMATIONS
    {
        if (doingAttackMove) return;

        enemyCurrentMovementSpeed = enemyClassSpeed * enemyCombatWalkSpeedMultiplier;
        playerDistanceFromEnemy = horizontalDistanceFromPlayer();

        if (closeInForAttack) characterMovementDirection.x = getMoveTowardsPlayerDirection();
        else characterMovementDirection.x = 0.0f;

        //Distance between player and enemy after curent frame calculation
        if (characterFacingRight == true)
        {
            projectedDistanceFromPlayer = playerDistanceFromEnemy - (Time.deltaTime * enemyCurrentMovementSpeed * characterMovementDirection.x);
        }
        else projectedDistanceFromPlayer = playerDistanceFromEnemy + (Time.deltaTime * enemyCurrentMovementSpeed * characterMovementDirection.x);

        if (projectedDistanceFromPlayer > playerContactAvoidRange)
        {
            rb2d.velocity = new Vector2(enemyCurrentMovementSpeed * characterMovementDirection.x, rb2d.velocity.y);
        }
        else
        {
            rb2d.velocity = Vector2.zero;
        }
    }

    public void performArrowShot()
    {
        var arrowObject = ObjectPooler.Instance.SpawnFromPool("Arrow", projectileSpawnPosition[0].position, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        arrowObject.GetComponent<ArcProjectile>().initializeProjectile(projectileSpawnPosition[0].position, playerTransform.position, normalShotTopHeight, normalShotFlightTime);
        arrowObject.SetActive(true);
    }

    public void performPowerShot()
    {
        var arrowObject = ObjectPooler.Instance.SpawnFromPool("Arrow", projectileSpawnPosition[0].position, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        arrowObject.GetComponent<ArcProjectile>().initializeProjectile(projectileSpawnPosition[0].position, playerTransform.position, powerShotTopHeight, powerShotFlightTime);
        arrowObject.SetActive(true);
    }

    public void performArcShot()
    {
        var arrowObject = ObjectPooler.Instance.SpawnFromPool("Arrow", projectileSpawnPosition[0].position, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        arrowObject.GetComponent<ArcProjectile>().initializeProjectile(projectileSpawnPosition[0].position, playerTransform.position, arcShotTopHeight, arcShotFlightTime);
        arrowObject.SetActive(true);
    }

    void performCustomAttack(string attackName)
    {

    }
    #region Platform Switching
    public override void ChangePlatforms(Vector2 tar)
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
    #endregion
}