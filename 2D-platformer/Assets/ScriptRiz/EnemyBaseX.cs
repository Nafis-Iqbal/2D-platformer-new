using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBaseX : MonoBehaviour
{
    [SerializeField]
    public float health;
    [HideInInspector]
    public bool notPatrolling;
    [HideInInspector]
    public Rigidbody2D rb;
    [SerializeField]
    private Transform groundChecker , platformChecker;
    [HideInInspector]
    public Transform player;
    [SerializeField]
    public GameObject knife;
    [SerializeField]
    public LayerMask environmentMask;
    [HideInInspector]
    private bool turn, turn1;
    public bool canHit;
    [HideInInspector]
    public bool isActivated;
    [SerializeField]
    public Animator animatorEnemyGround;
    [SerializeField]
    public GameObject bullet;
    [HideInInspector]
    public float walkSpeed = 100f;
    [SerializeField]
    public float range = 5f;
    public float battleCryRange = 3f;
    [HideInInspector]
    float playerDistance;
    [HideInInspector]
    public float timeBetweenHits = 1.15f;
    [HideInInspector]
    float foodRadius = .4f;
    [SerializeField]
    public float attackRange;

    [SerializeField]
    public Animator animator;

    [SerializeField]
    public bool attacking;

    public bool battleCryEnd = false;
    public bool farAnimationPlayed = true;
    public int randomNumber = 100;
    public virtual void Start(){
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        notPatrolling = true;
        canHit = true;
        attacking = false;
    }

    // Update is called once per frame
    void Update(){
        if(player == null){
            playerDistance = 100f;
            notPatrolling = true;
        }
        turn = !Physics2D.OverlapCircle(groundChecker.position, foodRadius, environmentMask);
        turn1 = Physics2D.OverlapCircle(platformChecker.position, foodRadius, environmentMask);

        if (notPatrolling){
            Patrol();
        }
        playerDistance = Vector2.Distance(transform.position, player.position);

        if (playerDistance <= range && player.position.y + 2f >= transform.position.y && transform.position.y + 3f >= player.position.y)
        {
            if (battleCryEnd) {
                attacking = true;
                attack();
            }else{
                rb.velocity = Vector3.zero;
                notPatrolling = false;
                animator.Play("BattleCry");
            }
        }else{
            notPatrolling = true;
            attacking = false;

        }

        if (attacking && (turn1 || turn)) {
            //Repositioning state;
            if (turn1) {
                //Play reposition up animation 
                animator.Play("repositionUp");
                rb.velocity = new Vector2(rb.velocity.x, walkSpeed * Time.fixedDeltaTime);
            } 
            // else {
            //     Debug.Log("repositionDown");
            //     rb.velocity = new Vector2(rb.velocity.x, -20f * Time.fixedDeltaTime);
            // }
        } else {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }

        if (playerDistance >= battleCryRange && !farAnimationPlayed){
            rb.velocity = Vector3.zero;
            notPatrolling = false;
            animator.Play("BattleCry");
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
        rb.velocity = new Vector2(walkSpeed * Time.fixedDeltaTime , rb.velocity.y);
    }
    // to flip enemy
    public void Flip(){
        Vector3 Scale = transform.localScale;

        Scale.x *= -1;

        transform.localScale = Scale;

        walkSpeed *= -1;
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
        // play aproaching animation
        if (player.position.x > transform.position.x && transform.localScale.x < 0 || player.position.x < transform.position.x && transform.localScale.x > 0){
            Flip();
        }
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange){
            notPatrolling = false;
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
        else
        {
            notPatrolling = true;
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
        rb.velocity = new Vector2(walkSpeed * Time.fixedDeltaTime , rb.velocity.y);
        notPatrolling = true;
        animator.Play("Patrolling animation");
        if(battleCryEnd){
            farAnimationPlayed = true;
        }
        battleCryEnd = true;
        randomNumber = 100;
        canHit = true;
    }
}


