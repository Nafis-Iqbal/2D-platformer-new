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
    public Transform camRef;

    #region Variables Declaration

    #region Assets and Dependencies
    public GameObject playerVirtualCamera;
    public GameObject overViewVirtualCamera;
    public GameObject player;
    public CinemachineVirtualCamera playerCameraBrain;
    private CinemachineFramingTransposer cameraBrainBodyProperties;
    private Transform playerTransform;
    private PlayerMovement playerMovementScript;
    private PlayerColumn playerColumn;
    private PlayerJump playerJump;
    private PlayerGrapplingGun playerGrapplingGun;
    private PlayerCombatSystem playerCombatSystemScript;

    [HideInInspector]
    public bool cameraBrainStat, playerSpawned;
    #endregion

    #region Camera States & Mechanical Properties
    [Header("CAMERA STATES & MECHANICAL PROPERTIES")]
    private float lensLerpingTempValue, screenXTempValue, screenYTempValue;
    public float lensChangeSmoothingFloatValue;
    public float rightLookScreenXValue, leftLookScreenXValue;
    public float lookDownScreenYValue, lookDefaultScreenYValue, lookUpScreenYValue;//lookDefaultScreenYValue depends on level section
    public float horizontalViewTransitionSpeed, verticalViewTransitionSpeed;
    public bool playerLookingDown, lookDownHeld, lookDownDuringFall;
    float shakeTime;
    public Color defaultCameraBackground;
    public bool cameraLookingAheadRight, cameraLookingDown;
    float lastLookDownHeldTime, lastRightLookTime, lastLeftLookTime;
    public float cameraLookDownTime, waitTimeForViewDirectionChange;
    public bool screenXViewTransitionInProgress, screenYViewTransitionInProgress;

    [Header("CAMERA LENS ZOOM VALUES")]
    public bool defaultState;
    public bool combatState, sprintState, slidingState, climbState, grappleState, lookDownState;
    public float defaultZoomValue = 22.0f;
    public float defaultCombatModeZoomValue;
    public float sprintZoomValue;
    public float slidingZoomValue;
    public float climbHoldZoomValue, climbIdleZoomValue;
    public float grappleSwingZoomValue;
    public float lookDownZoomValue;
    #endregion

    #region Scripting Variables
    private Vector3 temp;
    private int camStat, lastPlayerPosition, lastActiveCamera;//0 for player below ; 1 for player level ; 2 for player above
    private bool turnCameraBrainOn;
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

        cameraBrainStat = turnCameraBrainOn = true;
        playerSpawned = false;
        playerLookingDown = lookDownHeld = lookDownDuringFall = false;
        cameraLookingAheadRight = true;
        shakeTime = 0.0f;
        lastRightLookTime = lastLeftLookTime = Time.time;

        overViewVirtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = defaultZoomValue;
        cameraBrainBodyProperties = playerCameraBrain.GetCinemachineComponent<CinemachineFramingTransposer>();
        lastActiveCamera = 0;

        playerTransform = GameManager.Instance.playerTransform;
        playerMovementScript = playerTransform.GetComponent<PlayerMovement>();
        playerColumn = playerTransform.GetComponent<PlayerColumn>();
        playerJump = playerTransform.GetComponent<PlayerJump>();
        playerGrapplingGun = playerTransform.GetComponent<PlayerGrapplingGun>();
        playerCombatSystemScript = playerTransform.GetComponent<PlayerCombatSystem>();

        initStateVariables();
        defaultState = true;
        screenXViewTransitionInProgress = screenYViewTransitionInProgress = false;
    }

    void initStateVariables()
    {
        defaultState = combatState = sprintState = slidingState = climbState = grappleState = lookDownState = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelTransitionScript.Instance.isPlayerOutdoors)
        {
            #region Lens Changing
            initStateVariables();
            if (playerCombatSystemScript.combatMode)
            {
                //changeCameraLensForOutdoors(defaultCombatModeZoomValue);//Will be different for boss encounters, change later
                cameraLensChangerNew(defaultCombatModeZoomValue);
                combatState = true;
            }
            else if (playerMovementScript.isSprinting)
            {
                //changeCameraLensForOutdoors(sprintZoomValue);
                cameraLensChangerNew(sprintZoomValue);
                sprintState = true;
            }
            else if (playerMovementScript.isSliding || playerMovementScript.isSlideJumping)
            {
                //changeCameraLensForOutdoors(slidingZoomValue);
                cameraLensChangerNew(slidingZoomValue);
                slidingState = true;
            }
            else if (playerGrapplingGun.isExecuting || playerGrapplingGun.playerIsOnAir)
            {
                //changeCameraLensForOutdoors(grappleSwingZoomValue);
                cameraLensChangerNew(grappleSwingZoomValue);
                grappleState = true;
            }
            else if (playerColumn.isExecuting)
            {
                //changeCameraLensForOutdoors(climbIdleZoomValue);
                cameraLensChangerNew(climbIdleZoomValue);
                climbState = true;
            }
            else if (playerColumn.isHoldLooking)
            {
                //changeCameraLensForOutdoors(climbHoldZoomValue);
                cameraLensChangerNew(climbHoldZoomValue);
                climbState = true;
            }
            else if (lookDownHeld || lookDownDuringFall)
            {
                //changeCameraLensForOutdoors(lookDownZoomValue);
                cameraLensChangerNew(lookDownZoomValue);
                lookDownState = true;
            }
            else
            {
                //changeCameraLensForOutdoors(defaultZoomValue);
                cameraLensChangerNew(defaultZoomValue);
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

                    // if (cameraLookingAheadRight == false && triggerViewDirectionChange == false)
                    // {
                    //     triggerViewDirectionChange = true;
                    // }
                    if (Time.time - lastLeftLookTime > waitTimeForViewDirectionChange)
                    {
                        changeCameraViewDirectionXNew(rightLookScreenXValue);
                        cameraLookingAheadRight = true;
                    }

                    //changeCameraViewDirectionXNew(rightLookScreenXValue);
                    //cameraLookingAheadRight = true;
                }
                else//ScreenX = .7 look left
                {
                    lastLeftLookTime = Time.time;

                    if (Time.time - lastRightLookTime > waitTimeForViewDirectionChange)
                    {
                        changeCameraViewDirectionXNew(leftLookScreenXValue);
                        cameraLookingAheadRight = false;
                    }

                    //changeCameraViewDirectionXNew(leftLookScreenXValue);
                    //cameraLookingAheadRight = false;
                }

                if (lookDownHeld == true || lookDownDuringFall == true)//ScreenY = .25 look Down
                {
                    //screenYViewTransitionInProgress = true;
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
                        playerVirtualCamera.GetComponent<CinemachineVirtualCamera>().
                        GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                    cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0.0f;
                }
            }
        }

    }

    public void changeCameraBackgroundColor(Color newColor)
    {
        mainCamera.backgroundColor = newColor;
    }

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

    public void changeCameraLensForOutdoors(float lensZoomValue)
    {
        //the script for replacing orthographic size
        //Debug.Log("lens: " + lensZoomValue);
        lensLerpingTempValue = playerVirtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize;
        if (Mathf.Abs(lensLerpingTempValue - lensZoomValue) < .01f) return;

        StartCoroutine(cameraLensChanger(lensZoomValue));
    }

    public void changeCameraLensForIndoors()
    {
        float targetZoomValue = LevelTransitionScript.Instance.levelSections[LevelTransitionScript.Instance.currentActiveLevelSection].sceneDefaultCameraLensValue;
        //the script for replacing orthographic size
        lensLerpingTempValue = playerVirtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize;
        if (Mathf.Abs(lensLerpingTempValue - targetZoomValue) < .01f) return;

        StartCoroutine(cameraLensChanger(targetZoomValue));
    }

    public void changeCameraLensDefault()
    {
        //the script for replacing orthographic size
        lensLerpingTempValue = playerVirtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize;
        StartCoroutine(cameraLensChanger(defaultZoomValue));
    }

    IEnumerator cameraLensChanger(float targetOrthographicValue)
    {
        if (lensLerpingTempValue > targetOrthographicValue)
        {
            while (lensLerpingTempValue > targetOrthographicValue)
            {
                lensLerpingTempValue -= Time.deltaTime * lensChangeSmoothingFloatValue;
                playerVirtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = lensLerpingTempValue;
                yield return null;
            }
        }
        else
        {
            while (lensLerpingTempValue < targetOrthographicValue)
            {
                lensLerpingTempValue += Time.deltaTime * lensChangeSmoothingFloatValue;
                playerVirtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = lensLerpingTempValue;
                yield return null;
            }
        }
    }

    public void cameraLensChangerNew(float targetOrthographicValue)
    {
        lensLerpingTempValue = playerVirtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize;

        if (Mathf.Abs(lensLerpingTempValue - targetOrthographicValue) > .01f)
        {
            if (lensLerpingTempValue > targetOrthographicValue)
            {
                lensLerpingTempValue -= Time.deltaTime * lensChangeSmoothingFloatValue;
                playerVirtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = lensLerpingTempValue;
            }
            else
            {
                lensLerpingTempValue += Time.deltaTime * lensChangeSmoothingFloatValue;
                playerVirtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = lensLerpingTempValue;
            }
        }
    }

    public void changeCameraViewDirectionXNew(float targetScreenXValue)
    {
        //the script for replacing orthographic size
        screenXTempValue = cameraBrainBodyProperties.m_ScreenX;
        //Debug.Log("check val1 : " + screenXTempValue + " val2: " + targetScreenXValue);

        if (Mathf.Abs(screenXTempValue - targetScreenXValue) > .01f)
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

        if (Mathf.Abs(screenYTempValue - targetScreenYValue) > .01f)
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

        playerVirtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = targetPlayer.transform;
    }
    #endregion

    #region Camera Shake Effects
    #endregion
}
