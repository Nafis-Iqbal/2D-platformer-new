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
            // other.transform.GetComponent<EnemyCombatManager>().takeDamage(PlayerCombatManager.ShurikenDamage);
            Debug.Log("enemy damage(shuriken): " + PlayerCombatManager.Instance.shurikenDamage);
            other.transform.GetComponent<EnemyBase>().health -= PlayerCombatManager.Instance.shurikenDamage;
            gameObject.SetActive(false);
        } else {
            playerShuriken.Stop();
        }
    }
}