using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[System.Serializable]
public class LevelSectionInfo
{
    public int sectionID;

    [Header("SECTION PROPERTIES")]
    public Vector3 sectionInActiveScale;
    public bool spawnPlayerFacingRight;
    public bool isBackgroundSection;
    public bool isBuildingInterior;
    public float buildingInteriorLightIntensity;
    public float sceneDefaultCameraLensValue;
    public Color cameraBackgroundColor;

    [Header("SECTION OBJECTS")]
    public GameObject sectionObject;
    public GameObject midBackgroundObjectCollection;

    public GameObject backPropsLayerObjectCollection;

    public GameObject playerLayerObjectCollection;

    public GameObject playerLayerShapeObjectCollection;

    public GameObject pathLayerObjectCollection;

    public GameObject frontPropsLayerObjectCollection;

    public GameObject foreGroundLayerObjectCollection;

    public GameObject foreGroundLayerShapeObjectCollection;

    public Collider2D[] objectWithColliders;
    public GameObject[] playerSpawnPositions = new GameObject[2];
}
