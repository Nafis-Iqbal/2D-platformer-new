using System;
using System.Collections;
using System.Collections.Generic;
using GMTK.PlatformerToolkit;
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
        playerInputActions.Player.Dash.performed += OnDash;

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
    }

    private void OnShurikenAttack(InputAction.CallbackContext context)
    {
        playerShurikenAttack.OnShurikenAttack(context);
    }

    private void OnSwordAttack(InputAction.CallbackContext context)
    {
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
        playerDash.OnDash(context);
    }

    private void OnRoll(InputAction.CallbackContext context) {
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
