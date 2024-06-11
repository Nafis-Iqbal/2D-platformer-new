using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverCartController : InteractionBase
{
    public Rigidbody2D rb2d;
    //public HingeJoint2D playerHingeJoint;
    public Animator[] wheelsAnimator = new Animator[4];
    public Transform wheelGear;
    public float maxWheelSpeed = 3.0f, visualSpeedMultiplier = 1.0f, gearRotateSpeed;
    public float defaultMass, ridingStateMass;
    public float grabReleaseVelocityLimit;
    public float pushForce, pullForce;
    public float distanceJointOffsetForVehicle;

    public List<Collider2D> cargoBodyColliders = new List<Collider2D>();
    int cargoBodiesCount;
    public float cargoConvertedMass = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        if (rb2d == null) rb2d = GetComponent<Rigidbody2D>();
        rb2d.mass = defaultMass;

        inUse = false;
        interactionStartTime = interactionEndTime = Time.time;
    }

    void OnEnable()
    {
        cargoBodiesCount = 0;
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

            if (wheelGear != null) wheelGear.Rotate(new Vector3(0.0f, 0.0f, -1.0f), gearRotateSpeed);

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

            if (wheelGear != null) wheelGear.Rotate(Vector3.forward, gearRotateSpeed);

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

            //playerMovement.transform.SetParent(transform);
            playerMovement.playerDistanceJoint.connectedBody = rb2d;
            playerMovement.playerDistanceJoint.enabled = true;
            playerMovement.playerDistanceJoint.distance = distanceJointOffsetForVehicle;

            interactionStartTime = Time.time;
            rb2d.mass = ridingStateMass;

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
        //playerMovement.transform.SetParent(null);
        playerMovement.playerDistanceJoint.connectedBody = null;
        playerMovement.playerDistanceJoint.enabled = false;

        rb2d.mass = defaultMass;

        interactionEndTime = Time.time;
        playerSpineAnimator.SetBool("InteractB", false);
        playerSpineAnimator.SetBool("PushObject", false);
        playerSpineAnimator.SetBool("PullObject", false);

        playerMovement.exitPushPullObjectState();
    }

    public override void OnUseObjectFunction1(bool useStatus, Collider2D cargoCollider2D)
    {
        base.OnUseObjectFunction1(useStatus);
        if (cargoCollider2D.transform.GetComponent<InteractionBase>().inUse == true) return;

        Debug.Log("object9 " + cargoCollider2D.gameObject);
        if (useStatus == true)
        {
            if (!cargoBodyColliders.Contains(cargoCollider2D)) cargoBodyColliders.Add(cargoCollider2D);
            cargoCollider2D.GetComponent<Rigidbody2D>().mass = cargoConvertedMass;
            cargoCollider2D.transform.GetComponent<InteractionBase>().inCargoState = true;
        }
        else
        {
            if (cargoBodyColliders.Contains(cargoCollider2D)) cargoBodyColliders.Remove(cargoCollider2D);
            cargoCollider2D.GetComponent<Rigidbody2D>().mass = cargoCollider2D.transform.GetComponent<InteractionBase>().defaultMass;
            cargoCollider2D.transform.GetComponent<InteractionBase>().inCargoState = false;
        }
    }

    public override void OnUseObjectFunction2(bool useStatus)
    {
        base.OnUseObjectFunction2(useStatus);

        //When Player gets on the cart
        if (useStatus == true)
        {
            for (int i = 0; i < cargoBodyColliders.Count; i++)
            {
                cargoBodyColliders[i].attachedRigidbody.mass = cargoBodyColliders[i].GetComponent<InteractionBase>().defaultMass;
            }
        }
        //Player exits the cart
        else if (useStatus == false)
        {
            for (int i = 0; i < cargoBodyColliders.Count; i++)
            {
                cargoBodyColliders[i].attachedRigidbody.mass = cargoConvertedMass;
            }
        }
    }
}
