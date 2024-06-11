using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public GameObject gamePausePanel, aboutPanel, controlsPanel, testNotesPanel, gameOverPanel, pauseButton;
    public GameObject xBoxLayout, ps4Layout, viewXBoxButton, viewPS4Button, viewGamepadButton, closeGamepadLayoutButton, returnToMenu;

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

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    #region  Game UI Integration
    public void onGamePause(bool command)
    {
        gamePausePanel.SetActive(command);
        pauseButton.SetActive(!command);

        if (command)//Pause Game when true
        {
            Time.timeScale = 0.05f;
            PlayerInputManager.Instance.DisablePlayerControls();
        }
        else
        {
            Time.timeScale = 1.0f;
            PlayerInputManager.Instance.EnablePlayerControls();
        }
    }

    public void toggleAboutPanel(bool command)
    {
        aboutPanel.SetActive(command);
    }

    public void toggleControlsPanel(bool command)
    {
        controlsPanel.SetActive(command);
        xBoxLayout.SetActive(false);
        ps4Layout.SetActive(false);
        returnToMenu.SetActive(command);
    }

    public void toggleNotesPanel(bool command)
    {
        testNotesPanel.SetActive(command);
    }

    public void viewXboxLayout()
    {
        xBoxLayout.SetActive(true);
        ps4Layout.SetActive(false);
    }

    public void viewPS4Layout()
    {
        xBoxLayout.SetActive(false);
        ps4Layout.SetActive(true);
    }

    public void viewGamepadMenu()
    {
        viewGamepadButton.SetActive(false);

        xBoxLayout.SetActive(false);
        ps4Layout.SetActive(true);

        viewXBoxButton.SetActive(true);
        viewPS4Button.SetActive(true);

        closeGamepadLayoutButton.SetActive(true);
        returnToMenu.SetActive(false);
    }

    public void viewControlsMenu()
    {
        viewGamepadButton.SetActive(true);

        xBoxLayout.SetActive(false);
        ps4Layout.SetActive(false);

        viewXBoxButton.SetActive(false);
        viewPS4Button.SetActive(false);

        closeGamepadLayoutButton.SetActive(false);
        returnToMenu.SetActive(true);
    }
    #endregion

    public void reloadLastCheckpoint()
    {
        gameOverPanel.SetActive(false);
        pauseButton.SetActive(true);

        Time.timeScale = 1.0f;
        PlayerInputManager.Instance.EnablePlayerControls();
    }

    public void reloadGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void quitGame()
    {
#if UNITY_EDITOR
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnPlayerDead()
    {
        gameOverPanel.SetActive(true);
        pauseButton.SetActive(false);

        Time.timeScale = 0.05f;
        PlayerInputManager.Instance.DisablePlayerControls();
    }
}
