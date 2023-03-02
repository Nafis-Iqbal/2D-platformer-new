﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    private bool isBlockable;
    public int id;
    healthofPlayer playerScript;

    private void Start() {
        playerScript = EnemyManager.Instance.player.GetComponent <healthofPlayer>();
    }

    // private void OnEnable() {
    //     playerScript = EnemyManager.Instance.player.GetComponent <healthofPlayer>();
    // }
    public void SetIfBlockable(bool temp){
        isBlockable = temp;
    }


    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")){
            if(id == 0){
                playerScript.takeDamage(30);
            }else{
                playerScript.takeDamage(60);
            }

            gameObject.SetActive(false);
        }
    }
}
