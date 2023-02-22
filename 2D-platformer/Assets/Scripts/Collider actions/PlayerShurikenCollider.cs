﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShurikenCollider : MonoBehaviour {
    private PlayerShuriken playerShuriken;
    private void Awake() {
        playerShuriken = GetComponent<PlayerShuriken>();
    }
    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Enemy") {
            other.transform.GetComponent<EnemyHealth>().takeDamage(PlayerCombatManager.ShurikenDamage);
            gameObject.SetActive(false);
        } else {
            playerShuriken.Stop();
        }
    }
}