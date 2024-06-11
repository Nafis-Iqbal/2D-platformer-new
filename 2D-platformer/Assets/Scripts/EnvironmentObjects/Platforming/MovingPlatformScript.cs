using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformScript : MonoBehaviour
{
    Rigidbody2D platformRB2D;
    public bool isMovementActive;
    public bool isTriggeredPlatform;

    public bool syncMovement;
    public bool syncInSameDirection;
    public MovingPlatformScript syncPlatformScript;
    public int syncPlatformID;

    bool waitBeforeTransition;
    public GameObject targetPointsCollection;
    int platformsPointCount;
    public int destinationPlatformInd;
    public int currentPlatformInd;
    public float platformMoveSpeed, platformWaitTime;
    public float lastDistanceCheckTime, lastDestinationReachedTime, distanceBetweenPlatforms;
    public Vector2 movementDirection, currentPos, destinationPos;

    // Start is called before the first frame update
    void Start()
    {
        lastDistanceCheckTime = Time.time;
        platformRB2D = GetComponent<Rigidbody2D>();
        platformsPointCount = targetPointsCollection.transform.childCount;
        currentPlatformInd = 0;
        destinationPlatformInd = 1;
        lastDestinationReachedTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMovementActive == false) return;
        if (waitBeforeTransition && Time.time - lastDestinationReachedTime < platformWaitTime)
        {
            if (Time.time - lastDestinationReachedTime < platformWaitTime)
            {
                platformRB2D.velocity = Vector2.zero;
                return;
            }
            else
            {
                waitBeforeTransition = false;
            }
        }

        if (syncMovement)
        {
            if (syncInSameDirection)
            {
                if (syncPlatformScript.destinationPlatformInd != destinationPlatformInd) return;
            }
            else if (syncPlatformScript.destinationPlatformInd == destinationPlatformInd) return;
        }

        if (Time.time - lastDistanceCheckTime > 1.0f)
        {
            lastDistanceCheckTime = Time.time;
            distanceBetweenPlatforms = Vector3.Distance(transform.position, targetPointsCollection.transform.GetChild(destinationPlatformInd).transform.position);
        }

        movementDirection = (targetPointsCollection.transform.GetChild(destinationPlatformInd).transform.position - transform.position).normalized;
        platformRB2D.velocity = movementDirection * platformMoveSpeed;

        if (distanceBetweenPlatforms < 0.5f && Time.time - lastDestinationReachedTime > 2.0f)
        {
            if (isTriggeredPlatform == false)
            {
                lastDestinationReachedTime = Time.time;
                updatePlatformDestination();
            }
            else
            {
                lastDestinationReachedTime = Time.time;
                isMovementActive = false;
                platformRB2D.simulated = false;
                platformRB2D.velocity = Vector2.zero;

                currentPlatformInd = destinationPlatformInd;
                destinationPlatformInd = (destinationPlatformInd + 1) % platformsPointCount;
            }
        }
    }

    void updatePlatformDestination()
    {
        transform.position = targetPointsCollection.transform.GetChild(destinationPlatformInd).transform.position;

        currentPlatformInd = destinationPlatformInd;
        destinationPlatformInd = (destinationPlatformInd + 1) % platformsPointCount;
        // if (!syncMovement)
        // {
        //     currentPlatformInd = destinationPlatformInd;
        //     destinationPlatformInd = (destinationPlatformInd + 1) % platformsPointCount;
        // }
        // else
        // {
        //     if (syncInSameDirection)
        //     {
        //         currentPlatformInd = syncPlatformScript.currentPlatformInd;
        //         destinationPlatformInd = syncPlatformScript.destinationPlatformInd;
        //     }
        //     else
        //     {
        //         currentPlatformInd = (syncPlatformScript.currentPlatformInd + 1) % platformsPointCount;
        //         destinationPlatformInd = (syncPlatformScript.destinationPlatformInd + 1) % platformsPointCount;
        //     }
        // }
        waitBeforeTransition = true;
    }

    public void headToDestinationPlatform(int destinationPlatformID)
    {
        isMovementActive = true;
        destinationPlatformInd = destinationPlatformID;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && isMovementActive == true)
        {
            other.transform.GetComponent<PlayerJump>().onMovingPlatform = true;
            other.transform.GetComponent<PlayerMovement>().movingPlatformRB2D = platformRB2D;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && isMovementActive == true)
        {
            other.transform.GetComponent<PlayerJump>().onMovingPlatform = false;
        }
    }
}
