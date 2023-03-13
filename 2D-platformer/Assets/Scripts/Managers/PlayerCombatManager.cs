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

    [Header("Heavy Attack")]
    public int heavyAttackDamage;

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
    [SerializeField] private int health;
    [SerializeField] private float healthUIUpdateDuration = 2f;

    private PlayerBlockDefense playerBlockDefense;
    private PlayerSwordAttack playerSwordAttack;
    private PlayerShurikenAttack playerShurikenAttack;
    private PlayerProjectileAttack playerProjectileAttack;
    private PlayerHeavyAttack playerHeavyAttack;
    private float normalizedHealth = 1f;
    private int maxHealth = 100;

    [Header("Block defense")]
    public bool isBlocking;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        health = maxHealth;

        playerBlockDefense = GameManager.Instance.playerTransform.GetComponent<PlayerBlockDefense>();
        playerSwordAttack = GameManager.Instance.playerTransform.GetComponent<PlayerSwordAttack>();
        playerShurikenAttack = GameManager.Instance.playerTransform.GetComponent<PlayerShurikenAttack>();
        playerProjectileAttack = GameManager.Instance.playerTransform.GetComponent<PlayerProjectileAttack>();
        playerHeavyAttack = GameManager.Instance.playerTransform.GetComponent<PlayerHeavyAttack>();
    }

    private void Update() {
        playerSwordAttack.attackCooldownTime = swordCooldownTime;
        playerShurikenAttack.attackCooldownTime = shurikenCooldownTime;
        playerProjectileAttack.attackCooldownTime = projectileCooldownTime;
        heavyAttackDamage = playerHeavyAttack.attackDamage;
    }



    private void Start() {
        if (playerHealthSlider == null) {
            Debug.LogError("playerHealthSlider is required. Drag and drop health slider ui.");
        }
    }

    /// <summary>
    /// Applies damage to the player
    /// </summary>
    /// <param name="damageAmount">The amount of damage to apply to the player</param>
    /// <param name="isBlockableAttack">Is the attack blockable or not.</param>
    public void TakeDamage(int damageAmount, bool isBlockableAttack) {
        if (PlayerInvulnerability.isInvulnerable) {
            return;
        }

        if (!isBlocking) {
            // !blocking
            health -= damageAmount;
            Debug.Log($"took damage: {damageAmount}");
        } else {
            if (isBlockableAttack) {
                // blocking and blockable attack
                playerBlockDefense.HandleAttack(damageAmount);
            } else {
                // blocking and !blockable attack

                if (playerBlockDefense.currentStamina >= playerBlockDefense.maxStamina / 2f) {
                    // blocking and !blockable attack and stamina is greater than half full
                    health -= (damageAmount / 2);
                    Debug.Log($"took damage(half): {damageAmount / 2}");
                } else {
                    // blocking and !blockable attack and stamina is less than half full
                    health -= damageAmount;
                    Debug.Log($"took damage: {damageAmount}");
                }
                Debug.Log("stamina: 0");
                playerBlockDefense.currentStamina = 0f;
            }
        }
        UpdateHealthUI();
        playerBlockDefense.UpdateStaminaUI();
    }

    private void UpdateHealthUI() {
        normalizedHealth = (float)health / (float)maxHealth;
        DOTween.To(() => playerHealthSlider.value, x => playerHealthSlider.value = x, normalizedHealth, healthUIUpdateDuration);
        if (health <= 0) {
            health = 0;
            normalizedHealth = 0;
            gameObject.SetActive(false);
        }
    }
}
