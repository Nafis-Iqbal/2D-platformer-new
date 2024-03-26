using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerInputManager : MonoBehaviour
{
    // Instance for Singleton. 
    public static PlayerInputManager Instance;
    public PlayerCombatSystem playerCombatSystemScript;
    public PlayerInputActions playerInputActions;
    private PlayerMovement playerMovement;
    private PlayerDodge playerDodge;
    private PlayerRoll playerRoll;
    private PlayerColumn playerColumn;
    private PlayerJump playerJump;
    private PlayerGrapplingGun playerGrapplingGun;

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

        var playerTransform = GameManager.Instance.playerTransform;
        playerCombatSystemScript = playerTransform.GetComponent<PlayerCombatSystem>();

        playerMovement = playerTransform.GetComponent<PlayerMovement>();
        playerDodge = playerTransform.GetComponent<PlayerDodge>();
        playerRoll = playerTransform.GetComponent<PlayerRoll>();
        playerColumn = playerTransform.GetComponent<PlayerColumn>();
        playerJump = playerTransform.GetComponent<PlayerJump>();
        playerGrapplingGun = playerTransform.GetComponent<PlayerGrapplingGun>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        // player move
        playerInputActions.Player.Move.started += OnMove;
        playerInputActions.Player.Move.performed += OnMove;
        playerInputActions.Player.Move.canceled += OnMove;

        //player move
        playerInputActions.Player.Run.started += OnRun;
        playerInputActions.Player.Run.performed += OnRun;
        playerInputActions.Player.Run.canceled += OnRun;

        //Jump
        playerInputActions.Player.Jump.performed += OnJump;

        // player dodge and roll
        //playerInputActions.Player.JumpRoll.started += OnJump;
        playerInputActions.Player.RollDodge.canceled += OnDodge;
        playerInputActions.Player.RollDodge.performed += OnRoll;
        playerInputActions.Player.RollDodge.Enable();

        // player column jump
        playerInputActions.Player.ColumnJumpClimb.performed += OnColumnJumpClimb;

        // player column jump direction set
        playerInputActions.Player.ColumnJumpDirection.started += OnColumnJumpDirection;
        playerInputActions.Player.ColumnJumpDirection.canceled += OnColumnJumpDirection;

        // player column move
        playerInputActions.Player.ColumnMove.started += OnColumnMove;
        playerInputActions.Player.ColumnMove.performed += OnColumnMove;
        playerInputActions.Player.ColumnMove.canceled += OnColumnMove;

        // player sword attack
        playerInputActions.Player.HolsterWeapon.started += OnHolsterWeapon;
        playerInputActions.Player.WeaponAttack1MultiTapAttack2RollAttack.canceled += OnWeaponAttack1RollAttack;
        playerInputActions.Player.WeaponAttack1MultiTapAttack2RollAttack.performed += OnWeaponAttack2;
        playerInputActions.Player.WeaponM3Attack3.started += OnWeaponAttack3;
        playerInputActions.Player.RunningAttack4.started += OnRunAttack;

        // player charged attack
        playerInputActions.Player.ChargedAttack6.started += OnChargedAttack;
        playerInputActions.Player.ChargedAttack6.performed += OnChargedAttack;
        playerInputActions.Player.ChargedAttack6.canceled += OnChargedAttack;

        // player shuriken attack
        playerInputActions.Player.UseCombatItem.started += OnCombatItemUse;

        // player projectile attack
        playerInputActions.Player.UseItem.started += OnItemUse;

        // player grappling gun
        playerInputActions.Player.GrapplingGun.started += OnGrapplingGun;
        playerInputActions.Player.GrapplingGun.Disable();

        playerInputActions.Player.GrappleBoost.started += OnGrappleBoost;
        playerInputActions.Player.GrappleBoost.Disable();

        // player blocking defense
        playerInputActions.Player.Blocking.started += OnBlockingDefense;
        playerInputActions.Player.Blocking.canceled += OnBlockingDefense;

        // player walk is being maintained by old input system in PlayerMovement
        // playerInputActions.Player.Walk.started += OnWalk;
        // playerInputActions.Player.Walk.performed += OnWalk;
        // playerInputActions.Player.Walk.canceled += OnWalk;
    }

    private void Update()
    {
        // handle input interference

        // run
        if (playerJump.isCharging ||
            (playerCombatSystemScript.isBlockingCached && playerJump.onGround) ||
            (playerCombatSystemScript.lightAttackExecuting && playerJump.onGround) ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerColumn.isExecuting || playerRoll.isExecuting || playerDodge.isExecuting)
        {
            playerInputActions.Player.Move.Disable();
        }
        else
        {
            playerInputActions.Player.Move.Enable();
        }

        // jump
        if (playerRoll.isExecuting ||
            playerDodge.isExecuting ||
            playerColumn.isExecuting ||
            playerCombatSystemScript.lightAttackExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached ||
            playerColumn.isClimbingLedge)
        {
            playerInputActions.Player.Jump.Disable();
        }
        else
        {
            playerInputActions.Player.Jump.Enable();
        }

        // roll
        if (!playerJump.onGround ||
            playerJump.isCharging ||
            playerDodge.isExecuting ||
            playerCombatSystemScript.lightAttackExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.RollDodge.Disable();
        }
        else
        {
            playerInputActions.Player.RollDodge.Enable();
        }

        // dodge
        if (playerCombatSystemScript.lightAttackExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerRoll.isExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.RollDodge.Disable();
        }
        else
        {
            playerInputActions.Player.RollDodge.Enable();
        }

        // column ledge grab

        // column jump

        // column jump direction

        // column move
        if (playerColumn.isExecuting)
        {
            playerInputActions.Player.ColumnJumpClimb.Enable();
            playerInputActions.Player.ColumnMove.Enable();
            playerInputActions.Player.ColumnJumpDirection.Enable();
        }
        else
        {
            playerInputActions.Player.ColumnJumpClimb.Disable();
            playerInputActions.Player.ColumnMove.Disable();
            playerInputActions.Player.ColumnJumpDirection.Disable();
        }

        // light attack
        if (playerDodge.isExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerGrapplingGun.isExecuting)
        {
            playerInputActions.Player.WeaponAttack1MultiTapAttack2RollAttack.Disable();
        }
        else
        {
            playerInputActions.Player.WeaponAttack1MultiTapAttack2RollAttack.Enable();
        }

        // heavy attack
        if (!playerJump.onGround ||
            playerRoll.isExecuting ||
            playerDodge.isExecuting ||
            playerCombatSystemScript.lightAttackExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.ChargedAttack6.Disable();
        }
        else
        {
            playerInputActions.Player.ChargedAttack6.Enable();
        }

        // shuriken attack
        if (playerRoll.isExecuting ||
            playerDodge.isExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerCombatSystemScript.lightAttackExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.UseCombatItem.Disable();
        }
        else
        {
            playerInputActions.Player.UseCombatItem.Enable();
        }

        // projectile attack
        if (playerRoll.isExecuting ||
            playerDodge.isExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerCombatSystemScript.lightAttackExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.UseItem.Disable();
        }
        else
        {
            playerInputActions.Player.UseItem.Enable();
        }

        // blocking
        if (playerRoll.isExecuting ||
            playerDodge.isExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerCombatSystemScript.lightAttackExecuting ||
            playerGrapplingGun.isExecuting)
        {
            playerInputActions.Player.Blocking.Disable();
        }
        else
        {
            playerInputActions.Player.Blocking.Enable();
        }
    }

    private void OnBlockingDefense(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnBlockDefense(context);
    }

    private void OnGrapplingGun(InputAction.CallbackContext context)
    {
        playerGrapplingGun.OnGrapplingGun(context);
    }

    private void OnGrappleBoost(InputAction.CallbackContext context)
    {
        playerGrapplingGun.OnGrappleBoost(context);
    }

    private void OnItemUse(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnItemUse(context);
    }

    private void OnCombatItemUse(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnCombatItemUse(context);
    }

    private void OnHolsterWeapon(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnWeaponHolsterPrompted(context);
    }

    private void OnWeaponAttack1RollAttack(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnWeaponAttack1RollAttack(context);
    }

    private void OnWeaponAttack2(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnWeaponAttack2(context);
    }

    private void OnWeaponAttack3(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnWeaponAttack3(context);
    }

    private void OnRunAttack(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnRunAttack(context);
    }

    private void OnChargedAttack(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnChargedAttack(context);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        //Debug.Log("Jump Called");
        playerJump.OnJump(context);
    }

    private void OnColumnMove(InputAction.CallbackContext context)
    {
        playerColumn.OnColumnMove(context);
    }

    private void OnColumnJumpDirection(InputAction.CallbackContext context)
    {
        //Debug.Log("Why is this called");
        playerColumn.OnColumnJumpDirection(context);
    }

    private void OnColumnJumpClimb(InputAction.CallbackContext context)
    {
        playerColumn.OnColumnJumpClimb(context);
    }

    private void OnDodge(InputAction.CallbackContext context)
    {
        playerDodge.OnDodge(context);
    }

    private void OnRoll(InputAction.CallbackContext context)
    {
        playerRoll.OnRoll(context);
    }

    private void OnWalk(InputAction.CallbackContext context)
    {
        playerInputActions.Player.Move.Disable();
        playerInputActions.Player.Move.Enable();

        playerMovement.OnWalk(context);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        playerMovement.OnMovement(context);
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        playerMovement.OnRun(context);
    }
}
