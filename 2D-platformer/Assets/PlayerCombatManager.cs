using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombatManager : MonoBehaviour {
    public static int SwordDamage;
    public static int ShurikenDamage;
    public static int ProjectileDamage;
    public static float ShurikenRotationSpeed;
    public static float ShurikenSpeed;
    public static float ShurikenDisappearDelay;

    [Header("Sword Attack")]
    [SerializeField] private int swordDamage;
    [SerializeField] private float swordCooldownTime;

    [Header("Shuriken Attack")]
    [SerializeField] private int shurikenDamage;
    [SerializeField] private float shurikenCooldownTime;
    [SerializeField] private float shurikenRotationSpeed;
    [SerializeField] private float shurikenSpeed;
    [SerializeField] private float shurikenDisappearDelay;

    [Header("Projectile Attack")]
    [SerializeField] private int projectileDamage;
    [SerializeField] private float projectileCooldownTime;

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
        playerSwordAttack = GameManager.Instance.playerTransform.GetComponent<PlayerSwordAttack>();
        playerShurikenAttack = GameManager.Instance.playerTransform.GetComponent<PlayerShurikenAttack>();
        playerProjectileAttack = GameManager.Instance.playerTransform.GetComponent<PlayerProjectileAttack>();
    }

    private void Update() {
        SwordDamage = swordDamage;
        ShurikenDamage = shurikenDamage;
        ProjectileDamage = projectileDamage;
        playerSwordAttack.attackCooldownTime = swordCooldownTime;
        playerShurikenAttack.attackCooldownTime = shurikenCooldownTime;
        playerProjectileAttack.attackCooldownTime = projectileCooldownTime;
        ShurikenRotationSpeed = shurikenRotationSpeed;
        ShurikenSpeed = shurikenSpeed;
        ShurikenDisappearDelay = shurikenDisappearDelay;
    }



    private void Start() {
        if (playerHealthSlider == null) {
            Debug.LogError("playerHealthSlider is required. Drag and drop health slider ui.");
        }
    }

    public void TakeDamage(int damageAmount , bool blockable) {
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
