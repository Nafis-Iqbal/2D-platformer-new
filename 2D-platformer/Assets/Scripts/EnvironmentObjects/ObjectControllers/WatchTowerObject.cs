using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchTowerObject : BreakableObjects
{
    public GameObject initialTowerSpriteObject, brokenBaseSpriteObject, brokenTopSpriteObject;

    public EnemyBase rangedEnemyScript;
    public float combatRangeMultiplier;
    public float attackIntervalTime;
    public Collider2D[] collidersToDisableOnDestroy = new Collider2D[2];

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        brokenBaseSpriteObject.SetActive(false);
        //brokenTopSpriteObject.SetActive(false);
    }

    public void OnEnable()
    {
        rangedEnemyScript.enemyAttackRange *= combatRangeMultiplier;
        rangedEnemyScript.minTimeBetweenAttacks = attackIntervalTime;
        rangedEnemyScript.turnToPlayerTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void updateObjectAppearanceOnDamage()
    {
        base.updateObjectAppearanceOnDamage();
        Debug.Log("What? Whay");
        if (currentStateInd == 1)
        {
            Debug.Log("What? Whay 1");
            initialTowerSpriteObject.SetActive(false);
            brokenBaseSpriteObject.SetActive(true);
            brokenTopSpriteObject.SetActive(true);
        }
        else if (currentStateInd == 2)
        {
            Debug.Log("What? Whay 2");
            for (int i = 0; i < collidersToDisableOnDestroy.Length; i++)
            {
                collidersToDisableOnDestroy[i].enabled = false;
            }

            rangedEnemyScript.isDead = true;
            brokenTopSpriteObject.GetComponent<SpriteBreaker>().Break();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Damn");
        if (other.CompareTag("Player"))
        {
            rangedEnemyScript.playerInsideStationaryCombatRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            rangedEnemyScript.playerInsideStationaryCombatRange = false;
        }
    }
}
