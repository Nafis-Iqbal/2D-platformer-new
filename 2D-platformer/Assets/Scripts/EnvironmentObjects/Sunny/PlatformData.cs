using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformData : MonoBehaviour
{
    public Color efffectColor;
    public Transform startPos;
    public Transform endPos;
    public healthofPlayer playerScript;

    private void Start()
    {
        playerScript = EnemyManager.Instance.player.GetComponent<healthofPlayer>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerScript.setPlatInfo(startPos.position, endPos.position, efffectColor);
        }
        if (other.CompareTag("Enemy"))
        {
            other.gameObject.transform.GetComponent<EnemyBase>().setEnemyPlatform(startPos.position, endPos.position);
        }
    }
}
