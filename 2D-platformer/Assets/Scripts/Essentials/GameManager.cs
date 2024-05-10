using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Spine.Unity;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform playerTransform;
    public PlayerMovement playerMovementScript;
    public int playerCurrentPlatformID = -1, playerCurrentPlatformLevel = -1;
    public CinemachineVirtualCamera virtualCamera;
    public Animator playerSpineAnimator;
    public Rigidbody2D playerRB2D;
    public static GameManager Instance;
    public int playerSortOrderInLayer;

    private void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("playerTransform is required. Drag and drop player object.");
        }
        if (virtualCamera == null)
        {
            Debug.LogError("virtualCamera is required. Drag and drop Follow camera object.");
        }
        if (playerSpineAnimator == null)
        {
            Debug.LogError("playerSpineAnimator is required. Drag and drop player spine animator.");
        }

        playerMovementScript = playerTransform.GetComponent<PlayerMovement>();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
}
