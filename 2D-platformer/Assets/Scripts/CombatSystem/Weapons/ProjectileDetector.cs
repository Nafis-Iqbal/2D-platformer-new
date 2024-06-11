using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDetector : MonoBehaviour
{
    public bool isPlayer, isEnemy, projectileInRange;
    public int inRangeProjectileCount;
    ProjectileWeapon projectileWeaponScript;
    float projectileExitTime;
    // Start is called before the first frame update
    void OnEnable()
    {
        projectileExitTime = Time.time;
        projectileInRange = false;
        inRangeProjectileCount = 0;
    }

    void Update()
    {
        if (inRangeProjectileCount <= 0 && Time.time - projectileExitTime > 1.5f)
        {
            projectileInRange = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Cocks");
        if (isEnemy)
        {
            if (other.transform.CompareTag("Projectile") == true)
            {
                //Debug.Log("Balls");
                projectileWeaponScript = other.transform.GetComponent<ProjectileWeapon>();

                if (projectileWeaponScript.isPlayerProjectile == true)
                {
                    projectileInRange = true;
                    inRangeProjectileCount++;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isEnemy)
        {
            if (other.transform.CompareTag("Projectile") == true)
            {
                projectileWeaponScript = other.transform.GetComponent<ProjectileWeapon>();

                if (projectileWeaponScript.isPlayerProjectile == true)
                {
                    inRangeProjectileCount--;
                    if (inRangeProjectileCount <= 0)
                    {
                        projectileExitTime = Time.time;
                    }
                }
            }
        }
    }
}
