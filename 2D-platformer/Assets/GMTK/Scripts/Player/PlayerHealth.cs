using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private float health = 100f;
    [SerializeField] private float healthUIUpdateDuration = 2f;

    private float normalizedHealth = 1f;
    private float maxHealth = 100f;

    public void OnHitBySword(float damage) {
        health -= damage;
        normalizedHealth = health / maxHealth;
        DOTween.To(() => playerHealthSlider.value, x => playerHealthSlider.value = x, normalizedHealth, healthUIUpdateDuration);
        if (health <= 0f) {
            health = 0f;
            normalizedHealth = 0f;
            gameObject.SetActive(false);
        }
    }
}
