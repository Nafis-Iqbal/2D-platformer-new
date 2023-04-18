using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwordAttack : MonoBehaviour {
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private float timeElapsedSinceAttack = 0f;
    [HideInInspector] public float attackCooldownTime = 1f;
    [SerializeField] private bool isAttacking = false;

    private PlayerColumn playerColumn;

    private void Awake() {
        playerColumn = GameManager.Instance.playerTransform.GetComponent<PlayerColumn>();
    }

    private void Update() {
        if (isAttacking) {
            timeElapsedSinceAttack += Time.deltaTime;
            if (timeElapsedSinceAttack > attackCooldownTime) {
                timeElapsedSinceAttack = 0f;
                isAttacking = false;
            }
        }

    }
    public void OnSwordAttack(InputAction.CallbackContext context) {
        if (!isAttacking && !playerColumn.hasGrabbedColumn) {
            isAttacking = true;
            playerAnimator.Play("sword attack");
        }
    }
}
