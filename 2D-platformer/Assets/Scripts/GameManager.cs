using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public Transform playerTransform;
    public CinemachineVirtualCamera virtualCamera;
    public Animator playerAnimator;
    public static GameManager Instance;

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
    }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        virtualCamera.Follow = playerTransform;

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("PlayerShuriken"));
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("PlayerProjectile"));
    }
}
