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
    private CharacterMovement characterMovement;
    private CharacterDash characterDash;
    private CharacterRoll characterRoll;
    private CharacterColumn characterColumn;
    private CharacterJump characterJump;


    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        var playerTransform = GameManager.Instance.playerTransform;
        characterMovement = playerTransform.GetComponent<CharacterMovement>();
        characterDash = playerTransform.GetComponent<CharacterDash>();
        characterRoll = playerTransform.GetComponent<CharacterRoll>();
        characterColumn = playerTransform.GetComponent<CharacterColumn>();
        characterJump = playerTransform.GetComponent<CharacterJump>();

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
    }

    private void OnJump(InputAction.CallbackContext context) {
        characterJump.OnJump(context);
    }

    private void OnColumnMove(InputAction.CallbackContext context) {
        characterColumn.OnColumnMove(context);
    }

    private void OnColumnJumpDirection(InputAction.CallbackContext context) {
        characterColumn.OnColumnJumpDirection(context);
    }

    private void OnColumnJump(InputAction.CallbackContext context) {
        characterColumn.OnColumnJump(context);
    }

    private void OnColumnGrab(InputAction.CallbackContext context) {
        characterColumn.OnColumnGrab(context);
    }

    private void OnDash(InputAction.CallbackContext context) {
        characterDash.OnDash(context);
    }

    private void OnRoll(InputAction.CallbackContext context) {
        characterRoll.OnRoll(context);
    }

    private void OnWalk(InputAction.CallbackContext context) {
        playerInputActions.Player.Run.Disable();
        playerInputActions.Player.Run.Enable();

        characterMovement.OnWalk(context);
    }

    public void OnRun(InputAction.CallbackContext context) {
        characterMovement.OnMovement(context);
    }

}
