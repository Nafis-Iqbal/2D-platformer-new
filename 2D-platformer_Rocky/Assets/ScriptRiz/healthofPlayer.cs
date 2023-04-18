﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthofPlayer : MonoBehaviour
{
    public Color platColor;
    public bool isGround;
    public bool PlatChanged;
    public Vector2 leftBox , rightBox;

    private float health = 100f;

    // private void OnTriggerEnter2D(Collider2D other) {
    //     if(other.CompareTag("Knife")){
    //         health -= 15f;
    //     }
    //     if(other.CompareTag("HeavyKnife")){
    //         health -= 30f;
    //     }
    // }
    
    float x;
    float y;

    private PlayerGround playerGround;

    private void Start() {
        playerGround = GameManager.Instance.playerTransform.GetComponent<PlayerGround>();
    }
    private void FixedUpdate() {
        isGround = playerGround.isGrounded;

        if(health <= 0f){
            Destroy(gameObject);
        }
    }

    IEnumerator change(){
            yield return new WaitForSeconds(.1f);
            PlatChanged = false;
    }

    public void setPlatInfo(Vector2 left , Vector2 right,Color PlatColor){
        leftBox = left;
        rightBox = right;
        platColor = PlatColor;
        PlatChanged = true;
        StartCoroutine(change());
    }

    public void takeDamage(int val , bool blockable){
        
        health -= val;
        Debug.Log(health);
    }
}