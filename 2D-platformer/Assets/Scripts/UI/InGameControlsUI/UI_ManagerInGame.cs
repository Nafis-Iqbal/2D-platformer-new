using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UI_ManagerInGame : MonoBehaviour
{

    #region Game Status vars

    #endregion

    #region Scene Objects
    [Header("Scene Objects")]
    public GameObject gamePausePanel;
    public GameObject loadoutPanel;

    [Header("Pilot and Titan UI")]
    public GameObject[] uiAssistObjects = new GameObject[6];
    public GameObject interfacingButtons;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    #endregion

    #region Scoreboard Vars
    #endregion

    #region Dependencies
    [Header("Dependencies")]
    public UI_FeedbackScript UIFeedback;
    public UI_GameNotifications gameNotifications;
    #endregion

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    #region UI Utility Methods
    public void setPlayerUIObject(bool status)
    {
        if (status == true)
        {

        }
        else
        {

        }
    }

    public void setInventoryUIObject(bool status)
    {
        if (status == true)
        {

        }
        else
        {

        }
    }
    #endregion

    #region Scene UI Methods
    public void openOrClosePauseGamePanel(bool command)
    {
        //command true means pause game
        gamePausePanel.SetActive(command);
        setPlayerUIObject(!command);
    }

    public void openOrCloseLoadoutPanel(bool command)
    {
        loadoutPanel.SetActive(command);
        //DISABLE PLAYER INPUT HERE

        setPlayerUIObject(!command);
    }
    #endregion

    #region GameOver Methods

    #endregion

    #region Extra Utilities Methods

    #endregion
}
