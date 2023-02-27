using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlowMotion : MonoBehaviour {
    [SerializeField, Range(0f, 4f)] private float animationSlowMotionMultiplier = 1f;
    [SerializeField, Range(0f, 4f)] private float playerMovementSpeedMultiplier = 1f;
    [SerializeField, Range(0f, 4f)] private float playerJumpSpeedMultiplier = 1f;
    [SerializeField, Range(0f, 4f)] private float slowMotionDuration = 0.5f;

    private Animator playerAnimator;
    private PlayerMovement playerMovement;
    private PlayerJump playerJump;

    private float originalMovementMaxSpeed;
    private float originalMovementMaxAcceleration;
    private float originalMovementMaxDeceleration;
    private float originalMovementMaxTurnSpeed;
    private float originalMovementMaxAirAcceleration;
    private float originalMovementMaxAirDeceleration;
    private float originalMovementMaxAirTurnSpeed;
    private float originalMovementWalkMultiplier;

    private float timeToJumpApex;
    private float upwardMovementMultiplier;
    private float downwardMovementMultiplier;
    private float defaultGravityScale;
    private float speedLimit;

    private void Awake() {
        playerAnimator = GameManager.Instance.playerAnimator;
        playerMovement = GameManager.Instance.playerTransform.GetComponent<PlayerMovement>();
        playerJump = GameManager.Instance.playerTransform.GetComponent<PlayerJump>();

        originalMovementMaxSpeed = playerMovement.maxSpeed;
        originalMovementMaxAcceleration = playerMovement.maxAcceleration;
        originalMovementMaxDeceleration = playerMovement.maxDeceleration;
        originalMovementMaxTurnSpeed = playerMovement.maxTurnSpeed;
        originalMovementMaxAirAcceleration = playerMovement.maxAirAcceleration;
        originalMovementMaxAirDeceleration = playerMovement.maxAirDeceleration;
        originalMovementMaxAirTurnSpeed = playerMovement.maxAirTurnSpeed;
        originalMovementWalkMultiplier = playerMovement.walkMultiplier;

        timeToJumpApex = playerJump.timeToJumpApex;
        upwardMovementMultiplier = playerJump.upwardMovementMultiplier;
        downwardMovementMultiplier = playerJump.downwardMovementMultiplier;
        defaultGravityScale = playerJump.defaultGravityScale;
        speedLimit = playerJump.speedLimit;

    }

    private void Update() {

        // slow motion logic here
        if (Input.GetKeyDown(KeyCode.X)) {
            EnterSlowMotion();
        }
    }

    public void EnterSlowMotion() {
        StartCoroutine(EnterSlowMotionRoutine());
    }

    IEnumerator EnterSlowMotionRoutine() {
        Debug.Log("Slow motion started for player");
        playerAnimator.speed = animationSlowMotionMultiplier;

        playerMovement.maxSpeed = originalMovementMaxSpeed * playerMovementSpeedMultiplier;
        playerMovement.maxAcceleration = originalMovementMaxAcceleration * playerMovementSpeedMultiplier;
        playerMovement.maxDeceleration = originalMovementMaxDeceleration * playerMovementSpeedMultiplier;
        playerMovement.maxTurnSpeed = originalMovementMaxTurnSpeed * playerMovementSpeedMultiplier;
        playerMovement.maxAirAcceleration = originalMovementMaxAirAcceleration * playerMovementSpeedMultiplier;
        playerMovement.maxAirDeceleration = originalMovementMaxAirDeceleration * playerMovementSpeedMultiplier;
        playerMovement.maxAirTurnSpeed = originalMovementMaxAirTurnSpeed * playerMovementSpeedMultiplier;
        playerMovement.walkMultiplier = originalMovementWalkMultiplier * playerMovementSpeedMultiplier;

        playerJump.timeToJumpApex = timeToJumpApex * 1f / playerJumpSpeedMultiplier;
        playerJump.upwardMovementMultiplier = upwardMovementMultiplier * 1f / playerJumpSpeedMultiplier;
        playerJump.downwardMovementMultiplier = downwardMovementMultiplier * 1f / playerJumpSpeedMultiplier;
        playerJump.defaultGravityScale = defaultGravityScale * 1f / playerJumpSpeedMultiplier;
        playerJump.speedLimit = speedLimit * playerJumpSpeedMultiplier;

        yield return new WaitForSeconds(slowMotionDuration);
        Debug.Log("Slow motion ended for player");

        playerAnimator.speed = 1f;

        playerMovement.maxSpeed = originalMovementMaxSpeed;
        playerMovement.maxAcceleration = originalMovementMaxAcceleration;
        playerMovement.maxDeceleration = originalMovementMaxDeceleration;
        playerMovement.maxTurnSpeed = originalMovementMaxTurnSpeed;
        playerMovement.maxAirAcceleration = originalMovementMaxAirAcceleration;
        playerMovement.maxAirDeceleration = originalMovementMaxAirDeceleration;
        playerMovement.maxAirTurnSpeed = originalMovementMaxAirTurnSpeed;
        playerMovement.walkMultiplier = originalMovementWalkMultiplier;

        playerJump.timeToJumpApex = timeToJumpApex;
        playerJump.upwardMovementMultiplier = upwardMovementMultiplier;
        playerJump.downwardMovementMultiplier = downwardMovementMultiplier;
        playerJump.defaultGravityScale = defaultGravityScale;
        playerJump.speedLimit = speedLimit;
    }
}
