using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Microsoft.Unity.VisualStudio.Editor;

public class EnemyBase : MonoBehaviour
{
    public int enemyID;
    [Header("DRAG AND DROPS")]
    public Animator enemySpineAnimator;
    public GameObject colliderObject;
    protected Rigidbody2D rb2d;
    protected PlayerCombatSystem playerCombatSystemScript;
    protected PlayerMovement playerMovementScript;
    [SerializeField]
    protected HealthStaminaSystem characterHealthStaminaScript;
    [SerializeField]
    protected ProjectileDetector projectileDetectorScript;
    public Transform playerTransform;
    protected Rigidbody2D playerRigidBody;
    public GameObject meleeWeaponObject;
    public GameObject checkHeightObject1, checkHeightObject2;
    public LayerMask environmentMask;
    public LayerMask repositionLayer;

    [Header("MOVEMENT SYSTEM")]
    public float minLandHeight;
    protected float enemyClassSpeed;
    protected Vector2 characterMovementDirection = Vector2.zero;
    protected float enemyCurrentMovementSpeed = 100f;
    //protected float projectedDistanceFromPlayer;
    public float projectedDistanceFromPlayer;
    //protected float playerContactAvoidRange;
    public float playerContactAvoidRange;
    protected float enemyPatrolSpeedMultiplier, enemyCombatWalkSpeedMultiplier, enemyCombatRunSpeedMultiplier;
    //JUMP
    public float nextWayPointDistance = 3f;
    public float jumpNodeHeightRequirement = .8f;
    public float jumpModifier = .3f;//What is this?
    public float jumpCheckOffset = .1f;//What is this?

    [Header("PATROLLING SYSTEM")]
    public float patrolDirectionalDuration;
    float lastPatrolTurnTime;
    bool turnCharacterNow;
    public bool enemyAlreadyAlerted;

    [Header("ENEMY MOVEMENT STATES")]
    public bool canMove = true;
    public bool canBlock;
    public bool initialCharacterFacingRight;
    public bool characterFacingRight;
    public bool isGrounded;
    public bool isPatrolling;//false means combat mode
    public bool isAlerted;
    public bool isChangingCombatMode;
    public bool isRepositioning;
    bool cachedIsRepositioning;
    public bool isFleeingFromPlayer;
    public bool isChangingPlatform;
    public bool isReadyToClimb;
    [Header("ENEMY COMBAT STATES")]
    public bool isDead;
    public bool canAttackPlayer = true;
    public bool isAttacking;//in close combat range and performing attacks
    bool cachedIsAttacking;
    public bool isDefensive = false;
    public bool doingAttackMove;
    public bool closeInForAttack;
    public bool moveAwayAfterAttack;
    public bool isAIRoutineInterrupted;
    public bool isCharacterBlocking;
    public bool isPoiseBroken, isStunned, isKnockedOffGround;
    public bool weaponInLethalState;
    public bool playHitEffectOnScale = false;

    [Header("PLAYER TARGETTING AND REPOSITIONING")]
    //protected float playerDistanceFromEnemy;
    public float turnToPlayerTime;
    [SerializeField]
    protected bool isTurningTowardsPlayer;
    public float playerDistanceFromEnemy;
    public bool groundBasedEnemy;
    public bool canChangePlatform;
    int enemyPlatformLevel, playerPlatformLevel;
    public int enemyPlatformID, playerPlatformID;
    public bool targetInHorizontalRange = false;
    public bool targetInVerticalRange = false;
    public float playerHorizontalDetectionDistance;
    public int playerVerticalDetectionLevels;
    public float minimumDistanceForReposition = 50f;

    [Header("COMBAT ANIMATION SYSTEM")]
    public float charScaleShrinkSpeed = 60.0f;
    public float shrinkSizeMultiplier = .85f;
    float initialCharacterYScale;
    bool charScaleDecreasing;
    public int currentAttackID;
    public float minTimeBetweenAttacks;
    protected float lastAttackTime;
    public float baseMomentumForce;

    protected float enemyBattleCryRange;
    public float enemyTrackingRange;
    public float enemyAttackRange;

    public bool hasToAndFroMovement;
    protected float toAndFroMoveFrequency;
    public int toAndFroRandomNumber;
    public int comboAttackID;

    float deathTime;
    public float timeBeforeDisable = 3.5f;

    protected EnemyClassInfo enemyData;
    protected EnemyAttackInfo[] enemyAttacks = new EnemyAttackInfo[2];
    protected float lastToFroCheckTime;

    [Header("CUSTOM BEHAVIOUR")]
    public bool followEnabled = false;
    public bool jumpEnabled = true;
    public bool lookAtPlayerEnabled = true;
    public bool hasComboAttack = false;
    public bool hasAutoBlock = false;
    public bool hasBackWalkAnimationStates = false;
    public bool hasAlertStates;
    public bool canRestartPatrol;
    public bool movesAwayAfterAttack;
    public bool changesDirectionDuringAttack;

    [Header("A* PATHFINDING")]
    public float pathUpdateSeconds = .5f;//What is this??
    private Path pathAiToFollow;
    Seeker seeker;
    private int currentWayPoint = 0;//What is this??

    [Header("CHANGING PLATFORM")]
    public Vector2 playerPlatformRightEndPosition;
    public Vector2 playerPlatformLeftEndPosition;
    Vector2 platformLeftEndBox, platformRightEndBox;//Target Platform Reposition Point

