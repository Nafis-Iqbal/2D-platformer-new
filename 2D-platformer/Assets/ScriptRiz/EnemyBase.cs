using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField]
    public float health;
    [HideInInspector]
    private bool notPatrolling;
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
    private bool turn , turn1 , canHit;
    [HideInInspector]
    public bool isActivated;
    [SerializeField]
    public Animator animatorEnemyGround;
    [SerializeField]
    public GameObject bullet;
    [SerializeField]
    float walkSpeed = 100f , range = 5f;
    [HideInInspector]
    float playerDistance;
    [SerializeField]
    float timeBetweenHits = 1.15f;
    [HideInInspector]
    float foodRadius = .4f;
    [SerializeField]
    float attackRange = 1f;
    void Start(){
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        notPatrolling = true;
        canHit = true;
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
    public void Flip(){
        Vector3 Scale = transform.localScale;

        Scale.x *= -1;

        transform.localScale = Scale;

        walkSpeed *= -1;
    }

    public virtual IEnumerator hit(){
        canHit = false;
        yield return new WaitForSeconds(timeBetweenHits);
        knife.SetActive(true);
        yield return new WaitForSeconds(.5f);
        knife.SetActive(false);
        canHit = true;
    }

    public virtual void attack(){
        if (player.position.x > transform.position.x && transform.localScale.x < 0 || player.position.x < transform.position.x && transform.localScale.x > 0){
            Flip();
        }
        float distance = Mathf.Abs(player.transform.position.x - transform.position.x);

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
