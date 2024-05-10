using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverCartController : InteractionBase
{
    public Animator[] wheelsAnimator = new Animator[4];
    public Transform wheelGear;
    public float maxWheelSpeed = 3.0f, visualSpeedMultiplier = 1.0f, gearRotateSpeed;
    public float grabReleaseVelocityLimit;
    public float pushForce, pullForce;
    public HingeJoint2D fixedJointForPlayer;
    Rigidbody2D rb2d;
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        rb2d = GetComponent<Rigidbody2D>();

        fixedJointForPlayer = GetComponent<HingeJoint2D>();
        fixedJointForPlayer.enabled = inUse = false;
        interactionStartTime = interactionEndTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (rb2d.velocity.x > 0.03f)//
        {
            for (int i = 0; i < wheelsAnimator.Length; i++)
            {
                wheelsAnimator[i].SetFloat("MovementSpeed", Mathf.Min(rb2d.velocity.x * visualSpeedMultiplier, maxWheelSpeed));
            }

            wheelGear.Rotate(new Vector3(0.0f, 0.0f, -1.0f), gearRotateSpeed);

            if (inUse && rb2d.velocity.x > grabReleaseVelocityLimit)
            {
                releaseCartFromPlayer();
            }
        }
        else if (rb2d.velocity.x < -0.03f)
        {
            for (int i = 0; i < wheelsAnimator.Length; i++)
            {
                wheelsAnimator[i].SetFloat("MovementSpeed", Mathf.Min(rb2d.velocity.x * visualSpeedMultiplier, maxWheelSpeed));
            }

            wheelGear.Rotate(Vector3.forward, gearRotateSpeed);

            if (rb2d.velocity.x < -grabReleaseVelocityLimit)
            {
                releaseCartFromPlayer();
            }
        }
        else//All wheels stop
        {
            for (int i = 0; i < wheelsAnimator.Length; i++)
            {
                wheelsAnimator[i].SetFloat("MovementSpeed", 0.0f);
            }
        }
    }

    public override void OnInteract()
    {
        if (!inUse)
        {
            inUse = true;
            playerSpineAnimator.SetInteger("InteractID", targetObjectID);
            playerSpineAnimator.SetBool("InteractB", true);

            playerMovement.transform.SetParent(transform);
            GameManager.Instance.playerRB2D.simulated = false;
            interactionStartTime = Time.time;

            playerMovement.enterPushPullObjectState(rb2d, pushForce, pullForce);
            //Code to unify objects
        }
        else
        {
            releaseCartFromPlayer();
            //Code to break down objects
        }
    }

    void releaseCartFromPlayer()
    {
        inUse = false;
        playerMovement.transform.SetParent(null);
        GameManager.Instance.playerRB2D.simulated = true;
        interactionEndTime = Time.time;
        playerSpineAnimator.SetBool("InteractB", false);
        playerSpineAnimator.SetBool("PushObject", false);
        playerSpineAnimator.SetBool("PullObject", false);

        playerMovement.exitPushPullObjectState();
    }
}
