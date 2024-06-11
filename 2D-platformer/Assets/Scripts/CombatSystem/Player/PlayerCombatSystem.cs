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
    public GameObject meleeWeaponObject;

    [Header("Combat State")]
    public bool combatMode = false;
    public bool weaponInLethalState = false;
    public bool inKnockedOffAnim = false;
    public bool isKnockedOffGround = false;
    public float knockedOffVelocity;
    public bool isPlayerRolling;
    public bool isHurt;
    public bool isDead;
    public bool isPoiseBroken;
    bool playHitEffectOnScale;
    public bool canChangeDirectionDuringAttack;

    [Header("Combat Item Details")]
    public bool usingCombatItem = false;
    public bool usingUtilityItem = false;
    public Transform projectileSpawnPosition;
    public int activeCombatItemID, activeUtilityItemID;
    public string combatItemName = "ThrowingKnife";

    [Header("Light Attack Details")]
    public bool nextLightAttackQueued;
    [HideInInspector]
    public int activeLightAttackID;
    public float playerScaleShrinkSpeed;
    public float shrinkSizeMultiplier;
    float initialPlayerYScale;
    bool playerScaleDecreasing;
    public bool rollAttackInProgress;
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
        activeLightAttackID = 0;
        nextLightAttackQueued = false;
        rollAttackInProgress = false;
        isDead = false;
        canChangeDirectionDuringAttack = true;
    }

    private void Start()
    {
        if (playerHealthStaminaScript == null)
        {
            Debug.LogError("playerHealthStaminaScript is required. Drag and drop stamina slider object.");
        }
    }

    private void Update()
    {
        isPlayerRolling = playerRoll.isExecuting;
        #region Blocking
        if (isBlockingRequested && playerHealthStaminaScript.currentStamina >= minimumStaminaToBlock)
        {
            isBlocking = true;
            playerSpineAnimator.SetBool("Defence", true);
            // playerSpineAnimator.SetTrigger("blockTrigger");
        }
        else
        {
            isBlocking = false;
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
        if (combatMode == false) return;

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
        if (!lightAttackExecuting && !playerColumn.hasGrabbedColumn && !playerMovementScript.isSprinting)
        {
            if (combatMode == true)
            {
                combatMode = playerJump.combatMode = false;

                playerSpineAnimator.SetBool("CombatMode", false);
            }
            else
            {
                combatMode = playerJump.combatMode = true;

                playerSpineAnimator.SetBool("CombatMode", true);
                playerSpineAnimator.SetInteger("WeaponID", 1);
            }
        }
    }

    public void OnWeaponLightAttack(InputAction.CallbackContext context)
    {
        if (combatMode == false || MovementLimiter.instance.playerCanAttack == false || nextLightAttackQueued || rollAttackInProgress) return;

        if (!playerRoll.isExecuting && playerMovementScript.isSprinting == true) return;

        if (playerColumn.hasGrabbedColumn) return;

        if (isPlayerRolling == true)
        {
            rollAttackInProgress = true;
            currentPlayerAttackID = 5;
            playerSpineAnimator.SetTrigger("Attack");
            playerSpineAnimator.SetInteger("AttackID", 5);
        }
        else
        {
            if (activeLightAttackID == 0)//Light Attack started
            {
                activeLightAttackID++;
                currentPlayerAttackID = activeLightAttackID;
                playerSpineAnimator.SetTrigger("Attack");
                playerSpineAnimator.SetInteger("AttackID", 1);
            }
            else if (activeLightAttackID == 1)//First Repeating Attack queued, currentAttackID not updated
            {
                activeLightAttackID++;
                nextLightAttackQueued = true;
                playerSpineAnimator.SetBool("LAttackQueued", true);
            }
            else if (activeLightAttackID > 1 && activeLightAttackID < 4)//Second and further repeating attacks queued, currentAttackID always 1 behind lightAttackID
            {
                activeLightAttackID++;
                currentPlayerAttackID = activeLightAttackID - 1;
                nextLightAttackQueued = true;
                playerSpineAnimator.SetBool("LAttackQueued", true);
            }
            else if (activeLightAttackID == 4)
            {
                return;
            }
        }

    }

    public void OnWeaponAttack3(InputAction.CallbackContext context)
    {
        if (MovementLimiter.instance.playerCanAttack == false || combatMode == false) return;

        if (!lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            currentPlayerAttackID = 3;
            playerSpineAnimator.SetTrigger("Attack");
            playerSpineAnimator.SetInteger("AttackID", 3);
        }
    }

    public void OnRunAttack(InputAction.CallbackContext context)
    {
        if (MovementLimiter.instance.playerCanAttack == false || combatMode == false || !playerMovementScript.isSprinting || playerRoll.isExecuting) return;

        if (!lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            currentPlayerAttackID = 4;
            playerSpineAnimator.SetTrigger("Attack");
            playerSpineAnimator.SetInteger("AttackID", 4);
        }
    }

    public void OnRollAttack(InputAction.CallbackContext context)
    {
        if (MovementLimiter.instance.playerCanAttack == false || combatMode == false) return;

        if (!lightAttackExecuting && !playerColumn.hasGrabbedColumn)
        {
            currentPlayerAttackID = 5;
            playerSpineAnimator.SetTrigger("Attack");
            playerSpineAnimator.SetInteger("AttackID", 5);
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
        if (MovementLimiter.instance.playerCanAttack == false || combatMode == false) return;

        if (context.phase == InputActionPhase.Started)
        {
            cutOffHeavyAttackCharge = currentHeavyAttackCharge = 0.0f;
            isHeavyAttackKeyPressed = true;
            playerSpineAnimator.ResetTrigger("ChargeAttack");
            playerSpineAnimator.SetBool("Charging", true);
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
        playerSpineAnimator.SetTrigger("ChargeAttack");
        playerSpineAnimator.SetBool("Charging", false);
        timeElapsedSinceHeavyAttack = 0f;
        heavyAttackDone = true;
        //cutOffHeavyAttackCharge = 0.0f;
    }

    private void ExecuteLowPoweredAttack()
    {
        currentPlayerAttackID = 6;
        Debug.Log("heavy attack (low damage)...");
        heavyAttackDamageMultiplier = (float)failedHeavyAttackDamage / successfulHeavyAttackDamage;
        currentHeavyAttackCharge = 0f;
        isHeavyAttackKeyPressed = false;
        playerSpineAnimator.SetTrigger("ChargeAttack");
        playerSpineAnimator.SetBool("Charging", false);
        timeElapsedSinceHeavyAttack = 0f;
        heavyAttackDone = true;
    }
    #endregion
    public void TakeDamage(EnemyAttackInfo weaponAttackInfo, bool attackFromRight, bool isChargeAttack = false)
    {
        if (isDead == true) return;
        //Debug.Log("Dam");
        //Debug.Log("dmgA: " + weaponAttackInfo.attackDamage + "stmA: " + weaponAttackInfo.attackStaminaDamage + "rightBool: " + attackFromRight);
        if (!isBlocking || (isBlocking && IncomingAttackDirectionFacingPlayer(attackFromRight) == false) || (isBlocking && playerHealthStaminaScript.staminaCompletelyDepleted == true))//i not blocking, or attacked from back
        {
            //Debug.Log("Dam1");
            if (isChargeAttack == true)
            {
                PlayOnHitEffect();

                if (!isHurt && !isKnockedOffGround) playerSpineAnimator.SetTrigger("KnockedOffGroundShort");
                //Debug.Log("fkn velocity is: " + weaponAttackInfo.attackKnockedOffVelocity);
                knockedOffVelocity = weaponAttackInfo.attackKnockedOffVelocity;

                if (playerHealthStaminaScript.modifyHealth(-weaponAttackInfo.attackDamage) < 0.03f)
                {
                    playerSpineAnimator.SetTrigger("DeadOnGround");
                    isDead = true;

                    GameController.Instance.OnPlayerDead();
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
                    isDead = true;

                    GameController.Instance.OnPlayerDead();
                }
                else if (playerHealthStaminaScript.poiseBroken == true)
                {
                    //play hurt animation
                    if (!isHurt && !isKnockedOffGround) playerSpineAnimator.SetTrigger("Hurt");
                    playerHealthStaminaScript.recoverPoise();
                }
            }
        }
        else if (isBlocking && playerHealthStaminaScript.staminaCompletelyDepleted == false)
        {
            //Debug.Log("Dam2");
            if (weaponAttackInfo.isBlockable)
            {
                //Debug.Log("Dam3");
                //charging blockable attacks
                if (isChargeAttack)
                {
                    if (playerHealthStaminaScript.currentStamina >= weaponAttackInfo.attackStaminaDamage)
                    {
                        HandleAttackDuringDefence(weaponAttackInfo.attackStaminaDamage);
                        //playerSpineAnimator.SetTrigger("DefenceChargeHit");
                        if (!isHurt && !isKnockedOffGround) playerSpineAnimator.SetTrigger("DefenceChargeHit");
                        knockedOffVelocity = weaponAttackInfo.defenceKnockedOffVelocity;
                    }
                    else
                    {
                        playerHealthStaminaScript.modifyStamina(-999.0f);
                        if (!isHurt && !isKnockedOffGround) playerSpineAnimator.SetTrigger("KnockedOffGroundShort");
                        knockedOffVelocity = weaponAttackInfo.attackKnockedOffVelocity;

                        if (playerHealthStaminaScript.modifyHealth(-(weaponAttackInfo.attackDamage - playerHealthStaminaScript.currentStamina)) < 0.10f)
                        {
                            playerSpineAnimator.SetTrigger("DeadOnGround");
                            isDead = true;

                            GameController.Instance.OnPlayerDead();
                        }
                    }
                }
                else
                {
                    //Debug.Log("Dam4");
                    // normal blockable attacks
                    if (playerHealthStaminaScript.currentStamina >= weaponAttackInfo.attackStaminaDamage)
                    {
                        //Debug.Log("Dam5");
                        HandleAttackDuringDefence(weaponAttackInfo.attackStaminaDamage);
                        playerSpineAnimator.SetTrigger("DefenceHit");
                    }
                    else
                    {
                        //Debug.Log("Dam6");
                        playerHealthStaminaScript.modifyStamina(-999.0f);
                        playerSpineAnimator.SetTrigger("PoiseBreak");

                        if (playerHealthStaminaScript.modifyHealth(-(weaponAttackInfo.attackDamage - playerHealthStaminaScript.currentStamina)) < 0.10f)
                        {
                            playerSpineAnimator.SetTrigger("Death");
                            isDead = true;

                            GameController.Instance.OnPlayerDead();
                        }
                    }
                }
            }
            else
            {
                //Debug.Log("Dam7");
                float finalDamageAmount;
                PlayOnHitEffect();
                // if current stamina more than half of total stamina, damage will be reduced to half
                if (playerHealthStaminaScript.currentStamina >= playerHealthStaminaScript.totalStamina / 2f)
                {
                    //Debug.Log("Dam8");
                    finalDamageAmount = weaponAttackInfo.attackDamage * .6f;
                }
                else // else will get full damage
                {
                    //Debug.Log("Dam9");
                    finalDamageAmount = weaponAttackInfo.attackDamage;
                }

                if (isChargeAttack)
                {
                    if (!isHurt && !isKnockedOffGround) playerSpineAnimator.SetTrigger("KnockedOffGroundShort");
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
                        isDead = true;

                        GameController.Instance.OnPlayerDead();
                    }
                }
                else
                {
                    //Debug.Log("Dam10");
                    if (!isChargeAttack)
                    {
                        if (!isHurt && !isKnockedOffGround) playerSpineAnimator.SetTrigger("Hurt");
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
        isHurt = true;
        isBlocking = false;

        lightAttackExecuting = heavyAttackExecuting = false;
        nextLightAttackQueued = false;
        activeLightAttackID = 0;

        usingCombatItem = usingUtilityItem = false;
    }

    public void OnPlayerHurtEnd()
    {
        isHurt = false;
    }

    public void OnPlayerPoiseBreakStart()
    {
        isPoiseBroken = true;
    }

    public void OnPlayerPoiseBreakEnd()
    {
        isPoiseBroken = false;
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
        if (isDead == true) return;
        Debug.Log("PDam");
        PlayOnHitEffect();
        // !blocking
        if (playerHealthStaminaScript.modifyHealth(-damageAmount) < 0.10f)
        {
            playerSpineAnimator.SetTrigger("Death");
            isDead = true;

            GameController.Instance.OnPlayerDead();
        }
    }

    public void TakeEnvironmentDamage(float damageAmount)
    {
        if (isDead == true) return;

        PlayOnHitEffect();
        // !blocking
        if (playerHealthStaminaScript.modifyHealth(-damageAmount) < 0.10f)
        {
            playerSpineAnimator.SetTrigger("Death");
            isDead = true;

            GameController.Instance.OnPlayerDead();
        }
    }

    public void TakeProjectileShieldDamage(float damageAmount, float staminaDamageAmount, bool isBlockableAttack = true)
    {
        if (isDead == true) return;
        Debug.Log("PSDam");

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
                    isDead = true;

                    GameController.Instance.OnPlayerDead();
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

    #region Unused
    IEnumerator sampleCoRoutine()
    {
        yield return new WaitForSeconds(.1f);
        bool platChanged = false;
        //Do stuff here
    }

    // public void OnWeaponAttack1RollAttack(InputAction.CallbackContext context)
    // {
    //     if (combatMode == false || MovementLimiter.instance.playerCanAttack == false) return;
    //     else if (!playerRoll.isExecuting && playerMovementScript.isSprinting == true) return;

    //     if (!lightAttackExecuting && !playerColumn.hasGrabbedColumn)
    //     {
    //         if (isPlayerRolling == true)
    //         {
    //             playerRoll.isExecuting = isPlayerRolling = false;
    //             currentPlayerAttackID = 5;
    //             playerSpineAnimator.SetTrigger("Attack");
    //             playerSpineAnimator.SetInteger("AttackID", 5);
    //         }
    //         else
    //         {
    //             currentPlayerAttackID = 1;
    //             playerSpineAnimator.SetTrigger("Attack");
    //             playerSpineAnimator.SetInteger("AttackID", 1);
    //         }
    //     }
    // }

    // public void OnWeaponAttack2(InputAction.CallbackContext context)
    // {
    //     if (combatMode == false || playerMovementScript.isSprinting == true || MovementLimiter.instance.playerCanAttack == false || playerRoll.isExecuting) return;

    //     if (!lightAttackExecuting && !playerColumn.hasGrabbedColumn)
    //     {
    //         currentPlayerAttackID = 2;
    //         playerSpineAnimator.SetTrigger("Attack");
    //         playerSpineAnimator.SetInteger("AttackID", 2);
    //     }
    // }
    #endregion
}
