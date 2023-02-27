using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public int id;
    healthofPlayer playerScript;

    private void Start() {
        playerScript = EnemyManager.Instance.player.GetComponent <healthofPlayer>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")){
            if(id == 0){
                playerScript.takeDamage(30);
            }

            gameObject.SetActive(false);
        }
    }
}
