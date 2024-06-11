using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    public bool isCheckpointSet;
    public LevelSectionController parentLevelSectionObject;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (InMapGameController.Instance.activeCheckPointController != null) InMapGameController.Instance.activeCheckPointController.isCheckpointSet = false;

            InMapGameController.Instance.lastCheckpointSpawn = transform;
            InMapGameController.Instance.activeCheckPointController = this;
            InMapGameController.Instance.activeCheckPointLevelSection = parentLevelSectionObject;
            isCheckpointSet = true;
        }
    }
}
