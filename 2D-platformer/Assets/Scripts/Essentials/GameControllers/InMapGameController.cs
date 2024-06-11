using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InMapGameController : MonoBehaviour
{
    public static InMapGameController Instance;
    public LevelSectionController activeLevelSectionScript;
    public Transform lastCheckpointSpawn;
    public CheckpointController activeCheckPointController;
    public LevelSectionController activeCheckPointLevelSection;
    bool gameSessionLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameSessionLoaded = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameSessionLoaded == false)
        {
            gameSessionLoaded = true;
            activeLevelSectionScript.isSectionActive = true;
            activeLevelSectionScript.ActivateAllObjects();
        }
    }

    public void reloadLastCheckpoint()
    {
        gameSessionLoaded = true;
        GameManager.Instance.playerTransform.gameObject.SetActive(false);
        CameraController.Instance.triggerRespawnPlayerCameraTransition(lastCheckpointSpawn.position);

        activeLevelSectionScript.resetEnemyObjectStatesAndPositions();
        activeLevelSectionScript = activeCheckPointLevelSection;
        activeCheckPointLevelSection.isSectionActive = true;
        activeLevelSectionScript.ActivateAllObjects();

        GameController.Instance.reloadLastCheckpoint();
    }

    public void SaveProgress()
    {

    }
}