    [Header("BATTLE CRY")]
    public bool triggeredBattleCry = false;
    public float battleCryMinimumInterval = 5.0f;
    protected bool hasBattleCry;
    float lastBattleCryTime;
    public bool battleCryInProgress;
    float playerMoveDirectionCheckTime;
    int currentPlayerMoveDirection, previousPlayerMoveDirection, finalPlayerMoveDirection;
    Vector2 tempVelocityVector;

    [Header("BLOCKING & DEFENSIVE STANCE")]
    public float defensiveStanceHealthLimit;
    public bool hitByProjectile;
    protected float hitByProjectileTime;
    protected bool blockingStanceVelocityUpdated;

    public virtual void OnEnable()
    {
        Physics2D.IgnoreCollision(transform.GetComponent<Collider2D>(), colliderObject.GetComponent<Collider2D>());

        //SYSTEM SETUP
        acquireDependencies();
        resetAnimationsSystem();

        playerPlatformID = GameManager.Instance.playerCurrentPlatformID;
        playerPlatformLevel = GameManager.Instance.playerCurrentPlatformLevel;

        isDead = false;
        enemyAlreadyAlerted = false;
        lastAttackTime = Time.time;
        doingAttackMove = false;

        isTurningTowardsPlayer = false;
        isDefensive = false;
        hitByProjectile = false;
        hitByProjectileTime = Time.time;
        isCharacterBlocking = false;
        closeInForAttack = false;
        isReadyToClimb = false;
        isAIRoutineInterrupted = false;
        isGrounded = true;
        isPoiseBroken = isStunned = isKnockedOffGround = false;
        isFleeingFromPlayer = false;
        isDead = false;
        weaponInLethalState = false;
        charScaleDecreasing = false;
        playHitEffectOnScale = false;
        initialCharacterYScale = transform.localScale.y;
        previousPlayerMoveDirection = 0;

        isPatrolling = true;
        canAttackPlayer = true;
        blockingStanceVelocityUpdated = false;

        //GATHER ENEMY UNIQUE INFO
        enemyData = EnemyManager.Instance.enemyData[enemyID];

        canBlock = enemyData.canBlock;
        defensiveStanceHealthLimit = enemyData.defensiveStanceHealthLimit;

        enemyClassSpeed = enemyData.enemyMovementSpeed;
        enemyPatrolSpeedMultiplier = enemyData.enemyPatrolSpeedMultiplier;
        enemyCombatRunSpeedMultiplier = enemyData.enemyCombatRunSpeedMultiplier;
        enemyCombatWalkSpeedMultiplier = enemyData.enemyCombatWalkSpeedMultiplier;

        hasAlertStates = enemyData.hasAlertStates;
        canRestartPatrol = enemyData.canRestartPatrol;
        playerHorizontalDetectionDistance = enemyData.enemyHorizontalDetectionDistance;
        playerVerticalDetectionLevels = enemyData.enemyVerticalDetectionLevels;
        enemyTrackingRange = enemyData.enemyTrackingRange;
        enemyAttackRange = enemyData.enemyAttackRange;
        minTimeBetweenAttacks = enemyData.enemyMinTimeBetweenAttacks;
        hasToAndFroMovement = enemyData.hasToAndFroMovement;
        hasBackWalkAnimationStates = enemyData.hasBackWalkAnimationStates;
        toAndFroMoveFrequency = enemyData.toAndFroMoveFrequency;

        canChangePlatform = enemyData.canChangePlatforms;
        groundBasedEnemy = enemyData.groundBased;
        movesAwayAfterAttack = enemyData.movesAwayAfterAttack;
        turnToPlayerTime = enemyData.turnToPlayerTime;
        changesDirectionDuringAttack = enemyData.changesDirectionDuringAttack;

        hasBattleCry = enemyData.hasBattleCry;
        enemyBattleCryRange = enemyData.enemyBattleCryRange;
        battleCryMinimumInterval = enemyData.battleCryFrequency;
        playerContactAvoidRange = enemyData.playerAvoidRange;

        enemyAttacks = enemyData.enemyAttacks;

        lastToFroCheckTime = Time.time;
        currentAttackID = -1;
    }

    public void acquireDependencies()
    {
        isRepositioning = false;

        rb2d = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
        characterHealthStaminaScript = GetComponent<HealthStaminaSystem>();

        //InvokeRepeating("updatePath", 0f, pathUpdateSeconds);

        playerTransform = GameManager.Instance.playerTransform;
        playerRigidBody = playerTransform.GetComponent<Rigidbody2D>();
        playerCombatSystemScript = playerTransform.GetComponent<PlayerCombatSystem>();
        playerMovementScript = playerTransform.GetComponent<PlayerMovement>();
    }

    public void resetAnimationsSystem()
    {
        characterFacingRight = initialCharacterFacingRight;

        triggeredBattleCry = false;
        isAlerted = false;
        lastBattleCryTime = Time.time;

        if (characterFacingRight) characterMovementDirection = Vector2.right;
        else characterMovementDirection = Vector2.left;
    }

    private void getPlayerPlatformInfo()
    {
        playerPlatformID = GameManager.Instance.playerCurrentPlatformID;
        playerPlatformLevel = GameManager.Instance.playerCurrentPlatformLevel;
    }

