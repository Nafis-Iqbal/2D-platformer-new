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
    //public LevelTransitionScript levelTransitionObject;

    public bool lookDataRequired = false;
    public Vector2 mousePosition, joystickPosition;
    [SerializeField]
    public int connectedGamepadCount;

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

        // player look
        playerInputActions.Player.LookMouse.performed += OnLook;
        playerInputActions.Player.LookMouse.canceled += OnLook;
        playerInputActions.Player.LookMouse.Disable();

        playerInputActions.Player.LookGamepad.performed += OnLook;
        playerInputActions.Player.LookGamepad.canceled += OnLook;
        playerInputActions.Player.LookGamepad.Disable();

        // player look down
        playerInputActions.Player.LookDown.performed += OnLookDown;
        playerInputActions.Player.LookDown.canceled += OnLookDown;

        // player move
        playerInputActions.Player.Move.started += OnMove;
        playerInputActions.Player.Move.performed += OnMove;
        playerInputActions.Player.Move.canceled += OnMove;

        //player move
        playerInputActions.Player.Run.started += OnRun;
        playerInputActions.Player.Run.performed += OnRun;
        playerInputActions.Player.Run.canceled += OnRun;

        //Jump
        //playerInputActions.Player.Jump.started += OnJump;
        playerInputActions.Player.Jump.performed += OnJump;
        //playerInputActions.Player.Jump.canceled += OnJump;

        // player slide jump
        playerInputActions.Player.SlideJump.performed += OnSlideJump;

        // player dodge and roll
        playerInputActions.Player.RollDodge.canceled += OnDodge;
        playerInputActions.Player.RollDodge.performed += OnRoll;
        playerInputActions.Player.RollDodge.Enable();

        // player column move
        playerInputActions.Player.ColumnMove.started += OnColumnMove;
        playerInputActions.Player.ColumnMove.performed += OnColumnMove;
        playerInputActions.Player.ColumnMove.canceled += OnColumnMove;

        // player column jump direction set
        playerInputActions.Player.ColumnJumpDirection.started += OnColumnJumpDirection;
        playerInputActions.Player.ColumnJumpDirection.canceled += OnColumnJumpDirection;

        // player column jump
        playerInputActions.Player.ColumnJumpClimb.performed += OnColumnJumpClimb;

        // player grappling gun
        playerInputActions.Player.GrapplingGun.started += OnGrapplingGun;
        playerInputActions.Player.GrapplingGun.Disable();

        playerInputActions.Player.GrappleBoost.started += OnGrappleBoost;
        playerInputActions.Player.GrappleBoost.Disable();

        // player sword attack
        playerInputActions.Player.HolsterWeapon.started += OnHolsterWeapon;

        playerInputActions.Player.WeaponLightAttack.performed += OnWeaponLightAttack;
        playerInputActions.Player.WeaponM3Attack3.started += OnWeaponAttack3;
        playerInputActions.Player.RunningAttack4.started += OnRunAttack;

        // player charged attack
        playerInputActions.Player.ChargedAttack6.started += OnChargedAttack;
        playerInputActions.Player.ChargedAttack6.performed += OnChargedAttack;
        playerInputActions.Player.ChargedAttack6.canceled += OnChargedAttack;

        // player blocking defense
        playerInputActions.Player.Blocking.started += OnBlockingDefense;
        playerInputActions.Player.Blocking.canceled += OnBlockingDefense;

        // player use combat item
        playerInputActions.Player.UseCombatItem.started += OnCombatItemUse;

        // player use item
        playerInputActions.Player.UseItem.started += OnItemUse;

        // player interact
        playerInputActions.Player.Interact.performed += OnInteract;
        playerInputActions.Player.Interact.Enable();
    }

    private void Update()
    {
        connectedGamepadCount = Gamepad.all.Count;
        // handle input interference
        if (lookDataRequired)
        {
            if (connectedGamepadCount > 0) playerInputActions.Player.LookGamepad.Enable();
            else playerInputActions.Player.LookMouse.Enable();
        }
        else
        {
            playerInputActions.Player.LookMouse.Disable();
            playerInputActions.Player.LookGamepad.Disable();
        }

        // run
        if (playerJump.isCharging ||
            (playerCombatSystemScript.isBlockingCached && playerJump.onGround) ||
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
            playerMovement.isSliding ||
            playerCombatSystemScript.lightAttackExecuting ||
            playerCombatSystemScript.heavyAttackExecuting ||
            playerCombatSystemScript.isKnockedOffGround ||
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

        // roll dodge
        if (!playerJump.onGround ||
            playerJump.isCharging ||
            playerDodge.isExecuting ||
            playerRoll.isExecuting ||
            playerCombatSystemScript.isKnockedOffGround ||
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
            //playerInputActions.Player.WeaponAttack1MultiTapAttack2RollAttack.Disable();
            playerInputActions.Player.WeaponLightAttack.Disable();
        }
        else
        {
            //playerInputActions.Player.WeaponAttack1MultiTapAttack2RollAttack.Enable();
            playerInputActions.Player.WeaponLightAttack.Enable();
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

    // private void OnWeaponAttack1RollAttack(InputAction.CallbackContext context)
    // {
    //     playerCombatSystemScript.OnWeaponAttack1RollAttack(context);
    // }

    private void OnWeaponLightAttack(InputAction.CallbackContext context)
    {
        playerCombatSystemScript.OnWeaponLightAttack(context);
    }

    // private void OnWeaponAttack2(InputAction.CallbackContext context)
    // {
    //     playerCombatSystemScript.OnWeaponAttack2(context);
    // }

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

    private void OnSlideJump(InputAction.CallbackContext context)
    {
        playerMovement.OnSlideJump(context);
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

    public void OnLookDown(InputAction.CallbackContext context)
    {
        CameraController.Instance.OnLookDown(context);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        mousePosition = context.ReadValue<Vector2>();
        if (connectedGamepadCount == 0)
        {
            mousePosition.x = mousePosition.x - Screen.width / 2;
            mousePosition.y = mousePosition.y - Screen.height / 2;
        }
        //Debug.Log("mP: " + mousePosition);
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        playerMovement.OnInteract();
    }

    // public void OnInteract(InputAction.CallbackContext context)
    // {
    //     LevelTransitionScript.Instance.OnInteract();
    // }
}
