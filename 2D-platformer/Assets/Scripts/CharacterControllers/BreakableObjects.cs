using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObjects : MonoBehaviour
{
    protected HealthStaminaSystem healthStaminaScript;
    public HitEffectScript hitEffectScript;
    // Start is called before the first frame update
    void Start()
    {
        healthStaminaScript = GetComponent<HealthStaminaSystem>();
        hitEffectScript = GetComponent<HitEffectScript>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void TakeDamage(float objectDamage)
    {

    }
}
