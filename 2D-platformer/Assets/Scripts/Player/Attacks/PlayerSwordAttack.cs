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
    [SerializeField] private bool attacked = false;

    private PlayerColumn playerColumn;

    private void Awake() {
        playerColumn = GameManager.Instance.playerTransform.GetComponent<PlayerColumn>();
    }

    private void Update() {
        if (attacked) {
            timeElapsedSinceAttack += Time.deltaTime;
            if (timeElapsedSinceAttack > attackCooldownTime) {
                timeElapsedSinceAttack = 0f;
                attacked = false;
                // isAttacking = false;
            }
        }

    }
    public void OnSwordAttack(InputAction.CallbackContext context) {
        if (!attacked && !isExecuting && !playerColumn.hasGrabbedColumn) {
            // isAttacking = true;
            attacked = true;
            playerAnimator.Play("light attack");
            GameManager.Instance.playerSpineAnimator.Play("light attack");
        }
    }
}
