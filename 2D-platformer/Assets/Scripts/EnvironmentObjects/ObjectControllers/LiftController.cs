using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftController : MonoBehaviour
{
    public MovingPlatformScript liftBasePlatformScript;
    public Collider2D liftTriggerCollider;
    public Collider2D liftTopCollider;
    bool waitForFeedback;

    // Start is called before the first frame update
    void Start()
    {
        waitForFeedback = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (waitForFeedback == true)
        {
            if (liftBasePlatformScript.isMovementActive == false)
            {
                waitForFeedback = false;
                if (liftBasePlatformScript.destinationPlatformInd == 1)
                {
                    liftTopCollider.enabled = false;
                }
                else liftTopCollider.enabled = true;
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            liftBasePlatformScript.isMovementActive = true;
            liftBasePlatformScript.GetComponent<Rigidbody2D>().simulated = true;
            waitForFeedback = true;
        }
    }
}
