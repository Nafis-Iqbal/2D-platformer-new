using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;
using UnityEngine.SceneManagement;
using CodeMonkey.Utils;

public class TestSome : MonoBehaviour
{
    // keep a copy of the executing script
    private IEnumerator coroutine;
    public HealthStaminaSystem testHealthScript;

    void OnEnable()
    {
        Transform uiCanvas = GameObject.Find("CanvasV1").transform;
        CMDebug.ButtonUI(uiCanvas, new Vector2(320, 0), "Reload", () => reloadScene());
    }

    // Use this for initialization
    void Start()
    {

    }

    // print to the console every 3 seconds.
    // yield is causing WaitAndPrint to pause every 3 seconds
    public IEnumerator WaitAndPrint(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            print("WaitAndPrint " + Time.time + " " + coroutine);
        }
    }

    void Update()
    {
        
    }

    public void testHealthScriptHealth()
    {
        testHealthScript.modifyHealth(20.0f);
    }

    public void reloadScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
