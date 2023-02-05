using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyBase : MonoBehaviour
{
    //reposition 
    public bool noReposition;
    Vector2 lBox, rBox;
    public bool inRepositioningPhase;
    public healthofPlayer playerScript;
    private bool isActivated;
    public float minimumDistanceForRepo = 50f;
    //reposition End
    [SerializeField]
    public LayerMask repositionLayer;
    [SerializeField]
    public float health;
    [SerializeField]
    public bool notPatrolling;
    [HideInInspector]
    public Rigidbody2D rb;
    [SerializeField]
    private Transform groundChecker , platformChecker , botGroundCheck;
    [SerializeField]
    public GameObject knife;
    [SerializeField]
    public LayerMask environmentMask;
    [HideInInspector]
    private bool turn, turn1 , isInRepositionPoint;
    public bool canHit;
    [SerializeField]
    public GameObject bullet;
    public float battleCryRange = 3f;
    [HideInInspector]
    float playerDistance;
    [HideInInspector]
    public float timeBetweenHits = 1.15f;
    [HideInInspector]
    float foodRadius = .4f;

    [SerializeField]
    public Animator animator;

    [SerializeField]
    public bool attacking;

    public bool battleCryEnd = false;
    public bool farAnimationPlayed = true;
    public int randomNumber = 100;

    public float attackRange;

    // Pathfinding and attacking informations;
    [Header("pathfinding")]
    public Transform player;
    public float activateDistance = 10f;
    public float pathUpdateSeconds = .5f;

    [Header("Physics")]
    public float speed = 100f;
    public float nextWayPointDistant = 3f;
    public float jumbNodeHeightRequirment = .8f;
    public float jumpModifier = .3f;
    public float jumpCheckOffset = .1f;

    [Header("Custom Behavior")]
    public bool followEnabled = false;
    public bool jumpEnabled = true;
    public bool directionLookEnabled = true;

    private Path path;
    private int currentWayPoint = 0;
    public bool isGrounded = false;
    Seeker seeker;

    [SerializeField]
    public float walkSpeed;
    public virtual void Start(){
        startNesesarries();
    }

    public void startNesesarries(){
        inRepositioningPhase = false;
        isActivated = false;
        seeker = GetComponent<Seeker>();

        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);

        player = GameObject.FindGameObjectWithTag("Player").transform;

        playerScript = player.GetComponent <healthofPlayer>();
    }

    void Update(){

        // rayCasting();

        if(playerScript.PlatChanged){
            rayCasting();
        }

        if(player == null){
            playerDistance = 100f;
            notPatrolling = true;
        }
        turn = !Physics2D.OverlapCircle(groundChecker.position, foodRadius, environmentMask);
        turn1 = Physics2D.OverlapCircle(platformChecker.position, foodRadius, environmentMask);

        isInRepositionPoint = Physics2D.OverlapCircle(platformChecker.position, foodRadius, repositionLayer);

        if (!inRepositioningPhase)
        {
            if (notPatrolling)
            {
                Patrol();
            }

            if (playerDistance >= battleCryRange && !farAnimationPlayed)
            {
                rb.velocity = Vector3.zero;
                notPatrolling = false;
                animator.Play("BattleCry");
            }
        }
    }

    private void FixedUpdate() {
        isGrounded = Physics2D.OverlapCircle(botGroundCheck.position, foodRadius, environmentMask);
        bool atJumpPoint = Physics2D.OverlapCircle(platformChecker.position, foodRadius, environmentMask);
        Vector2 targetPoint = closestRepoPoint();

        if(TargetInDistance()){
            if(repositionable(targetPoint) && !noReposition) {
                Reposition(targetPoint);
            }

            // if (!inRepositioningPhase)
            // {
                notPatrolling = false;
                if (battleCryEnd)
                {
                    attacking = true;
                    followEnabled = true;
                }
                else
                {
                    notPatrolling = false;
                    animator.Play("BattleCry");
                }
                if (followEnabled && tDistance() > attackRange)
                {
                    pathFollow();
                }
                if (tDistance() <= attackRange)
                {
                    attack();
                }
            // }
        }else {
            notPatrolling = true;
            attacking = false;
        }

        if(rb.velocity.x != 0f) {
            animator.Play("Patrolling animation");
        }

        if(atJumpPoint && isGrounded && TargetInDistance()) {
            rb.AddForce(Vector2.up * speed * jumpModifier);
        }
    }

    private void UpdatePath(){
        if(followEnabled && TargetInDistance() && seeker.IsDone()){
            seeker.StartPath(rb.position, player.position, OnPathComplete);
        }
    }

    private void pathFollow(){
        if(path == null){
            return;
        }

        // Reached end of path
        if(currentWayPoint >= path.vectorPath.Count) {
            return;
        }

        //direction calculation
        Vector2 direction = ((Vector2)path.vectorPath[currentWayPoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        //jump
        if(jumpEnabled && isGrounded) {
            if(direction.y > jumbNodeHeightRequirment) {
                rb.AddForce(Vector2.up * speed * jumpModifier);
            }
        }

        //movement
        // rb.AddForce(force);
        rb.velocity = new Vector2(force.x , rb.velocity.y);

        if(playerDistance < nextWayPointDistant) {
            currentWayPoint++;
        }

        if(directionLookEnabled) {
            if(rb.velocity.x < -0.05f) {
                transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }else if(rb.velocity.x > .05f) {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    private bool TargetInDistance(){
        return Vector2.Distance(transform.position, player.transform.position) < activateDistance;
    }

    private float tDistance(){
        return Vector2.Distance(transform.position, player.transform.position);
    }

    private void OnPathComplete(Path p) {
        if(!p.error) {
            path = p;
            currentWayPoint = 0;
        }
    }




    // make player patroll
    void Patrol(){
        // play walking animation;
        animator.Play("Patrolling animation");
        // animator.SetInteger("state", 0);
        if(turn || turn1){
            Flip();
        }

        float speedDirection = (transform.localScale.x / Mathf.Abs(transform.localScale.x));
        rb.velocity = new Vector2(walkSpeed * speedDirection * Time.fixedDeltaTime , rb.velocity.y);
    }
    // to flip enemy
    public void Flip(){
        Vector3 Scale = transform.localScale;

        Scale.x *= -1;

        transform.localScale = Scale;
    }

    public virtual IEnumerator hit(){
        canHit = false;
        animator.Play("Idle");
        // animator.SetInteger("state", 1);
        yield return new WaitForSeconds(timeBetweenHits);
        animator.Play("attacking");
        // animator.SetInteger("state", 2);
        yield return new WaitForSeconds(timeBetweenHits);
        knife.SetActive(true);
        yield return new WaitForSeconds(.2f);
        randomNumber = Random.Range(1,5);
        knife.SetActive(false);
        canHit = true;
    }

    public virtual void attack(){
        rb.velocity = Vector3.zero;
        if (canHit){
            farAnimationPlayed = false;
            StartCoroutine(hit());
        }

        if(randomNumber == 3){
            canHit = false;
            animator.Play("BattleCry");
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("bullet")){
            health -= 20f;
            if(health <= 0f){
                StartCoroutine(death(.1f));
            }
        }
    }

    IEnumerator death(float time){
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    public void animationEnd(){
        rb.velocity = new Vector2(speed * Time.fixedDeltaTime , rb.velocity.y);
        notPatrolling = true;
        animator.Play("Patrolling animation");
        if(battleCryEnd){
            farAnimationPlayed = true;
        }
        battleCryEnd = true;
        randomNumber = 100;
        canHit = true;
    }

    private Vector2 closestRepoPoint() {
        if(Vector2.Distance(transform.position, playerScript.rightBox) < Vector2.Distance(transform.position, playerScript.leftBox)){
            return new Vector2(playerScript.rightBox.x - 1.5f , playerScript.rightBox.y);
        }
        return new Vector2(playerScript.leftBox.x + 1.5f , playerScript.leftBox.y);
    }

    public virtual void Reposition(Vector2 tar) {
        transform.position = new Vector2(tar.x, tar.y);

        if (tar.x == transform.position.x && tar.y == transform.position.y)
        {
            noReposition = true;
        }
    }

    bool repositionable(Vector2 target) {
        return Vector2.Distance(transform.position, target) <= minimumDistanceForRepo;
    }


    public void rayCasting() {
        RaycastHit2D hitLeft = Physics2D.Raycast(platformChecker.position, Vector2.left);
        RaycastHit2D hitright = Physics2D.Raycast(platformChecker.position, Vector2.right);

        Debug.DrawRay(platformChecker.position, Vector2.left * 10f, Color.red);
        Debug.DrawRay(platformChecker.position, Vector2.right * 10f, Color.green);

        if(hitLeft.collider.gameObject.CompareTag("RepositionBox")) {
            lBox = hitLeft.collider.gameObject.transform.position;
        }
        if(hitright.collider.gameObject.CompareTag("RepositionBox")) {
            rBox = hitright.collider.gameObject.transform.position;
        }
        
        // Debug.Log(lBox);
        // Debug.Log(playerScript.leftBox);

        if((lBox == playerScript.leftBox && hitright.collider.gameObject.CompareTag("Player")) 
        || (rBox == playerScript.rightBox && hitLeft.collider.gameObject.CompareTag("Player"))){
            Debug.Log("insame");
            speed = 100f;
            inRepositioningPhase = false;
            noReposition = true;
        }else{
            if(!inRepositioningPhase)
                noReposition = false;
        }
    }
}
