using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObjects : MonoBehaviour
{
    public float totalHealth;
    public float currentHealth;
    public bool hasDamageStates;
    public int currentStateInd = 0;
    public float[] damageStateMinimumHealthValues = new float[3];
    public HitEffectScript hitEffectScript;
    // Start is called before the first frame update
    public virtual void Start()
    {
        hitEffectScript = GetComponent<HitEffectScript>();
        currentHealth = totalHealth;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void TakeDamage(float objectDamage)
    {
        currentHealth -= objectDamage;
        hitEffectScript.PlayOnHitEffect();

        if (currentHealth <= damageStateMinimumHealthValues[currentStateInd])
        {
            currentStateInd++;

            //update face of props or object
            updateObjectAppearanceOnDamage();
        }
    }

    public virtual void updateObjectAppearanceOnDamage()
    {
        Debug.Log("why the hell");
    }
}
