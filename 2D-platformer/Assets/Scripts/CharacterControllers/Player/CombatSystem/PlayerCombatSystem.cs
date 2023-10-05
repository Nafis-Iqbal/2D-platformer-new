using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCombatSystem : MonoBehaviour
{

    [Header("Drag & Drops")]
    public Animator playerSpineAnimator;
    public Slider playerStaminaSlider;
    public GameObject blockDefenseObject;
    private PlayerColumn playerColumn;
    private PlayerJump playerJump;

    [Header("Shuriken Attack Details")]
    public GameObject shurikenPrefab;
    public float shurikenAttackCooldownTime = 1f;
    public bool shurikenAttackExecuting = false;
    private float timeElapsedSinceShurikenAttack = 0f;

    [Header("Projectile Attack Details")]
    public GameObject projectilePrefab;
    public float projectileAttackCooldownTime = 0.3f;
    public bool projectileAttackExecuting = false;
    private float timeElapsedSinceProjectileAttack = 0f;

    [Header("Light Attack Details")]
    public float lightAttackCooldownTime = 1f;
    public bool lightAttackExecuting = false;
    public bool lightAttacked = false;
    private float timeElapsedSinceLightAttack = 0f;

    [Header("Heavy Attack Details")]
    public float totalHeavyAttackChargeAmount = 100f;
    public float heavyAttackChargeFillRate = 2f;
    public int successfulHeavyAttackDamage = 20;
    public int failedHeavyAttackDamage = 12;
    public int heavyAttackDamage;
    public float heavyAttackDamageMultiplier = 1f;
    public float heavyAttackCooldownTime = 1f;
    private float currentHeavyAttackCharge = 0f;
    private float timeElapsedSinceHeavyAttack = 0f;
    private bool isHeavyAttackKeyPressed = false;
    private bool heavyAttackDone = true;
    public bool canHeavyAttack = true;
    public bool heavyAttackExecuting;


    [Header("Stamina & Block Costs")]
    public float maxStamina = 100f;
    public float normalStaminaBarFillRate = 2f;
    public float blockingStaminaBarFillRate = 1f;
    public float minimumStaminaToBlock = 20f;
    public float staminaUIUpdateDuration = 0.3f;
    public float normalizedStamina = 1f;
    public bool isBlocking;
    public float currentStamina;
    public bool isBlockingCached;
    private bool isBlockingRequested = false;

    private void Awake()
    {
        isBlocking = false;
        playerSpineAnimator = GameManager.Instance.playerSpineAnimator;
        currentStamina = maxStamina;
        playerJump = GameManager.Instance.playerTransform.GetComponent<PlayerJump>();
        playerColumn = GameManager.Instance.playerTransform.GetComponent<PlayerColumn>();
    }

    private void Start()
    {
        if (playerStaminaSlider == null)
        {
            Debug.LogError("playerStaminaSlider is required. Drag and drop stamina slider object.");
        }
        if (blockDefenseObject == null)
        {
            Debug.LogError("blockDefenseObject is required. Drag and drop block defense object.");
        }
    }

    private void Update()
    {
        #region Blocking
        if (isBlockingRequested && currentStamina >= minimumStaminaToBlock)
        {
            isBlocking = true;
            blockDefenseObject.SetActive(true);
            playerSpineAnimator.SetBool("Blocking", true);
            // playerSpineAnimator.SetTrigger("blockTrigger");
        }
        else
        {
            isBlocking = false;
            blockDefenseObject.SetActive(false);
            playerSpineAnimator.SetBool("Blocking", false);
            // playerSpineAnimator.ResetTrigger("blockTrigger");
        }
        isBlockingCached = isBlocking;
        if (isBlocking)
        {
            currentStamina += blockingStaminaBarFillRate * Time.deltaTime;
        }
        else
        {
            currentStamina += normalStaminaBarFillRate * Time.deltaTime;
        }
        StaminaBoundCheck();
        UpdateStaminaUI();
        PlayerCombatManager.Instance.isBlocking = isBlocking;
        #endregion

        #region Light Attack
        if (lightAttacked)
        {
            timeElapsedSinceLightAttack += Time.deltaTime;
            if (timeElapsedSinceLightAttack > lightAttackCooldownTime)
            {
                timeElapsedSinceLightAttack = 0f;
                lightAttacked = false;
                // isAttacking = false;
            }
        }
        #endregion

        #region Heavy Attack
        if (heavyAttackDone)
        {
            timeElapsedSinceHeavyAttack += Time.deltaTime;
            if (timeElapsedSinceHeavyAttack > heavyAttackCooldownTime)
            {
                canHeavyAttack = true;
                timeElapsedSinceHeavyAttack = 0f;
                heavyAttackDone = false;
            }
            else
            {
                canHeavyAttack = false;
            }
        }

        if (isHeavyAttackKeyPressed && canHeavyAttack && playerJump.onGround)
        {
            ChargeForAttack();
        }
        else
        {
            currentHeavyAttackCharge = 0f;
        }
        if (currentHeavyAttackCharge >= totalHeavyAttackChargeAmount)
        {
            ExecuteHighPoweredAttack();
        }
        BoundCheckCharge();
        heavyAttackDamage = (int)Math.Ceiling(successfulHeavyAttackDamage * heavyAttackDamageMultiplier);
        #endregion

        #region Projectile Attack
        if (projectileAttackExecuting)
        {
            timeElapsedSinceProjectileAttack += Time.deltaTime;
            if (timeElapsedSinceProjectileAttack > projectileAttackCooldownTime)
            {
                timeElapsedSinceProjectileAttack = 0f;
                projectileAttackExecuting = false;
            }
        }
        #endregion

        #region Shuriken Attack
        if (shurikenAttackExecuting)
        {
            timeElapsedSinceShurikenAttack += Time.deltaTime;
            if (timeElapsedSinceShurikenAttack > shurikenAttackCooldownTime)
            {
                timeElapsedSinceShurikenAttack = 0f;
                shurikenAttackExecuting = false;
            }
        }
        #endregion
    }

    #region Projectile Attack
    internal void OnProjectileAttack(InputAction.CallbackContext context)
    {
        if (!projectileAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            playerSpineAnimator.Play("projectile throw");
            var projectile = ObjectPooler.Instance.SpawnFromPool("PlayerProjectile", transform.position, Quaternion.identity);
            projectile.GetComponent<PlayerProjectile>().Deploy(Vector2.right * transform.localScale.x);
        }
    }
    #endregion

    #region Shuriken Attack
    public void OnShurikenAttack(InputAction.CallbackContext context)
    {
        if (!shurikenAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            playerSpineAnimator.Play("shuriken throw");
            var shuriken = ObjectPooler.Instance.SpawnFromPool("PlayerShuriken", transform.position, Quaternion.identity);
            shuriken.GetComponent<PlayerShuriken>().Deploy(Vector2.right * transform.localScale.x);
        }
    }
    #endregion

    #region Player Block
    public void OnBlockDefense(InputAction.CallbackContext context)
    {
        // Debug.Log($"block: {context}");
        if (context.phase == InputActionPhase.Started)
        {
            isBlockingRequested = true;
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            isBlockingRequested = false;
        }
    }

    public void HandleAttack(int damageAmount)
    {
        currentStamina -= damageAmount;
        Debug.Log($"stamina decrease: {damageAmount}");
    }

    public void UpdateStaminaUI()
    {
        normalizedStamina = currentStamina / maxStamina;
        DOTween.To(() => playerStaminaSlider.value, x => playerStaminaSlider.value = x, normalizedStamina, staminaUIUpdateDuration);
        StaminaBoundCheck();
    }

    public void StaminaBoundCheck()
    {
        if (currentStamina > maxStamina)
        {
            currentStamina = maxStamina;
        }
        if (currentStamina <= 0f)
        {
            currentStamina = 0f;
            normalizedStamina = 0f;
        }
    }
    #endregion

    #region Light Attack
    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (!lightAttacked && !lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            lightAttacked = true;
            GameManager.Instance.playerSpineAnimator.Play("light attack");
        }
    }
    #endregion

    #region Heavy Attack
    private void ChargeForAttack()
    {
        currentHeavyAttackCharge += Time.deltaTime * heavyAttackChargeFillRate;
        GameManager.Instance.playerSpineAnimator.SetBool("Charging", true);
    }

    private void ExecuteHighPoweredAttack()
    {
        Debug.Log("heavy attack (high damage)...");
        heavyAttackDamageMultiplier = 1f;
        isHeavyAttackKeyPressed = false;
        currentHeavyAttackCharge = 0f;
        GameManager.Instance.playerSpineAnimator.SetBool("Charging", false);
        timeElapsedSinceHeavyAttack = 0f;
        heavyAttackDone = true;
    }

    private void BoundCheckCharge()
    {
        if (currentHeavyAttackCharge < 0f)
        {
            currentHeavyAttackCharge = 0f;
        }
        else if (currentHeavyAttackCharge > totalHeavyAttackChargeAmount)
        {
            currentHeavyAttackCharge = totalHeavyAttackChargeAmount;
        }
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            isHeavyAttackKeyPressed = true;
        }
        else if (context.phase == InputActionPhase.Canceled && isHeavyAttackKeyPressed)
        {
            if (canHeavyAttack)
            {
                ExecuteLowPoweredAttack();
            }
            else
            {
                isHeavyAttackKeyPressed = false;
            }
        }
    }

    private void ExecuteLowPoweredAttack()
    {
        Debug.Log("heavy attack (low damage)...");
        heavyAttackDamageMultiplier = (float)failedHeavyAttackDamage / successfulHeavyAttackDamage;
        currentHeavyAttackCharge = 0f;
        isHeavyAttackKeyPressed = false;
        GameManager.Instance.playerSpineAnimator.SetBool("Charging", false);
        timeElapsedSinceHeavyAttack = 0f;
        heavyAttackDone = true;
    }
    #endregion
}
