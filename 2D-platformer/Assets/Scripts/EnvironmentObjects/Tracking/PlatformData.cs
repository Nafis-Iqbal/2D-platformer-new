using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformData : MonoBehaviour
{
    public int platformID, platformLevel;
    public Color efffectColor;
    public Transform leftEndPosition;
    public Transform rightEndPosition;

    private void Start()
    {

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.playerCurrentPlatformID = platformID;
            GameManager.Instance.playerCurrentPlatformLevel = platformLevel;
        }

        if (other.CompareTag("Enemy"))
        {
            other.gameObject.transform.GetComponent<EnemyBase>().setEnemyPlatform(platformID, platformLevel);
        }
    }
}
