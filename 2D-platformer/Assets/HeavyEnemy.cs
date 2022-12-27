using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyEnemy : MonoBehaviour
{
    public float health;
    public bool notPatrolling;
    [HideInInspector]
    public Rigidbody2D rb;
    public Transform groundChecker , platformChecker;
    Transform player;
    public GameObject knife;
    public LayerMask environmentMask;
    private bool turn , canHit;
    public bool isActivated;
    public Animator animatorEnemyGround;
    public GameObject bullet;


    float walkSpeed = 50f , range = 8f , playerDistance ;
    float timeBetweenHits = 2f;
    float foodRadius = .4f;
    bool turn1;
    void Start(){
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        notPatrolling = true;
        canHit = true;
        knife.SetActive(false);
    }

    // Update is called once per frame
    void Update(){
        turn = !Physics2D.OverlapCircle(groundChecker.position, foodRadius, environmentMask);
        turn1 = Physics2D.OverlapCircle(platformChecker.position, foodRadius, environmentMask);

        if (notPatrolling){
            Patrol();
        }
        playerDistance = Vector2.Distance(transform.position, player.position);

        if (playerDistance <= range && player.position.y + 2f >= transform.position.y)
        {
            attack();
        }
        else
        {
            notPatrolling = true;
        }

    }
    // make player patroll
    void Patrol(){
        if(turn || turn1){
            Flip();
        }
        rb.velocity = new Vector2(walkSpeed * Time.fixedDeltaTime , rb.velocity.y);
    }
    // to flip enemy
    void Flip(){
        Vector3 Scale = transform.localScale;

        Scale.x *= -1;

        transform.localScale = Scale;

        walkSpeed *= -1;
    }

    IEnumerator hit(){
        canHit = false;
        yield return new WaitForSeconds(timeBetweenHits);
        knife.SetActive(true);
        yield return new WaitForSeconds(Time.fixedDeltaTime);
        knife.SetActive(false);
        canHit = true;
    }

    void attack(){
        if (player.position.x > transform.position.x && transform.localScale.x < 0 || player.position.x < transform.position.x && transform.localScale.x > 0){
            Flip();
        }
        float distance = Mathf.Abs(player.transform.position.x - transform.position.x);
        float attackRange = 2f;

        if (distance <= attackRange){
            notPatrolling = false;
            rb.velocity = Vector3.zero;
            if (canHit){
                StartCoroutine(hit());
            }
        }
        else notPatrolling = true;

        
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
}
