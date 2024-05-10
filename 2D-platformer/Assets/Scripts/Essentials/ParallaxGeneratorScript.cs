using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxGeneratorScript : MonoBehaviour
{
    public GameObjectArray[] backgroundObjectsLayers = new GameObjectArray[3];
    Rigidbody2D playerRigidbody;

    public bool playerMovingRight, playerMovingLeft, playerStandStill;
    public float playerMovementDirection, baseScrollSpeed;

    // Start is called before the first frame update
    void Start()
    {
        playerRigidbody = GameManager.Instance.playerTransform.GetComponent<Rigidbody2D>();
        playerMovingRight = playerMovingLeft = false;
        playerStandStill = true;
        playerMovementDirection = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerRigidbody.velocity.x > .5f)
        {
            playerStandStill = false;
            playerMovingRight = true;
            playerMovementDirection = -1.0f;
        }
        else if (playerRigidbody.velocity.x < -.5f)
        {
            playerStandStill = false;
            playerMovingLeft = true;
            playerMovementDirection = 1.0f;
        }
        else
        {
            playerStandStill = true;
            playerMovementDirection = 0.0f;
        }

        if (playerStandStill == false)
        {
            for (int i = 0; i < backgroundObjectsLayers.Length; i++)
            {
                float layerScrollSpeed = backgroundObjectsLayers[i].arrayPropertyValue;
                float scrollDirection = backgroundObjectsLayers[i].arrayPropertySign;

                for (int j = 0; j < backgroundObjectsLayers[i].gameObjectsArray.Length; j++)
                {
                    backgroundObjectsLayers[i].gameObjectsArray[j].transform.Translate(Vector3.right * baseScrollSpeed * layerScrollSpeed * scrollDirection * playerMovementDirection);
                }
            }
        }
    }
}
