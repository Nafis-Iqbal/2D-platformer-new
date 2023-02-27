using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWorldSlowDown : MonoBehaviour {

    [Range(0.01f, 1f)]
    public float gameSpeed = 0.5f;

    public bool isSlowMotionActive = false;

    // public float originalFixedDeltaTime;

    private PlayerMovement playerMovement;
    private PlayerEffect playerEffect;
    private PlayerJump playerJump;
    private Animator playerAnimator;

    private Vector3 originalJumpSquashSettings;
    private Vector3 originalLandSquashSettings;
    private float originalTempSpeedWithoutAccel;
    private float originalTempSpeedAccel;
    private float originalTempSpeedTurn;
    private float originalPlayerAnimatorSpeed;
    private float originalTimeToJumpApex;
    private float originalSwordCooldownTime;
    private float originalShurikenCooldownTime;
    private float originalProjectileCooldownTime;

    private void Awake() {
        playerMovement = GameManager.Instance.playerTransform.GetComponent<PlayerMovement>();
        playerEffect = GameManager.Instance.playerTransform.GetComponent<PlayerEffect>();
        playerJump = GameManager.Instance.playerTransform.GetComponent<PlayerJump>();
        playerAnimator = GameManager.Instance.playerAnimator;

        originalTempSpeedWithoutAccel = playerMovement.tempSpeedWithoutAccel;
        originalTempSpeedAccel = playerMovement.tempSpeedAccel;
        originalTempSpeedTurn = playerMovement.tempSpeedTurn;
        originalJumpSquashSettings = playerEffect.jumpSquashSettings;
        originalLandSquashSettings = playerEffect.landSquashSettings;
        originalPlayerAnimatorSpeed = playerAnimator.speed;
        originalTimeToJumpApex = playerJump.timeToJumpApex;
        originalSwordCooldownTime = PlayerCombatManager.Instance.swordCooldownTime;
        originalShurikenCooldownTime = PlayerCombatManager.Instance.shurikenCooldownTime;
        originalProjectileCooldownTime = PlayerCombatManager.Instance.projectileCooldownTime;
    }

    private void Update() {
        // slow motion logic here
        if (Input.GetKeyDown(KeyCode.Z)) {
            if (!isSlowMotionActive) {
                ActivateSlowMotion();
            } else {
                DisableSlowMotion();
            }
        }
    }

    private void DisableSlowMotion() {
        isSlowMotionActive = false;
        Time.timeScale = 1f;
        // Time.fixedDeltaTime = originalFixedDeltaTime;

        UpdatePlayerSpeed(1f);
    }

    private void ActivateSlowMotion() {
        isSlowMotionActive = true;
        Time.timeScale = gameSpeed;
        // originalFixedDeltaTime = Time.fixedDeltaTime;
        // Time.fixedDeltaTime = Time.timeScale * 0.02f;

        UpdatePlayerSpeed(gameSpeed);

    }

    private void UpdatePlayerSpeed(float speed) {
        playerMovement.tempSpeedWithoutAccel = originalTempSpeedWithoutAccel / speed;
        playerMovement.tempSpeedAccel = originalTempSpeedAccel * speed;
        playerMovement.tempSpeedTurn = (Mathf.Abs(speed - 1f) <= 0.001f) ? 1f : 4000f;

        playerEffect.jumpSquashSettings = new Vector3(originalJumpSquashSettings.x, originalJumpSquashSettings.y, originalJumpSquashSettings.z * speed);
        playerEffect.landSquashSettings = new Vector3(originalLandSquashSettings.x, originalLandSquashSettings.y, originalLandSquashSettings.z * speed);

        playerAnimator.speed = originalPlayerAnimatorSpeed / speed;

        playerJump.timeToJumpApex = originalTimeToJumpApex * speed;

        PlayerCombatManager.Instance.swordCooldownTime = originalSwordCooldownTime * speed;
        PlayerCombatManager.Instance.shurikenCooldownTime = originalShurikenCooldownTime * speed;
        PlayerCombatManager.Instance.projectileCooldownTime = originalProjectileCooldownTime * speed;
    }
}
