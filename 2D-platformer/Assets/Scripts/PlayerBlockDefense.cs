using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class PlayerBlockDefense : MonoBehaviour {
    private Animator playerAnimator;
    [SerializeField] private Slider playerStaminaSlider;
    public bool isExecuting;
    public bool isBlocking = false;
    [SerializeField] private bool isBlockingRequested = false;
    [SerializeField] private GameObject blockDefenseObject;
    [SerializeField] private float normalStaminaBarFillRate = 2f;
    [SerializeField] private float blockingStaminaBarFillRate = 1f;
    public float maxStamina = 100f;
    [SerializeField] private float minimumStaminaToBlock = 20f;
    public float currentStamina;
    [SerializeField] private float normalizedStamina = 1f;
    [SerializeField] private float staminaUIUpdateDuration = 0.3f;

    private void Awake() {
        playerAnimator = GameManager.Instance.playerAnimator;
        currentStamina = maxStamina;
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
