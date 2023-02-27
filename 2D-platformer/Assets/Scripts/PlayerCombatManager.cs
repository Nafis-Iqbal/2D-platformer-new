using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombatManager : MonoBehaviour {
    public static PlayerCombatManager Instance;

    [Header("Sword Attack")]
    public int swordDamage;
    public float swordCooldownTime;

    [Header("Shuriken Attack")]
    public int shurikenDamage;
    public float shurikenCooldownTime;
    public float shurikenRotationSpeed;
    public float shurikenSpeed;
    public float shurikenDisappearDelay;

    [Header("Projectile Attack")]
    public int projectileDamage;
    public float projectileCooldownTime;

    [Header("Player health section")]
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private int health = 100;
    [SerializeField] private float healthUIUpdateDuration = 2f;

    private PlayerSwordAttack playerSwordAttack;
    private PlayerShurikenAttack playerShurikenAttack;
    private PlayerProjectileAttack playerProjectileAttack;
    private float normalizedHealth = 1f;
    private int maxHealth = 100;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        playerSwordAttack = GameManager.Instance.playerTransform.GetComponent<PlayerSwordAttack>();
        playerShurikenAttack = GameManager.Instance.playerTransform.GetComponent<PlayerShurikenAttack>();
        playerProjectileAttack = GameManager.Instance.playerTransform.GetComponent<PlayerProjectileAttack>();
    }

    private void Update() {
        playerSwordAttack.attackCooldownTime = swordCooldownTime;
        playerShurikenAttack.attackCooldownTime = shurikenCooldownTime;
        playerProjectileAttack.attackCooldownTime = projectileCooldownTime;
    }



    private void Start() {
        if (playerHealthSlider == null) {
            Debug.LogError("playerHealthSlider is required. Drag and drop health slider ui.");
        }
    }

    public void TakeDamage(int damageAmount) {
        health -= damageAmount;
        normalizedHealth = (float)health / (float)maxHealth;
        DOTween.To(() => playerHealthSlider.value, x => playerHealthSlider.value = x, normalizedHealth, healthUIUpdateDuration);
        if (health <= 0) {
            health = 0;
            normalizedHealth = 0;
            gameObject.SetActive(false);
        }
    }

}
