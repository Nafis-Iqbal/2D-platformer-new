using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    // Instance for Singleton.
    public static PlayerInputManager Instance;
    public PlayerCombatSystem playerCombatSystemScript;
    public PlayerInputActions playerInputActions;
    private PlayerMovement playerMovement;
    private PlayerDash playerDash;
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
        playerDash = playerTransform.GetComponent<PlayerDash>();
        playerRoll = playerTransform.GetComponent<PlayerRoll>();
        playerColumn = playerTransform.GetComponent<PlayerColumn>();
        playerJump = playerTransform.GetComponent<PlayerJump>();
        playerGrapplingGun = playerTransform.GetComponent<PlayerGrapplingGun>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        // player run
        playerInputActions.Player.Run.started += OnRun;
        playerInputActions.Player.Run.performed += OnRun;
        playerInputActions.Player.Run.canceled += OnRun;

        // player walk is being maintained by old input system in PlayerMovement
        // playerInputActions.Player.Walk.started += OnWalk;
        // playerInputActions.Player.Walk.performed += OnWalk;
        // playerInputActions.Player.Walk.canceled += OnWalk;

        // player jump
        playerInputActions.Player.Jump.started += OnJump;
        playerInputActions.Player.Jump.performed += OnJump;
        playerInputActions.Player.Jump.canceled += OnJumpReleased;

        // player roll
        playerInputActions.Player.Roll.started += OnRoll;

        // player dash
        playerInputActions.Player.Dash.performed += OnDash;

        // player column jump
        playerInputActions.Player.ColumnJump.performed += OnColumnJump;

        // player column jump direction set
        playerInputActions.Player.ColumnJumpDirection.started += OnColumnJumpDirection;
        playerInputActions.Player.ColumnJumpDirection.canceled += OnColumnJumpDirection;

        // player column move
        playerInputActions.Player.ColumnMove.started += OnColumnMove;
        playerInputActions.Player.ColumnMove.performed += OnColumnMove;
        playerInputActions.Player.ColumnMove.canceled += OnColumnMove;

        // player sword attack
        playerInputActions.Player.SwordAttack.started += OnSwordAttack;

        // player shuriken attack
        playerInputActions.Player.ShurikenAttack.started += OnShurikenAttack;

        // player projectile attack
        playerInputActions.Player.ProjectileAttack.started += OnProjectileAttack;

        // player grappling gun
        playerInputActions.Player.GrapplingGun.started += OnGrapplingGun;

        // player blocking defense
        playerInputActions.Player.Blocking.started += OnBlockingDefense;
        playerInputActions.Player.Blocking.canceled += OnBlockingDefense;

        // player heavy attack
        playerInputActions.Player.HeavyAttack.started += OnHeavyAttack;
        playerInputActions.Player.HeavyAttack.canceled += OnHeavyAttack;
    }

    private void Update()
    {
        // handle input interference

        // run
        if (playerJump.isCharging ||
            (playerCombatSystemScript.isBlockingCached && playerJump.onGround) ||
            (playerCombatSystemScript.lightAttackExecuting && playerJump.onGround) ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerColumn.isExecuting)
        {
            playerInputActions.Player.Run.Disable();
        }
        else
        {
            playerInputActions.Player.Run.Enable();
        }

        // jump
        if (playerRoll.isExecuting ||
            playerDash.isExecuting ||
            playerColumn.isExecuting ||
            playerCombatSystemScript.lightAttackExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
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
            playerDash.isExecuting ||
            playerCombatSystemScript.lightAttackExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.Roll.Disable();
        }
        else
        {
            playerInputActions.Player.Roll.Enable();
        }

        // dash
        if (playerCombatSystemScript.lightAttackExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerRoll.isExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.Dash.Disable();
        }
        else
        {
            playerInputActions.Player.Dash.Enable();
        }

        // column ledge grab

        // column jump

        // column jump direction

        // column move
        if (playerColumn.isExecuting)
        {
            playerInputActions.Player.ColumnJump.Enable();
            playerInputActions.Player.ColumnMove.Enable();
            playerInputActions.Player.ColumnJumpDirection.Enable();
        }
        else
        {
            playerInputActions.Player.ColumnJump.Disable();
            playerInputActions.Player.ColumnMove.Disable();
            playerInputActions.Player.ColumnJumpDirection.Disable();
        }

        // light attack
        if (playerRoll.isExecuting ||
            playerDash.isExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.SwordAttack.Disable();
        }
        else
        {
            playerInputActions.Player.SwordAttack.Enable();
        }

        // heavy attack
        if (!playerJump.onGround ||
            playerRoll.isExecuting ||
            playerDash.isExecuting ||
            playerCombatSystemScript.lightAttackExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.HeavyAttack.Disable();
        }
        else
        {
            playerInputActions.Player.HeavyAttack.Enable();
        }

        // shuriken attack
        if (playerRoll.isExecuting ||
            playerDash.isExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerCombatSystemScript.lightAttackExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.ShurikenAttack.Disable();
        }
        else
        {
            playerInputActions.Player.ShurikenAttack.Enable();
        }

        // projectile attack
        if (playerRoll.isExecuting ||
            playerDash.isExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerCombatSystemScript.lightAttackExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.ProjectileAttack.Disable();
        }
        else
        {
            playerInputActions.Player.ProjectileAttack.Enable();
        }

        // grappling gun
        if (playerRoll.isExecuting ||
            playerDash.isExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerCombatSystemScript.lightAttackExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.GrapplingGun.Disable();
        }
        else
        {
            playerInputActions.Player.GrapplingGun.Enable();
        }

        // blocking
        if (playerRoll.isExecuting ||
            playerDash.isExecuting ||
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

    private void OnHeavyAttack(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnHeavyAttack(context);
    }

    private void OnBlockingDefense(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnBlockDefense(context);
    }

    private void OnGrapplingGun(InputAction.CallbackContext context)
    {
        playerGrapplingGun.OnGrapplingGun(context);
    }

    private void OnProjectileAttack(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnProjectileAttack(context);
    }

    private void OnShurikenAttack(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnShurikenAttack(context);
    }

    private void OnSwordAttack(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnLightAttack(context);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        playerJump.OnJump(context);
    }

    private void OnJumpReleased(InputAction.CallbackContext context)
    {
        playerJump.OnJumpReleased(context);
    }

    private void OnColumnMove(InputAction.CallbackContext context)
    {
        playerColumn.OnColumnMove(context);
    }

    private void OnColumnJumpDirection(InputAction.CallbackContext context)
    {
        playerColumn.OnColumnJumpDirection(context);
    }

    private void OnColumnJump(InputAction.CallbackContext context)
    {
        playerColumn.OnColumnJump(context);
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        playerDash.OnDash(context);
    }

    private void OnRoll(InputAction.CallbackContext context)
    {
        playerRoll.OnRoll(context);
    }

    private void OnWalk(InputAction.CallbackContext context)
    {
        playerInputActions.Player.Run.Disable();
        playerInputActions.Player.Run.Enable();

        playerMovement.OnWalk(context);
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        playerMovement.OnMovement(context);
    }

}
