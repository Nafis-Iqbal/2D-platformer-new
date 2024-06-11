using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    private PlayerCombatSystem playerCombatSystemScript;
    private Transform playerTransform;
    private Rigidbody2D playerRB2D;
    private PlayerMovement playerMovement;
    private PlayerJump playerJump;
    private PlayerRoll playerRoll;
    private PlayerDodge playerDodge;
    private PlayerColumn playerColumn;
    private PlayerAppearanceScript playerAppearanceScript;

    private void Awake()
    {
        playerTransform = GameManager.Instance.playerTransform;
        playerRB2D = playerTransform.GetComponent<Rigidbody2D>();
        playerCombatSystemScript = playerTransform.GetComponent<PlayerCombatSystem>();

        playerMovement = playerTransform.GetComponent<PlayerMovement>();
        playerJump = playerTransform.GetComponent<PlayerJump>();
        playerColumn = playerTransform.GetComponent<PlayerColumn>();
        playerRoll = playerTransform.GetComponent<PlayerRoll>();
        playerDodge = playerTransform.GetComponent<PlayerDodge>();
        playerAppearanceScript = playerTransform.GetComponent<PlayerAppearanceScript>();
    }

    #region Player Movement Limiter
    public void RestrictPlayerMovement()
    {
        MovementLimiter.instance.playerCanMove = false;
        playerMovement.stopPlayerCompletely();
    }

    public void EnablePlayerMovement()
    {
        MovementLimiter.instance.playerCanMove = true;
    }

    public void RestrictPlayerCombatMoves()
    {
        MovementLimiter.instance.playerCanAttack = false;
    }

    public void EnablePlayerCombatMoves()
    {
        MovementLimiter.instance.playerCanAttack = true;
    }

    public void RestrictPlayerParkourMoves()
    {
        MovementLimiter.instance.playerCanParkour = false;
    }

    public void EnablePlayerParkourMoves()
    {
        MovementLimiter.instance.playerCanParkour = true;
    }

    public void RestrictPlayerRigidbody()
    {
        Debug.Log("simu1");
        playerRB2D.simulated = false;
    }

    public void EnablePlayerRigidbody()
    {
        Debug.Log("simu2");
        playerRB2D.simulated = true;
    }
    #endregion

    #region Movement Physics Tweaks
    public void DisableCharging()
    {
        playerJump.DisableCharging();
    }

    public void ResetRollingVariables()
    {
        playerRoll.DeactivateRolling();
        playerCombatSystemScript.rollAttackInProgress = false;
    }

    public void ResetDodgingVariables()
    {
        playerDodge.DeactivateDodging();
    }

    public void MovementJumpRollMomentum()
    {
        MovementOnJumpEnd();
        playerMovement.applyPlayerMomentum(10.0f);
    }
    #endregion

    #region Combat Physics Tweaks
    public void ArmedWeaponsOnSwordSlash()
    {
        playerMovement.applyPlayerMomentum(.50f);
    }

    public void ArmedWeaponsOnSwordSwing()
    {
        playerMovement.applyPlayerMomentum(.50f);
    }

    public void ArmedWeaponsOnSwordThrust()
    {
        playerMovement.applyPlayerMomentum(1.25f);
    }

    public void ArmedWeaponsJumpAttackMomentum()
    {
        playerMovement.applyPlayerMomentum(12.0f);
    }

    public void ArmedWeaponsChargedAttackMomentum()
    {
        playerMovement.applyVariablePlayerMomentum("ChargeAttack", 20.0f);
    }
    #endregion

    #region Combat Projectile Weapons
    public void ThrowableItemsOnProjectileThrow()
    {
        playerCombatSystemScript.shootProjectile();
    }
    #endregion

    #region Combat State Change Methods
    public void LightAttackStarted()
    {
        RestrictPlayerMovement();
        playerCombatSystemScript.playerSpineAnimator.SetBool("LAttackQueued", false);
        playerCombatSystemScript.lightAttackExecuting = true;
        if (playerCombatSystemScript.activeLightAttackID <= 1) playerCombatSystemScript.nextLightAttackQueued = false;
    }

    public void LightAttackEnded()
    {
        playerCombatSystemScript.lightAttackExecuting = false;
        playerCombatSystemScript.nextLightAttackQueued = false;
        playerCombatSystemScript.rollAttackInProgress = false;
        playerCombatSystemScript.activeLightAttackID = 0;

        playerRoll.isExecuting = playerCombatSystemScript.isPlayerRolling = false;
        EnablePlayerMovement();
    }

    public void LightAttackChunkEnded()
    {
        playerCombatSystemScript.nextLightAttackQueued = false;
    }

    public void HeavyAttackStarted()
    {
        playerCombatSystemScript.heavyAttackExecuting = true;
    }

    public void HeavyAttackEnded()
    {
        playerCombatSystemScript.heavyAttackExecuting = false;
    }

    public void EnableWeaponLethality()
    {
        playerCombatSystemScript.weaponInLethalState = true;
        playerCombatSystemScript.meleeWeaponObject.SetActive(true);
    }

    public void DisableWeaponLethality()
    {
        playerCombatSystemScript.weaponInLethalState = false;
        playerCombatSystemScript.meleeWeaponObject.SetActive(false);
    }

    public void CombatPromptsHurtStart()
    {
        RestrictPlayerMovement();
        playerCombatSystemScript.OnPlayerHurtStart();
    }

    public void CombatPromptsHurtEnd()
    {
        EnablePlayerMovement();
        playerCombatSystemScript.OnPlayerHurtEnd();
    }

    public void CombatPromptsCombatItemUseStart()
    {
        RestrictPlayerMovement();
        playerCombatSystemScript.usingCombatItem = true;
    }

    public void CombatPromptsCombatItemUseEnd()
    {
        EnablePlayerMovement();
        playerCombatSystemScript.usingCombatItem = false;
    }

    public void CombatPromptsKnockedOffAnimStart()
    {
        RestrictPlayerMovement();
        playerCombatSystemScript.inKnockedOffAnim = true;
        playerCombatSystemScript.lightAttackExecuting = false;
    }

    public void CombatPromptsKnockedOffAnimEnd()
    {
        EnablePlayerMovement();
        playerCombatSystemScript.inKnockedOffAnim = false;
    }

    public void CombatPromptsOnKnockedOffStart()
    {
        playerCombatSystemScript.isKnockedOffGround = true;
        playerRoll.isExecuting = false;
        playerDodge.isExecuting = false;
    }

    public void CombatPromptsOnKnockedOffMid()
    {
        playerCombatSystemScript.knockedOffVelocity *= .50f;
    }

    public void CombatPromptsOnKnockedOffEnd()
    {
        playerCombatSystemScript.isKnockedOffGround = false;
    }
    #endregion

    #region Player Movement Animation Events
    public void MovementOnJumpStart()
    {
        MovementLimiter.instance.playerCanMove = true;
        MovementLimiter.instance.playerCanAttack = false;
        playerAppearanceScript.jumpEffects();
        playerJump.DisableCharging();
        playerJump.StartJump();
        playerJump.jumpAnimInProgress = true;
        playerJump.jumpInProgress = true;

        PlayerInputManager.Instance.playerInputActions.Player.Jump.Disable();
        PlayerInputManager.Instance.playerInputActions.Player.RollDodge.Disable();
    }

    public void MovementOnJumpAnimEnd()
    {
        playerJump.jumpAnimInProgress = false;
        playerJump.wallJumpAnimInProgress = false;
        playerMovement.isSlideJumping = false;
    }

    public void MovementOnJumpEnd()
    {
        playerJump.jumpInProgress = false;
        RestrictPlayerMovement();
    }

    public void MovementOnSlideJump()
    {
        playerMovement.AddSlideJumpForce();
    }

    public void MovementOnSlide()
    {
        playerJump.ResetJumpVariables();
    }

    public void MovementOnWallLadderJumpStart()
    {
        playerJump.wallJumpAnimInProgress = true;
    }

    public void MovementOnWallLadderJumpEnd()
    {
        playerJump.wallJumpAnimInProgress = false;
        playerJump.spineAnimator.SetBool("SlideJump", false);
    }

    public void ResetOnJumpDropEnd()
    {
        playerJump.DisableCharging();
        playerMovement.slideJumpQueued = false;
        playerJump.ResetJumpVariables();
        MovementLimiter.instance.playerCanParkour = true;
        MovementLimiter.instance.playerCanMove = true;
        MovementLimiter.instance.playerCanAttack = true;

        PlayerInputManager.Instance.playerInputActions.Player.RollDodge.Enable();
        PlayerInputManager.Instance.playerInputActions.Player.Jump.Enable();
    }

    public void TriggerSystemsPlayerOnAir()
    {
        PlayerInputManager.Instance.playerInputActions.Player.GrapplingGun.Enable();
    }

    public void TriggerSystemsPlayerLanding()
    {
        PlayerInputManager.Instance.playerInputActions.Player.GrapplingGun.Disable();
        playerMovement.slideJumpQueued = false;
        playerJump.ResetJumpVariables();
        playerJump.jumpAnimInProgress = false;
        playerJump.wallJumpAnimInProgress = false;
    }

    public void MovementEnableGrappleBoost()
    {
        PlayerInputManager.Instance.playerInputActions.Player.GrappleBoost.Enable();
        playerJump.ResetJumpVariables();
    }

    public void DisableGrappleBoost()
    {
        PlayerInputManager.Instance.playerInputActions.Player.GrappleBoost.Disable();
    }

    public void MovementOnClimbLedgeStart()
    {
        playerColumn.isClimbingLedge = true;
        EnablePlayerParkourMoves();
        RestrictPlayerRigidbody();
    }

    public void MovementOnClimbLedgeEnd()
    {
        playerColumn.isClimbingLedge = false;
        playerJump.lastClimbUpTime = Time.time;
        MovementLimiter.instance.playerCanAttack = true;
        EnablePlayerRigidbody();
    }
    #endregion
}
