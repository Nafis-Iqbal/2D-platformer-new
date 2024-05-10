using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaDenialWeaponCollider : MonoBehaviour
{
    public bool isPlayerWeapon;
    public bool targetInRange;
    [Tooltip("How frequently damage is applied in seconds")] public float damageTickFrequency;
    [Tooltip("Damage applied per tick")] public float damageTickValue;

    HealthStaminaSystem targetHealthStaminaSystem;

    void Awake()
    {

    }

    void OnDisable()
    {

    }
    void Update()
    {

    }
}
