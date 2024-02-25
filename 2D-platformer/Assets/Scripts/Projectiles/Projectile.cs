using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.U2D;

public class Projectile : MonoBehaviour
{
    public bool projectileHit;
    private Rigidbody2D rb2d;
    private float towerX;
    private float targetX;
    private float targetY;
    private float dist;
    private float nextX;
    private float baseY;
    private float height;
    private float spawnTime, projectileHitTime;
    public Vector2 tempVelocity;
    public float projectileLaunchAngle, projectileLaunchVelocity;
    public float timeToDisableAfterHit = .1f;
    public float timeToDisable = .1f;
    [SerializeField]
    public bool lookAtTarget;
    private double pi = 3.1416;
    private float arrowOrientationAngle;

    // Start is called before the first frame update
    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        Debug.Log("Arrow enabled");
        spawnTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        float timeElapsed = Time.time - spawnTime;

        if (Time.time - spawnTime > timeToDisable)
        {
            //Debug.Log("Arrow disabled");
            gameObject.SetActive(false);
            return;
        }
        else if (projectileHit == true)
        {
            rb2d.simulated = false;
            Debug.Log("epalsed: " + timeElapsed);
            gameObject.SetActive(false);
            return;
        }

        if (lookAtTarget == true)
        {
            tempVelocity.x = projectileLaunchVelocity * Mathf.Cos(projectileLaunchAngle) * 1.5f;//Don't know why multiplying with 1.5 makes it work
            tempVelocity.y = (projectileLaunchVelocity * Mathf.Sin(projectileLaunchAngle)) - (9.8f * timeElapsed);
            rb2d.velocity = tempVelocity;

            arrowOrientationAngle = Mathf.Atan2(tempVelocity.y, tempVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(arrowOrientationAngle, Vector3.forward);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Platforms") || collision.transform.CompareTag("Player"))
        {
            //Debug.Log("Arrow hit");
            projectileHit = true;
            projectileHitTime = Time.time;
        }
    }

    public void initializeProjectile(Vector3 startPosition, Vector3 aimPosition, float projectileMaxHeight, float projectileFlightTime)
    {
        rb2d.simulated = true;
        transform.position = startPosition;

        projectileHit = false;
        float flightPathDistance = Vector3.Distance(startPosition, aimPosition);

        float radianToDegreeMultiplier = (float)pi / 180.0f;
        float sqrtValue = Mathf.Sqrt(2.0f * 9.8f * projectileMaxHeight);

        //V = sqrt(2gh)/sin(r) AND S = vcos(r) * T
        projectileLaunchAngle = Mathf.Atan((sqrtValue * projectileFlightTime) / flightPathDistance);
        projectileLaunchVelocity = flightPathDistance / (projectileFlightTime * Mathf.Cos(projectileLaunchAngle));
        //Debug.Log("dist " + flightPathDistance + " ini vel: " + projectileLaunchVelocity + " time: " + projectileFlightTime);
    }
}
