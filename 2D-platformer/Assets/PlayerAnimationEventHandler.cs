using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour {
    private PlayerSwordAttack playerSwordAttack;

    private void Awake() {
        playerSwordAttack = GameManager.Instance.playerTransform.GetComponent<PlayerSwordAttack>();
    }

    public void LightAttackStarted() {
        playerSwordAttack.isExecuting = true;
        Debug.Log("light attack started...");
    }

    public void LightAttackEnded() {
        playerSwordAttack.isExecuting = false;
        Debug.Log("light attack ended...");
    }
}
