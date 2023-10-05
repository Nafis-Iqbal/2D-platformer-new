using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrapplingGun : MonoBehaviour
{
    public bool playerIsOnAir = false;
    [Header("Scripts Ref:")]
    public GrapplingRope grapplingRope;
    [SerializeField] private PlayerJump playerJump;

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
    public bool canGrapple = false;
    [SerializeField] private float jumpButtonHoldTime = 0.5f;
    [Header("Releasing:")]
    public float hangSwingForce = 10f;
    [SerializeField] private float releaseForceMultiplierX = 1f;
    [SerializeField] private float releaseForceMultiplierY = 1f;
    [SerializeField] private float releaseRopeMovementDisableDuration = 1f;

    public bool isExecuting = false;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerJump = GetComponent<PlayerJump>();
        playerMovement = GameManager.Instance.playerTransform.GetComponent<PlayerMovement>();
    }

    public void OnGrapplingGun(InputAction.CallbackContext context)
    {
        var playerSpineAnimator = GameManager.Instance.playerSpineAnimator;
        if (!grapplingRope.isGrappling)
        {
            if (canGrapple)
            {
                playerJump.Jump(jumpButtonHoldTime);
                playerSpineAnimator.SetBool("GrapplingHookThrow", true);
                SetGrapplePoint();
                isExecuting = true;
            }
        }
        else
        {
            playerSpineAnimator.SetBool("GrapplingHookThrow", false);
            grapplingRope.enabled = false;
            m_springJoint2D.enabled = false;
            m_rigidbody.AddForce(new Vector2(
                m_rigidbody.velocity.x * releaseForceMultiplierX,
                m_rigidbody.velocity.y * releaseForceMultiplierY),
                ForceMode2D.Impulse);
            if (m_rigidbody.velocity.x > 0f)
            {
                playerMovement.rotateRight();
            }
            else if (m_rigidbody.velocity.x < 0f)
            {
                playerMovement.rotateLeft();
            }
            isExecuting = false;
        }
    }

    // private void Update() {
    //     Debug.Log(m_rigidbody.velocity);
    // }

    private void Start()
    {
        grapplingRope.enabled = false;
        m_springJoint2D.enabled = false;

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
