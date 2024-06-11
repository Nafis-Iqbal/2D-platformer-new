using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPlantController : BreakableObjects
{
    public int enemyID;
    public Animator monsterPlantAnimator;
    public bool isDead;
    public float health;
    public SectionUnlockScript vinedWallUnlockScript;
    public GameObject vineToDeactivate;

    // Start is called before the first frame update
    void Start()
    {

    }

    void OnEnable()
    {
        isDead = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void TakeDamage(float objectDamage)
    {
        if (isDead) return;

        health -= objectDamage;

        hitEffectScript.PlayOnHitEffect();
        monsterPlantAnimator.SetTrigger("LHurt");

        if (health <= 0.0f)
        {
            isDead = true;
            monsterPlantAnimator.SetTrigger("Death");
            vinedWallUnlockScript.updateObjectiveCountStates("DestroyPlant", vineToDeactivate);
        }
    }
}
