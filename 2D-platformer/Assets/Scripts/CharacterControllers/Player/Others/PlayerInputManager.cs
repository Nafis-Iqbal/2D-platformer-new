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

        // player run
        playerInputActions.Player.Run.started += OnRun;
        playerInputActions.Player.Run.performed += OnRun;
        playerInputActions.Player.Run.canceled += OnRun;

        // player jump and roll
        //playerInputActions.Player.JumpRoll.started += OnJump;
        playerInputActions.Player.JumpRoll.canceled += OnJump;
        playerInputActions.Player.JumpRoll.performed += OnRoll;

        // player dodge
        playerInputActions.Player.Dodge.performed += OnDodge;

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
        playerInputActions.Player.WeaponMultiTapAttack1Attack2.canceled += OnWeaponAttack1;
        playerInputActions.Player.WeaponMultiTapAttack1Attack2.performed += OnWeaponAttack2;
        playerInputActions.Player.WeaponM3Attack3.started += OnWeaponAttack3;
        playerInputActions.Player.RunningAttack4.started += OnRunAttack;

        // player charged attack
        playerInputActions.Player.ChargedAttack6.started += OnChargedAttack;
        playerInputActions.Player.ChargedAttack6.canceled += OnChargedAttack;

        // player shuriken attack
        playerInputActions.Player.ShurikenAttack.started += OnShurikenAttack;

        // player projectile attack
        playerInputActions.Player.ProjectileAttack.started += OnProjectileAttack;

        // player grappling gun
        playerInputActions.Player.GrapplingGun.started += OnGrapplingGun;

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
            playerDodge.isExecuting ||
            playerColumn.isExecuting ||
            playerCombatSystemScript.lightAttackExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.JumpRoll.Disable();
        }
        else
        {
            playerInputActions.Player.JumpRoll.Enable();
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
            playerInputActions.Player.JumpRoll.Disable();
        }
        else
        {
            playerInputActions.Player.JumpRoll.Enable();
        }

        // dash
        if (playerCombatSystemScript.lightAttackExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerRoll.isExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.Dodge.Disable();
        }
        else
        {
            playerInputActions.Player.Dodge.Enable();
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
        if (playerRoll.isExecuting ||
            playerDodge.isExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerGrapplingGun.isExecuting ||
            playerCombatSystemScript.isBlockingCached)
        {
            playerInputActions.Player.WeaponMultiTapAttack1Attack2.Disable();
        }
        else
        {
            playerInputActions.Player.WeaponMultiTapAttack1Attack2.Enable();
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
            playerInputActions.Player.ShurikenAttack.Disable();
        }
        else
        {
            playerInputActions.Player.ShurikenAttack.Enable();
        }

        // projectile attack
        if (playerRoll.isExecuting ||
            playerDodge.isExecuting ||
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
            playerDodge.isExecuting ||
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

    private void OnProjectileAttack(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnProjectileAttack(context);
    }

    private void OnShurikenAttack(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnShurikenAttack(context);
    }

    private void OnHolsterWeapon(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnWeaponHolsterPrompted(context);
    }

    private void OnWeaponAttack1(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnWeaponAttack1(context);
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
        playerInputActions.Player.Run.Disable();
        playerInputActions.Player.Run.Enable();

        playerMovement.OnWalk(context);
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        //if (context.phase == InputActionPhase.Canceled) Debug.Log("contekst is " + context.phase);

        playerMovement.OnMovement(context);
    }

}
