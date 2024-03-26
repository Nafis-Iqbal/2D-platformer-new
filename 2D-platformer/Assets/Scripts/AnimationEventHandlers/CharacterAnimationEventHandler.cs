using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationEventHandler : MonoBehaviour
{
    public EnemyBase enemyBaseScript;

    public void MovementPauseEndDuringPatrol()
    {
        enemyBaseScript.movementPauseEndDuringPatrol();
    }

    #region Enemy Movement Limiter
    public void RestrictCharacterMovement()
    {
        enemyBaseScript.canMove = false;
        enemyBaseScript.stopCharacterCompletely();
    }

    public void EnableCharacterMovement()
    {
        enemyBaseScript.canMove = true;
    }

    public void RestrictCharacterCombatMoves()
    {
        enemyBaseScript.canAttackPlayer = false;
    }

    public void EnablePlayerCombatMoves()
    {
        enemyBaseScript.canAttackPlayer = true;
    }

    public void RestrictCharacterRigidbody()
    {
        enemyBaseScript.disableCharacterRigidbody();
    }

    public void EnablePlayerRigidbody()
    {
        enemyBaseScript.enableCharacterRigidbody();
    }
    #endregion

    #region Movement Physics Tweaks

    #endregion

    #region Combat Physics Tweaks
    public void ArmedWeaponsOnSwordSlash()
    {
        enemyBaseScript.applyCharacterMomentum(1.0f);
    }

    public void ArmedWeaponsOnSwordSwing()
    {
        enemyBaseScript.applyCharacterMomentum(1.0f);
    }

    public void ArmedWeaponsOnSwordThrust()
    {
        enemyBaseScript.applyCharacterMomentum(1.0f);
    }

    public void ArmedWeaponsJumpAttackMomentum()
    {
        enemyBaseScript.applyCharacterMomentum(12.0f);
    }

    public void ArmedWeaponsSpearThrustMomentum()
    {
        enemyBaseScript.applyCharacterMomentum(2.0f);
    }

    #endregion

    #region Combat State Change Methods
    public void LightAttackStarted()
    {
        RestrictCharacterMovement();
        enemyBaseScript.doingAttackMove = true;
    }

    public void LightAttackEnded()
    {
        enemyBaseScript.combatPromptsOnAttackEnd();
    }

    public void HeavyAttackStarted()
    {
        RestrictCharacterMovement();
        enemyBaseScript.doingAttackMove = true;
    }

    public void HeavyAttackEnded()
    {
        enemyBaseScript.combatPromptsOnAttackEnd();
    }

    public void PatrolToCombatEnd()
    {
        enemyBaseScript.onPatrolToCombatEnd();
    }

    public void EnableWeaponLethality()
    {
        enemyBaseScript.weaponInLethalState = true;
        enemyBaseScript.meleeWeaponObject.SetActive(true);
    }

    public void DisableWeaponLethality()
    {
        enemyBaseScript.weaponInLethalState = false;
        enemyBaseScript.meleeWeaponObject.SetActive(false);
    }

    public void BattleCryStart()
    {
        RestrictCharacterMovement();
        enemyBaseScript.battleCryInProgress = true;
    }

    public void BattleCryEnd()
    {
        EnableCharacterMovement();
        enemyBaseScript.battleCryInProgress = false;
    }

    public void CombatPromptsHurtStart()
    {
        RestrictCharacterMovement();
        enemyBaseScript.onCharacterHurtStart();
    }

    public void CombatPromptsHurtEnd()
    {
        EnableCharacterMovement();
        enemyBaseScript.onCharacterHurtEnd();
    }

    public void CombatPromptsAlertedStart()
    {
        RestrictCharacterMovement();
        enemyBaseScript.isAlerted = true;
    }

    public void CombatPromptsAlertedEnd()
    {
        EnableCharacterMovement();
        enemyBaseScript.isAlerted = false;
    }

    public void CombatPromptsIdleToCombatStart()
    {
        RestrictCharacterMovement();
        enemyBaseScript.isChangingCombatMode = true;
    }

    public void CombatPromptsIdleToCombatEnd()
    {
        EnableCharacterMovement();
        enemyBaseScript.isChangingCombatMode = false;
    }
    #endregion

    #region Enable Disable

    #endregion
    #region Character Movement Animation Events
    public void MovementOnJumpStart()
    {
        // MovementLimiter.instance.playerCanMove = true;
        // playerAppearanceScript.jumpEffects();
        // playerJump.DisableCharging();
        // playerJump.StartJump();
        // playerJump.jumpAnimInProgress = true;

        // PlayerInputManager.Instance.playerInputActions.Player.Jump.Disable();
        // PlayerInputManager.Instance.playerInputActions.Player.RollDodge.Disable();
    }

    public void MovementOnJumpAnimEnd()
    {
        //playerJump.jumpAnimInProgress = false;
    }

    public void ResetOnJumpDropEnd()
    {
        // playerJump.DisableCharging();
        // MovementLimiter.instance.playerCanParkour = true;
        // MovementLimiter.instance.playerCanMove = true;

        // PlayerInputManager.Instance.playerInputActions.Player.RollDodge.Enable();
        // PlayerInputManager.Instance.playerInputActions.Player.Jump.Enable();
    }

    public void TriggerSystemsCharacterOnAir()
    {

    }

    public void TriggerSystemsCharacterLanding()
    {

    }
    #endregion
}
