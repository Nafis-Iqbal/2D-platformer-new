﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour {
    private PlayerSwordAttack playerSwordAttack;
    private GameObject lightAttackHitBox;

    private void Awake() {
        playerSwordAttack = GameManager.Instance.playerTransform.GetComponent<PlayerSwordAttack>();
        lightAttackHitBox = GameManager.Instance.lightAttackHitBox;
    }

    public void LightAttackStarted() {
        playerSwordAttack.isExecuting = true;
        Debug.Log("light attack started...");
    }

    public void LightAttackEnded() {
        playerSwordAttack.isExecuting = false;
        Debug.Log("light attack ended...");
    }

    public void LightAttackDamageStarted() {
        lightAttackHitBox.SetActive(true);
    }

    public void LightAttackDamageEnded() {
        lightAttackHitBox.SetActive(false);
    }
}
