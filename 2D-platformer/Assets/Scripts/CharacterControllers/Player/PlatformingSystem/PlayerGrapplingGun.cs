using System;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrapplingGun : MonoBehaviour
{
    public bool playerIsOnAir = false;
    [Header("Scripts Ref:")]
    public GrapplingRope grapplingRope;
    private PlayerMovement playerMovement;
    private Transform playerTransform;
    [SerializeField] private PlayerJump playerJump;
    public Animator playerSpineAnimator;

    [Header("Main Camera:")]
    public Camera m_camera;

    [Header("Transform Ref:")]
    public Transform gunHolder;
    public Transform gunPivot;
    public Transform firePoint;

    [Header("Physics Ref:")]
    public SpringJoint2D m_springJoint2D;
    public Rigidbody2D m_rigidbody;

    [Header("Distance:")]
    [SerializeField] private bool hasMaxDistance = false;
    [SerializeField] private float maxDistnace = 20;

    private enum LaunchType
    {
        Transform_Launch,
        Physics_Launch
    }

    [Header("Launching:")]
    [SerializeField] private bool launchToPoint = true;
    [SerializeField] private LaunchType launchType = LaunchType.Physics_Launch;
    [SerializeField] private float launchSpeed = 1;

    [Header("No Launch To Point")]
    [SerializeField] private bool autoConfigureDistance = false;
    [SerializeField] private float targetDistance = 3;
    [SerializeField] private float targetFrequncy = 1;

    [HideInInspector] public Vector2 grapplePoint;
    [HideInInspector] public Vector2 grappleDistanceVector;
    [Header("Player State")]
    public bool canGrapple = false;
    public bool isExecuting = false;

    [Header("Release Jump")]
    public float hangSwingForce = 10f;
    [SerializeField] private float jumpButtonHoldTime = 0.5f;
    [SerializeField, Range(0f, 15.0f)] private float minReleaseForceX = 1f;
    [SerializeField, Range(0f, 15.0f)] private float minReleaseForceY = 1f;
    public float grappleJumpForce;
    [SerializeField] private float releaseRopeMovementDisableDuration = 1f;

    [Header("Swinging Tune")]
    public bool swingEndAnimPlaying = false;
    [SerializeField, Range(0f, 50.0f)] public float requiredTurnVelocity;
    bool playerTilted = false;
    public float playerTiltOffset;
    [Range(0f, 30.0f)] public float grappleBoostIncrement;
    public float maxSwingVelocity;
    Quaternion currentPlayerRotation;
    Vector2 grapplePointAngleVector;
    float angleWithGrapplePoint;
    float lastFrameVelocity;
    Vector2 tempSwingVelocity;

    private void Awake()
    {
        playerTransform = GameManager.Instance.playerTransform;
        playerJump = GetComponent<PlayerJump>();
        playerMovement = GameManager.Instance.playerTransform.GetComponent<PlayerMovement>();
        playerSpineAnimator = GameManager.Instance.playerSpineAnimator;
    }

    private void OnEnable()
    {
        playerTilted = false;
    }

    private void Start()
    {
        grapplingRope.enabled = false;
        m_springJoint2D.enabled = false;
        lastFrameVelocity = m_rigidbody.velocity.x;

    }

    private void Update()
    {
        if (isExecuting)
        {

            if (m_rigidbody.velocity.x > 0f)
            {
                playerMovement.rotateRight();
            }
            else if (m_rigidbody.velocity.x < 0f)
            {
                playerMovement.rotateLeft();
            }

            //Swing End mechanics
            //Debug.Log("swing: " + m_rigidbody.velocity.x);
            if (Mathf.Abs(m_rigidbody.velocity.x) < lastFrameVelocity)//Player moving towards amplitude
            {
                if (Mathf.Abs(m_rigidbody.velocity.x) < requiredTurnVelocity)
                {
                    playerSpineAnimator.SetBool("GrappleSwingEnd", true);
                }
            }
            else if (Mathf.Abs(m_rigidbody.velocity.x) > lastFrameVelocity)//Player moving towards center
            {
                playerSpineAnimator.SetBool("GrappleSwingEnd", false);
            }

            lastFrameVelocity = Mathf.Abs(m_rigidbody.velocity.x);

            grapplePointAngleVector = m_springJoint2D.connectedAnchor - new Vector2(gunPivot.position.x, gunPivot.position.y);
            angleWithGrapplePoint = Vector2.SignedAngle(grapplePointAngleVector, Vector2.up);
            currentPlayerRotation.eulerAngles = new Vector3(0.0f, 0.0f, -angleWithGrapplePoint + playerTiltOffset);
            playerTransform.localRotation = currentPlayerRotation;
            playerTilted = true;
            //Debug.Log("Angle GGP" + angleWithGrapplePoint);
        }
        else
        {
            if (playerTilted == true)
            {
                playerTilted = false;
                currentPlayerRotation.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                playerTransform.localRotation = currentPlayerRotation;
            }
        }
    }

    public void OnGrapplingGun(InputAction.CallbackContext context)//grapple & release
    {
        if (!grapplingRope.isGrappling)
        {
            if (canGrapple)
            {
                playerSpineAnimator.SetBool("GrapplingHookThrow", true);
                SetGrapplePoint();
                isExecuting = true;
            }
        }
        else
        {
            PlayerInputManager.Instance.playerInputActions.Player.GrappleBoost.Disable();

            playerSpineAnimator.SetBool("GrapplingHookThrow", false);
            playerSpineAnimator.SetBool("GrappleSwingEnd", false);
            grapplingRope.enabled = false;
            m_springJoint2D.enabled = false;

            // m_rigidbody.AddForce(new Vector2(
            //     m_rigidbody.velocity.x + minReleaseForceX,
            //     m_rigidbody.velocity.y + minReleaseForceY),
            //     ForceMode2D.Impulse);
            Vector2 grappleJumpDirection = Vector2.Perpendicular(m_springJoint2D.connectedAnchor - new Vector2(gunPivot.position.x, gunPivot.position.y)).normalized;

            if (m_rigidbody.velocity.x > 0f)
            {
                playerMovement.rotateRight();

                // m_rigidbody.AddForce(new Vector2(
                // m_rigidbody.velocity.x + minReleaseForceX,
                // m_rigidbody.velocity.y + minReleaseForceY),
                // ForceMode2D.Impulse);
                m_rigidbody.velocity = -grappleJumpDirection * grappleJumpForce;

                // m_rigidbody.AddForce(-grappleJumpDirection * grappleJumpForce,
                // ForceMode2D.Impulse);
            }
            else if (m_rigidbody.velocity.x < 0f)
            {
                playerMovement.rotateLeft();

                // m_rigidbody.AddForce(new Vector2(
                // m_rigidbody.velocity.x - minReleaseForceX,
                // m_rigidbody.velocity.y + minReleaseForceY),
                m_rigidbody.velocity = grappleJumpDirection * grappleJumpForce;

                // m_rigidbody.AddForce(grappleJumpDirection * grappleJumpForce,
                // ForceMode2D.Impulse);
            }
            isExecuting = false;
        }
    }

    public void OnGrappleBoost(InputAction.CallbackContext context)
    {
        tempSwingVelocity = m_rigidbody.velocity;

        if (context.ReadValue<float>() > 0f)
        {
            tempSwingVelocity.x += grappleBoostIncrement;
        }
        else if (context.ReadValue<float>() < 0f)
        {
            tempSwingVelocity.x -= grappleBoostIncrement;
        }

        if (tempSwingVelocity.x > maxSwingVelocity) tempSwingVelocity.x = maxSwingVelocity;
        else if (Mathf.Abs(tempSwingVelocity.x) > maxSwingVelocity) tempSwingVelocity.x = -maxSwingVelocity;

        m_rigidbody.velocity = tempSwingVelocity;
    }

    void SetGrapplePoint()
    {
        grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position;
        grapplingRope.enabled = true;
    }

    public void Grapple()
    {
        m_springJoint2D.autoConfigureDistance = false;
        if (!launchToPoint && !autoConfigureDistance)
        {
            m_springJoint2D.distance = targetDistance;
            m_springJoint2D.frequency = targetFrequncy;
        }
        if (!launchToPoint)
        {
            if (autoConfigureDistance)
            {
                m_springJoint2D.autoConfigureDistance = true;
                m_springJoint2D.frequency = 0;
            }

            m_springJoint2D.connectedAnchor = grapplePoint;
            m_springJoint2D.enabled = true;
        }
        else
        {
            switch (launchType)
            {
                case LaunchType.Physics_Launch:
                    m_springJoint2D.connectedAnchor = grapplePoint;

                    Vector2 distanceVector = firePoint.position - gunHolder.position;

                    m_springJoint2D.distance = distanceVector.magnitude;
                    m_springJoint2D.frequency = launchSpeed;
                    m_springJoint2D.enabled = true;
                    break;
                case LaunchType.Transform_Launch:
                    m_rigidbody.velocity = Vector2.zero;
                    break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint != null && hasMaxDistance)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxDistnace);
        }
    }

    public void PlayerReleasedRope()
    {
        playerIsOnAir = true;
        StartCoroutine(PlayerIsNotOnAirCoroutine());
    }

    IEnumerator PlayerIsNotOnAirCoroutine()
    {
        yield return new WaitForSeconds(releaseRopeMovementDisableDuration);
        playerIsOnAir = false;
    }

    public void UpdateGrappleTargetPosition(Vector3 position)
    {
        if (!grapplingRope.isGrappling)
        {
            grapplePoint = position;
        }
    }
}
