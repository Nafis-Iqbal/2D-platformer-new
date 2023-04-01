using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public Transform playerTransform;
    public CinemachineVirtualCamera virtualCamera;
    public Animator playerAnimator;
    public Animator playerSpineAnimator;
    public static GameManager Instance;
    public GameObject lightAttackHitBox;

    private void Start() {
        if (playerTransform == null) {
            Debug.LogError("playerTransform is required. Drag and drop player object.");
        }
        if (virtualCamera == null) {
            Debug.LogError("virtualCamera is required. Drag and drop Follow camera object.");
        }
        if (playerAnimator == null) {
            Debug.LogError("playerAnimator is required. Drag and drop player sprite animator.");

        }
        if (playerSpineAnimator == null) {
            Debug.LogError("playerSpineAnimator is required. Drag and drop player spine animator.");

        }
        if (lightAttackHitBox == null) {
            Debug.LogError("lightAttackHitBox is required. Drag and drop player light hit box.");
        }
    }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        // virtualCamera.Follow = playerTransform;
    }
}
