using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private float health = 100f;
    [SerializeField] private float healthUIUpdateDuration = 2f;

    [Header("Demo damage effect")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damagedColor;
    [SerializeField] private Color normalColor;

    private float normalizedHealth = 1f;
    private float maxHealth = 100f;

    private void Start() {
        if (playerHealthSlider == null) {
            Debug.LogError("playerHealthSlider is required. Drag and drop health slider ui.");
        }
    }

    private void Update() {
        Debug.Log(spriteRenderer.color);
    }

    public void TakeDamage(float damageAmount) {
        spriteRenderer.color = damagedColor;
        StartCoroutine(changeToNormalColorCoroutine());
        health -= damageAmount;
        normalizedHealth = health / maxHealth;
        DOTween.To(() => playerHealthSlider.value, x => playerHealthSlider.value = x, normalizedHealth, healthUIUpdateDuration);
        if (health <= 0f) {
            health = 0f;
            normalizedHealth = 0f;
            gameObject.SetActive(false);
        }
    }

    IEnumerator changeToNormalColorCoroutine() {
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.color = normalColor;
    }
}
