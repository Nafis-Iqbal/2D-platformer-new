using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportCartController : InteractionBase
{
    Rigidbody2D transportCartRB2D;
    [Header("Drag & Drops")]
    public Animator[] wheelsAnimator = new Animator[4];
    public Transform wheelGear;
    public Transform playerDrivePosition;
    public LevelSectionController currentLevelSectionScript;

    [Header("Wheel Visuals")]
    public float maxWheelSpeed = 3.0f;
    public float visualSpeedMultiplier = .1f, gearRotateSpeed;

    [Header("Cart Movement Speeds")]
    public float normalAcceleration;
    public float brakedDecceleration, boostedAcceleration;
    public float normalVelocity, brakedVelocity, boostedVelocity;
    public float rampBoostedTimeLimit;
    public float currentAcceleration, currentVelocity;
    float lastSpeedBoostedTime;

    [Header("Cart Orientation")]
    public float groundCheckHeight;
    public float maxCartRotationAngle;
    public float currentCartRotationAngle;
    public float orientationCorrectionSpeed = 20.0f;
    public GameObject stabilizerCheckObject1, stabilizerCheckObject2;
    public Transform initialStabilizerPosition1B, initialStabilizerPosition2B;

    [Header("States & Others")]
    public float inUseCameraZoomValue;
    public bool cartAccelerating, cartDeccelerating;
    public float defaultMass, ridingStateMass;
    public bool facingRight, speedAlteredState, boostedByRampState;
    [HideInInspector]
    public float lastCarDecceleratingTime;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        transportCartRB2D = GetComponent<Rigidbody2D>();
        transportCartRB2D.mass = defaultMass;
        currentAcceleration = normalAcceleration;
        currentVelocity = normalVelocity;
        lastSpeedBoostedTime = lastCarDecceleratingTime = Time.time;
        boostedByRampState = false;
        cartAccelerating = cartDeccelerating = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (cartDeccelerating == false && Time.time - lastCarDecceleratingTime > 2.5f)
        {
            currentLevelSectionScript.alterRampCollidersState(true);
        }

        //REVERT TO DEFAULT CAR SPEED
        if (!speedAlteredState && !boostedByRampState)
        {
            currentAcceleration = normalAcceleration;
            currentVelocity = normalVelocity;
            cartAccelerating = true;
        }

        //WHEEL VISUAL SPEED ASSIGNMENT 
        if (transportCartRB2D.velocity.x > 0.03f)//
        {
            for (int i = 0; i < wheelsAnimator.Length; i++)
            {
                wheelsAnimator[i].SetFloat("MovementSpeed", Mathf.Min(transportCartRB2D.velocity.x * visualSpeedMultiplier, maxWheelSpeed));
            }

            if (wheelGear != null) wheelGear.Rotate(new Vector3(0.0f, 0.0f, -1.0f), gearRotateSpeed);
        }
        else if (transportCartRB2D.velocity.x < -0.03f)
        {
            for (int i = 0; i < wheelsAnimator.Length; i++)
            {
                wheelsAnimator[i].SetFloat("MovementSpeed", Mathf.Min(transportCartRB2D.velocity.x * visualSpeedMultiplier, maxWheelSpeed));
            }

            if (wheelGear != null) wheelGear.Rotate(Vector3.forward, gearRotateSpeed);
        }
        else//All wheels stop
        {
            for (int i = 0; i < wheelsAnimator.Length; i++)
            {
                wheelsAnimator[i].SetFloat("MovementSpeed", 0.0f);
            }
        }

        if (boostedByRampState)
        {
            if (Time.time - lastSpeedBoostedTime > rampBoostedTimeLimit)
            {
                currentVelocity = normalVelocity;
                boostedByRampState = false;
                transportCartRB2D.gravityScale = 4.0f;
            }
        }

        onGround = GroundCheck();
        if (onGround) transportCartRB2D.gravityScale = 4.0f;

        //ADD FORCE, PRODUCE ACCELERATION FOR CART
        if (boostedByRampState || (inUse && onGround))
        {
            if (facingRight)//Moving Right
            {
                if (cartAccelerating && transportCartRB2D.velocity.x < currentVelocity)
                {
                    transportCartRB2D.AddForce(currentAcceleration * Vector2.right);
                }
                else if (cartDeccelerating && transportCartRB2D.velocity.x > currentVelocity)
                {
                    transportCartRB2D.AddForce(currentAcceleration * Vector2.right);
                }
            }
            else if (!facingRight && transportCartRB2D.velocity.x > -currentVelocity)//Moving Left
            {
                transportCartRB2D.AddForce(currentAcceleration * Vector2.left);
            }

            if (!boostedByRampState) transportCartRB2D.gravityScale = 4.0f;
        }
        else if (onGround == false)//Force downwards, so that it stays grounded
        {
            transportCartRB2D.gravityScale = 10.0f;
            //transportCartRB2D.angularDrag = 5.0f;
        }

        if (onGround == false)
        {
            currentCartRotationAngle = transform.localRotation.eulerAngles.z;
            if (currentCartRotationAngle > maxCartRotationAngle || currentCartRotationAngle < -maxCartRotationAngle)
            {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0.0f, 0.0f, 0.0f), orientationCorrectionSpeed);
            }
        }

        //Gizmos.color = Color.red;
        Debug.DrawLine(stabilizerCheckObject1.transform.position, initialStabilizerPosition1B.position);
        Debug.DrawLine(stabilizerCheckObject2.transform.position, initialStabilizerPosition2B.position);
    }

    public override void OnInteract()
    {
        if (!inUse)
        {
            transportCartRB2D.mass = ridingStateMass;

            playerSpineAnimator.SetInteger("InteractID", targetObjectID);
            playerSpineAnimator.SetBool("InteractB", true);

            PlayerInputManager.Instance.playerInputActions.Player.UseObjectFunction1.Enable();
            PlayerInputManager.Instance.playerInputActions.Player.UseObjectFunction2.Enable();

            CameraController.Instance.objectInUseZoomValue = inUseCameraZoomValue;
            CameraController.Instance.wideAngleObjectInUse = true;

            playerMovement.transform.position = playerDrivePosition.position;

            GameManager.Instance.playerRB2D.simulated = false;
            interactionStartTime = Time.time;

            playerMovement.setParent(transform);
            playerMovement.enterObjectControlState();

            inUse = true;

            if (facingRight)
            {
                if (playerMovement.playerFacingRight == false)
                {
                    playerMovement.rotateRight();
                }
            }
            else
            {
                if (playerMovement.playerFacingRight == true)
                {
                    playerMovement.rotateLeft();
                }
            }
        }
        else
        {
            transportCartRB2D.mass = defaultMass;

            playerSpineAnimator.SetBool("InteractB", false);

            PlayerInputManager.Instance.playerInputActions.Player.UseObjectFunction1.Disable();
            PlayerInputManager.Instance.playerInputActions.Player.UseObjectFunction2.Disable();

            CameraController.Instance.wideAngleObjectInUse = false;
            GameManager.Instance.playerRB2D.simulated = true;
            playerMovement.setParent(null);
            playerMovement.exitObjectControlState();

            inUse = false;
        }
    }

    public override void OnUseObjectFunction1(bool useStatus)
    {
        if (boostedByRampState == true) return;

        if (useStatus)
        {
            currentAcceleration = boostedAcceleration;
            currentVelocity = boostedVelocity;
            cartAccelerating = true;

            cartDeccelerating = false;
            PlayerInputManager.Instance.playerInputActions.Player.UseObjectFunction2.Disable();
        }
        else
        {
            currentAcceleration = normalAcceleration;
            currentVelocity = normalVelocity;

            PlayerInputManager.Instance.playerInputActions.Player.UseObjectFunction2.Enable();
        }

        speedAlteredState = useStatus;
    }

    public override void OnUseObjectFunction2(bool useStatus)
    {
        if (boostedByRampState == true) return;

        if (useStatus)
        {
            currentAcceleration = brakedDecceleration;
            currentVelocity = brakedVelocity;
            cartDeccelerating = true;
            currentLevelSectionScript.alterRampCollidersState(false);

            cartAccelerating = false;
            PlayerInputManager.Instance.playerInputActions.Player.UseObjectFunction1.Disable();
        }
        else
        {
            currentAcceleration = normalAcceleration;
            currentVelocity = normalVelocity;
            cartDeccelerating = false;
            lastCarDecceleratingTime = Time.time;
            cartAccelerating = true;

            PlayerInputManager.Instance.playerInputActions.Player.UseObjectFunction1.Enable();
        }

        speedAlteredState = useStatus;
    }

    public void BoostSpeedByRamp(float boostMultiplier)
    {
        if (!boostedByRampState)
        {
            lastSpeedBoostedTime = Time.time;
            currentVelocity *= boostMultiplier;
            currentAcceleration *= boostMultiplier;
            boostedByRampState = true;
            transportCartRB2D.gravityScale = 1.0f;
        }
    }

    bool GroundCheck()
    {
        LayerMask mask = LayerMask.GetMask("Ground");

        //RaycastHit2D Hit2DFront = Physics2D.Raycast(stabilizerCheckObject1.transform.position, stabilizerCheckObject1.transform.position - initialStabilizerPosition1B.position, groundCheckHeight, mask);
        // RaycastHit2D Hit2DBack = Physics2D.Raycast(stabilizerCheckObject2.transform.position, stabilizerCheckObject2.transform.position - initialStabilizerPosition2B.position, groundCheckHeight, mask);

        RaycastHit2D Hit2DFront = Physics2D.Raycast(stabilizerCheckObject1.transform.position, initialStabilizerPosition1B.position - stabilizerCheckObject1.transform.position, groundCheckHeight, mask);
        RaycastHit2D Hit2DBack = Physics2D.Raycast(stabilizerCheckObject2.transform.position, initialStabilizerPosition2B.position - stabilizerCheckObject2.transform.position, groundCheckHeight, mask);

        if (Hit2DFront.collider == null || Hit2DBack.collider == null) return false;
        else if (Hit2DFront.collider.CompareTag("Platforms") && Hit2DBack.collider.CompareTag("Platforms")) return true;
        else return false;
    }

    // void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.CompareTag("Platforms"))
    //     {
    //         onGround = true;
    //     }
    // }

    // void OnTriggerExit2D(Collider2D other)
    // {
    //     if (other.CompareTag("Platforms"))
    //     {
    //         onGround = false;
    //     }
    // }
}
