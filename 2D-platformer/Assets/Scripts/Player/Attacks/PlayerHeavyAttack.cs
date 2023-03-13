using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHeavyAttack : MonoBehaviour {
    [SerializeField] private float totalChargeAmount = 100f;
    [SerializeField] private float chargeFillRate = 2f;
    [SerializeField] private float currentCharge = 0f;
    public int successfulAttackDamage = 20;
    public int failedAttackDamage = 12;
    [HideInInspector] public int attackDamage;
    private bool isKeyPressed = false;

    [HideInInspector] public float attackCooldownTime = 1f;
    [HideInInspector] public float attackDamageMultiplier = 1f;
    [Header("Debug fields")]
    [SerializeField] private float timeElapsedSinceAttack = 0f;
    [SerializeField] private bool canAttack = true;
    [SerializeField] private bool attackDone = true;
    public bool isExecuting;

    private Animator playerAnimator;

    private void Awake() {
        playerAnimator = GameManager.Instance.playerAnimator;
    }

    private void Update() {
        if (attackDone) {
            timeElapsedSinceAttack += Time.deltaTime;
            if (timeElapsedSinceAttack > attackCooldownTime) {
                canAttack = true;
                timeElapsedSinceAttack = 0f;
                attackDone = false;
            } else {
                canAttack = false;
            }
        }

        isExecuting = (isKeyPressed && canAttack);

        if (isKeyPressed && canAttack) {
            ChargeForAttack();
        } else {
            currentCharge = 0f;
        }
        if (currentCharge >= totalChargeAmount) {
            ExecuteHighPoweredAttack();
        }
        BoundCheckCharge();
        attackDamage = (int)Math.Ceiling(successfulAttackDamage * attackDamageMultiplier);

    }

    private void ChargeForAttack() {
        currentCharge += Time.deltaTime * chargeFillRate;
        playerAnimator.SetBool("isCharging", true);
    }

    private void ExecuteHighPoweredAttack() {
        Debug.Log("heavy attack (high damage)...");
        attackDamageMultiplier = 1f;
        isKeyPressed = false;
        currentCharge = 0f;
        playerAnimator.SetBool("isCharging", false);
        playerAnimator.Play("heavy attack");
        timeElapsedSinceAttack = 0f;
        attackDone = true;
    }

    private void BoundCheckCharge() {
        if (currentCharge < 0f) {
            currentCharge = 0f;
        } else if (currentCharge > totalChargeAmount) {
            currentCharge = totalChargeAmount;
        }
    }

    public void OnHeavyAttack(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            isKeyPressed = true;
        } else if (context.phase == InputActionPhase.Canceled && isKeyPressed) {
            if (canAttack) {
                ExecuteLowPoweredAttack();
            } else {
                isKeyPressed = false;
            }
        }
    }

    private void ExecuteLowPoweredAttack() {
        Debug.Log("heavy attack (low damage)...");
        attackDamageMultiplier = (float)failedAttackDamage / successfulAttackDamage;
        currentCharge = 0f;
        isKeyPressed = false;
        playerAnimator.SetBool("isCharging", false);
        playerAnimator.Play("heavy attack");
        timeElapsedSinceAttack = 0f;
        attackDone = true;
    }
}
