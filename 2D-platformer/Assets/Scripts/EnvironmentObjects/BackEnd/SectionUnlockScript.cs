using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectionUnlockScript : MonoBehaviour
{
    public bool sectionUnlocked = false, sectionOpenEventDone = false;
    public GameObjectStruct[] objectiveCounts = new GameObjectStruct[1];
    public GameObjectStruct[] objectiveRequirements = new GameObjectStruct[1];

    public GameObject[] gameObjectsToActivate = new GameObject[2];
    public GameObject[] gameObjectsToDeactivate = new GameObject[2];
    public Animator[] animationsToTrigger = new Animator[2];

    public Collider2D[] collidersToEnable = new Collider2D[2];
    public Collider2D[] collidersToDisable = new Collider2D[2];

    // Start is called before the first frame update
    void OnEnable()
    {
        sectionUnlocked = sectionOpenEventDone = false;

        for (int i = 0; i < objectiveCounts.Length; i++)
        {
            objectiveCounts[i].intProperty = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (sectionUnlocked && !sectionOpenEventDone)
        {
            sectionOpenEventDone = true;

            for (int i = 0; i < gameObjectsToActivate.Length; i++)
            {
                gameObjectsToActivate[i].SetActive(true);
            }

            for (int i = 0; i < gameObjectsToDeactivate.Length; i++)
            {
                gameObjectsToDeactivate[i].SetActive(false);
            }

            for (int i = 0; i < animationsToTrigger.Length; i++)
            {
                animationsToTrigger[i].SetTrigger("Destroy");
            }

            for (int i = 0; i < collidersToEnable.Length; i++)
            {
                collidersToEnable[i].enabled = true;
            }

            for (int i = 0; i < collidersToDisable.Length; i++)
            {
                collidersToDisable[i].enabled = false;
            }
        }
    }

    public void updateObjectiveCountStates(string objectiveName, GameObject objectToDeactivate = null)
    {
        for (int i = 0; i < objectiveCounts.Length; i++)
        {
            if (objectiveCounts[i].gameObjectName == objectiveName)
            {
                objectiveCounts[i].intProperty++;
                if (objectToDeactivate != null) objectToDeactivate.SetActive(false);
                break;
            }
        }

        int requirmentsMatched = 0;
        for (int i = 0; i < objectiveRequirements.Length; i++)
        {
            for (int j = 0; j < objectiveCounts.Length; j++)
            {
                if (objectiveCounts[j].gameObjectName == objectiveRequirements[i].gameObjectName)
                {
                    if (objectiveCounts[j].intProperty >= objectiveRequirements[i].intProperty) requirmentsMatched++;
                    break;
                }
            }
        }
        if (requirmentsMatched == objectiveRequirements.Length) sectionUnlocked = true;
    }
}
