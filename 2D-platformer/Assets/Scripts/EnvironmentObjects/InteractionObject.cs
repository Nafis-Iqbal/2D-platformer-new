using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionObject : MonoBehaviour
{
    public GameObject uiPrompt;
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
        if (other.transform.CompareTag("Player") && Time.time - interactionBaseScript.interactionEndTime > 1.0f)
        {
            playerCount++;
            isPlayerInZone = true;
            interactionBaseScript.isInteractionEnabled = true;
            interactionBaseScript.targetObjectID = targetObjectID;
            interactionBaseScript.targetSpawnPositionID = targetSpawnPositionID;
            GameManager.Instance.playerMovementScript.interactionObject = this;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform.CompareTag("Player") && Time.time - interactionBaseScript.interactionStartTime > 1.0f)
        {
            playerCount--;
            if (playerCount <= 0)
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
