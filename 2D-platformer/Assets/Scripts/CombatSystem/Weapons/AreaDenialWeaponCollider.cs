using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaDenialWeaponCollider : MonoBehaviour
{
    public string specialWeaponName;
    public bool isPlayerWeapon;
    public bool targetInRange;
    public float finalEnlargedAreaScale, scaleUpSpeed;
    public float weaponTotalActiveTime;

    [Tooltip("How frequently damage is applied in seconds")] public float damageTickFrequency;
    float weaponActivationTime, lastDamageTickAppliedTime;
    Vector3 tempAreaScale;
    EnemyBase enemyBaseScript;
    PlayerCombatSystem playerCombatScript;
    WeaponClassInfo areaDenialWeaponInfo;

    void Awake()
    {
        lastDamageTickAppliedTime = Time.time;
        //areaDenialWeaponInfo = WeaponsManager.Instance.getAreaDenialWeaponsData(specialWeaponName);
    }

    void OnEnable()
    {
        weaponActivationTime = Time.time;
        areaDenialWeaponInfo = WeaponsManager.Instance.getAreaDenialWeaponsData(specialWeaponName);
    }

    void OnDisable()
    {
        targetInRange = false;
    }

    void Update()
    {
        if (Time.time - weaponActivationTime > weaponTotalActiveTime)
        {
            gameObject.SetActive(false);
        }

        tempAreaScale = transform.localScale;
        if (tempAreaScale.x < finalEnlargedAreaScale)
        {
            tempAreaScale += scaleUpSpeed * Vector3.one * Time.deltaTime;
            transform.localScale = tempAreaScale;
        }

        if (targetInRange && Time.time - lastDamageTickAppliedTime > damageTickFrequency)
        {
            lastDamageTickAppliedTime = Time.time;

            if (isPlayerWeapon)
            {
                enemyBaseScript.TakeProjectileDamage(areaDenialWeaponInfo.damageToEnemy, areaDenialWeaponInfo.staminaDamageToEnemy, false);
            }
            else
            {
                playerCombatScript.TakeProjectileDamage(areaDenialWeaponInfo.damageToPlayer);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && isPlayerWeapon)
        {
            targetInRange = true;
            enemyBaseScript = other.GetComponent<EnemyBase>();
        }
        else if (other.CompareTag("Player") && !isPlayerWeapon)
        {
            targetInRange = true;
            playerCombatScript = other.GetComponent<PlayerCombatSystem>();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && isPlayerWeapon)
        {
            targetInRange = false;
            enemyBaseScript = null;
        }
        else if (other.CompareTag("Player") && !isPlayerWeapon)
        {
            targetInRange = false;
            playerCombatScript = null;
        }
    }
}
