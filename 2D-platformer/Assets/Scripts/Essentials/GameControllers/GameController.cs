using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject gamePausePanel, aboutPanel, controlsPanel, testNotesPanel, pauseButton;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onGamePause(bool command)
    {
        gamePausePanel.SetActive(command);
        pauseButton.SetActive(!command);
    }

    public void toggleAboutPanel(bool command)
    {
        aboutPanel.SetActive(command);
    }

    public void toggleControlsPanel(bool command)
    {
        controlsPanel.SetActive(command);
    }

    public void toggleNotesPanel(bool command)
    {
        testNotesPanel.SetActive(command);
    }

    public void reloadGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
