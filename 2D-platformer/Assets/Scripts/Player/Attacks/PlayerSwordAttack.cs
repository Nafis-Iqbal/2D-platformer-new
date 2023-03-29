using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwordAttack : MonoBehaviour {
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private float timeElapsedSinceAttack = 0f;
    [HideInInspector] public float attackCooldownTime = 1f;
    public bool isExecuting = false;

    private PlayerColumn playerColumn;

    private void Awake() {
        playerColumn = GameManager.Instance.playerTransform.GetComponent<PlayerColumn>();
    }

    private void Update() {
        if (isExecuting) {
            timeElapsedSinceAttack += Time.deltaTime;
            if (timeElapsedSinceAttack > attackCooldownTime) {
                timeElapsedSinceAttack = 0f;
                // isAttacking = false;
            }
        }

    }
    public void OnSwordAttack(InputAction.CallbackContext context) {
        if (!isExecuting && !playerColumn.hasGrabbedColumn) {
            // isAttacking = true;
            playerAnimator.Play("sword attack");
            GameManager.Instance.playerSpineAnimator.Play("sword attack");
        }
    }
}
