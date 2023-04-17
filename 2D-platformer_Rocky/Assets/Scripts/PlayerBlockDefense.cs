using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class PlayerBlockDefense : MonoBehaviour {
    [SerializeField] private Slider playerStaminaSlider;
    [SerializeField] private GameObject blockDefenseObject;
    public float maxStamina = 100f;
    [SerializeField] private float normalStaminaBarFillRate = 2f;
    [SerializeField] private float blockingStaminaBarFillRate = 1f;
    [SerializeField] private float minimumStaminaToBlock = 20f;
    public float currentStamina;
    [SerializeField] private float staminaUIUpdateDuration = 0.3f;

    [Header("Debug fields")]
    [SerializeField] private float normalizedStamina = 1f;
    public bool isExecuting;
    [SerializeField] private bool isBlockingRequested = false;
    public bool isBlocking = false;

    private Animator playerAnimator;

    private void Awake() {
        playerAnimator = GameManager.Instance.playerAnimator;
        currentStamina = maxStamina;
    }

    private void Start() {
        if (playerStaminaSlider == null) {
            Debug.LogError("playerStaminaSlider is required. Drag and drop stamina slider object.");
        }
        if (blockDefenseObject == null) {
            Debug.LogError("blockDefenseObject is required. Drag and drop block defense object.");
        }
    }

    private void Update() {
        if (isBlockingRequested && currentStamina >= minimumStaminaToBlock) {
            isBlocking = true;
            blockDefenseObject.SetActive(true);
            playerAnimator.SetBool("isBlocking", true);
        } else {
            isBlocking = false;
            blockDefenseObject.SetActive(false);
            playerAnimator.SetBool("isBlocking", false);
        }
        isExecuting = isBlocking;
        if (isBlocking) {
            currentStamina += blockingStaminaBarFillRate * Time.deltaTime;
        } else {
            currentStamina += normalStaminaBarFillRate * Time.deltaTime;
        }
        StaminaBoundCheck();
        UpdateStaminaUI();
        PlayerCombatManager.Instance.isBlocking = isBlocking;
    }

    public void OnBlockDefense(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            isBlockingRequested = true;
        } else if (context.phase == InputActionPhase.Canceled) {
            isBlockingRequested = false;
        }
    }

    public void HandleAttack(int damageAmount) {
        currentStamina -= damageAmount;
        Debug.Log($"stamina decrease: {damageAmount}");
    }

    public void UpdateStaminaUI() {
        normalizedStamina = currentStamina / maxStamina;
        DOTween.To(() => playerStaminaSlider.value, x => playerStaminaSlider.value = x, normalizedStamina, staminaUIUpdateDuration);
        StaminaBoundCheck();
    }

    public void StaminaBoundCheck() {
        if (currentStamina > maxStamina) {
            currentStamina = maxStamina;
        }
        if (currentStamina <= 0f) {
            currentStamina = 0f;
            normalizedStamina = 0f;
        }
    }
}
