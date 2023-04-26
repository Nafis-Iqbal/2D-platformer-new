using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour {
    private PlayerSwordAttack playerSwordAttack;
    private PlayerHeavyAttack playerHeavyAttack;
    private GameObject lightAttackHitBox;
    private PlayerJump playerJump;

    private void Awake() {
        var playerTransform = GameManager.Instance.playerTransform;
        playerSwordAttack = playerTransform.GetComponent<PlayerSwordAttack>();
        playerHeavyAttack = playerTransform.GetComponent<PlayerHeavyAttack>();
        playerJump = playerTransform.GetComponent<PlayerJump>();
        lightAttackHitBox = GameManager.Instance.lightAttackHitBox;
    }

    public void LightAttackStarted() {
        playerSwordAttack.isExecuting = true;
    }

    public void LightAttackEnded() {
        playerSwordAttack.isExecuting = false;
    }

    public void HeavyAttackStarted() {
        playerHeavyAttack.isExecuting = true;
    }

    public void HeavyAttackEnded() {
        playerHeavyAttack.isExecuting = false;
    }

    public void AddJumpForce() {
        playerJump.StartJump();
    }

    public void DisableCharging() {
        playerJump.DisableCharging();
    }

}
