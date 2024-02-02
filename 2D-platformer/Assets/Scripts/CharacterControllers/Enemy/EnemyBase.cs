using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyBase : MonoBehaviour
{
    [Header("Drag and Drops")]
    protected Rigidbody2D rb;
    protected PlayerCombatSystem playerCombatSystemScript;
    public GameObject animatorGameobject;
    public Animator enemySpineAnimator;
    public Transform groundChecker, platformChecker, botGroundCheck;//What is this?? -- botGroundCheck..diff between gndcheck and platformchck
    public LayerMask environmentMask;//What is this? -- Why is an environment Mask needed? Which layers are being masked here?
    public LayerMask repositionLayer;//What is this??--Why and how Layer is used for repositioning

    [Header("Player Targetting And Repositioning")]
    public Transform player;
    public float activateDistance = 10f;//What is this??
    public float pathUpdateSeconds = .5f;//What is this??
    public bool isRepositioning;
    public bool inRepositioningPhase;
    public bool notInRepositioningPhase;//What is this?? -- Why three bools for representing repositioning phase
    public float minimumDistanceForReposition = 50f;
    public float battleCryRange = 3f;
    private float playerDistanceFromEnemy;
    private float foodRadius = .4f;//What is this?? -- Was it supposed to be detection Radius?? Is it copied from another code??
    public bool targetInRange = false;
    public bool isReadyToClimb = false;

    [Header("Enemy Patrolling System")]
    private bool firstTurnAroundDuringPatrol;
    private bool secondTurnAroundDuringPatrol, isInRepositionPoint;//Last variable is never used
    [Header("Enemy Combat & Animation System")]
    public float enemyHealth;
    public float enemyAttackRange;
    public GameObject knife;//What is this? -- Is knife a placeholder for all melee weapons??
    public GameObject UnblockableKnife;//What is this? -- Why use gameobject instead of bool for indicating unblockable property??
    public bool comboMode = false;
    public float comboAttackTime = 2f;
    public bool canHit;//What is this?? -- Which approach was followed for animation
    [HideInInspector]
    public float timeBetweenHits = 1.15f;//What is this?? -- Which approach was followed for animation

    [Header("Enemy States")]
    public bool isGrounded = false;
    public bool playerFacingRight;
    public bool shouldBePatrolling;

    [Header("Enemy Movement")]
    public float enemyMovementSpeed = 100f;
    public float enemyWalkSpeed;
    public float jumpNodeHeightRequirement = .8f;
    public float jumpModifier = .3f;//What is this?
    public float jumpCheckOffset = .1f;//What is this?
    public float nextWayPointDistance = 3f;

    [Header("A* Pathfinding")]
    private Path path;
    Seeker seeker;

    [Header("Custom Behaviour")]
    public bool followEnabled = false;
    public bool jumpEnabled = true;
    public bool lookAtPlayerEnabled = true;

    [Header("Confused or Useless")]
    public Vector2 playerRight;
    public Vector2 playerLeft;//What is this??--Used in repositioning
    public bool attacking;//What is this?? -- Which approach was followed for animation
    public bool battleCryEnd = false;//Probably indicates battlecry animation is over
    public bool farAnimationPlayed = true;//What is this??
    public int randomNumber = 100;//What is this?? -- How is it affecting the randoness, and what is it affecting
    Vector2 lBox, rBox;//What is this??
    Vector2 preTarget;//What is this??
    private int currentWayPoint = 0;//What is this??

    public virtual void Start()
    {
        acquireDependencies();
    }

    public void acquireDependencies()
    {
        comboMode = false;
        inRepositioningPhase = false;

        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
        enemySpineAnimator = animatorGameobject.GetComponent<Animator>();

        InvokeRepeating("updatePath", 0f, pathUpdateSeconds);

        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCombatSystemScript = player.GetComponent<PlayerCombatSystem>();
    }

    public void resetAnimationsSystem()
    {
        playerFacingRight = true;
    }

    void Update()
    {
        // if player dies then patrol as usual..
        if (player == null)
        {
            firstTurnAroundDuringPatrol = !Physics2D.OverlapCircle(groundChecker.position, foodRadius, environmentMask);
            secondTurnAroundDuringPatrol = Physics2D.OverlapCircle(platformChecker.position, foodRadius, environmentMask);
            Patrol();
            return;
        }

        // to check if the enemey needs to reposition according to players position or not.....
        if (isGrounded)
        {
            rb.gravityScale = 1f;
            if ((lBox == playerLeft) || (rBox == playerRight))
            {
                inRepositioningPhase = false;
                isRepositioning = false;
            }
            else
            {
                if (!inRepositioningPhase && targetInDistance())
                    isRepositioning = true;
            }
        }

        // to check if enemy needs to flip or not
        firstTurnAroundDuringPatrol = !Physics2D.OverlapCircle(groundChecker.position, foodRadius, environmentMask);
        secondTurnAroundDuringPatrol = Physics2D.OverlapCircle(platformChecker.position, foodRadius, environmentMask);

        isInRepositionPoint = Physics2D.OverlapCircle(platformChecker.position, foodRadius, repositionLayer);

        if (!inRepositioningPhase)
        {
            if (shouldBePatrolling)
            {
                Patrol();
            }

            if (playerDistanceFromEnemy >= battleCryRange && !farAnimationPlayed)
            {
                rb.velocity = Vector3.zero;
                shouldBePatrolling = false;
                enemySpineAnimator.Play("BattleCry");
                //enemySpineAnimator.SetTrigger("BattleCry");
            }
        }
    }

    private void FixedUpdate()
    {

        isGrounded = Physics2D.OverlapCircle(botGroundCheck.position, foodRadius, environmentMask);
        Vector2 targetPoint = closestRepositionPoint();

        // when player is in alerting distance of enemy
        if (targetInDistance())
        {
            targetInRange = true;
            if (repositionable(targetPoint) && isRepositioning)
            {
                shouldBePatrolling = false;
                seeker.enabled = false;
                preTarget = targetPoint;
                Reposition(targetPoint);
            }
            else
            {
                targetInRange = false;
                shouldBePatrolling = true;
            }
            // when enemy and player on the same platform
            if (!isRepositioning)
            {
                faceTowardsPlayer();

                shouldBePatrolling = false;
                // check if enemy needs to cry for battle
                if (battleCryEnd)
                {
                    attacking = true;
                    followEnabled = true;
                }
                else
                {
                    enemySpineAnimator.Play("BattleCry");
                    //enemySpineAnimator.SetTrigger("BattleCry");
                }
                // check if enemy needs to approach player
                if (followEnabled && distanceFromPlayer() > enemyAttackRange)
                {
                    pathFollow();
                }
                // check if player in attack range
                if (distanceFromPlayer() <= enemyAttackRange)
                {
                    attack();
                }
            }
            // when target is not in alerting distance
        }
        else
        {
            targetInRange = false;
            if (isRepositioning)
            {
                transform.position = preTarget;
                isRepositioning = false;
                rb.gravityScale = 1f;
                isReadyToClimb = false;
                shouldBePatrolling = true;
            }
            if (!isReadyToClimb)
            {
                isRepositioning = false;
            }
            shouldBePatrolling = true;
            attacking = false;
        }

        if (rb.velocity.x != 0f)
        {
            enemySpineAnimator.Play("Patrol");
            //enemySpineAnimator.SetTrigger("Patrol");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerWeapon"))
        {
            enemyHealth -= 20f;
            if (enemyHealth <= 0f)
            {
                StartCoroutine(enemyDeath(.1f));
            }
        }
    }

    #region Player Tracking and Repositioning
    private void updatePath()
    {
        if (followEnabled && targetInDistance() && seeker.IsDone())
        {
            seeker.StartPath(rb.position, player.position, onPathComplete);
        }
    }

    private void pathFollow()//What is this?? -- Why is this function usig addForce, shouldn't it be taken care of by A* pathfinder??
    {
        if (path == null)
        {
            return;
        }

        // Reached end of path
        if (currentWayPoint >= path.vectorPath.Count)
        {
            return;
        }

        //direction calculation
        Vector2 direction = ((Vector2)path.vectorPath[currentWayPoint] - rb.position).normalized;
        Vector2 force = direction * enemyMovementSpeed * Time.deltaTime;

        //jump
        if (jumpEnabled && isGrounded && !isRepositioning)
        {
            if (direction.y > jumpNodeHeightRequirement)
            {
                rb.AddForce(Vector2.up * enemyMovementSpeed * jumpModifier);
            }
        }

        //movement
        rb.velocity = new Vector2(force.x, rb.velocity.y);

        if (playerDistanceFromEnemy < nextWayPointDistance)
        {
            currentWayPoint++;
        }

        if (lookAtPlayerEnabled)//Why auxillary function isn't applied here??
        {
            if (rb.velocity.x < -0.05f)
            {
                transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (rb.velocity.x > .05f)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    private bool targetInDistance()
    {
        return Vector2.Distance(transform.position, player.transform.position) < activateDistance;
    }

    private float distanceFromPlayer()
    {
        return Vector2.Distance(transform.position, player.transform.position);
    }

    private void onPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWayPoint = 0;
        }
    }

    private Vector2 closestRepositionPoint()
    {
        if (Vector2.Distance(transform.position, playerCombatSystemScript.transform.position) < Vector2.Distance(transform.position, playerLeft))
        {
            return new Vector2(playerCombatSystemScript.transform.position.x - 1f, playerCombatSystemScript.transform.position.y);
        }
        return new Vector2(playerLeft.x + 1f, playerLeft.y);
    }

    public Vector2 enemyClosestRepositionStartPoint()////What is this?? -- What's the purpose of this animation? Where is it used?
    {
        float scale = .5f;
        if (Vector2.Distance(transform.position, playerCombatSystemScript.transform.position) < Vector2.Distance(transform.position, playerLeft))
        {
            return new Vector2(lBox.x + scale, transform.position.y);
        }
        return new Vector2(rBox.x - scale, transform.position.y);
    }

    public virtual void Reposition(Vector2 tar)//What's the purpose of this method??
    {
        transform.position = new Vector2(tar.x, tar.y);

        if (tar.x == transform.position.x && tar.y == transform.position.y)
        {
            isRepositioning = false;
        }
    }

    bool repositionable(Vector2 target)
    {
        return Vector2.Distance(transform.position, target) <= minimumDistanceForReposition;
    }

    //Platforming
    public void setEnemyPlatform(Vector2 left, Vector2 right)//Where is this used??
    {
        if (transform.position.x <= right.x && transform.position.x >= left.x
        && transform.position.y <= left.y + 5f && transform.position.y >= left.y - 5f)
        {
            lBox = left;
            rBox = right;
            Debug.Log(lBox);
            seeker.enabled = true;
            isReadyToClimb = false;
        }
    }
    #endregion
    void Patrol()
    {
        // play walking animation;
        enemySpineAnimator.Play("Patrol");
        //enemySpineAnimator.SetTrigger("Patrol");
        if (firstTurnAroundDuringPatrol || secondTurnAroundDuringPatrol)
        {
            Flip();
        }

        float speedDirection = transform.localScale.x / Mathf.Abs(transform.localScale.x);
        rb.velocity = new Vector2(enemyWalkSpeed * speedDirection * Time.fixedDeltaTime, rb.velocity.y);
    }

    #region Auxillary Functions 
    // to flip enemy
    public void Flip()
    {
        Vector3 Scale = transform.localScale;

        Scale.x *= -1;

        transform.localScale = Scale;
    }

    void faceTowardsPlayer()
    {
        if (player.position.x >= transform.position.x)
        {
            if (playerFacingRight == false)
            {
                playerFacingRight = true;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else
        {
            if (playerFacingRight == true)
            {
                playerFacingRight = false;
                transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }
    #endregion

    #region Animation System and Combat Abilities
    public virtual IEnumerator hit()//What is this?? -- Which approach was followed for animation
    {
        canHit = false;
        enemySpineAnimator.Play("Idle");
        //enemySpineAnimator.SetTrigger("Idle");

        yield return new WaitForSeconds(timeBetweenHits);
        enemySpineAnimator.Play("LightAttack");
        //enemySpineAnimator.SetTrigger("Attack1");

        yield return new WaitForSeconds(timeBetweenHits);
        knife.SetActive(true);

        yield return new WaitForSeconds(.2f);
        randomNumber = Random.Range(1, 5);

        knife.SetActive(false);
        canHit = true;
    }

    public virtual void attack()//What is this?? -- Which approach was followed for animation
    {
        rb.velocity = Vector3.zero;

        if (comboMode)
        {
            StartCoroutine(comboAttack(0));
            return;
        }

        if (canHit)
        {
            farAnimationPlayed = false;
            StartCoroutine(hit());
        }

        if (randomNumber == 3)
        {
            canHit = false;
            enemySpineAnimator.SetTrigger("BattleCry");
        }
    }

    public void animationEnd()//What is this?? -- Which specific animation end
    {
        rb.velocity = new Vector2(enemyMovementSpeed * Time.fixedDeltaTime, rb.velocity.y);
        shouldBePatrolling = true;
        enemySpineAnimator.SetTrigger("Patrol");
        if (battleCryEnd)
        {
            farAnimationPlayed = true;
        }
        battleCryEnd = true;
        randomNumber = 100;
        canHit = true;
    }

    public void damagePlayerStartBlockable()
    {
        knife.SetActive(true);
    }

    public void damagePlayerStartUnBlockable()
    {
        UnblockableKnife.SetActive(true);
    }
    public void damagePlayerEnd()
    {
        knife.SetActive(false);
        UnblockableKnife.SetActive(false);
    }

    IEnumerator enemyDeath(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    public virtual IEnumerator comboAttack(int type)
    {
        comboMode = false;
        canHit = false;
        string preAnim, atkAnim;
        if (type == 0)
        {
            atkAnim = "ComboAttackType0";
        }
        else
        {
            atkAnim = "ComboAttacktype1";
        }
        enemySpineAnimator.SetTrigger("LightAttack");
        yield return new WaitForSeconds(comboAttackTime);
        canHit = true;
    }
    #endregion

    #region Special Abilities & Effects
    public void slowMotionstate(float multiplier, float dureation)
    {
        enemyMovementSpeed *= multiplier;
        enemyWalkSpeed *= multiplier;
        float prevSpeed = enemySpineAnimator.speed;
        enemySpineAnimator.speed = prevSpeed * multiplier;

        StartCoroutine(leaveSlowMotonState(multiplier, dureation, prevSpeed));
    }

    IEnumerator leaveSlowMotonState(float multiplier, float duration, float prevSpeed)
    {
        yield return new WaitForSeconds(duration);
        enemyMovementSpeed /= multiplier;
        enemyWalkSpeed /= multiplier;
        enemySpineAnimator.speed = prevSpeed;
        Debug.Log("pre");
    }
    #endregion
}
