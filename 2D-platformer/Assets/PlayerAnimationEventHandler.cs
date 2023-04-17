using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour {
    private PlayerSwordAttack playerSwordAttack;
    private GameObject lightAttackHitBox;
    private PlayerJump playerJump;

    private void Awake() {
        var playerTransform = GameManager.Instance.playerTransform;
        playerSwordAttack = playerTransform.GetComponent<PlayerSwordAttack>();
        playerJump = playerTransform.GetComponent<PlayerJump>();
        lightAttackHitBox = GameManager.Instance.lightAttackHitBox;
    }

    public void LightAttackStarted() {
        playerSwordAttack.isExecuting = true;
    }

    public void LightAttackEnded() {
        playerSwordAttack.isExecuting = false;
    }

    public void LightAttackDamageStarted() {
        lightAttackHitBox.SetActive(true);
    }

    public void LightAttackDamageEnded() {
        lightAttackHitBox.SetActive(false);
    }

    public void AddJumpForce() {
        playerJump.StartJump();
    }

}
