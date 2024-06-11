using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponClassInfo
{
    public string weaponName;
    [Header("DAMAGE")]
    public float damageToPlayer;
    public float staminaDamageToPlayer;
    public float damageToEnemy;
    public float staminaDamageToEnemy;

    public float damageToObjects;
    [Header("ENVIRONMENT INTERACTIONS")]
    public float enemyCollisionForce;
    public float playerCollisionForce;
    public float objectCollisionForce;
    public float weaponDamageMultiplier;
    [Header("PROJECTILES")]
    public float rotationSpeed;
    public float horizontalSpeed;
    public float timeToDisable;
    public float projectileHitForce;
}
