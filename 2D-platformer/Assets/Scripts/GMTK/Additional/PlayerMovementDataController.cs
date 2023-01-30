using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerJump))]
public class PlayerMovementDataController : MonoBehaviour {
    [SerializeField] PresetObject _preset;

    PlayerMovement _moveScript;
    PlayerJump _jumpScript;

    PresetObject _installedPreset;

    void Awake() {
        _moveScript = GetComponent<PlayerMovement>();
        _jumpScript = GetComponent<PlayerJump>();

        InstallPresetData();
    }

    private void FixedUpdate() {

        if (_installedPreset != _preset) {
            InstallPresetData();
        }

    }
    private void InstallPresetData() {

        //MOVE
        _moveScript.maxAcceleration = _preset.Acceleration;
        _moveScript.maxSpeed = _preset.TopSpeed;
        _moveScript.maxDeceleration = _preset.Deceleration;
        _moveScript.maxTurnSpeed = _preset.TurnSpeed;


        //JUMP
        _moveScript.maxAirAcceleration = _preset.AirControl;
        _moveScript.maxAirDeceleration = _preset.AirBrake;
        _jumpScript.jumpHeight = _preset.JumpHeight;
        _jumpScript.timeToJumpApex = _preset.TimeToApex;
        _jumpScript.downwardMovementMultiplier = _preset.DownwardMovementMultiplier;
        _jumpScript.jumpCutOff = _preset.JumpCutoff;
        _jumpScript.maxAirJumps = _preset.DoubleJump;
        _jumpScript.variablejumpHeight = _preset.VariableJumpHeight;
        _moveScript.maxAirTurnSpeed = _preset.AirControlActual;

        _installedPreset = _preset;
    }





}
