using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponCollider : MonoBehaviour
{
    public string weaponName;
    float weaponDamage;
    int enemyID;
    public bool isEnemyWeapon;
    float lastObjectHitTime, hitCoolDownTime = 3.0f;
    public EnemyBase enemyBaseScript;
    private PlayerCombatSystem playerCombatSystemScript;
    private WeaponClassInfo meleeWeaponData = null;
    private EnemyClassInfo enemyClassData = null;

    private Color debugColliderColor = Color.blue;

    private void Awake()
    {
        playerCombatSystemScript = GameManager.Instance.playerTransform.GetComponent<PlayerCombatSystem>();
        lastObjectHitTime = Time.time;

        if (isEnemyWeapon)
        {
            enemyID = enemyBaseScript.enemyID;
            enemyClassData = EnemyManager.Instance.enemyData[enemyID];
        }
    }

    void OnEnable()
    {
        if (meleeWeaponData == null)
        {
            meleeWeaponData = WeaponsManager.Instance.getMeleeWeaponsData(weaponName);
            return;
        }
    }

    void OnDisable()
    {

    }

    private void OnDrawGizmos()
    {
        var collider = GetComponent<Collider2D>();
        Gizmos.color = debugColliderColor;
        Gizmos.DrawCube(collider.bounds.center, collider.bounds.size);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled) return;

        if (other.transform.CompareTag("Enemy") && !isEnemyWeapon)
        {
            lastObjectHitTime = Time.time;
            debugColliderColor = Color.red;
            int currentAttackID = playerCombatSystemScript.currentPlayerAttackID;

            bool attackFromRight;
            if (other.transform.position.x > transform.root.transform.position.x) attackFromRight = false;
            else attackFromRight = true;

            enemyBaseScript = other.transform.GetComponent<EnemyBase>();
            enemyBaseScript.TakeDamage(PlayerManager.Instance.swordShieldAttacks[currentAttackID], attackFromRight);

            var force = Vector2.right * playerCombatSystemScript.transform.localScale * meleeWeaponData.enemyCollisionForce;
            other.transform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }
        else if (other.transform.CompareTag("Player") && isEnemyWeapon && other.isTrigger == false)
        {
            if (playerCombatSystemScript.isKnockedOffGround == true) return;
            lastObjectHitTime = Time.time;
            debugColliderColor = Color.blue;
            int currentAttackID = enemyBaseScript.currentAttackID;

            bool attackFromRight;
            if (other.transform.position.x > transform.root.transform.position.x) attackFromRight = false;
            else attackFromRight = true;

            playerCombatSystemScript.TakeDamage(enemyClassData.enemyAttacks[currentAttackID], attackFromRight);

            var force = Vector2.right * enemyBaseScript.transform.localScale * meleeWeaponData.playerCollisionForce;
            other.transform.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }
        else if (other.transform.CompareTag("Objects"))
        {
            lastObjectHitTime = Time.time;
            other.transform.GetComponent<BreakableObjects>().TakeDamage(30.0f);
        }
    }
}
