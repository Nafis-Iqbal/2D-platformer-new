using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class EnemyFootSoldier : EnemyBase
{
    private float dist;
    private float nextX;
    private float baseY;
    private float height;
    private Vector3 startPos;
    float currentTime;
    private EnemyClassInfo enemyData;

    [Header("MELEE ENEMY")]
    private EnemyAttackInfo[] enemyAttacks = new EnemyAttackInfo[2];
    float lastToFroCheckTime;

    public override void OnEnable()
    {
        base.OnEnable();//Dependencies acquired here, common initialization done

        isPatrolling = true;
        canAttackPlayer = true;
        blockingStanceVelocityUpdated = false;

        enemyData = EnemyManager.Instance.enemyData[enemyID];

        canBlock = enemyData.canBlock;
        defensiveStanceHealthLimit = enemyData.defensiveStanceHealthLimit;

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
        toAndFroMoveFrequency = enemyData.toAndFroMoveFrequency;

        canChangePlatform = enemyData.canChangePlatforms;
        groundBasedEnemy = enemyData.groundBased;
        movesAwayAfterAttack = enemyData.movesAwayAfterAttack;
        changesDirectionDuringAttack = enemyData.changesDirectionDuringAttack;

        enemyBattleCryRange = enemyData.enemyBattleCryRange;
        battleCryMinimumInterval = enemyData.battleCryFrequency;
        playerContactAvoidRange = enemyData.playerAvoidRange;

        enemyAttacks = enemyData.enemyAttacks;

        lastToFroCheckTime = Time.time;
        currentAttackID = -1;
    }

    public override void HandleRepositionAnimation()
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

    public override void HandleRepositionMovement(Vector2 targetPlayerPosition)
    {
        if (canMove == false) return;

        if (isFleeingFromPlayer)
        {
            characterMovementDirection.x = getMoveAwayFromPlayerDirection();
        }
        else characterMovementDirection.x = getMoveTowardsPlayerDirection();

        rb2d.velocity = new Vector2(enemyCurrentMovementSpeed * characterMovementDirection.x, rb2d.velocity.y);
    }

    public override void HandleAttackPlayerAnimation()
    {
        if (!isDefensive && canBlock)
        {
            if (characterHealthStaminaScript.currentHealth <= defensiveStanceHealthLimit)
            {
                isDefensive = true;
            }
        }

        if (isDefensive)
        {
            //Debug.Log("Trying to enter");
            if ((isPlayerFacingCharacter() == true && playerCombatSystemScript.usingCombatItem == true && doingAttackMove == false) || projectileDetectorScript.projectileInRange)
            {
                isCharacterBlocking = true;
                closeInForAttack = false;
                enemySpineAnimator.SetBool("Defence", true);

                if (blockingStanceVelocityUpdated == false)
                {
                    rb2d.velocity = Vector2.zero;
                    blockingStanceVelocityUpdated = true;
                }
            }
            else
            {
                isCharacterBlocking = false;
                blockingStanceVelocityUpdated = false;
                enemySpineAnimator.SetBool("Defence", false);
            }
        }

        if (isCharacterBlocking) return;

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

                if (enemyAttacks[currentAttackID].lastAttackUsedTime > 0.0f)//Means the attack has been used atleast once
                {
                    if ((currentTime - enemyAttacks[currentAttackID].lastAttackUsedTime) < enemyAttacks[currentAttackID].attackMinCooldownTime) currentAttackID = 1;//Generate random number within range here;
                }
            }

            if (currentAttackID > 0)
            {
                enemySpineAnimator.SetBool("BackWalk", false);
                if (playerDistanceFromEnemy > enemyAttacks[currentAttackID].attackRange)//Reduce distance to player immediately and attack
                {
                    closeInForAttack = true;
                    lastToFroCheckTime = Time.time;
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
                        //currentAttackID = -1;
                    }
                    else
                    {
                        performCustomAttack(enemyAttacks[currentAttackID].attackName);
                    }
                }
            }
        }
        else if (doingAttackMove == false)//Not in attack animation
        {
            if (currentTime - lastToFroCheckTime > toAndFroMoveFrequency)
            {
                lastToFroCheckTime = currentTime;

                if (moveAwayAfterAttack == true)
                {
                    moveAwayAfterAttack = false;
                    assignCharacterMovementDirection(0);
                }
                else if (hasToAndFroMovement && playerInsideCombatMovementRange() == true)
                {
                    toAndFroRandomNumber = Random.Range(0, 2);//random number between 0 and 1, 0 means backwalk, 1 means normal walk
                    if (toAndFroRandomNumber == 1) assignCharacterMovementDirection(1);
                    else assignCharacterMovementDirection(0);
                }
                else
                {
                    assignCharacterMovementDirection(1);
                }
            }
        }
    }

    public override void HandleAttackPlayerMovement()//MODIFY METHODS TO INTEGRATE MECANIM ANIMATIONS
    {
        if (!canMove || doingAttackMove) return;

        if (isCharacterBlocking)
        {
            rb2d.velocity = Vector2.zero;
            return;
        }

        enemyCurrentMovementSpeed = enemyClassSpeed * enemyCombatWalkSpeedMultiplier;

        if (closeInForAttack == true)
        {
            characterMovementDirection.x = getMoveTowardsPlayerDirection();
        }
        else if (toAndFroRandomNumber == 0)
        {
            characterMovementDirection.x = getMoveAwayFromPlayerDirection();
        }

        //Distance between player and enemy after curent frame calculation
        if (characterFacingRight == true)
        {
            projectedDistanceFromPlayer = horizontalDistanceFromPlayer() - (Time.deltaTime * enemyCurrentMovementSpeed * characterMovementDirection.x);
        }
        else projectedDistanceFromPlayer = horizontalDistanceFromPlayer() + (Time.deltaTime * enemyCurrentMovementSpeed * characterMovementDirection.x);

        if (projectedDistanceFromPlayer > playerContactAvoidRange && projectedDistanceFromPlayer < enemyTrackingRange)
        {
            rb2d.velocity = new Vector2(enemyCurrentMovementSpeed * characterMovementDirection.x, rb2d.velocity.y);
        }
        else
        {
            rb2d.velocity = Vector2.zero;
            enemySpineAnimator.SetInteger("MoveSpeed", 0);
        }
    }

    void HandleCharacterDefense()
    {

    }

    void assignCharacterMovementDirection(int toAndFroInd)
    {
        if (toAndFroInd == 0)
        {
            enemySpineAnimator.SetBool("BackWalk", true);
            characterMovementDirection.x = getMoveAwayFromPlayerDirection();
        }
        else
        {
            enemySpineAnimator.SetBool("BackWalk", false);
            characterMovementDirection.x = getMoveTowardsPlayerDirection();
        }
    }
    void performCustomAttack(string attackName)
    {

    }

    #region Platform Switching
    public override void ChangePlatforms(Vector2 targetPosition)
    {
        if (targetPosition.y > transform.position.y)
        {
            Vector2 RepositionStartPoint = enemyClosestRepositionStartPoint();
            if (Mathf.Abs(transform.position.x - RepositionStartPoint.x) > 0.05f)
            {
                if (!isReadyToClimb)
                {
                    towardsRepositionPoint(RepositionStartPoint);
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
                towardsFinalTarget(targetPosition);
            }
            float dist = Mathf.Abs(transform.position.y - targetPosition.y);
            if (dist < 0.05f)
            {
                isRepositioning = false;
                rb2d.gravityScale = 1f;
                isReadyToClimb = false;
            }
        }
        else
        {
            startPos = transform.position;
            rb2d.gravityScale = 0f;
            targetPosition.y += 1f;
            calculateVelocity(targetPosition);
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
        ParticleController.instance.moveEnemyParti(true);
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
                isRepositioning = false;
                rb2d.gravityScale = 1f;
            }
        }
    }
    #endregion
}
