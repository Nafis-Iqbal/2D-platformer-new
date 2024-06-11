using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.ComponentModel;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    Camera mainCamera;

    #region Variables Declaration

    #region Assets and Dependencies
    public GameObject player;
    public CinemachineVirtualCamera playerCameraBrain;
    public CinemachineVirtualCamera overviewCameraBrain;
    [HideInInspector]
    public CinemachineVirtualCamera overrideCameraBrain;
    public Transform farBackgroundObjects;
    private CinemachineFramingTransposer cameraBrainBodyProperties;
    private Transform playerTransform;
    private PlayerMovement playerMovementScript;
    private PlayerColumn playerColumn;
    private PlayerJump playerJump;
    private PlayerGrapplingGun playerGrapplingGun;
    private PlayerCombatSystem playerCombatSystemScript;

    [HideInInspector]
    public bool playerSpawned;
    #endregion

    [Header("CAMERA MECHANICAL PROPERTIES")]
    public bool changeFarBackgroundScale;
    public float lensChangeSmoothingFloatValue;
    public float rightLookScreenXValue, leftLookScreenXValue;
    public float horizontalViewTransitionSpeed, verticalViewTransitionSpeed;
    public float lookDownScreenYValue, lookDefaultScreenYValue, lookUpScreenYValue;//lookDefaultScreenYValue depends on level section
    public float cameraLookDownTime, waitTimeForViewDirectionChange;

    [Header("Camera States")]
    public bool defaultState;
    public bool defaultCameraSettingsOverrideMode;
    public bool combatState, sprintState, slidingState, climbState, grappleState, lookDownState, wideAngleObjectInUse;
    public bool cameraZoomOverrideMode;
    public bool cameraLookingAheadRight, cameraLookingDown;
    public bool playerLookingDown, lookDownHeld, lookDownDuringFall;
    public bool screenXViewTransitionInProgress, screenYViewTransitionInProgress;

    [Header("CAMERA LENS ZOOM VALUES")]

    public float defaultZoomValue = 22.0f;
    public float defaultCombatModeZoomValue;
    public float overrideCameraYValue;
    public float sprintZoomValue;
    public float slidingZoomValue;
    public float climbHoldZoomValue, climbIdleZoomValue;
    public float grappleSwingZoomValue;
    public float lookDownZoomValue;
    public float objectInUseZoomValue;
    public float farBackgroundMultiplier;//currentZoomValu/defaultZoomValue

    #region Camera Level Section Override
    public int activeCameraPriorityValue = 20, inactiveCameraPriorityValue = 15;
    #endregion

    #region Scripting Vars & Others
    float shakeTime;
    public Color defaultCameraBackground;
    float lensLerpingTempValue, screenXTempValue, screenYTempValue;
    float lastLookDownHeldTime, lastRightLookTime, lastLeftLookTime;
    Vector3 initialFarBackgroundScale, tempFarBackgroundScale, scaleChangeMultiplier;
    bool lensZoomReached;
    #endregion

    #endregion

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
        mainCamera = GetComponent<Camera>();

        playerSpawned = false;
        playerLookingDown = lookDownHeld = lookDownDuringFall = false;
        cameraLookingAheadRight = true;
        shakeTime = 0.0f;
        lastRightLookTime = lastLeftLookTime = Time.time;

        overviewCameraBrain.m_Lens.OrthographicSize = defaultZoomValue;
        cameraBrainBodyProperties = playerCameraBrain.GetCinemachineComponent<CinemachineFramingTransposer>();

        playerTransform = GameManager.Instance.playerTransform;
        playerMovementScript = playerTransform.GetComponent<PlayerMovement>();
        playerColumn = playerTransform.GetComponent<PlayerColumn>();
        playerJump = playerTransform.GetComponent<PlayerJump>();
        playerGrapplingGun = playerTransform.GetComponent<PlayerGrapplingGun>();
        playerCombatSystemScript = playerTransform.GetComponent<PlayerCombatSystem>();

        playerCameraBrain.m_Follow = playerTransform;

        initStateVariables();
        defaultState = true;
        defaultCameraSettingsOverrideMode = false;
        screenXViewTransitionInProgress = screenYViewTransitionInProgress = false;

        //Variable having special function
        cameraZoomOverrideMode = false;
        lensZoomReached = false;
    }

    void initStateVariables()
    {
        defaultState = combatState = sprintState = slidingState = climbState = grappleState = lookDownState;
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelTransitionScript.Instance.isPlayerOutdoors && cameraZoomOverrideMode == false)
        {
            #region Lens Changing
            initStateVariables();
            if (playerCombatSystemScript.combatMode)
            {
                //changeCameraLensForOutdoors(defaultCombatModeZoomValue);//Will be different for boss encounters, change later
                cameraLensChangerNew(defaultCombatModeZoomValue);
                farBackgroundScaleChange(defaultCombatModeZoomValue / defaultZoomValue);
                combatState = true;
            }
            else if (playerMovementScript.isSprinting)
            {
                //changeCameraLensForOutdoors(sprintZoomValue);
                cameraLensChangerNew(sprintZoomValue);
                farBackgroundScaleChange(sprintZoomValue / defaultZoomValue);
                sprintState = true;
            }
            else if (playerMovementScript.isSliding || playerMovementScript.isSlideJumping)
            {
                //changeCameraLensForOutdoors(slidingZoomValue);
                cameraLensChangerNew(slidingZoomValue);
                farBackgroundScaleChange(slidingZoomValue / defaultZoomValue);
                slidingState = true;
            }
            else if (playerGrapplingGun.isExecuting || playerGrapplingGun.playerIsOnAir)
            {
                //changeCameraLensForOutdoors(grappleSwingZoomValue);
                cameraLensChangerNew(grappleSwingZoomValue);
                farBackgroundScaleChange(grappleSwingZoomValue / defaultZoomValue);
                grappleState = true;
            }
            else if (playerColumn.isExecuting)
            {
                //changeCameraLensForOutdoors(climbIdleZoomValue);
                cameraLensChangerNew(climbIdleZoomValue);
                farBackgroundScaleChange(climbIdleZoomValue / defaultZoomValue);
                climbState = true;
            }
            else if (playerColumn.isHoldLooking)
            {
                //changeCameraLensForOutdoors(climbHoldZoomValue);
                cameraLensChangerNew(climbHoldZoomValue);
                farBackgroundScaleChange(climbHoldZoomValue / defaultZoomValue);
                climbState = true;
            }
            else if (lookDownHeld || lookDownDuringFall)
            {
                //changeCameraLensForOutdoors(lookDownZoomValue);
                cameraLensChangerNew(lookDownZoomValue);
                farBackgroundScaleChange(lookDownZoomValue / defaultZoomValue);
                lookDownState = true;
            }
            else if (wideAngleObjectInUse)
            {
                cameraLensChangerNew(objectInUseZoomValue, 7.5f);
                farBackgroundScaleChange(objectInUseZoomValue / defaultZoomValue, 7.5f);
                defaultState = false;
            }
            else
            {
                //changeCameraLensForOutdoors(defaultZoomValue);
                cameraLensChangerNew(defaultZoomValue, 7.5f);
                farBackgroundScaleChange(1.0f, 7.5f);
                defaultState = true;
            }
            #endregion

            #region Camera View Direction
            if (playerMovementScript.isSliding || playerMovementScript.isSlideJumping || playerGrapplingGun.isExecuting || playerGrapplingGun.playerIsOnAir)//center camera
            {
                //Debug.Log("Amar heda");
                changeCameraViewDirectionXNew(.5f);
                changeCameraViewDirectionYNew(.5f);
            }
            else if (playerColumn.isHoldLooking || playerColumn.isExecuting)//opposite of player facing direction
            {
                if (playerMovementScript.playerFacingRight == true)//ScreenX = .3 look right
                {
                    changeCameraViewDirectionXNew(leftLookScreenXValue);
                    cameraLookingAheadRight = false;
                }
                else//ScreenX = .7 look left
                {
                    changeCameraViewDirectionXNew(rightLookScreenXValue);
                    cameraLookingAheadRight = true;
                }

                if (lookDownHeld == true || lookDownDuringFall == true)//ScreenY = .25 look Down
                {
                    if (cameraBrainBodyProperties.m_ScreenY > lookDownScreenYValue)
                    {
                        cameraBrainBodyProperties.m_ScreenY -= Time.deltaTime * verticalViewTransitionSpeed;
                    }
                }
                else if (Time.time - lastLookDownHeldTime > cameraLookDownTime)//ScreenY = .7 look level
                {
                    if (cameraBrainBodyProperties.m_ScreenY < lookDefaultScreenYValue)
                    {
                        cameraBrainBodyProperties.m_ScreenY += Time.deltaTime * verticalViewTransitionSpeed;
                    }
                }
            }
            else // point camera towards facing direction
            {
                if (playerMovementScript.playerFacingRight == true)//ScreenX = .3 look right
                {
                    lastRightLookTime = Time.time;

                    if (Time.time - lastLeftLookTime > waitTimeForViewDirectionChange)
                    {
                        changeCameraViewDirectionXNew(rightLookScreenXValue);
                        cameraLookingAheadRight = true;
                    }
                }
                else//ScreenX = .7 look left
                {
                    lastLeftLookTime = Time.time;

                    if (Time.time - lastRightLookTime > waitTimeForViewDirectionChange)
                    {
                        changeCameraViewDirectionXNew(leftLookScreenXValue);
                        cameraLookingAheadRight = false;
                    }
                }

                if (defaultCameraSettingsOverrideMode)
                {
                    changeCameraViewDirectionYNew(overrideCameraYValue);
                }
                else if (lookDownHeld == true || lookDownDuringFall == true)//ScreenY = .25 look Down
                {
                    if (cameraBrainBodyProperties.m_ScreenY > lookDownScreenYValue)
                    {
                        cameraBrainBodyProperties.m_ScreenY -= Time.deltaTime * verticalViewTransitionSpeed;
                    }
                }
                else if (Time.time - lastLookDownHeldTime > cameraLookDownTime)//ScreenY = .7 look level
                {
                    if (cameraBrainBodyProperties.m_ScreenY < lookDefaultScreenYValue)
                    {
                        cameraBrainBodyProperties.m_ScreenY += Time.deltaTime * verticalViewTransitionSpeed;
                    }
                }
            }
            #endregion

            if (shakeTime > 0)
            {
                shakeTime -= Time.deltaTime;
                if (shakeTime <= 0)
                {
                    CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
                        playerCameraBrain.
                        GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                    cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0.0f;
                }
            }
        }
    }

    #region Camera Mechanical Methods
    public void cameraLensChangerNew(float targetOrthographicValue, float zoomSpeedMultiplier = 1.0f)
    {
        lensLerpingTempValue = playerCameraBrain.m_Lens.OrthographicSize;

        if (Mathf.Abs(lensLerpingTempValue - targetOrthographicValue) > .35f)
        {
            if (lensLerpingTempValue > targetOrthographicValue)
            {
                lensLerpingTempValue -= Time.deltaTime * lensChangeSmoothingFloatValue * zoomSpeedMultiplier;
                playerCameraBrain.m_Lens.OrthographicSize = lensLerpingTempValue;
            }
            else
            {
                lensLerpingTempValue += Time.deltaTime * lensChangeSmoothingFloatValue * zoomSpeedMultiplier;
                playerCameraBrain.m_Lens.OrthographicSize = lensLerpingTempValue;
            }
        }
    }

    public void farBackgroundScaleChange(float targetScaleValue, float scaleSpeedMultiplier = 1.0f)
    {
        if (changeFarBackgroundScale == false) return;

        tempFarBackgroundScale = farBackgroundObjects.transform.localScale;

        if (Mathf.Abs(tempFarBackgroundScale.x - targetScaleValue) > .05f)
        {
            if (tempFarBackgroundScale.x > targetScaleValue)
            {
                tempFarBackgroundScale -= Vector3.one * Time.deltaTime * lensChangeSmoothingFloatValue * scaleSpeedMultiplier;
                farBackgroundObjects.transform.localScale = tempFarBackgroundScale;
            }
            else
            {
                tempFarBackgroundScale += Vector3.one * Time.deltaTime * lensChangeSmoothingFloatValue * scaleSpeedMultiplier;
                farBackgroundObjects.transform.localScale = tempFarBackgroundScale;
            }
        }
    }

    public void changeCameraViewDirectionXNew(float targetScreenXValue)
    {
        //the script for replacing orthographic size
        screenXTempValue = cameraBrainBodyProperties.m_ScreenX;
        //Debug.Log("check val1 : " + screenXTempValue + " val2: " + targetScreenXValue);

        if (Mathf.Abs(screenXTempValue - targetScreenXValue) > .035f)
        {
            if (screenXTempValue > targetScreenXValue)
            {
                screenXTempValue -= Time.deltaTime * horizontalViewTransitionSpeed;
                cameraBrainBodyProperties.m_ScreenX = Mathf.Max(screenXTempValue, targetScreenXValue);
            }
            else
            {
                screenXTempValue += Time.deltaTime * horizontalViewTransitionSpeed;
                cameraBrainBodyProperties.m_ScreenX = Mathf.Min(screenXTempValue, targetScreenXValue);
            }
        }
    }

    public void changeCameraViewDirectionYNew(float targetScreenYValue)
    {
        //the script for replacing orthographic size
        screenYTempValue = cameraBrainBodyProperties.m_ScreenY;

        if (Mathf.Abs(screenYTempValue - targetScreenYValue) > .035f)
        {
            if (screenYTempValue > targetScreenYValue)
            {
                screenYTempValue -= Time.deltaTime * verticalViewTransitionSpeed;
                cameraBrainBodyProperties.m_ScreenY = Mathf.Max(screenYTempValue, targetScreenYValue);
            }
            else
            {
                screenYTempValue += Time.deltaTime * verticalViewTransitionSpeed;
                cameraBrainBodyProperties.m_ScreenY = Mathf.Min(screenYTempValue, targetScreenYValue);
            }
        }
    }

    public void CameraSystemOverrideForLevelSection(CinemachineVirtualCamera overrideCamera)
    {
        cameraZoomOverrideMode = true;

        overrideCameraBrain = overrideCamera;
        overrideCameraBrain.m_Priority = activeCameraPriorityValue;

        playerCameraBrain.m_Priority = inactiveCameraPriorityValue;
    }

    public void DisableCameraOverrideMode()
    {
        cameraZoomOverrideMode = false;
        overrideCameraBrain.m_Priority = inactiveCameraPriorityValue;

        playerCameraBrain.m_Priority = activeCameraPriorityValue;

    }
    #endregion

    #region Camera Look Down Methods
    public void OnLookDown(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            //Debug.Log("Look dooown!!!");
            lookDownHeld = true;
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            //Debug.Log("Look up!!!");
            lookDownHeld = false;
            lastLookDownHeldTime = Time.time;
        }
    }

    public void OnLookDownDuringFall()
    {
        //Debug.Log("Look dooown!!!");
        if (playerGrapplingGun.isExecuting == true || playerGrapplingGun.playerIsOnAir || playerColumn.isExecuting || playerMovementScript.isSlideJumping) return;

        lookDownDuringFall = true;
    }

    public void OnLookDownDuringFallEnd()
    {
        //Debug.Log("Look dooown!!!");
        lookDownDuringFall = false;
    }
    #endregion

    #region Accessory Methods
    //Zoom IDs 1-4 for Titans, rest for pilot weapons
    public void acquirePlayerObject(GameObject targetPlayer, int targetZoomID)
    {
        if (playerSpawned == false) playerSpawned = true;

        player = targetPlayer;
        if (targetZoomID < 0)
        {
            changeCameraLensDefault();
        }

        playerCameraBrain.Follow = targetPlayer.transform;
    }

    public void changeCameraBackgroundColor(Color newColor)
    {
        mainCamera.backgroundColor = newColor;
    }
    #endregion

    #region Camera Effects
    public void triggerRespawnPlayerCameraTransition(Vector3 spawnPosition)
    {
        //trasition effect code
        if (playerTransform.parent == null)
        {
            playerTransform.position = spawnPosition;
            playerTransform.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
        else
        {
            playerTransform.parent.transform.position = spawnPosition;
            playerTransform.parent.transform.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        playerTransform.gameObject.SetActive(true);
        MovementLimiter.instance.playerCanMove = true;
    }
    #endregion

    #region Unused
    public void changeCameraLensForOutdoors(float lensZoomValue)
    {
        //the script for replacing orthographic size
        lensLerpingTempValue = playerCameraBrain.m_Lens.OrthographicSize;
        if (Mathf.Abs(lensLerpingTempValue - lensZoomValue) < .01f) return;

        StartCoroutine(cameraLensChanger(lensZoomValue));
    }

    public void changeCameraLensForIndoors()
    {
        float targetZoomValue = LevelTransitionScript.Instance.levelSections[LevelTransitionScript.Instance.currentActiveLevelSection].sceneDefaultCameraLensValue;
        //the script for replacing orthographic size
        lensLerpingTempValue = playerCameraBrain.m_Lens.OrthographicSize;
        if (Mathf.Abs(lensLerpingTempValue - targetZoomValue) < .01f) return;

        StartCoroutine(cameraLensChanger(targetZoomValue));
    }

    public void changeCameraLensDefault()
    {
        //the script for replacing orthographic size
        lensLerpingTempValue = playerCameraBrain.m_Lens.OrthographicSize;
        StartCoroutine(cameraLensChanger(defaultZoomValue));
    }

    IEnumerator cameraLensChanger(float targetOrthographicValue)
    {
        if (lensLerpingTempValue > targetOrthographicValue)
        {
            while (lensLerpingTempValue > targetOrthographicValue)
            {
                lensLerpingTempValue -= Time.deltaTime * lensChangeSmoothingFloatValue;
                playerCameraBrain.m_Lens.OrthographicSize = lensLerpingTempValue;
                yield return null;
            }
        }
        else
        {
            while (lensLerpingTempValue < targetOrthographicValue)
            {
                lensLerpingTempValue += Time.deltaTime * lensChangeSmoothingFloatValue;
                playerCameraBrain.m_Lens.OrthographicSize = lensLerpingTempValue;
                yield return null;
            }
        }
    }
    #endregion
}
