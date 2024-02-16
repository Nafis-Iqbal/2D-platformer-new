using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class EnemyMelee : EnemyBase
{
    private float dist;
    private float nextX;
    private float baseY;
    private float height;
    private Vector3 startPos;
    public int currentAttackID;
    float currentTime;
    private EnemyClassInfo enemyData;

    [Header("MELEE ENEMY")]
    public int enemyID;
    public float toAndFroMoveFrequency;
    public EnemyAttackInfo[] enemyAttacks = new EnemyAttackInfo[2];
    float lastToFroCheckTime;
    float projectedDistanceFromPlayer;
    public int toAndFroRandomNumber;
    public float playerContactAvoidRange = .4f;

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
        changesDirectioDuringAttack = enemyData.changesDirectioDuringAttack;

        enemyHealth = enemyData.enemyHealth;
        enemyStamina = enemyData.enemyStamina;
        enemyBattleCryRange = enemyData.enemyBattleCryRange;
        playerContactAvoidRange = enemyData.playerAvoidRange;

        lastToFroCheckTime = Time.time;
        currentAttackID = -1;
    }

    public override void Reposition(Vector2 targetPlayerPosition)
    {
        if (targetPlayerPosition.x < transform.position.x)
        {
            //GO LEFT
            characterMovementDirection.x = -1.0f;
        }
        else
        {
            characterMovementDirection.x = 1.0f;
            //GO RIGHT
        }
        rb2d.velocity = new Vector2(enemyCurrentMovementSpeed * characterMovementDirection.x, rb2d.velocity.y);
    }

    public override void handleAttackPlayerAnimation()
    {
        currentTime = Time.time;
        playerDistanceFromEnemy = horizontalDistanceFromPlayer();
        enemySpineAnimator.SetInteger("MoveSpeed", 1);

        //canAttack variable changed from animation
        if (currentTime - lastAttackTime > minTimeBetweenAttacks && doingAttackMove == false)
        {
            if (currentAttackID < 0)
            {
                currentAttackID = Random.Range(0, 2);//randomly generate
                if (currentTime - lastAttackTime > enemyAttacks[currentAttackID].attackMinCooldownTime) currentAttackID = 1;//Generate random number within range here;
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
                    doingAttackMove = true;
                    if (enemyAttacks[currentAttackID].isCustomAttack == false)
                    {
                        rb2d.velocity = Vector3.zero;

                        lastAttackTime = Time.time;
                        enemyAttacks[currentAttackID].useAttack();
                        enemySpineAnimator.SetTrigger("Attack");
                        enemySpineAnimator.SetInteger("AttackID", currentAttackID);
                        currentAttackID = -1;
                        closeInForAttack = false;
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
                else if (hasToAndFroMovement)
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

    public override void handleAttackPlayerMovement()//MODIFY METHODS TO INTEGRATE MECANIM ANIMATIONS
    {
        if (!canMove || doingAttackMove) return;

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

        if (projectedDistanceFromPlayer > playerContactAvoidRange)
        {
            rb2d.velocity = new Vector2(enemyCurrentMovementSpeed * characterMovementDirection.x, rb2d.velocity.y);
        }
    }

    void performCustomAttack(string attackName)
    {

    }

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
        particleController.instance.moveEnemyParti(true);
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
}
