using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    public bool resetSpawnSystemOnPlayerLeave;
    public bool isSpawnPointActive = false;
    public GameObject[] enemyObjectSpawnList = new GameObject[3];
    public Transform spawnPoint;
    int currentSpawnInd = 0;
    public float spawnIntervalTime;
    float lastEnemySpawnTime;
    [Tooltip("Number of enemies to spawn at a time")]
    public int enemySpawnCount;
    public int maxEnemyCount;
    int enemyCount = 0;
    // Start is called before the first frame update
    void OnEnable()
    {
        currentSpawnInd = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSpawnPointActive)
        {
            if (Time.time - lastEnemySpawnTime > spawnIntervalTime && enemyCount < maxEnemyCount)
            {
                for (int i = 0; i < enemySpawnCount; i++)
                {
                    spawnEnemy();
                }
            }
        }
    }

    public void spawnEnemy()
    {
        if (currentSpawnInd == enemyObjectSpawnList.Length) return;

        enemyObjectSpawnList[currentSpawnInd].transform.position = spawnPoint.position;
        enemyObjectSpawnList[currentSpawnInd].SetActive(true);
        lastEnemySpawnTime = Time.time;
        currentSpawnInd++;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isSpawnPointActive = true;
        }
        else if (other.CompareTag("Enemy"))
        {
            enemyCount++;
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isSpawnPointActive = false;
        }
        else if (other.CompareTag("Enemy"))
        {
            enemyCount--;
        }
    }

    public void resetSpawnPoint()
    {
        for (int i = 0; i < enemyObjectSpawnList.Length; i++)
        {
            enemyObjectSpawnList[i].SetActive(false);
            enemyObjectSpawnList[i].transform.position = spawnPoint.position;
        }
    }
}
