using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WeightPuzzleObject : MonoBehaviour
{
    public bool isConnectionActive;
    public bool hasReorientation, playerAlreadyAffected;
    public bool moveToNewPosition;
    public Vector3 displacementDirection;
    float displacementMagnitude;
    public float displacementSpeed;
    public float initialHeight, lastObjectEntryTime;
    Vector3 targetPosition;
    public float jointSnapLimit, playerWeightEffect, objectWeightEffect;
    public Collider2D weightTriggerCollider;
    public Collider2D[] activateCollidersOnSnap = new Collider2D[2];
    // Start is called before the first frame update
    void Start()
    {
        isConnectionActive = true;
        initialHeight = transform.position.y;
        moveToNewPosition = false;
        lastObjectEntryTime = Time.time;
        playerAlreadyAffected = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (moveToNewPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * displacementSpeed);

            if (Vector3.Distance(transform.position, targetPosition) < 1.0f) moveToNewPosition = false;
        }

        if (isConnectionActive == true && transform.position.y - initialHeight > jointSnapLimit)
        {
            isConnectionActive = false;

            for (int i = 0; i < activateCollidersOnSnap.Length; i++)
            {
                activateCollidersOnSnap[i].enabled = true;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (moveToNewPosition == true || Time.time - lastObjectEntryTime < 2.0f) return;

        if (other.CompareTag("Player") && playerAlreadyAffected == false)
        {
            displacementMagnitude = playerWeightEffect;
            targetPosition = transform.position + (displacementDirection * displacementMagnitude);
            moveToNewPosition = true;
            lastObjectEntryTime = Time.time;
            if (hasReorientation == false) playerAlreadyAffected = true;
        }
        else if (other.CompareTag("Objects"))
        {
            displacementMagnitude = objectWeightEffect;
            targetPosition = transform.position + (displacementDirection * displacementMagnitude);
            moveToNewPosition = true;
            lastObjectEntryTime = Time.time;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // //Debug.Log("other object: " + other.gameObject);
        // if (moveToNewPosition == true) return;

        // if (other.CompareTag("Player"))
        // {
        //     displacementMagnitude = playerWeightEffect;
        //     targetPosition = transform.position - (displacementDirection * displacementMagnitude);
        //     moveToNewPosition = true;
        // }
        // else if (other.CompareTag("Objects"))
        // {
        //     displacementMagnitude = objectWeightEffect;
        //     targetPosition = transform.position - (displacementDirection * displacementMagnitude);
        //     moveToNewPosition = true;
        // }
    }
}