    public void checkPlayerPositionUpdateData()
    {
        if (playerInCombatRange())
        {
            if (groundBasedEnemy == true)
            {
                playerDistanceFromEnemy = horizontalDistanceFromPlayer();

                if (targetInHorizontalRange)//Start pursuing Player
                {
                    if (battleCryInProgress == true)
                    {
                        if (!isTurningTowardsPlayer)
                        {
                            isTurningTowardsPlayer = true;
                            faceTowardsPlayer();
                        }

                        isFleeingFromPlayer = false;
                        isRepositioning = false;
                        isAttacking = false;
                        isPatrolling = false;
                    }
                    else if (isPatrolling == true)
                    {
                        isPatrolling = false;
                        if (hasAlertStates)
                        {
                            isAlerted = true;
                            canMove = false;
                            enemySpineAnimator.SetTrigger("Alerted");

                            if (enemyAlreadyAlerted == false)
                            {
                                enemyAlreadyAlerted = true;
                                enemySpineAnimator.SetBool("PatrolMode", false);
                            }
                        }

                        if (!isTurningTowardsPlayer)
                        {
                            isTurningTowardsPlayer = true;
                            faceTowardsPlayer();
                        }
                    }
                    else if (isAlerted == true)
                    {
                        canMove = false;
                    }
                    else if (isChangingCombatMode == true)
                    {
                        canMove = false;
                    }
                    //(MOST IMPORTANT STATE LOGIC)
                    else if (isRepositioning == false && doingAttackMove == false)//In Attacking distance
                    {
                        if (playerDistanceFromEnemy < playerContactAvoidRange)//Avoid
                        {
                            canMove = true;
                            isRepositioning = true;
                            isFleeingFromPlayer = true;
                            isAttacking = false;
                        }
                        else if (playerDistanceFromEnemy > enemyTrackingRange)//Track/Chase
                        {
                            canMove = true;
                            isRepositioning = true;
                            isFleeingFromPlayer = false;
                            isAttacking = false;
                        }
                        else if (playerDistanceFromEnemy > playerContactAvoidRange && playerDistanceFromEnemy < enemyTrackingRange)//Attack
                        {
                            canMove = true;
                            isRepositioning = false;
                            isFleeingFromPlayer = false;
                            isAttacking = true;
                        }
                    }
                    else if (isRepositioning == true)
                    {
                        if (playerDistanceFromEnemy > playerContactAvoidRange && playerDistanceFromEnemy < enemyTrackingRange)
                        {
                            isRepositioning = false;
                            isFleeingFromPlayer = false;
                            isAttacking = true;
                        }
                    }
                }
                else if (targetInVerticalRange)//Start changing Platform
                {
                    isChangingPlatform = true;
                    isPatrolling = false;

                    if (enemyAlreadyAlerted == false)
                    {
                        enemyAlreadyAlerted = true;
                        enemySpineAnimator.SetBool("PatrolMode", false);
                    }
                }
            }
        }
        else if (canRestartPatrol && doingAttackMove == false)
        {
            isPatrolling = true;

            isAlerted = false;
            enemyAlreadyAlerted = false;

            isRepositioning = false;
            isFleeingFromPlayer = false;
            isAttacking = false;
        }
        else
        {
            if (enemyAlreadyAlerted == false)
            {
                isPatrolling = true;
            }
        }
    }
    void Update()
    {
        if (isDead && Time.time - deathTime > timeBeforeDisable)
        {
            gameObject.SetActive(false);
            return;
        }

        if (playerPlatformID < 0) getPlayerPlatformInfo();

        #region Player Detection
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            return;
        }
        else
        {
            checkPlayerPositionUpdateData();
        }
        #endregion

        #region Player Movement Direction
        if (Time.time - playerMoveDirectionCheckTime > 1.5f)
        {
            tempVelocityVector = playerRigidBody.velocity;

            if (tempVelocityVector.x < -0.1f) currentPlayerMoveDirection = -1;
            else if (tempVelocityVector.x > 0.1f) currentPlayerMoveDirection = 1;
            else currentPlayerMoveDirection = 0;

            playerMoveDirectionCheckTime = Time.time;

            if (previousPlayerMoveDirection < 0 && currentPlayerMoveDirection < 0)
            {
                finalPlayerMoveDirection = -1;
            }
            else if (previousPlayerMoveDirection > 0 && currentPlayerMoveDirection > 0)
            {
                finalPlayerMoveDirection = 1;
            }
            else finalPlayerMoveDirection = 0;

            previousPlayerMoveDirection = currentPlayerMoveDirection;
        }
        #endregion

        #region Patrolling
        if (isPatrolling == true && canMove == true)
        {
            HandlePatrollingAnimation();
        }
        else if (battleCryInProgress == true)
        {
            HandleBattleCryAnimation();
        }
        #endregion
        else
        {
            //GROUND BASED ENEMY
            if (groundBasedEnemy == true)
            {
                if (isGrounded == false && isChangingPlatform == false)//ON AIR
                {
                    enemySpineAnimator.SetBool("OnAir", true);
                }
                #region Changing Platform
                if (isChangingPlatform)//Changing Platform
                {

                }
                #endregion
                #region Combat Mode
                else if (isPatrolling == false)//Combat Mode
                {
                    if (hasBattleCry == true && battleCryInProgress == false && Time.time - lastBattleCryTime > battleCryMinimumInterval)
                    {
                        if (getPlayerAttackingMovementStatus() == false && playerDistanceFromEnemy > enemyBattleCryRange && isDefensive == false)
                        {
                            triggeredBattleCry = true;
                        }
                    }

                    if (triggeredBattleCry)
                    {
                        triggeredBattleCry = false;
                        isRepositioning = false;
                        lastBattleCryTime = Time.time;

                        enemySpineAnimator.SetInteger("MoveSpeed", 0);
                        enemySpineAnimator.SetTrigger("BattleCry");
                        stopCharacterCompletely();
                    }
                    else if (isAttacking == false)//PURSUING PLAYER
                    {
                        if (canMove && isRepositioning)//Reposition For Attack, through running
                        {
                            HandleRepositionAnimation();
                        }
                    }
                    else//IN ATTACKING PLAYER MODE
                    {
                        //Process attacking or defending stance
                        HandleAttackPlayerAnimation();
                    }
                }
                #endregion
            }
            else if (groundBasedEnemy == false)
            {

            }
        }

