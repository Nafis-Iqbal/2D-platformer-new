using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour {
    // Instance for Singleton.
    public static PlayerInputManager Instance;
    public PlayerInputActions playerInputActions;
    private PlayerMovement playerMovement;
    private PlayerDash playerDash;
    private PlayerRoll playerRoll;
    private PlayerColumn playerColumn;
    private PlayerJump playerJump;
    private PlayerSwordAttack playerSwordAttack;
    private PlayerShurikenAttack playerShurikenAttack;
    private PlayerProjectileAttack playerProjectileAttack;
    private PlayerGrapplingGun playerGrapplingGun;
    private PlayerBlockDefense playerBlockDefense;
    private PlayerHeavyAttack playerHeavyAttack;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        var playerTransform = GameManager.Instance.playerTransform;
        playerMovement = playerTransform.GetComponent<PlayerMovement>();
        playerDash = playerTransform.GetComponent<PlayerDash>();
        playerRoll = playerTransform.GetComponent<PlayerRoll>();
        playerColumn = playerTransform.GetComponent<PlayerColumn>();
        playerJump = playerTransform.GetComponent<PlayerJump>();
        playerSwordAttack = playerTransform.GetComponent<PlayerSwordAttack>();
        playerShurikenAttack = playerTransform.GetComponent<PlayerShurikenAttack>();
        playerProjectileAttack = playerTransform.GetComponent<PlayerProjectileAttack>();
        playerGrapplingGun = playerTransform.GetComponent<PlayerGrapplingGun>();
        playerBlockDefense = playerTransform.GetComponent<PlayerBlockDefense>();
        playerHeavyAttack = playerTransform.GetComponent<PlayerHeavyAttack>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        // player run
        playerInputActions.Player.Run.started += OnRun;
        playerInputActions.Player.Run.performed += OnRun;
        playerInputActions.Player.Run.canceled += OnRun;

        // player walk
        playerInputActions.Player.Walk.started += OnWalk;
        playerInputActions.Player.Walk.canceled += OnWalk;

        // player jump
        playerInputActions.Player.Jump.started += OnJump;
        playerInputActions.Player.Jump.performed += OnJump;
        playerInputActions.Player.Jump.canceled += OnJump;

        // player roll
        playerInputActions.Player.Roll.performed += OnRoll;

        // player dash
        // playerInputActions.Player.Dash.started += OnDash;
        playerInputActions.Player.Dash.performed += OnDash;
        // playerInputActions.Player.Dash.canceled += OnDash;

        // player column ledge grab
        playerInputActions.Player.ColumnLedgeGrab.performed += OnColumnGrab;

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

    private void Update() {
        // handle input interference

        if (playerBlockDefense.isExecuting ||
            playerHeavyAttack.isExecuting ||
            playerColumn.isExecuting) {
            playerInputActions.Player.Run.Disable();
            playerInputActions.Player.Walk.Disable();
            playerInputActions.Player.Jump.Disable();
        } else {
            playerInputActions.Player.Run.Enable();
            playerInputActions.Player.Walk.Enable();
            playerInputActions.Player.Jump.Enable();
        }

        if (playerColumn.isExecuting) {
            playerInputActions.Player.ColumnJump.Enable();
            playerInputActions.Player.ColumnMove.Enable();
            playerInputActions.Player.ColumnJumpDirection.Enable();
        } else {
            playerInputActions.Player.ColumnJump.Disable();
            playerInputActions.Player.ColumnMove.Disable();
            playerInputActions.Player.ColumnJumpDirection.Disable();
        }
    }

    private void OnHeavyAttack(InputAction.CallbackContext context) {
        playerHeavyAttack.OnHeavyAttack(context);
    }

    private void OnBlockingDefense(InputAction.CallbackContext context) {
        playerBlockDefense.OnBlockDefense(context);
    }

    private void OnGrapplingGun(InputAction.CallbackContext context) {
        playerGrapplingGun.OnGrapplingGun(context);
    }

    private void OnProjectileAttack(InputAction.CallbackContext context) {
        playerProjectileAttack.OnProjectileAttack(context);
    }

    private void OnShurikenAttack(InputAction.CallbackContext context) {
        playerShurikenAttack.OnShurikenAttack(context);
    }

    private void OnSwordAttack(InputAction.CallbackContext context) {
        playerSwordAttack.OnSwordAttack(context);
    }

    private void OnJump(InputAction.CallbackContext context) {
        playerJump.OnJump(context);
    }

    private void OnColumnMove(InputAction.CallbackContext context) {
        playerColumn.OnColumnMove(context);
    }

    private void OnColumnJumpDirection(InputAction.CallbackContext context) {
        playerColumn.OnColumnJumpDirection(context);
    }

    private void OnColumnJump(InputAction.CallbackContext context) {
        playerColumn.OnColumnJump(context);
    }

    private void OnColumnGrab(InputAction.CallbackContext context) {
        playerColumn.OnColumnGrab(context);
    }

    private void OnDash(InputAction.CallbackContext context) {
        Debug.Log($"d context: {context.phase}");
        // playerDash.OnDash(context);
    }

    private void OnRoll(InputAction.CallbackContext context) {
        Debug.Log($"r context: {context.phase}");
        playerRoll.OnRoll(context);
    }

    private void OnWalk(InputAction.CallbackContext context) {
        playerInputActions.Player.Run.Disable();
        playerInputActions.Player.Run.Enable();

        playerMovement.OnWalk(context);
    }

    public void OnRun(InputAction.CallbackContext context) {
        playerMovement.OnMovement(context);
    }

}
