using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.U2D;

public class ProjectileWeapon : MonoBehaviour
{
    GameObject projectileOwnerObject;
    public string projectileName;
    protected WeaponClassInfo projectileInfo = null;
    public bool isProjectileActive, projectileHit;
    protected Rigidbody2D rb2d;
    protected PlayerCombatSystem playerCombatSystemScript;
    public bool isPlayerProjectile;
    public bool isUnblockable;
    protected float spawnTime, projectileHitTime, currentTime;
    protected Vector2 projectileDirection;
    public float timeToDisableAfterHit = 2.5f;
    public float timeToDisable = 4.5f;
    protected Vector3 initialScale;

    // Start is called before the first frame update
    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerCombatSystemScript = GameManager.Instance.playerTransform.GetComponent<PlayerCombatSystem>();
        initialScale = transform.localScale;
    }

    protected virtual void OnEnable()
    {
        if (isPlayerProjectile)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), GameManager.Instance.playerTransform.GetComponent<Collider2D>());
        }
        // else
        // {
        //     Collider2D[] collidersArray = projectileOwnerObject.GetComponentsInChildren<Collider2D>();
        //     for (int i = 0; i < collidersArray.Length; i++)
        //     {
        //         Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collidersArray[i]);
        //     }
        // }

        if (projectileInfo == null)
        {
            projectileInfo = WeaponsManager.Instance.getProjectileWeaponData(projectileName);
            timeToDisable = projectileInfo.timeToDisable;
        }

        spawnTime = Time.time;
        projectileHitTime = Time.time;
        isProjectileActive = true;

        transform.localScale = initialScale;
    }

    void OnDisable()
    {
        isProjectileActive = false;
        rb2d.simulated = true;
        transform.localScale = initialScale;
        transform.parent = null;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (projectileInfo == null)
        {
            projectileInfo = WeaponsManager.Instance.getProjectileWeaponData(projectileName);
            timeToDisable = projectileInfo.timeToDisable;
            return;
        }

        currentTime = Time.time;
        if (!projectileHit && currentTime - spawnTime > timeToDisable)
        {
            Debug.Log("Arrow disabled1: " + projectileName + " " + (currentTime - spawnTime) + " " + timeToDisable);
            gameObject.SetActive(false);
            return;
        }
        else if (projectileHit == true && currentTime - projectileHitTime > timeToDisableAfterHit)
        {
            Debug.Log("Arrow disabled2: " + (currentTime - projectileHitTime) + " " + timeToDisable);
            gameObject.SetActive(false);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPlayerProjectile && collision.transform.CompareTag("Enemy"))
        {
            return;
        }

        if (isProjectileActive && collision.transform.CompareTag("Platforms") || collision.transform.CompareTag("Shield") || collision.transform.CompareTag("Player")
        || collision.transform.CompareTag("Objects"))
        {
            Debug.Log("Arrow hit");
            projectileHit = true;
            projectileHitTime = Time.time;
            rb2d.simulated = false;
            isProjectileActive = false;
            transform.parent = collision.transform;

            //Do Damage to player
            if (collision.transform.CompareTag("Shield"))
            {
                if (collision.transform.root.transform.CompareTag("Player"))
                {
                    playerCombatSystemScript.TakeProjectileShieldDamage(projectileInfo.damageToPlayer, projectileInfo.staminaDamageToPlayer);
                }
                else
                {
                    collision.transform.GetComponent<EnemyBase>().TakeProjectileDamage(projectileInfo.damageToEnemy, projectileInfo.staminaDamageToEnemy);
                }
            }
            else if (collision.transform.CompareTag("Player"))
            {
                playerCombatSystemScript.TakeProjectileDamage(projectileInfo.damageToPlayer);
            }
            else if (collision.transform.CompareTag("Enemy"))
            {
                collision.transform.GetComponent<EnemyBase>().TakeProjectileDamage(projectileInfo.damageToEnemy, projectileInfo.staminaDamageToEnemy);
                collision.transform.GetComponent<Rigidbody2D>().AddForce(projectileDirection * projectileInfo.projectileHitForce, ForceMode2D.Impulse);
            }
        }
    }

    //PROJECTILE WITH ARC TRAJECTORY
    public virtual void initializeProjectile(Vector3 startPosition, Vector3 aimPosition, float projectileMaxHeight, float projectileFlightTime, GameObject projectileOwner, bool shooterFacingRight = true, bool playerProjectile = false)
    {
        rb2d.simulated = true;
        transform.position = startPosition;
        isPlayerProjectile = playerProjectile;
        projectileOwnerObject = projectileOwner;

        projectileHit = false;
    }

    //PROJECTILE WITH LINEAR TRAJECTORY
    public void initializeProjectile(Vector3 startPosition, Vector2 direction, bool playerProjectile = false)
    {
        rb2d.simulated = true;
        transform.position = startPosition;
        isPlayerProjectile = playerProjectile;

        projectileHit = false;

        projectileDirection = direction;
    }
}
