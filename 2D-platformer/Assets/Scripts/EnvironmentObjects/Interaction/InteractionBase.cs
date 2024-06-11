using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionBase : MonoBehaviour
{
    public bool isInteractionEnabled;
    public bool inCargoState;
    public float defaultMass = 1000;
    public bool inUse;
    public bool onGround;
    public int targetObjectID;
    public int targetSpawnPositionID;
    //[HideInInspector]
    public float interactionStartTime, interactionEndTime;
    protected PlayerMovement playerMovement;
    protected Animator playerSpineAnimator;

    // Start is called before the first frame update
    protected void Start()
    {
        playerMovement = GameManager.Instance.playerMovementScript;
        playerSpineAnimator = GameManager.Instance.playerSpineAnimator;
        onGround = true;
    }

    // Update is called once per frame
    protected void Update()
    {

    }

    public virtual void OnInteract()
    {

    }

    public virtual void OnInteract(bool isRightSide)
    {

    }

    public virtual void OnUseObjectFunction1(bool useStatus)
    {

    }

    public virtual void OnUseObjectFunction1(bool useStatus, Collider2D bodyCollider2D)
    {

    }

    public virtual void OnUseObjectFunction2(bool useStatus)
    {

    }

    public virtual void ForceCloseInteraction()
    {

    }
}
