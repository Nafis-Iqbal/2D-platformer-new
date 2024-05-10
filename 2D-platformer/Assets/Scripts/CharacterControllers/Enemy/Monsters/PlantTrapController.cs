using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class PlantTrapController : MonoBehaviour
{
    public Animator plantTrapAnimator;
    SpriteShapeAnchorScript spriteShapeAnchorPointScript;
    MovingPlatformScript platformMovementScript;

    [HideInInspector]
    public GameObject objectOnTop;
    public Vector3 objectInitialLocalPosition;
    public float surfaceMovementValue;
    public bool isObjectOnTop;
    public bool isObjectClosing;
    public bool isTrapClosed;
    public float trapCloseMovementThreshold, trapReopenDelay;
    float lastTrapClosedTime;
    // Start is called before the first frame update
    void Start()
    {
        spriteShapeAnchorPointScript = GetComponent<SpriteShapeAnchorScript>();
        platformMovementScript = GetComponent<MovingPlatformScript>();

        isObjectOnTop = false;
        isObjectClosing = false;
        isTrapClosed = false;
    }

    void OnEnable()
    {
        isObjectOnTop = false;
        isObjectClosing = false;
        isTrapClosed = false;
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

        if (isObjectOnTop && objectOnTop != null && isTrapClosed == false)
        {
            surfaceMovementValue = objectInitialLocalPosition.x - objectOnTop.transform.localPosition.x;
            if (Mathf.Abs(objectInitialLocalPosition.x - objectOnTop.transform.position.x) > trapCloseMovementThreshold)
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
        }
    }
}
