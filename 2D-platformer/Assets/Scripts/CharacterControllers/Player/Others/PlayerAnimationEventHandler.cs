using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    private PlayerCombatSystem playerCombatSystemScript;
    private GameObject lightAttackHitBox;
    private PlayerJump playerJump;

    private void Awake()
    {
        var playerTransform = GameManager.Instance.playerTransform;
        playerCombatSystemScript = playerTransform.GetComponent<PlayerCombatSystem>();

        playerJump = playerTransform.GetComponent<PlayerJump>();
        lightAttackHitBox = GameManager.Instance.lightAttackHitBox;
    }

    public void LightAttackStarted()
    {
        playerCombatSystemScript.lightAttackExecuting = true;
    }

    public void LightAttackEnded()
    {
        playerCombatSystemScript.lightAttackExecuting = false;
    }

    public void HeavyAttackStarted()
    {
        playerCombatSystemScript.heavyAttackExecuting = true;
    }

    public void HeavyAttackEnded()
    {
        playerCombatSystemScript.heavyAttackExecuting = false;
    }

    public void AddJumpForce()
    {
        playerJump.StartJump();
    }

    public void DisableCharging()
    {
        playerJump.DisableCharging();
    }

}