        #region Hit Effect
        if (playHitEffectOnScale)
        {
            Vector3 tempScale = transform.localScale;

            if (charScaleDecreasing)
            {
                if ((tempScale.y - Time.deltaTime * charScaleShrinkSpeed) > initialCharacterYScale * shrinkSizeMultiplier)
                {
                    tempScale.y = tempScale.y - (Time.deltaTime * charScaleShrinkSpeed);
                }
                else
                {
                    charScaleDecreasing = false;
                    tempScale.y = initialCharacterYScale * shrinkSizeMultiplier;
                }
            }
            else
            {
                if ((tempScale.y + Time.deltaTime * charScaleShrinkSpeed) < initialCharacterYScale)
                {
                    tempScale.y = tempScale.y + (Time.deltaTime * charScaleShrinkSpeed);
                }
                else
                {
                    tempScale.y = initialCharacterYScale;
                    charScaleDecreasing = true;
                    playHitEffectOnScale = false;
                }
            }
            transform.localScale = tempScale;
        }
        #endregion
    }

    private void FixedUpdate()
    {
        isGrounded = checkIfOnGroundHeight();

        if (canMove == false && isGrounded == true || isDead == true || isAlerted)
        {
            rb2d.velocity = Vector2.zero;
            return;
        }

        if (battleCryInProgress)
        {
            HandleBattleCryMovement();
        }
        else if (isPatrolling == true)
        {
            HandlePatrollingMovement();
        }
        else
        {
            if (groundBasedEnemy)
            {
                if (isGrounded == false && isChangingPlatform == false)
                {
                    //Falling object
                }
                else
                {
                    if (isChangingPlatform == true)
                    {
                        //CODE NEEDS MODIFICATION
                        targetInHorizontalRange = false;
                        isPatrolling = false;
                        isAttacking = false;
                    }
                    #region Combat Mode
                    else if (isPatrolling == false)// when enemy and player on the same platform
                    {
                        //ATTACK
                        if (isAttacking == true && canMove == true)
                        {
                            HandleAttackPlayerMovement();
                        }
                        //REPOSITION
                        else if (canMove == true && isRepositioning)
                        {
                            enemyCurrentMovementSpeed = enemyClassSpeed * enemyCombatRunSpeedMultiplier;
                            HandleRepositionMovement(playerTransform.position);
                        }
                        //BATTLE CRY
                        else if (isRepositioning == false && canMove == false)
                        {
                            if (!isTurningTowardsPlayer)
                            {
                                isTurningTowardsPlayer = true;
                                faceTowardsPlayer();
                            }
                            stopCharacterCompletely();
                        }
                    }
                    #endregion
                }
            }
            else if (groundBasedEnemy == false)
            {
                if (canMove == true)
                {
                    //CODE NEEDS MODIFICATION
                    Vector2 targetPoint = closestRepositionPoint();
                    targetInHorizontalRange = true;
                    if (characterRepositionNeeded(targetPoint) && isRepositioning)
                    {
                        isPatrolling = false;
                        seeker.enabled = false;
                        //Reposition(targetPoint);
                        // check if player in attack range
                        if (nodeDistanceFromPlayer() <= enemyTrackingRange)
                        {
                            HandleAttackPlayerMovement();
                        }
                    }
                    else
                    {
                        // check if enemy needs to approach player
                        if (followEnabled && nodeDistanceFromPlayer() > enemyTrackingRange)
                        {
                            pathFollow();
                        }
                        targetInHorizontalRange = false;
                        isPatrolling = true;
                    }
                }
            }
        }
    }

    #region Player Tracking and A*
    private void updatePath()//METHOD CALLED FROM BEGINNING ON REPEAT
    {
        if (followEnabled && playerInCombatRange() && seeker.IsDone())
        {
            seeker.StartPath(rb2d.position, playerTransform.position, onPathComplete);
        }
    }

    private void onPathComplete(Path followedPath)//HELPING METHOD CALLED FROM BEGINNING ON REPEAT
    {
        if (!followedPath.error)
        {
            pathAiToFollow = followedPath;
            currentWayPoint = 0;
        }
    }

    private void pathFollow()//Method used for A* pathfinding, some doubt about the use of AddForce
    {
        if (pathAiToFollow == null)
        {
            return;
        }

        // Reached end of path
        if (currentWayPoint >= pathAiToFollow.vectorPath.Count)
        {
            return;
        }

        //direction calculation
        Vector2 direction = ((Vector2)pathAiToFollow.vectorPath[currentWayPoint] - rb2d.position).normalized;
        Vector2 force = direction * enemyCurrentMovementSpeed * Time.deltaTime;

        //jump
        if (jumpEnabled && isGrounded && !isRepositioning)
        {
            if (direction.y > jumpNodeHeightRequirement)
            {
                rb2d.AddForce(Vector2.up * enemyCurrentMovementSpeed * jumpModifier);
            }
        }

        //movement
        rb2d.velocity = new Vector2(force.x, rb2d.velocity.y);

        if (playerDistanceFromEnemy < nextWayPointDistance)
        {
            currentWayPoint++;
        }

        if (lookAtPlayerEnabled)
        {
            if (!isTurningTowardsPlayer)
            {
                isTurningTowardsPlayer = true;
                faceTowardsPlayer();
            }
        }
    }
    #endregion

    #region Conditions & Shortcut Methods
    bool characterRepositionNeeded(Vector2 targetPosition)//WHAT IS IT'S ACTUAL PURPOSE??
    {
        return Vector2.Distance(transform.position, targetPosition) <= minimumDistanceForReposition;
    }

    private bool playerInCombatRange()
    {
        if (groundBasedEnemy == true)
        {
            //Check if bothe player and enemy are on same platform and within range
            if (enemyPlatformID == playerPlatformID && Mathf.Abs(transform.position.x - playerTransform.position.x) < playerHorizontalDetectionDistance)
            {
                targetInHorizontalRange = true;
            }
            else targetInHorizontalRange = false;

            //Check if enemy can switch platforms and within minimum levels to reposition
            if (canChangePlatform == false) targetInVerticalRange = false;
            else
            {
                //NEEDS MODIFICATIONS BASED ON A* OR GRAPHS
                if (Mathf.Abs(enemyPlatformLevel - playerPlatformLevel) > playerVerticalDetectionLevels)
                {
                    targetInVerticalRange = false;
                }
                else targetInVerticalRange = true;
            }

            if (targetInHorizontalRange || targetInVerticalRange) return true;
            else return false;
        }

        if (groundBasedEnemy == false)
        {
            //CALCULATE A* PATH
            return true;
        }
        return false;
    }

    protected bool playerInsideCombatMovementRange()
    {
        if (playerDistanceFromEnemy > playerContactAvoidRange + .15f && playerDistanceFromEnemy < enemyTrackingRange - .15f) return true;
        else return false;
    }

    protected float nodeDistanceFromPlayer()
    {
        return Vector2.Distance(transform.position, playerTransform.position);
    }

    protected float horizontalDistanceFromPlayer()
    {
        return Mathf.Abs(transform.position.x - playerTransform.position.x);
    }

    protected bool isPlayerAtRight()
    {
        if (playerTransform.position.x < transform.position.x)
        {
            return false;
        }
        else return true;
    }

    //Returns true if player moving towards character, false when moving away from character
    protected bool getPlayerAttackingMovementStatus()
    {
        if (characterFacingRight)
        {
            if (finalPlayerMoveDirection > 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        else if (!characterFacingRight)
        {
            if (finalPlayerMoveDirection < 0)
            {
                return false;
            }
            else return true;
        }

        return true;
    }

    protected float getMoveAwayFromPlayerDirection()
    {
        if (isPlayerAtRight())
        {
            return -1.0f;
        }
        else return 1.0f;
    }

    protected float getMoveTowardsPlayerDirection()
    {
        if (isPlayerAtRight())
        {
            return 1.0f;
        }
        else return -1.0f;
    }

    protected bool isPlayerFacingCharacter()
    {
        if (playerTransform.position.x > transform.position.x)
        {
            if (playerMovementScript.playerFacingRight == true) return false;
            else return true;
        }
        else if (playerTransform.position.x < transform.position.x)
        {
            if (playerMovementScript.playerFacingRight == true) return true;
            else return false;
        }
        else return true;
    }

    protected bool isCharacterFacingPlayer()
    {
        if (isPlayerAtRight() && characterFacingRight == true) return true;
        else if (!isPlayerAtRight() && characterFacingRight == false) return true;
        else return false;
    }
    #endregion

    #region Auxillary Functions 
    // to flip enemy
    public void flipCharacterDirection()
    {
        Vector3 Scale = transform.localScale;
        Scale.x *= -1.0f;
        transform.localScale = Scale;

        if (characterFacingRight == true)
        {
            characterFacingRight = false;
            characterMovementDirection = Vector2.left;
        }
        else
        {
            characterFacingRight = true;
            characterMovementDirection = Vector2.right;
        }
    }

    protected void faceTowardsPlayer()
    {
        StartCoroutine("faceTowardsPlayerCoroutine");
    }

    IEnumerator faceTowardsPlayerCoroutine()
    {
        yield return new WaitForSeconds(turnToPlayerTime);

        if (!doingAttackMove)
        {
            if (playerTransform.position.x >= transform.position.x)//Player is at right side of Enemy
            {
                if (characterFacingRight == false)
                {
                    Debug.Log("rot");
                    characterFacingRight = true;
                    if (initialCharacterFacingRight)
                    {
                        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                    }
                    else
                    {
                        transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                    }
                }
            }
            else//Player is at left side of Enemy
            {
                if (characterFacingRight == true)
                {
                    characterFacingRight = false;
                    if (initialCharacterFacingRight)
                    {
                        transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                    }
                    else
                    {
                        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                    }
                }
            }
        }

        isTurningTowardsPlayer = false;
    }

    protected void faceAwayFromPlayer()
    {
        if (playerTransform.position.x < transform.position.x)//Player is at left side of Enemy
        {
            if (characterFacingRight == false)
            {
                Debug.Log("fun");
                characterFacingRight = true;

                if (initialCharacterFacingRight)
                {
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
                else
                {
                    transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
            }
        }
        else//Player is at right side of Enemy
        {
            if (characterFacingRight == true)
            {
                characterFacingRight = false;

                if (initialCharacterFacingRight)
                {
                    transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
                else
                {
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
            }
        }
    }

    private bool checkIfOnGroundHeight()
    {
        LayerMask mask = LayerMask.GetMask("Ground");

        RaycastHit2D Hit1 = Physics2D.Raycast(checkHeightObject1.transform.position, Vector3.down, 100.0f, mask);
        RaycastHit2D Hit2 = Physics2D.Raycast(checkHeightObject2.transform.position, Vector3.down, 100.0f, mask);

        float finalMinLandHeight = .15f;
        // if (Hit1.collider != null && Hit2.collider != null)
        // {
        //     if (Hit1.distance > minLandHeight && Hit2.distance > minLandHeight)//if on high ground
        //     {
        //         return false;
        //     }
        //     else
        //     {
        //         return true;
        //     }
        // }

        // return false;

        if (Hit1.collider == null && Hit2.collider == null)
        {
            return false;
        }
        else if (Hit1.collider == null && Hit2.collider.CompareTag("Platforms") == true)
        {
            if (Hit2.distance > finalMinLandHeight)
            {
                return false;
            }
            else return true;
        }
        else if (Hit2.collider == null && Hit1.collider.CompareTag("Platforms") == true)
        {
            if (Hit1.distance > finalMinLandHeight)
            {
                return false;
            }
            else return true;
        }
        else if (Hit1.collider.CompareTag("Platforms") == true && Hit2.collider.CompareTag("Platforms") == true)
        {
            if (Hit1.distance > finalMinLandHeight && Hit2.distance > finalMinLandHeight)//if high on ground
            {
                //Debug.Log("On Air: " + Hit1.distance + " " + Hit2.distance + "lH: " + finalMinLandHeight);
                return false;
            }
            else
            {
                //Debug.Log("On Ground: " + Hit1.distance + "lH: " + minLandHeight);
                return true;//On Ground
            }
        }
        else
        {
            //Debug.Log("On bichi Ground: " + Hit1.distance + " gb " + Hit1.collider.gameObject);
            return false;//On Air
        }
    }

    public void getCharacterPlatformInfo()
    {
        LayerMask mask = LayerMask.GetMask("Ground");

        RaycastHit2D Hit1 = Physics2D.Raycast(checkHeightObject2.transform.position, Vector3.down, 20.0f, mask);

        if (Hit1.collider != null && Hit1.collider.CompareTag("Platforms") == true)
        {

        }
    }
    #endregion

    #region Routine Enemy Behaviour Methods
    public void setEnemyPlatform(Vector2 platformLeftEndPosition, Vector2 platformRightEndPosition, int platformID, int platformLevel)//Setting up objects for tracking player around Platforms
    {
        enemyPlatformID = platformID;
        enemyPlatformLevel = platformLevel;
        if (transform.position.x <= platformRightEndPosition.x && transform.position.x >= platformLeftEndPosition.x
        && transform.position.y <= platformLeftEndPosition.y + 5f && transform.position.y >= platformLeftEndPosition.y - 5f)
        {
            platformLeftEndBox = platformLeftEndPosition;
            platformRightEndBox = platformRightEndPosition;

            seeker.enabled = true;
            isReadyToClimb = false;
        }
    }
    void HandlePatrollingAnimation()//This method takes care of the enemy movement itself
    {
        enemySpineAnimator.SetBool("PatrolMode", true);

        if (characterFacingRight == true) characterMovementDirection.x = 1.0f;
        else characterMovementDirection.x = -1.0f;

        if (turnCharacterNow == false && Time.time - lastPatrolTurnTime > patrolDirectionalDuration)
        {
            stopCharacterCompletely();
            lastPatrolTurnTime = Time.time;
            turnCharacterNow = true;
            canMove = false;
            enemySpineAnimator.SetInteger("MoveSpeed", 0);
            enemyCurrentMovementSpeed = 0.0f;
        }
        else if (turnCharacterNow == false)
        {
            canMove = true;
            enemySpineAnimator.SetInteger("MoveSpeed", 1);
            enemyCurrentMovementSpeed = enemyClassSpeed * enemyPatrolSpeedMultiplier;
        }

    }

    void HandleBattleCryAnimation()
    {

    }

    public void movementPauseEndDuringPatrol()
    {
        if (isPatrolling == true)
        {
            canMove = true;
            turnCharacterNow = false;
            lastPatrolTurnTime = Time.time;
            flipCharacterDirection();
            enemySpineAnimator.SetInteger("MoveSpeed", 1);
        }
    }
    #endregion

    #region Overridable Base Methods
    public virtual void HandleAttackPlayerAnimation()//MODIFY METHODS TO INTEGRATE MECANIM ANIMATIONS
    {
        rb2d.velocity = Vector3.zero;

        enemySpineAnimator.SetTrigger("Attack");
        enemySpineAnimator.SetInteger("AttackID", 1);
    }

    public virtual void HandleAttackPlayerMovement()//MODIFY METHODS TO INTEGRATE MECANIM ANIMATIONS
    {
        if (doingAttackMove == false && canMove == true)
        {
            rb2d.velocity = new Vector2(enemyCurrentMovementSpeed * characterMovementDirection.x, rb2d.velocity.y);
        }
    }

    public virtual void performComboAttack(int comboAttackID)//REPLACE BY ANIMATIONS AND EVENTS
    {
        enemySpineAnimator.SetTrigger("Attack");
        enemySpineAnimator.SetInteger("AttackID", comboAttackID);
    }

    public virtual void HandleRepositionAnimation()//Overridden by all Derived classes
    {
        enemySpineAnimator.SetInteger("MoveSpeed", 2);
        if (hasToAndFroMovement) enemySpineAnimator.SetBool("BackWalk", false);

        if (enemyAlreadyAlerted == false)
        {
            enemyAlreadyAlerted = true;
            enemySpineAnimator.SetBool("PatrolMode", false);
        }
    }

    public virtual void HandleRepositionMovement(Vector2 targetPlayerPosition)//Overridden by all Derived classes
    {
        transform.position = new Vector2(targetPlayerPosition.x, targetPlayerPosition.y);

        if (targetPlayerPosition.x == transform.position.x && targetPlayerPosition.y == transform.position.y)
        {
            isRepositioning = false;
        }
    }

    public virtual void ChangePlatforms(Vector2 targetPlayerPosition)//Overridden by all Derived classes
    {
        transform.position = new Vector2(targetPlayerPosition.x, targetPlayerPosition.y);

        if (targetPlayerPosition.x == transform.position.x && targetPlayerPosition.y == transform.position.y)
        {
            isRepositioning = false;
        }
    }

    public virtual void HandlePatrollingMovement()
    {
        enemyCurrentMovementSpeed = enemyClassSpeed * enemyPatrolSpeedMultiplier;
        rb2d.velocity = new Vector2(enemyCurrentMovementSpeed * characterMovementDirection.x, rb2d.velocity.y);
    }

    public virtual void HandleBattleCryMovement()
    {
        rb2d.velocity = Vector2.zero;
    }

    #endregion

    #region Animation Events
    public void combatPromptsBattleCryEnd()//MODIFY AND OMIT UNNECESSARY VARIABLES
    {
        canMove = true;
        isRepositioning = true;
        rb2d.velocity = new Vector2(enemyCurrentMovementSpeed * characterMovementDirection.x, rb2d.velocity.y);

        enemySpineAnimator.SetInteger("MoveSpeed", 1);
    }

    public void combatPromptsOnAttackEnd()
    {
        canMove = true;
        lastAttackTime = Time.time;
        doingAttackMove = false;
        currentAttackID = -1;
        if (movesAwayAfterAttack)
        {
            moveAwayAfterAttack = true;
            if (groundBasedEnemy && hasToAndFroMovement && hasBackWalkAnimationStates)
            {
                enemySpineAnimator.SetBool("BackWalk", true);
            }
            else
            {

            }
        }
    }

    public void combatPromptsOnAttackStart()
    {

    }

    public void disableCharacterRigidbody()
    {
        rb2d.simulated = false;
    }

    public void enableCharacterRigidbody()
    {
        rb2d.simulated = true;
    }

    public void stopCharacterCompletely()
    {
        rb2d.velocity = Vector2.zero;
    }

    public void onPatrolToCombatEnd()
    {
        canMove = true;
        isRepositioning = true;
        enemySpineAnimator.SetInteger("MoveSpeed", 2);
    }

    public void onCharacterHurtStart()
    {
        canMove = false;

        cachedIsAttacking = isAttacking;
        isAttacking = false;

        doingAttackMove = false;

        cachedIsRepositioning = isRepositioning;
        isRepositioning = false;
    }

    public void onCharacterHurtEnd()
    {
        canMove = true;
        isAttacking = cachedIsAttacking;
        isRepositioning = cachedIsRepositioning;
    }

    public void applyCharacterMomentum(float momentumLevel)//momentum 
    {
        if (characterFacingRight)
        {
            //Debug.Log("mom" + Vector2.right * momentumLevel * baseMomentumForce);
            rb2d.AddForce(Vector2.right * momentumLevel * baseMomentumForce, ForceMode2D.Impulse);
        }
        else
        {
            //Debug.Log("fVec: " + momentumLevel + " " + baseMomentumForce);
            rb2d.AddForce(Vector2.left * momentumLevel * baseMomentumForce, ForceMode2D.Impulse);
        }
    }
    #endregion

    #region Special Abilities & Effects
    public void slowMotionstate(float slowSpeedMultiplier, float slowDownDuration)
    {
        enemyCurrentMovementSpeed *= slowSpeedMultiplier;

        float previousSpeed = enemySpineAnimator.speed;
        enemySpineAnimator.speed = previousSpeed * slowSpeedMultiplier;

        StartCoroutine(leaveSlowMotonState(slowSpeedMultiplier, slowDownDuration, previousSpeed));
    }

    IEnumerator leaveSlowMotonState(float multiplier, float duration, float prevSpeed)
    {
        yield return new WaitForSeconds(duration);
        enemyCurrentMovementSpeed /= multiplier;

        enemySpineAnimator.speed = prevSpeed;
    }
    #endregion

    #region Repositioning(REPURPOSE AND REIMPLEMENT)
    private Vector2 closestRepositionPoint()
    {
        if (Vector2.Distance(transform.position, playerTransform.transform.position) < Vector2.Distance(transform.position, playerPlatformLeftEndPosition))
        {
            return new Vector2(playerTransform.transform.position.x - 1f, playerTransform.transform.position.y);
        }
        return new Vector2(playerPlatformLeftEndPosition.x + 1f, playerPlatformRightEndPosition.y);
    }

    public Vector2 enemyClosestRepositionStartPoint()////What is this?? -- What's the purpose of this animation? Where is it used?
    {
        float scale = .5f;
        //If 
        if (Vector2.Distance(transform.position, playerTransform.transform.position) < Vector2.Distance(transform.position, playerPlatformLeftEndPosition))//enemyPlayerDistance < playerPlatformLeft
        {
            return new Vector2(platformLeftEndBox.x + scale, transform.position.y);
        }
        return new Vector2(platformRightEndBox.x - scale, transform.position.y);
    }
    #endregion

    #region Damage and Feedback
    public void TakeDamage(EnemyAttackInfo weaponAttackInfo, bool attackFromRight)
    {
        if (isDead == true) return;

        float finalDamage = weaponAttackInfo.attackDamage;
        if (IncomingAttackDirectionFacingPlayer(attackFromRight) == false)
        {
            finalDamage *= 2.0f;
        }

        PlayOnHitEffect();
        //Debug.Log("Dmg: " + damageAmount);
        if ((!isCharacterBlocking && !hasAutoBlock) || ((isCharacterBlocking || hasAutoBlock) && characterHealthStaminaScript.staminaCompletelyDepleted == true)
        || ((isCharacterBlocking || hasAutoBlock) && IncomingAttackDirectionFacingPlayer(attackFromRight) == false))
        {
            if (characterHealthStaminaScript.modifyHealth(-finalDamage) < 0.15f)
            {
                enemySpineAnimator.SetTrigger("Death");
                isDead = true;
                deathTime = Time.time;
            }
            else if (characterHealthStaminaScript.poiseBroken == true)
            {
                enemySpineAnimator.SetTrigger("Hurt");
                characterHealthStaminaScript.recoverPoise();
            }
        }
        else if ((isCharacterBlocking || hasAutoBlock) && characterHealthStaminaScript.staminaCompletelyDepleted == false)
        {
            Debug.Log("daamn");
            if (weaponAttackInfo.isBlockable)
            {
                // blocking and blockable attack
                if (characterHealthStaminaScript.currentStamina >= weaponAttackInfo.attackStaminaDamage)
                {
                    if (hasAutoBlock)
                    {
                        if (doingAttackMove == false)
                        {
                            characterHealthStaminaScript.modifyStamina(-weaponAttackInfo.attackStaminaDamage);
                            enemySpineAnimator.SetTrigger("DefenceHit");
                        }
                    }
                    else
                    {
                        characterHealthStaminaScript.modifyStamina(-weaponAttackInfo.attackStaminaDamage);
                        enemySpineAnimator.SetTrigger("DefenceHit");
                    }
                }
                else
                {
                    if (characterHealthStaminaScript.modifyHealth(-(weaponAttackInfo.attackDamage - characterHealthStaminaScript.currentStamina)) < 0.10f)
                    {
                        enemySpineAnimator.SetTrigger("Death");
                        isDead = true;
                    }
                }
            }
            else
            {
                PlayOnHitEffect();
                // if current stamina more than half of total stamina, damage will be reduced to half
                if (characterHealthStaminaScript.currentStamina >= characterHealthStaminaScript.totalStamina / 2f)
                {
                    characterHealthStaminaScript.modifyHealth(-(weaponAttackInfo.attackDamage / 2));
                }
                else // else will get full damage
                {
                    characterHealthStaminaScript.modifyHealth(-weaponAttackInfo.attackDamage);
                }

                characterHealthStaminaScript.modifyStamina(-1000);
            }
        }
    }

    public bool IncomingAttackDirectionFacingPlayer(bool attackFromRight)
    {
        //returns true when incoming attack from front, false when attacked from behind
        if (attackFromRight && characterFacingRight == true) return true;
        else if (!attackFromRight && characterFacingRight == false) return true;
        else return false;
    }

    public void TakeProjectileDamage(float damageAmount, float staminaDamageAmount, bool isBlockable = true)
    {
        PlayOnHitEffect();

        if (!isCharacterBlocking && !hasAutoBlock)
        {
            if (characterHealthStaminaScript.modifyHealth(-damageAmount) < 0.15f)
            {
                enemySpineAnimator.SetTrigger("Death");
                isDead = true;
                deathTime = Time.time;
            }
            else if (characterHealthStaminaScript.poiseBroken == true)
            {
                enemySpineAnimator.SetTrigger("Hurt");
                characterHealthStaminaScript.recoverPoise();
            }
        }
        else
        {
            if (isBlockable)
            {
                //Debug.Log("fuck");
                // blocking and blockable attack
                if (characterHealthStaminaScript.currentStamina >= staminaDamageAmount)
                {
                    characterHealthStaminaScript.modifyStamina(-staminaDamageAmount);
                    enemySpineAnimator.SetTrigger("DefenceHit");
                }
                else
                {
                    if (characterHealthStaminaScript.modifyHealth(-damageAmount / 2.0f) < 0.10f)
                    {
                        enemySpineAnimator.SetTrigger("Death");
                    }
                    else
                    {
                        enemySpineAnimator.SetTrigger("PoiseBreak");
                    }
                }
            }
            else
            {
                PlayOnHitEffect();
                // if current stamina more than half of total stamina, damage will be reduced to half
                if (characterHealthStaminaScript.currentStamina >= characterHealthStaminaScript.totalStamina / 2f)
                {
                    characterHealthStaminaScript.modifyHealth(-(damageAmount / 2));
                }
                else // else will get full damage
                {
                    characterHealthStaminaScript.modifyHealth(-damageAmount);
                }
                enemySpineAnimator.SetTrigger("PoiseBreak");
                characterHealthStaminaScript.modifyStamina(-1000);
            }
        }
    }

    public void PlayOnHitEffect()
    {
        playHitEffectOnScale = true;
        charScaleDecreasing = true;
    }
    #endregion
}
