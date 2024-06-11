using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentDamageObject : MonoBehaviour
{
    public float damageAmount;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("other g : " + other.gameObject);
        if (other.CompareTag("Objects"))
        {
            other.GetComponent<BreakableObjects>().TakeDamage(damageAmount);
        }
    }
}
