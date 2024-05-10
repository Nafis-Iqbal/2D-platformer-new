using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPlantController : BreakableObjects
{
    public int enemyID;
    public Animator monsterPlantAnimator;
    public Animator[] plantTrapAnimators = new Animator[6];
    public bool isDead;
    public float health;

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
        if (isDead)
        {
            for (int i = 0; i < plantTrapAnimators.Length; i++)
            {
                plantTrapAnimators[i].SetTrigger("Death");
            }
        }
    }

    public override void TakeDamage(float objectDamage)
    {
        Debug.Log("GGBoiz");
        health -= objectDamage;

        hitEffectScript.PlayOnHitEffect();
        monsterPlantAnimator.SetTrigger("LHurt");

        //if (healthStaminaScript.modifyHealth(-objectDamage) > 0.0f)
        if (health <= 0.0f)
        {
            isDead = true;
            monsterPlantAnimator.SetTrigger("Death");
        }
    }
}
