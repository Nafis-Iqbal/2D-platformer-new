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
    private PlayerMovement playerMovementScript;
    private PlayerColumn playerColumn;
    private PlayerJump playerJump;

    [Header("Combat State")]
    public bool combatMode = false;

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
    public float currentHeavyAttackCharge = 0f;
    public float cutOffHeavyAttackCharge = 0f;
    private float timeElapsedSinceHeavyAttack = 0f;
    public bool isHeavyAttackKeyPressed = false;
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
        playerMovementScript = GetComponent<PlayerMovement>();
        playerJump = GetComponent<PlayerJump>();
        playerColumn = GetComponent<PlayerColumn>();
    }

    private void OnEnable()
    {
        combatMode = false;
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
            playerSpineAnimator.SetBool("Defence", true);
            // playerSpineAnimator.SetTrigger("blockTrigger");
        }
        else
        {
            isBlocking = false;
            blockDefenseObject.SetActive(false);
            playerSpineAnimator.SetBool("Defence", false);
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
        UpdateStaminaUIAfterBlocking();
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
            if (currentHeavyAttackCharge > 0.0f) cutOffHeavyAttackCharge = currentHeavyAttackCharge;
        }
        else
        {
            currentHeavyAttackCharge = 0f;
        }

        if (currentHeavyAttackCharge >= totalHeavyAttackChargeAmount)
        {
            ExecuteHighPoweredAttack();
        }

        currentHeavyAttackCharge = Mathf.Min(currentHeavyAttackCharge, totalHeavyAttackChargeAmount);

        heavyAttackDamage = (int)Math.Ceiling(successfulHeavyAttackDamage * heavyAttackDamageMultiplier);
        #endregion

        #region Special Item Use
        if (projectileAttackExecuting)
        {
            timeElapsedSinceProjectileAttack += Time.deltaTime;
            if (timeElapsedSinceProjectileAttack > projectileAttackCooldownTime)
            {
                timeElapsedSinceProjectileAttack = 0f;
                projectileAttackExecuting = false;
            }
        }

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

    #region Player Defence
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

    public void HandleAttackDuringDefence(int damageAmount)
    {
        currentStamina -= damageAmount;
        Debug.Log($"stamina decrease: {damageAmount}");
    }

    public void UpdateStaminaUIAfterBlocking()
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

    #region Light Attack Variants
    public void OnWeaponHolsterPrompted(InputAction.CallbackContext context)
    {
        if (!lightAttacked && !lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            if (combatMode == true)
            {
                combatMode = playerJump.combatMode = false;

                GameManager.Instance.playerSpineAnimator.SetTrigger("CombatMode");
            }
            else
            {
                combatMode = playerJump.combatMode = true;

                GameManager.Instance.playerSpineAnimator.SetTrigger("CombatMode");
                GameManager.Instance.playerSpineAnimator.SetInteger("WeaponID", 1);
            }
        }
    }

    public void OnWeaponAttack1(InputAction.CallbackContext context)
    {
        if (combatMode == false || playerMovementScript.isSprinting == true || MovementLimiter.instance.playerCanAttack == false) return;

        if (!lightAttacked && !lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            lightAttacked = true;
            GameManager.Instance.playerSpineAnimator.SetTrigger("Attack");
            GameManager.Instance.playerSpineAnimator.SetInteger("AttackID", 1);
        }
    }

    public void OnWeaponAttack2(InputAction.CallbackContext context)
    {
        if (combatMode == false || playerMovementScript.isSprinting == true || MovementLimiter.instance.playerCanAttack == false) return;

        if (!lightAttacked && !lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            lightAttacked = true;
            GameManager.Instance.playerSpineAnimator.SetTrigger("Attack");
            GameManager.Instance.playerSpineAnimator.SetInteger("AttackID", 2);
        }
    }

    public void OnWeaponAttack3(InputAction.CallbackContext context)
    {
        if (MovementLimiter.instance.playerCanAttack == false || combatMode == false) return;

        if (!lightAttacked && !lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            lightAttacked = true;
            GameManager.Instance.playerSpineAnimator.SetTrigger("Attack");
            GameManager.Instance.playerSpineAnimator.SetInteger("AttackID", 3);
        }
    }

    public void OnRunAttack(InputAction.CallbackContext context)
    {
        if (MovementLimiter.instance.playerCanAttack == false || combatMode == false) return;

        if (!lightAttacked && !lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            lightAttacked = true;
            GameManager.Instance.playerSpineAnimator.SetTrigger("Attack");
            GameManager.Instance.playerSpineAnimator.SetInteger("AttackID", 4);
        }
    }

    public void OnRollAttack(InputAction.CallbackContext context)
    {
        if (MovementLimiter.instance.playerCanAttack == false || combatMode == false) return;

        if (!lightAttacked && !lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            lightAttacked = true;
            GameManager.Instance.playerSpineAnimator.SetTrigger("Attack");
            GameManager.Instance.playerSpineAnimator.SetInteger("AttackID", 5);
        }
    }
    #endregion

    #region Charged Attack
    private void ChargeForAttack()
    {
        if (MovementLimiter.instance.playerCanAttack == false || combatMode == false) return;

        currentHeavyAttackCharge += Time.deltaTime * heavyAttackChargeFillRate;
        //GameManager.Instance.playerSpineAnimator.SetBool("Charging", true);
    }

    public void OnChargedAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            cutOffHeavyAttackCharge = currentHeavyAttackCharge = 0.0f;
            isHeavyAttackKeyPressed = true;
            GameManager.Instance.playerSpineAnimator.ResetTrigger("ChargeAttack");
            GameManager.Instance.playerSpineAnimator.SetBool("Charging", true);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            if (canHeavyAttack)
            {
                ExecuteLowPoweredAttack();
            }
        }
    }

    private void ExecuteHighPoweredAttack()
    {
        Debug.Log("heavy attack (high damage)...");
        heavyAttackDamageMultiplier = 1f;
        isHeavyAttackKeyPressed = false;
        currentHeavyAttackCharge = 0f;
        GameManager.Instance.playerSpineAnimator.SetTrigger("ChargeAttack");
        GameManager.Instance.playerSpineAnimator.SetBool("Charging", false);
        timeElapsedSinceHeavyAttack = 0f;
        heavyAttackDone = true;
    }

    private void ExecuteLowPoweredAttack()
    {
        Debug.Log("heavy attack (low damage)...");
        heavyAttackDamageMultiplier = (float)failedHeavyAttackDamage / successfulHeavyAttackDamage;
        currentHeavyAttackCharge = 0f;
        isHeavyAttackKeyPressed = false;
        GameManager.Instance.playerSpineAnimator.SetTrigger("ChargeAttack");
        GameManager.Instance.playerSpineAnimator.SetBool("Charging", false);
        timeElapsedSinceHeavyAttack = 0f;
        heavyAttackDone = true;
    }
    #endregion

    #region Special Items Use
    public void OnItemUse(InputAction.CallbackContext context)
    {
        if (!projectileAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            playerSpineAnimator.Play("projectile throw");
            var projectile = ObjectPooler.Instance.SpawnFromPool("PlayerProjectile", transform.position, Quaternion.identity);
            projectile.GetComponent<PlayerProjectile>().Deploy(Vector2.right * transform.localScale.x);
        }
    }

    public void OnCombatItemUse(InputAction.CallbackContext context)
    {
        if (!shurikenAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            playerSpineAnimator.Play("shuriken throw");
            var shuriken = ObjectPooler.Instance.SpawnFromPool("PlayerShuriken", transform.position, Quaternion.identity);
            shuriken.GetComponent<PlayerShuriken>().Deploy(Vector2.right * transform.localScale.x);
        }
    }
    #endregion
}
