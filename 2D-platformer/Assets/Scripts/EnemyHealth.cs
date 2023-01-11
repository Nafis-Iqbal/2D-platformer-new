using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour {
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damagedColor;
    [SerializeField] private Color normalColor;

    public void takeDamage(float damageAmount) {
        Debug.Log("took damage " + damageAmount);
        spriteRenderer.color = damagedColor;
        StartCoroutine(changeToNormalColorCoroutine());
    }

    IEnumerator changeToNormalColorCoroutine() {
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.color = normalColor;
    }
}
