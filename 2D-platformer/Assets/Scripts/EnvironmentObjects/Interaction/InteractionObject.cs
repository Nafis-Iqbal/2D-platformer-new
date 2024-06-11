using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionObject : MonoBehaviour
{
    public GameObject uiPrompt;
    public bool hasAmbiguousColliderChecks;
    public Transform interactineReferenceObject;
    public float maximumCheckDistance;
    public float promptFloatingSpace, floatSpeed;
    bool moveDown;
    Vector3 startPosition;

    public bool isPlayerInZone;
    public int targetSpawnPositionID;
    public InteractionBase interactionBaseScript;
    [Tooltip("ID of the current object from which interaction is being done, ID represents nature of current transitional object, like doors or portals")] public int currentObjectID;
    [Tooltip("ID of the desired object that needs to be interacted with, ID represents nature of interaction")] public int targetObjectID;
    public int playerCount;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = uiPrompt.transform.localPosition;
        moveDown = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerInZone && !interactionBaseScript.inUse)
        {
            if (!uiPrompt.activeInHierarchy) uiPrompt.SetActive(true);
            if (moveDown)
            {
                if (uiPrompt.transform.localPosition.y > startPosition.y - promptFloatingSpace) uiPrompt.transform.Translate(Vector3.down * floatSpeed * Time.deltaTime);
                else moveDown = false;
            }
            else
            {
                if (uiPrompt.transform.localPosition.y < startPosition.y) uiPrompt.transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);
                else moveDown = true;
            }
        }
        else
        {
            uiPrompt.SetActive(false);
            //uiPrompt.transform.position = startPosition;
            moveDown = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Always focus on active interaction object for player, in PlayerMovement script for calculations to avoid bugs and ambiguity
        if (other.isTrigger == false && other.transform.CompareTag("Player") && Time.time - interactionBaseScript.interactionEndTime > 1.0f)
        {
            Debug.Log("player object:" + other.gameObject);
            if (hasAmbiguousColliderChecks)
            {
                //Debug.Log("Object: " + interactionBaseScript.transform.gameObject + " Dist1: " + Mathf.Abs(other.transform.position.x - interactineReferenceObject.position.x));
                if (Mathf.Abs(other.transform.position.x - interactineReferenceObject.position.x) > maximumCheckDistance) return;
            }

            if (GameManager.Instance.playerMovementScript.interactionObject == null)
            {
                playerCount++;
                isPlayerInZone = true;
                interactionBaseScript.isInteractionEnabled = true;

                interactionBaseScript.targetObjectID = targetObjectID;
                interactionBaseScript.targetSpawnPositionID = targetSpawnPositionID;
                GameManager.Instance.playerMovementScript.interactionObject = this;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //Always focus on active interaction object for player, in PlayerMovement script for calculations to avoid bugs and ambiguity
        if (GameManager.Instance.playerMovementScript.interactionObject == null) return;

        if (other.isTrigger == false && other.transform.CompareTag("Player") && Time.time - GameManager.Instance.playerMovementScript.interactionObject.interactionBaseScript.interactionStartTime > 1.0f)
        {
            if (hasAmbiguousColliderChecks)
            {
                //Debug.Log("Object: " + interactionBaseScript.transform.gameObject + " Dist1: " + Mathf.Abs(other.transform.position.x - interactineReferenceObject.position.x));
                if (Mathf.Abs(other.transform.position.x - interactineReferenceObject.position.x) > maximumCheckDistance) return;
            }
            Debug.Log("Time: " + Time.time + "int time: " + interactionBaseScript.interactionStartTime + " int script object " + interactionBaseScript.transform.gameObject);
            playerCount--;
            if (playerCount <= 0 && GameManager.Instance.playerMovementScript.interactionObject != null)
            {
                isPlayerInZone = false;
                playerCount = 0;
                interactionBaseScript.isInteractionEnabled = false;
                interactionBaseScript.targetObjectID = -1;

                GameManager.Instance.playerMovementScript.interactionObject = null;
            }
        }
    }
}
