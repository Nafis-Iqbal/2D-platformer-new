using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public Transform playerTransform;
    public CinemachineVirtualCamera virtualCamera;
    public Animator playerAnimator;
    public static GameManager Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        virtualCamera.Follow = playerTransform;
    }
}
