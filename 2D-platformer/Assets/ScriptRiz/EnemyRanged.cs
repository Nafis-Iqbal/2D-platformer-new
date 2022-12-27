using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRanged : MonoBehaviour
{
    public float health = 50f;
    public bool notPatrolling;
    public Rigidbody2D rb;
    public Transform rowPossition;
    public Transform groundChecker , platformChecker;
    Transform player;
    public LayerMask environmentMask;
    private bool turn , canThrow;
    public bool isActivated;
    public Animator animatorEnemyGround;
    public GameObject row;


    float walkSpeed = 100f , range = 5f , playerDistance ;
    float timeBetweenShoots = 1.15f;
    float foodRadius = .4f;
    bool turn1;
    void Start(){
        player = GameObject.FindGameObjectWithTag("Player").transform;
        notPatrolling = true;
        canThrow = true;
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
            notPatrolling = false;
            rb.velocity = Vector3.zero;
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


    IEnumerator Throw(){
        canThrow = false;
        yield return new WaitForSeconds(timeBetweenShoots);

        GameObject throwRow = Instantiate(row, rowPossition.position, Quaternion.identity);
        throwRow.transform.position = rowPossition.position;
        throwRow.transform.rotation = transform.rotation;

        int dir;
        if (transform.position.x > player.position.x)
        {
            dir = -1;
        }
        else
        {
            dir = 1;
        }

        Vector2 rowScale = throwRow.transform.localScale;
        rowScale.x *= dir;
        throwRow.transform.localScale = rowScale;
        canThrow = true;
    }

    void attack(){
        if (player.position.x > transform.position.x && transform.localScale.x < 0 || player.position.x < transform.position.x && transform.localScale.x > 0){
            Flip();
        }

        if (canThrow){
            StartCoroutine(Throw());
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
}