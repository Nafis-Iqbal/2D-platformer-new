using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class EnemySpearShield : EnemyBase
{
    private float dist;
    private float nextX;
    private float baseY;
    private float height;
    private Vector3 startPos;
    float currentTime;

    [Header("SPEAR SHIELD ENEMY")]
    public bool isChragingTowardsPlayer;
    public float chargingVelocity;

    public override void OnEnable()
    {
        base.OnEnable();//Dependencies acquired here, common initialization done
    }

    public override void HandleRepositionAnimation()
    {
        if (!isTurningTowardsPlayer && !isCharacterFacingPlayer())
        {
            isTurningTowardsPlayer = true;
            faceTowardsPlayer();
        }

        if (hasToAndFroMovement)
        {
            if (isFleeingFromPlayer)
            {
                enemySpineAnimator.SetInteger("MoveSpeed", 1);

                if (hasBackWalkAnimationStates) enemySpineAnimator.SetBool("BackWalk", true);
                else faceAwayFromPlayer();
            }
            else
            {
                enemySpineAnimator.SetInteger("MoveSpeed", 2);

                if (hasBackWalkAnimationStates) enemySpineAnimator.SetBool("BackWalk", false);
                else faceTowardsPlayer();
            }
        }

        if (enemyAlreadyAlerted == false)
        {
            enemyAlreadyAlerted = true;
            canMove = false;
            enemySpineAnimator.SetBool("PatrolMode", false);
        }
    }

    public override void HandleRepositionMovement(Vector2 targetPlayerPosition)
    {
        if (canMove == false || doingAttackMove == true) return;

        if (isFleeingFromPlayer)
        {
            characterMovementDirection.x = getMoveAwayFromPlayerDirection();
            enemyCurrentMovementSpeed = enemyClassSpeed * enemyCombatWalkSpeedMultiplier;
        }
        else
        {
            characterMovementDirection.x = getMoveTowardsPlayerDirection();
            enemyCurrentMovementSpeed = enemyClassSpeed * enemyCombatRunSpeedMultiplier;
        }

        rb2d.velocity = new Vector2(enemyCurrentMovementSpeed * characterMovementDirection.x, rb2d.velocity.y);
    }

    public override void HandleAttackPlayerAnimation()
    {
        currentTime = Time.time;
        playerDistanceFromEnemy = horizontalDistanceFromPlayer();

        if ((doingAttackMove == true && changesDirectionDuringAttack) || !doingAttackMove)//face player if not in attack animation
        {
            Debug.Log("Howw???");
            if (!isTurningTowardsPlayer && !isCharacterFacingPlayer())
            {
                isTurningTowardsPlayer = true;
                faceTowardsPlayer();
            }
        }
        else if (doingAttackMove == true) return;

        if (currentTime - lastAttackTime > minTimeBetweenAttacks && !doingAttackMove)
        {
            //CHOOSE ATTACK BASED ON PROBABILITY
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
                if (hasBackWalkAnimationStates) enemySpineAnimator.SetBool("BackWalk", false);
                else faceTowardsPlayer();

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
                        currentAttackID = 3;
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
        else//Tracking Player Movement
        {
            if (currentTime - lastToFroCheckTime > toAndFroMoveFrequency)
            {
                lastToFroCheckTime = currentTime;
                //Debug.Log("h2");
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
                else if (hasToAndFroMovement == false)
                {
                    if (playerDistanceFromEnemy < playerContactAvoidRange + .15f) assignCharacterMovementDirection(0);
                    else if (playerDistanceFromEnemy > enemyTrackingRange - .15f) assignCharacterMovementDirection(1);
                    else assignCharacterMovementDirection(0);
                }
            }
        }
    }

    public override void HandleAttackPlayerMovement()//MODIFY METHODS TO INTEGRATE MECANIM ANIMATIONS
    {
        if (!canMove || (doingAttackMove && !isChragingTowardsPlayer) || isCharacterBlocking)
        {
            rb2d.velocity = Vector2.zero;
            return;
        }

        if (isChragingTowardsPlayer)
        {
            if (characterFacingRight)
            {
                rb2d.velocity = new Vector2(chargingVelocity, 0.0f);
            }
            else
            {
                rb2d.velocity = new Vector2(-chargingVelocity, 0.0f);
            }
            return;
        }

        enemyCurrentMovementSpeed = enemyClassSpeed * enemyCombatWalkSpeedMultiplier;

        if (closeInForAttack == true)
        {
            characterMovementDirection.x = getMoveTowardsPlayerDirection();
        }

        //Distance between player and enemy after curent frame calculation
        if (characterFacingRight == true)
        {
            projectedDistanceFromPlayer = horizontalDistanceFromPlayer() - (Time.deltaTime * enemyCurrentMovementSpeed * characterMovementDirection.x);
        }
        else projectedDistanceFromPlayer = horizontalDistanceFromPlayer() + (Time.deltaTime * enemyCurrentMovementSpeed * characterMovementDirection.x);

        if (projectedDistanceFromPlayer > playerContactAvoidRange && projectedDistanceFromPlayer < enemyTrackingRange)
        {
            enemySpineAnimator.SetInteger("MoveSpeed", 1);
            rb2d.velocity = new Vector2(enemyCurrentMovementSpeed * characterMovementDirection.x, rb2d.velocity.y);
        }
        else
        {
            enemySpineAnimator.SetInteger("MoveSpeed", 0);
            rb2d.velocity = Vector2.zero;
        }
    }

    void assignCharacterMovementDirection(int toAndFroInd)
    {
        if (toAndFroInd == 0)
        {
            if (hasBackWalkAnimationStates) enemySpineAnimator.SetBool("BackWalk", true);
            else faceAwayFromPlayer();

            characterMovementDirection.x = getMoveAwayFromPlayerDirection();
        }
        else
        {
            if (hasBackWalkAnimationStates) enemySpineAnimator.SetBool("BackWalk", false);
            else faceTowardsPlayer();

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
