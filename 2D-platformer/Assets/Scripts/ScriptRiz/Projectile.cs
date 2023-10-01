using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject target;
    public float speed = 7f;
    private float towerX;
    private float targetX;
    private float targetY;
    private float dist;
    private float nextX;
    private float baseY;
    private float height;
    private Vector3 startPos;

    float lifeTime = 4f;
    bool colliding;
    float TimeToDistroy = .1f;

    // Start is called before the first frame update
    void Start()
    {
        colliding = false;
        StartCoroutine(CountDownTimer());
        startPos = transform.position;
        towerX = transform.position.x;
        target = GameObject.FindGameObjectWithTag("Player");
        targetX = target.transform.position.x;
        targetY = target.transform.position.y;

    }

    // Update is called once per frame
    void Update()
    {
        
        
        dist = targetX - towerX;
        nextX = Mathf.MoveTowards(transform.position.x, targetX, speed * Time.deltaTime);
        baseY = Mathf.Lerp(startPos.y , targetY , (nextX - towerX) / dist);
        height = .4f * (nextX - towerX) * (nextX - targetX) / (-0.25f * dist * dist);

        Vector3 movePosition = new Vector3(nextX, baseY + height, transform.position.z);
        transform.rotation = LookAtTarget(movePosition - transform.position);
        transform.position = movePosition;


        if (targetX == transform.position.x && targetY == transform.position.y)
        {
            float time = 1f;
            Destroy(gameObject);
            // StartCoroutine(VanishBullet(time));
        }
    }

    public static Quaternion LookAtTarget(Vector2 rotation){
        return Quaternion.Euler(0f, 0f, Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        colliding = true;
        Destroy(gameObject);
        StartCoroutine(VanishBullet(TimeToDistroy));

    }
    private void OnEnable()
    {
        colliding = false;
        StartCoroutine(CountDownTimer());
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
