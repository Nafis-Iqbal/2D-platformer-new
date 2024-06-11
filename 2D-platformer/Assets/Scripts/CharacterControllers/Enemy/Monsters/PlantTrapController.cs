using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class PlantTrapController : MonoBehaviour
{
    public Animator plantTrapAnimator;
    SpriteShapeAnchorScript spriteShapeAnchorPointScript;
    MovingPlatformScript platformMovementScript;
    public Transform playerTrapPosition, trapLengthRefPoint;
    RelativeJoint2D relativeJoint;

    //[HideInInspector]
    public GameObject objectOnTop;
    //public Vector3 objectInitialLocalPosition;
    public float objectInitialLocalDistance, currentLocalDistance;
    public float surfaceMovementValue, trappedPlayerDamage;
    public bool isObjectOnTop;
    public bool isObjectClosing;
    public bool isTrapClosed;
    public bool trapPlayer;
    public bool tetherPlayer, playerSuccessfullyTethered;
    public float trapCloseMovementThreshold, trapReopenDelay, tetherSpeed;
    float lastTrapClosedTime;
    // Start is called before the first frame update
    void Start()
    {
        spriteShapeAnchorPointScript = GetComponent<SpriteShapeAnchorScript>();
        platformMovementScript = GetComponent<MovingPlatformScript>();
        relativeJoint = GetComponent<RelativeJoint2D>();

        isObjectOnTop = false;
        isObjectClosing = false;
        isTrapClosed = false;

        if (platformMovementScript.isMovementActive)
        {
            relativeJoint.enabled = false;
        }
    }

    void OnEnable()
    {
        isObjectOnTop = false;
        isObjectClosing = false;
        isTrapClosed = false;
        trapPlayer = false;
        tetherPlayer = false;
        playerSuccessfullyTethered = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isObjectOnTop || platformMovementScript.isMovementActive)
        {
            spriteShapeAnchorPointScript.movePointAlongY = true;
        }
        else
        {
            spriteShapeAnchorPointScript.movePointAlongY = false;
        }

        if (trapPlayer == true && !tetherPlayer)
        {
            if (isObjectOnTop && objectOnTop.transform.CompareTag("Player"))
            {
                PlayerInputManager.Instance.DisablePlayerControls();
                objectOnTop.transform.GetComponent<PlayerCombatSystem>().TakeProjectileDamage(trappedPlayerDamage);
                tetherPlayer = true;
            }
        }

        if (tetherPlayer && playerSuccessfullyTethered == false)
        {
            if (Vector2.Distance(objectOnTop.transform.position, playerTrapPosition.position) > .1f)
            {
                objectOnTop.transform.position = Vector2.MoveTowards(objectOnTop.transform.position, playerTrapPosition.position, tetherSpeed * Time.deltaTime);
            }
            else playerSuccessfullyTethered = true;
        }

        if (isObjectOnTop && objectOnTop != null && isTrapClosed == false)
        {
            //surfaceMovementValue = objectInitialLocalPosition.x - objectOnTop.transform.localPosition.x;
            currentLocalDistance = Vector2.Distance(objectOnTop.transform.position, trapLengthRefPoint.position);
            surfaceMovementValue = Mathf.Abs(currentLocalDistance - objectInitialLocalDistance);

            //if (Mathf.Abs(objectInitialLocalPosition.x - objectOnTop.transform.position.x) > trapCloseMovementThreshold)
            if (surfaceMovementValue > trapCloseMovementThreshold)
            {
                plantTrapAnimator.SetTrigger("CloseTrap");
                lastTrapClosedTime = Time.time + 2.5f;
                isTrapClosed = true;
            }
        }
        else if (isTrapClosed == true && Time.time - lastTrapClosedTime > trapReopenDelay)
        {
            plantTrapAnimator.SetTrigger("ReopenTrap");
            isTrapClosed = false;

            PlayerInputManager.Instance.EnablePlayerControls();
            tetherPlayer = false;
            trapPlayer = false;
            playerSuccessfullyTethered = false;
        }
    }
}
