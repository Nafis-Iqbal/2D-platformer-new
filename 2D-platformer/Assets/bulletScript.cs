using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    float speed = 7f, lifeTime = 4f;
    Vector2 target;
    bool colliding;
    //destroy animation time if no animation then 0;
    float TimeToDistroy = .1f;

    // Start is called before the first frame update
    void Start()
    {
        colliding = false;
        StartCoroutine(CountDownTimer());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")){
            Debug.Log("Player");
        }
        colliding = true;
        Debug.Log("coll");
        Destroy(gameObject);
        StartCoroutine(VanishBullet(TimeToDistroy));

    }
    private void OnEnable()
    {
        colliding = false;
        StartCoroutine(CountDownTimer());
    }

    void Update()
    {
        Vector2 bulletEndPosition = transform.position;
        bulletEndPosition.x = transform.position.x + 10 * transform.localScale.x;
        if (colliding == false)
        {
            transform.position = Vector2.MoveTowards(transform.position, bulletEndPosition, speed * Time.deltaTime);
        }
        if (target.x == transform.position.x && target.y == transform.position.y)
        {
            float time = 1f;
            StartCoroutine(VanishBullet(time));
        }
    }
    IEnumerator CountDownTimer()
    {
        yield return new WaitForSeconds(lifeTime);

        StartCoroutine(VanishBullet(TimeToDistroy));
    }
    IEnumerator VanishBullet(float destroyTime)
    {
        
        yield return new WaitForSeconds(0f);
        Destroy(gameObject);
    }
}
