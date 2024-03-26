using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    private PlayerCombatSystem playerCombatSystemScript;

    [Header("Health and Stamina")]
    public float playerHealth;
    public float playerStamina, staminaRegenRate;
    public float poiseBreakHealthLimit;
    public float poiseBreakHealthResetInterval;
    [Header("Melee Attacks")]
    public string activeMeleeWeapon;
    public EnemyAttackInfo[] swordShieldAttacks = new EnemyAttackInfo[6];
    public EnemyAttackInfo[] dualSwordAttacks = new EnemyAttackInfo[6];

    [Header("Throwing Blade")]
    public int throwingBladeDamage;
    public float throwingBladeRotationSpeed;
    public float throwingBladeSpeed;
    public float throwingBladeDisappearDelay;

    [Header("Bombs")]
    public int bombDamage;
    public float bombRotationSpeed;
    public float bombSpeed;
    public float bombDisappearDelay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        playerCombatSystemScript = GameManager.Instance.playerTransform.GetComponent<PlayerCombatSystem>();
    }

    private void OnEnable()
    {

    }
}
