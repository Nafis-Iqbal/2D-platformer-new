using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Microsoft.Unity.VisualStudio.Editor;

public class EnemyBase : MonoBehaviour
{
    [Header("DRAG AND DROPS")]
    public Animator enemySpineAnimator;
    protected Rigidbody2D rb2d;
    protected PlayerCombatSystem playerCombatSystemScript;
    public Transform playerTransform;
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
    public bool characterFacingRight;
    public bool isGrounded;
    public bool isPatrolling;//false means combat mode
    public bool isRepositioning;
    public bool isFleeingFromPlayer;
    public bool isChangingPlatform;
    public bool isReadyToClimb;
    [Header("ENEMY COMBAT STATES")]
    public bool canAttackPlayer = true;
    public bool isAttacking;//in close combat range and performing attacks
    public bool doingAttackMove;
    public bool closeInForAttack;
    public bool moveAwayAfterAttack;
    public bool isPoiseBroken, isStunned;

    [Header("PLAYER TARGETTING AND REPOSITIONING")]
    public bool groundBasedEnemy;
    public bool canChangePlatform;
    public bool maintainsDistanceFromPlayer;
    //protected float playerDistanceFromEnemy;
    public float playerDistanceFromEnemy;
    int enemyPlatformLevel, playerPlatformLevel;
    public int enemyPlatformID, playerPlatformID;
    public bool targetInHorizontalRange = false;
    public bool targetInVerticalRange = false;
    public float minimumDistanceForReposition = 50f;//What is this??

    [Header("COMBAT ANIMATION SYSTEM")]
    public float enemyHealth;
    public float enemyStamina;

    public float minTimeBetweenAttacks;
    protected float lastAttackTime;
    public float baseMomentumForce;

    public float playerHorizontalDetectionDistance;
    public int playerVerticalDetectionLevels;
    protected float enemyBattleCryRange;
    public float enemyTrackingRange;
    public float enemyAttackRange;

    public bool hasToAndFroMovement;
    protected float toAndFroMoveFrequency;
    public int toAndFroRandomNumber;
    public bool hasComboAttack = false;
    public int comboAttackID;
    public bool movesAwayAfterAttack;
    public bool changesDirectionDuringAttack;

    public GameObject knife;
    public GameObject UnblockableKnife;

    [Header("A* PATHFINDING")]
    public float pathUpdateSeconds = .5f;//What is this??
    private Path pathAiToFollow;
    Seeker seeker;
    private int currentWayPoint = 0;//What is this??

    [Header("CUSTOM BEHAVIOUR")]
    public bool followEnabled = false;
    public bool jumpEnabled = true;
    public bool lookAtPlayerEnabled = true;

    [Header("CHANGING PLATFORM")]
    public Vector2 playerPlatformRightEndPosition;
    public Vector2 playerPlatformLeftEndPosition;
    Vector2 platformLeftEndBox, platformRightEndBox;//Target Platform Reposition Point
    public bool triggeredBattleCry = false;

    public virtual void OnEnable()
    {
        acquireDependencies();
        resetAnimationsSystem();

        playerPlatformID = GameManager.Instance.playerCurrentPlatformID;
        playerPlatformLevel = GameManager.Instance.playerCurrentPlatformLevel;

        enemyAlreadyAlerted = false;
        lastAttackTime = Time.time;
        doingAttackMove = false;
        closeInForAttack = false;
        isReadyToClimb = false;
        isGrounded = true;
        isPoiseBroken = isStunned = false;
        isFleeingFromPlayer = false;
    }

    public void acquireDependencies()
    {
        isRepositioning = false;

        rb2d = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();

        InvokeRepeating("updatePath", 0f, pathUpdateSeconds);

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerCombatSystemScript = playerTransform.GetComponent<PlayerCombatSystem>();
    }

