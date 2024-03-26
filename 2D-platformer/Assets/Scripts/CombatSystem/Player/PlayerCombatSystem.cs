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
    private HealthStaminaSystem playerHealthStaminaScript;
    private PlayerMovement playerMovementScript;
    private PlayerColumn playerColumn;
    private PlayerJump playerJump;
    private PlayerRoll playerRoll;
    private PlayerDodge playerDodge;
    public GameObject blockDefenseObject;
    public GameObject meleeWeaponObject;

    [Header("Combat State")]
    public bool combatMode = false;
    public bool weaponInLethalState = false;
    public bool isKnockedOffGround = false;
    public float knockedOffVelocity;
    bool playHitEffectOnScale;
    [Header("Combat Item Details")]
    public bool usingCombatItem = false;
    public bool usingUtilityItem = false;
    public Transform projectileSpawnPosition;
    public int activeCombatItemID, activeUtilityItemID;
    public string combatItemName = "ThrowingKnife";

    [Header("Light Attack Details")]
    public float playerScaleShrinkSpeed;
    public float shrinkSizeMultiplier;
    float initialPlayerYScale;
    bool playerScaleDecreasing;
    public int currentPlayerAttackID;
    public bool lightAttackExecuting = false;

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

    [Header("Block Costs")]
    public float minimumStaminaToBlock = 20f;
    public float blockStaminaMultiplier = 1.4f;
    public bool isBlocking;
    public bool isBlockingCached;
    private bool isBlockingRequested = false;

    private void Awake()
    {
        isBlocking = false;
        playerSpineAnimator = GameManager.Instance.playerSpineAnimator;
        playerMovementScript = GetComponent<PlayerMovement>();
        playerJump = GetComponent<PlayerJump>();
        playerColumn = GetComponent<PlayerColumn>();
        playerRoll = GetComponent<PlayerRoll>();
        playerDodge = GetComponent<PlayerDodge>();
        playerHealthStaminaScript = GetComponent<HealthStaminaSystem>();
    }

    private void OnEnable()
    {
        initialPlayerYScale = transform.localScale.y;
        combatMode = false;
        playHitEffectOnScale = playerScaleDecreasing = false;
        knockedOffVelocity = 0.0f;
    }

    private void Start()
    {
        if (playerHealthStaminaScript == null)
        {
            Debug.LogError("playerHealthStaminaScript is required. Drag and drop stamina slider object.");
        }
        if (blockDefenseObject == null)
        {
            Debug.LogError("blockDefenseObject is required. Drag and drop block defense object.");
        }
    }

    private void Update()
    {
        #region Blocking
        if (isBlockingRequested && playerHealthStaminaScript.currentStamina >= minimumStaminaToBlock)
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

        #endregion

        #region Hit Effect
        if (playHitEffectOnScale)
        {
            Vector3 tempScale = transform.localScale;

            if (playerScaleDecreasing)
            {
                if ((tempScale.y - Time.deltaTime * playerScaleShrinkSpeed) > initialPlayerYScale * shrinkSizeMultiplier)
                {
                    tempScale.y = tempScale.y - (Time.deltaTime * playerScaleShrinkSpeed);
                }
                else playerScaleDecreasing = false;
            }
            else
            {
                if ((tempScale.y + Time.deltaTime * playerScaleShrinkSpeed) < initialPlayerYScale * shrinkSizeMultiplier)
                {
                    tempScale.y = tempScale.y + (Time.deltaTime * playerScaleShrinkSpeed);
                }
                else
                {
                    tempScale.y = initialPlayerYScale;
                    playerScaleDecreasing = true;
                    playHitEffectOnScale = false;
                }
            }
            transform.localScale = tempScale;
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

    public void HandleAttackDuringDefence(float damageAmount)
    {
        //Debug.Log("in stamina: " + (damageAmount * blockStaminaMultiplier));
        playerHealthStaminaScript.modifyStamina(-(damageAmount * blockStaminaMultiplier));
    }
    #endregion

    #region Light Attack Variants
    public void OnWeaponHolsterPrompted(InputAction.CallbackContext context)
    {
        if (!lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            if (combatMode == true)
            {
                combatMode = playerJump.combatMode = false;

                GameManager.Instance.playerSpineAnimator.SetBool("CombatMode", false);
            }
            else
            {
                combatMode = playerJump.combatMode = true;

                GameManager.Instance.playerSpineAnimator.SetBool("CombatMode", true);
                GameManager.Instance.playerSpineAnimator.SetInteger("WeaponID", 1);
            }
        }
    }

    public void OnWeaponAttack1RollAttack(InputAction.CallbackContext context)
    {
        if (combatMode == false || playerMovementScript.isSprinting == true || MovementLimiter.instance.playerCanAttack == false) return;

        if (!lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            if (playerRoll.isExecuting == true)
            {
                playerRoll.isExecuting = false;
                currentPlayerAttackID = 5;
                GameManager.Instance.playerSpineAnimator.SetTrigger("Attack");
                GameManager.Instance.playerSpineAnimator.SetInteger("AttackID", 5);
            }
            else
            {
                currentPlayerAttackID = 1;
                GameManager.Instance.playerSpineAnimator.SetTrigger("Attack");
                GameManager.Instance.playerSpineAnimator.SetInteger("AttackID", 1);
            }
        }
    }

    public void OnWeaponAttack2(InputAction.CallbackContext context)
    {
        if (combatMode == false || playerMovementScript.isSprinting == true || MovementLimiter.instance.playerCanAttack == false) return;

        if (!lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            currentPlayerAttackID = 2;
            GameManager.Instance.playerSpineAnimator.SetTrigger("Attack");
            GameManager.Instance.playerSpineAnimator.SetInteger("AttackID", 2);
        }
    }

    public void OnWeaponAttack3(InputAction.CallbackContext context)
    {
        if (MovementLimiter.instance.playerCanAttack == false || combatMode == false) return;

        if (!lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            currentPlayerAttackID = 3;
            GameManager.Instance.playerSpineAnimator.SetTrigger("Attack");
            GameManager.Instance.playerSpineAnimator.SetInteger("AttackID", 3);
        }
    }

    public void OnRunAttack(InputAction.CallbackContext context)
    {
        if (MovementLimiter.instance.playerCanAttack == false || combatMode == false) return;

        if (!lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            currentPlayerAttackID = 4;
            GameManager.Instance.playerSpineAnimator.SetTrigger("Attack");
            GameManager.Instance.playerSpineAnimator.SetInteger("AttackID", 4);
        }
    }

    public void OnRollAttack(InputAction.CallbackContext context)
    {
        if (MovementLimiter.instance.playerCanAttack == false || combatMode == false) return;

        if (!lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            currentPlayerAttackID = 5;
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
        currentPlayerAttackID = 6;
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
        currentPlayerAttackID = 6;
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
    public void TakeDamage(EnemyAttackInfo weaponAttackInfo, bool attackFromRight, bool isChargeAttack = false)
    {
        if (playerRoll.isExecuting)
        {
            //More to be added
            return;
        }

        //Debug.Log("dmgA: " + weaponAttackInfo.attackDamage + "stmA: " + weaponAttackInfo.attackStaminaDamage + "rightBool: " + attackFromRight);
        if (!isBlocking || (isBlocking && IncomingAttackDirectionFacingPlayer(attackFromRight) == false) || (isBlocking && playerHealthStaminaScript.staminaCompletelyDepleted == true))//i not blocking, or attacked from back
        {
            if (isChargeAttack == true)
            {
                PlayOnHitEffect();

                playerSpineAnimator.SetTrigger("KnockedOffGroundShort");
                knockedOffVelocity = weaponAttackInfo.attackKnockedOffVelocity;

                if (playerHealthStaminaScript.modifyHealth(-weaponAttackInfo.attackDamage) < 0.03f)
                {
                    playerSpineAnimator.SetTrigger("DeadOnGround");
                }

                playerHealthStaminaScript.modifyStamina(-999.0f);
            }
            else
            {
                PlayOnHitEffect();
                // !blocking
                if (playerHealthStaminaScript.modifyHealth(-weaponAttackInfo.attackDamage) < 0.03f)
                {
                    playerSpineAnimator.SetTrigger("Death");
                }
                else if (playerHealthStaminaScript.poiseBroken == true)
                {
                    //play hurt animation
                    playerSpineAnimator.SetTrigger("Hurt");
                    playerHealthStaminaScript.recoverPoise();
                }
            }
        }
        else if (isBlocking && playerHealthStaminaScript.staminaCompletelyDepleted == false)
        {
            if (weaponAttackInfo.isBlockable)
            {
                //charging blockable attacks
                if (isChargeAttack)
                {
                    if (playerHealthStaminaScript.currentStamina >= weaponAttackInfo.attackStaminaDamage)
                    {
                        HandleAttackDuringDefence(weaponAttackInfo.attackStaminaDamage);
                        //playerSpineAnimator.SetTrigger("DefenceChargeHit");
                        playerSpineAnimator.SetTrigger("KnockedOffGroundShort");
                        knockedOffVelocity = weaponAttackInfo.defenceKnockedOffVelocity;
                    }
                    else
                    {
                        playerHealthStaminaScript.modifyStamina(-999.0f);
                        playerSpineAnimator.SetTrigger("KnockedOffGroundShort");
                        knockedOffVelocity = weaponAttackInfo.attackKnockedOffVelocity;

                        if (playerHealthStaminaScript.modifyHealth(-(weaponAttackInfo.attackDamage - playerHealthStaminaScript.currentStamina)) < 0.10f)
                        {
                            playerSpineAnimator.SetTrigger("DeadOnGround");
                        }
                    }
                }
                else
                {
                    // normal blockable attacks
                    if (playerHealthStaminaScript.currentStamina >= weaponAttackInfo.attackStaminaDamage)
                    {
                        HandleAttackDuringDefence(weaponAttackInfo.attackStaminaDamage);
                        playerSpineAnimator.SetTrigger("DefenceHit");
                    }
                    else
                    {
                        playerHealthStaminaScript.modifyStamina(-999.0f);
                        playerSpineAnimator.SetTrigger("PoiseBreak");

                        if (playerHealthStaminaScript.modifyHealth(-(weaponAttackInfo.attackDamage - playerHealthStaminaScript.currentStamina)) < 0.10f)
                        {
                            playerSpineAnimator.SetTrigger("Death");
                        }
                    }
                }
            }
            else
            {
                float finalDamageAmount;
                PlayOnHitEffect();
                // if current stamina more than half of total stamina, damage will be reduced to half
                if (playerHealthStaminaScript.currentStamina >= playerHealthStaminaScript.totalStamina / 2f)
                {
                    finalDamageAmount = weaponAttackInfo.attackDamage * .6f;
                }
                else // else will get full damage
                {
                    finalDamageAmount = weaponAttackInfo.attackDamage;
                }

                if (isChargeAttack)
                {
                    playerSpineAnimator.SetTrigger("KnockedOffGroundShort");
                    knockedOffVelocity = weaponAttackInfo.attackKnockedOffVelocity;
                }

                if (playerHealthStaminaScript.modifyHealth(finalDamageAmount) < .03f)
                {
                    if (isChargeAttack)
                    {
                        playerSpineAnimator.SetTrigger("DeadOnGround");
                    }
                    else
                    {
                        playerSpineAnimator.SetTrigger("Death");
                    }
                }
                else
                {
                    if (!isChargeAttack)
                    {
                        playerSpineAnimator.SetTrigger("Hurt");
                    }
                }

                playerHealthStaminaScript.modifyStamina(-1000);
            }
        }
    }

    public void PlayOnHitEffect()
    {
        playHitEffectOnScale = true;
        playerScaleDecreasing = true;
    }

    public void OnPlayerHurtStart()
    {
        isBlocking = false;
        lightAttackExecuting = heavyAttackExecuting = false;
        usingCombatItem = usingUtilityItem = false;
    }

    public void OnPlayerHurtEnd()
    {

    }

    public bool IncomingAttackDirectionFacingPlayer(bool attackFromRight)
    {
        //returns true when incoming attack from front, false when attacked from behind
        if (attackFromRight && playerMovementScript.playerFacingRight == true) return true;
        else if (!attackFromRight && playerMovementScript.playerFacingRight == false) return true;
        else return false;
    }

    public void TakeProjectileDamage(float damageAmount)
    {
        PlayOnHitEffect();
        // !blocking
        if (playerHealthStaminaScript.modifyHealth(-damageAmount) < 0.10f)
        {
            playerSpineAnimator.SetTrigger("Death");
        }
    }

    public void TakeProjectileShieldDamage(float damageAmount, float staminaDamageAmount, bool isBlockableAttack = true)
    {
        if (isBlockableAttack)
        {
            // blocking and blockable attack
            if (playerHealthStaminaScript.currentStamina >= staminaDamageAmount)
            {
                HandleAttackDuringDefence(staminaDamageAmount);
            }
            else
            {
                if (playerHealthStaminaScript.modifyHealth(-damageAmount / 2.0f) < 0.10f)
                {
                    playerSpineAnimator.SetTrigger("Death");
                }
            }
        }
        else
        {
            PlayOnHitEffect();
            // if current stamina more than half of total stamina, damage will be reduced to half
            if (playerHealthStaminaScript.currentStamina >= playerHealthStaminaScript.totalStamina / 2f)
            {
                playerHealthStaminaScript.modifyHealth(-(damageAmount / 2));
            }
            else // else will get full damage
            {
                playerHealthStaminaScript.modifyHealth(-damageAmount);
            }

            playerHealthStaminaScript.modifyStamina(-1000);
        }
    }

    #region Special Items Use
    public void OnItemUse(InputAction.CallbackContext context)
    {
        if (!usingUtilityItem && !playerColumn.hasGrabbedColumn && playerJump.onGround)
        {
            playerSpineAnimator.SetTrigger("UseItem");
            playerSpineAnimator.SetInteger("ItemID", activeUtilityItemID);
        }
    }

    public void OnCombatItemUse(InputAction.CallbackContext context)
    {
        if (!usingCombatItem && !playerColumn.hasGrabbedColumn && playerJump.onGround && playerJump.combatMode == true)
        {
            playerSpineAnimator.SetTrigger("ProjectileThrow");
            playerSpineAnimator.SetInteger("ProjectileID", activeCombatItemID);
        }
    }

    public void shootProjectile()
    {
        var projectile = ObjectPooler.Instance.SpawnFromPool(combatItemName, projectileSpawnPosition.position, Quaternion.identity);
        projectile.GetComponent<LinearProjectile>().initializeProjectile(projectileSpawnPosition.position, Vector2.right * transform.localScale.x, true);
        projectile.SetActive(true);
    }
    #endregion

    IEnumerator sampleCoRoutine()
    {
        yield return new WaitForSeconds(.1f);
        bool platChanged = false;
        //Do stuff here
    }
}
