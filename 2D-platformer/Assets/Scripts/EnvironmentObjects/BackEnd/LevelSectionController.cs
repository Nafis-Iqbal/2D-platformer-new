using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSectionController : MonoBehaviour
{
    public bool isSectionActive = false;
    public bool entryAtRight;
    public bool resetsEnemyStatesOnLeave;
    public bool spawnAllEnemiesInSection;
    public GameObject[] staticGameObjects = new GameObject[4];
    public GameObjectStruct[] enemyGameObjects = new GameObjectStruct[3];
    public GameObjectStruct[] NPCGameObjects = new GameObjectStruct[3];
    public EnemySpawnPoint[] enemySpawnPoints = new EnemySpawnPoint[3];
    public bool overrideDefaultCameraSettings;
    public float sectionOverrideCameraXValue, sectionOverrideCameraYValue;
    public bool hasSpeedBoostRamps;
    public SpeedBoostController[] speedBoostRampScripts = new SpeedBoostController[2];

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < staticGameObjects.Length; i++)
        {
            staticGameObjects[i].SetActive(false);
        }

        for (int i = 0; i < enemyGameObjects.Length; i++)
        {
            enemyGameObjects[i].structGameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isSectionActive == false) return;

        if (overrideDefaultCameraSettings)
        {
            if (CameraController.Instance.defaultCameraSettingsOverrideMode == false)
            {
                CameraController.Instance.defaultCameraSettingsOverrideMode = true;
                CameraController.Instance.overrideCameraYValue = sectionOverrideCameraYValue;
            }
        }
    }

    public void alterRampCollidersState(bool state)
    {
        for (int i = 0; i < speedBoostRampScripts.Length; i++)
        {
            speedBoostRampScripts[i].boostNormalCollider.enabled = state;
            speedBoostRampScripts[i].boostTriggerCollider.enabled = state;

            if (state == true)
            {
                speedBoostRampScripts[i].rampRenderer.sortingOrder = 3;
            }
            else
            {
                speedBoostRampScripts[i].rampRenderer.sortingOrder = 7;
            }
        }
    }

    public void ActivateAllObjects()
    {
        for (int i = 0; i < staticGameObjects.Length; i++)
        {
            staticGameObjects[i].SetActive(true);
        }

        for (int i = 0; i < enemyGameObjects.Length; i++)
        {
            if (spawnAllEnemiesInSection || enemyGameObjects[i].boolProperty) enemyGameObjects[i].structGameObject.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //Condition to confirm the player is entering or eksiting the specified zone
            //Zone Entry Condition
            if ((entryAtRight && other.transform.position.x > transform.position.x) || (!entryAtRight && other.transform.position.x < transform.position.x))
            {
                isSectionActive = true;
                InMapGameController.Instance.activeLevelSectionScript = this;

                for (int i = 0; i < staticGameObjects.Length; i++)
                {
                    staticGameObjects[i].SetActive(true);
                }

                for (int i = 0; i < enemyGameObjects.Length; i++)
                {
                    if (resetsEnemyStatesOnLeave)
                    {
                        enemyGameObjects[i].structGameObject.transform.GetComponent<EnemyBase>().AllStatesResetEnable = true;
                        enemyGameObjects[i].structGameObject.transform.position = enemyGameObjects[i].gameObjectSpawnPosition.position;
                    }

                    if (spawnAllEnemiesInSection || enemyGameObjects[i].boolProperty) enemyGameObjects[i].structGameObject.SetActive(true);
                }
            }
            //Zone Exit by exclusion
            else
            {
                isSectionActive = false;

                for (int i = 0; i < staticGameObjects.Length; i++)
                {
                    staticGameObjects[i].SetActive(false);
                }

                for (int i = 0; i < enemyGameObjects.Length; i++)
                {
                    enemyGameObjects[i].structGameObject.SetActive(false);
                }

                for (int i = 0; i < enemySpawnPoints.Length; i++)
                {
                    if (enemySpawnPoints[i].resetSpawnSystemOnPlayerLeave)
                    {
                        enemySpawnPoints[i].resetSpawnPoint();
                    }
                }
            }
        }
    }

    public void disableEnemyObjects()
    {

    }

    public void resetEnemyObjectStatesAndPositions()
    {
        for (int i = 0; i < enemyGameObjects.Length; i++)
        {
            enemyGameObjects[i].structGameObject.transform.GetComponent<EnemyBase>().AllStatesResetEnable = true;
            enemyGameObjects[i].structGameObject.transform.position = enemyGameObjects[i].gameObjectSpawnPosition.position;

            if (spawnAllEnemiesInSection || enemyGameObjects[i].boolProperty) enemyGameObjects[i].structGameObject.SetActive(true);
        }

        for (int i = 0; i < enemySpawnPoints.Length; i++)
        {
            if (enemySpawnPoints[i].resetSpawnSystemOnPlayerLeave)
            {
                enemySpawnPoints[i].resetSpawnPoint();
            }
        }
    }
}