    public void resetAnimationsSystem()
    {
        characterFacingRight = true;
        triggeredBattleCry = false;
        characterMovementDirection = Vector2.right;
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
                    if (isPatrolling == true)
                    {
                        isPatrolling = false;
                        canMove = false;
                        enemySpineAnimator.SetTrigger("Alerted");

                        if (enemyAlreadyAlerted == false)
                        {
                            enemyAlreadyAlerted = true;
                            enemySpineAnimator.SetBool("PatrolMode", false);
                        }
                    }
                    else if (triggeredBattleCry == true)
                    {

                    }
                    else if (isRepositioning == false && doingAttackMove == false)
                    {
                        if (playerDistanceFromEnemy < playerContactAvoidRange)
                        {
                            isRepositioning = true;
                            isFleeingFromPlayer = true;
                            isAttacking = false;
                        }
                        else if (playerDistanceFromEnemy > enemyTrackingRange)
                        {
                            isRepositioning = true;
                            isFleeingFromPlayer = false;
                            isAttacking = false;
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

        #region Patrolling
        if (isPatrolling == true && canMove == true)
        {
            HandlePatrollingAnimation();
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
                    if (isAttacking == false)//PURSUING PLAYER
                    {
                        if (canMove && isRepositioning)//Reposition For Attack, through running
                        {
                            HandleRepositionAnimation();
                        }
                        else if (triggeredBattleCry)//BATTLE CRY
                        {
                            triggeredBattleCry = false;
                            isRepositioning = false;
                            canMove = false;

                            enemySpineAnimator.SetInteger("MoveSpeed", 0);
                            enemySpineAnimator.SetTrigger("BattleCry");
                            stopCharacterCompletely();
                        }
                    }
                    else//IN ATTACKING PLAYER MODE
                    {
                        HandleAttackPlayerAnimation();
                    }
                }
                #endregion
            }
            else if (groundBasedEnemy == false)
            {

            }
        }
    }

    private void FixedUpdate()
    {
        isGrounded = checkIfOnGroundHeight();

        if (canMove == false && isGrounded == true)
        {
            rb2d.velocity = Vector2.zero;
            return;
        }

        if (isPatrolling == true)
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
                            faceTowardsPlayer();
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerWeapon"))
        {
            enemyHealth -= 20f;
            if (enemyHealth <= 0f)
            {
                enemySpineAnimator.SetTrigger("Death");
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
            faceTowardsPlayer();
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

    public void movementPauseEndDuringPatrol()
    {
        canMove = true;
        turnCharacterNow = false;
        flipCharacterDirection();
        enemySpineAnimator.SetInteger("MoveSpeed", 1);
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
        if (playerTransform.position.x >= transform.position.x)//Player is at right side of Enemy
        {
            if (characterFacingRight == false)
            {
                characterFacingRight = true;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else//Player is at left side of Enemy
        {
            if (characterFacingRight == true)
            {
                characterFacingRight = false;
                transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    protected void faceAwayFromPlayer()
    {
        if (playerTransform.position.x < transform.position.x)//Player is at left side of Enemy
        {
            if (characterFacingRight == false)
            {
                characterFacingRight = true;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else//Player is at right side of Enemy
        {
            if (characterFacingRight == true)
            {
                characterFacingRight = false;
                transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    private bool checkIfOnGroundHeight()
    {
        LayerMask mask = LayerMask.GetMask("Ground");

        RaycastHit2D Hit1 = Physics2D.Raycast(checkHeightObject1.transform.position, Vector3.down, 20.0f, mask);
        RaycastHit2D Hit2 = Physics2D.Raycast(checkHeightObject2.transform.position, Vector3.down, 20.0f, mask);

        if (Hit1.collider != null && Hit2.collider != null)
        {
            if (Hit1.distance > minLandHeight && Hit2.distance > minLandHeight)//if on high ground
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        return false;
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
        if (movesAwayAfterAttack)
        {
            moveAwayAfterAttack = true;
            if (groundBasedEnemy && hasToAndFroMovement) enemySpineAnimator.SetBool("BackWalk", true);
        }
    }

    public void combatPromptsOnAttackStart()
    {

    }

    public void combatPromptsDamageOnBlockableAttack()
    {
        knife.SetActive(true);
    }

    public void CombatPromptsDamageOnUnblockableAttack()
    {
        UnblockableKnife.SetActive(true);
    }

    public void combatPromptsDamagePlayerEnd()
    {
        knife.SetActive(false);
        UnblockableKnife.SetActive(false);
    }

    public void disableObjectOnDeath()
    {
        gameObject.SetActive(false);
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

    public void applyCharacterMomentum(float momentumLevel)//momentum 
    {
        if (characterFacingRight)
        {
            Debug.Log("mom" + Vector2.right * momentumLevel * baseMomentumForce);
            rb2d.AddForce(Vector2.right * momentumLevel * baseMomentumForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.Log("fVec: " + momentumLevel + " " + baseMomentumForce);
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
}
