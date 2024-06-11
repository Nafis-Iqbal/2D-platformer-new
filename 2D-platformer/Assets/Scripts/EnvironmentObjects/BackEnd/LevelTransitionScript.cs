using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class LevelTransitionScript : InteractionBase
{
    public static LevelTransitionScript Instance;
    public LevelSectionInfo[] levelSections = new LevelSectionInfo[5];
    LevelSectionInfo currentLevelSection;
    public int startActiveLevelSection = 0;
    public int currentActiveLevelSection;
    int targetLevelSection;
    public bool isLevelTransitionControlEnabled = false;
    public bool isPlayerOutdoors = true;

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

    }

    void OnEnable()
    {
        isInteractionEnabled = false;
        targetLevelSection = -1;

        for (int i = 0; i < levelSections.Length; i++)
        {
            for (int j = 0; j < levelSections[i].objectWithColliders.Length; j++)
            {
                levelSections[i].objectWithColliders[j].enabled = false;
            }
        }

        for (int j = 0; j < levelSections[currentActiveLevelSection].objectWithColliders.Length; j++)
        {
            levelSections[currentActiveLevelSection].objectWithColliders[j].enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        targetLevelSection = targetObjectID;

        if (isInteractionEnabled == true && isLevelTransitionControlEnabled == false)
        {
            isLevelTransitionControlEnabled = true;
            PlayerInputManager.Instance.playerInputActions.Player.Interact.Enable();
        }
        else if (isInteractionEnabled == false && isLevelTransitionControlEnabled == true)
        {
            isLevelTransitionControlEnabled = false;
            PlayerInputManager.Instance.playerInputActions.Player.Interact.Disable();
        }
    }

    public override void OnInteract()
    {
        levelSections[currentActiveLevelSection].sectionObject.transform.localScale = levelSections[currentActiveLevelSection].sectionInActiveScale;
        if (levelSections[currentActiveLevelSection].isBackgroundSection == false)
        {
            levelSections[currentActiveLevelSection].sectionObject.SetActive(false);
        }
        else
        {
            //change lighting parameters
            currentLevelSection = levelSections[currentActiveLevelSection];

            for (int i = 0; i < currentLevelSection.frontPropsLayerObjectCollection.transform.childCount; i++)
            {
                if (currentLevelSection.frontPropsLayerObjectCollection.transform.GetChild(i).GetComponent<SpriteRenderer>() != null)
                {
                    currentLevelSection.frontPropsLayerObjectCollection.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = "MidGround";
                }
                else currentLevelSection.frontPropsLayerObjectCollection.transform.GetChild(i).transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = "MidGround";
            }

            for (int i = 0; i < currentLevelSection.backPropsLayerObjectCollection.transform.childCount; i++)
            {
                currentLevelSection.backPropsLayerObjectCollection.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = "MidGround";
            }

            for (int i = 0; i < currentLevelSection.playerLayerObjectCollection.transform.childCount; i++)
            {
                currentLevelSection.playerLayerObjectCollection.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = "MidGround";
            }

            for (int i = 0; i < currentLevelSection.playerLayerShapeObjectCollection.transform.childCount; i++)
            {
                currentLevelSection.playerLayerShapeObjectCollection.transform.GetChild(i).GetComponent<SpriteShapeRenderer>().sortingLayerName = "MidGround";
            }

            for (int i = 0; i < currentLevelSection.pathLayerObjectCollection.transform.childCount; i++)
            {
                currentLevelSection.pathLayerObjectCollection.transform.GetChild(i).GetComponent<SpriteShapeRenderer>().sortingLayerName = "MidGround";
            }

            for (int i = 0; i < currentLevelSection.foreGroundLayerObjectCollection.transform.childCount; i++)
            {
                currentLevelSection.foreGroundLayerObjectCollection.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = "MidGround";
            }

            for (int i = 0; i < currentLevelSection.foreGroundLayerShapeObjectCollection.transform.childCount; i++)
            {
                currentLevelSection.foreGroundLayerShapeObjectCollection.transform.GetChild(i).GetComponent<SpriteShapeRenderer>().sortingLayerName = "MidGround";
            }

            for (int i = 0; i < currentLevelSection.objectWithColliders.Length; i++)
            {
                currentLevelSection.objectWithColliders[i].enabled = false;
            }
        }

        currentActiveLevelSection = targetLevelSection;
        currentLevelSection = levelSections[currentActiveLevelSection];

        currentLevelSection.sectionObject.SetActive(true);
        currentLevelSection.sectionObject.transform.localScale = Vector3.one;
        //change lighting parameters

        //trigger Camera Effect
        //Call delayed Coroutine to resume player controls once nekst section is loaded
        for (int i = 0; i < currentLevelSection.midBackgroundObjectCollection.transform.childCount; i++)
        {
            currentLevelSection.midBackgroundObjectCollection.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = "MidGround";
        }

        for (int i = 0; i < currentLevelSection.frontPropsLayerObjectCollection.transform.childCount; i++)
        {
            if (currentLevelSection.frontPropsLayerObjectCollection.transform.GetChild(i).GetComponent<SpriteRenderer>() != null)
            {
                currentLevelSection.frontPropsLayerObjectCollection.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = "FrontPropsLayer";
            }
            else
            {
                for (int j = 0; j < currentLevelSection.frontPropsLayerObjectCollection.transform.GetChild(i).transform.childCount; j++)
                {
                    currentLevelSection.frontPropsLayerObjectCollection.transform.GetChild(i).transform.GetChild(j).GetComponent<SpriteRenderer>().sortingLayerName = "FrontPropsLayer";
                }
            }
        }

        for (int i = 0; i < currentLevelSection.backPropsLayerObjectCollection.transform.childCount; i++)
        {
            if (currentLevelSection.backPropsLayerObjectCollection.transform.GetChild(i).GetComponent<SpriteRenderer>() != null)
            {
                currentLevelSection.backPropsLayerObjectCollection.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = "BackPropsLayer";
            }
            else
            {
                for (int j = 0; j < currentLevelSection.backPropsLayerObjectCollection.transform.GetChild(i).transform.childCount; j++)
                {
                    currentLevelSection.backPropsLayerObjectCollection.transform.GetChild(i).transform.GetChild(j).GetComponent<SpriteRenderer>().sortingLayerName = "BackPropsLayer";
                }
            }
        }

        for (int i = 0; i < currentLevelSection.playerLayerObjectCollection.transform.childCount; i++)
        {
            currentLevelSection.playerLayerObjectCollection.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = "PlayerLayer";
        }

        for (int i = 0; i < currentLevelSection.playerLayerShapeObjectCollection.transform.childCount; i++)
        {
            currentLevelSection.playerLayerShapeObjectCollection.transform.GetChild(i).GetComponent<SpriteShapeRenderer>().sortingLayerName = "PlayerLayer";
        }

        for (int i = 0; i < currentLevelSection.pathLayerObjectCollection.transform.childCount; i++)
        {
            currentLevelSection.pathLayerObjectCollection.transform.GetChild(i).GetComponent<SpriteShapeRenderer>().sortingLayerName = "PathLayer";
        }

        for (int i = 0; i < currentLevelSection.foreGroundLayerObjectCollection.transform.childCount; i++)
        {
            currentLevelSection.foreGroundLayerObjectCollection.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = "ForeGround";
        }

        for (int i = 0; i < currentLevelSection.foreGroundLayerShapeObjectCollection.transform.childCount; i++)
        {
            currentLevelSection.foreGroundLayerShapeObjectCollection.transform.GetChild(i).GetComponent<SpriteShapeRenderer>().sortingLayerName = "ForeGround";
        }

        for (int i = 0; i < currentLevelSection.objectWithColliders.Length; i++)
        {
            currentLevelSection.objectWithColliders[i].enabled = true;
        }

        //Changing Camera background color
        if (currentLevelSection.isBuildingInterior)
        {
            CameraController.Instance.changeCameraBackgroundColor(currentLevelSection.cameraBackgroundColor);
        }
        else CameraController.Instance.changeCameraBackgroundColor(CameraController.Instance.defaultCameraBackground);

        //Orient Player facing direction
        if (currentLevelSection.spawnPlayerFacingRight)
        {
            GameManager.Instance.playerMovementScript.rotateRight();
        }
        else GameManager.Instance.playerMovementScript.rotateLeft();

        //Spawn Player
        GameManager.Instance.playerTransform.position = currentLevelSection.playerSpawnPositions[targetSpawnPositionID].transform.position;
    }
}
