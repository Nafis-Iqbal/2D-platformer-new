using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoostController : MonoBehaviour
{
    public Collider2D boostTriggerCollider, boostNormalCollider;
    public float speedBoostMultiplier;
    public SpriteRenderer rampRenderer;

    void OnEnable()
    {
        rampRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Vehicle"))
        {
            if (other.GetComponent<TransportCartController>().cartDeccelerating) return;
            other.GetComponent<TransportCartController>().BoostSpeedByRamp(speedBoostMultiplier);
        }
    }
}
